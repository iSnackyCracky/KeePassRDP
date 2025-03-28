/*
 *  Copyright (C) 2018 - 2025 iSnackyCracky, NETertainer
 *
 *  This file is part of KeePassRDP.
 *
 *  KeePassRDP is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  KeePassRDP is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with KeePassRDP.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

using KeePass;
using KeePass.Resources;
using KeePass.UI;
using KeePassRDP.Utils;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassRDP
{
    internal class SecureProcess : Process
    {
        private static readonly Lazy<Type> _startupInfoType = new Lazy<Type>(
            () => AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).First(t => t.Name.EndsWith("STARTUPINFO")),
            LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<FieldInfo> _cbField = new Lazy<FieldInfo>(
            () => _startupInfoType.Value.GetField("cb", BindingFlags.Instance | BindingFlags.Public),
            LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<FieldInfo> _hStdInputField = new Lazy<FieldInfo>(
            () => _startupInfoType.Value.GetField("hStdInput", BindingFlags.Instance | BindingFlags.Public),
            LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<FieldInfo> _hStdOutputField = new Lazy<FieldInfo>(
            () => _startupInfoType.Value.GetField("hStdOutput", BindingFlags.Instance | BindingFlags.Public),
            LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<FieldInfo> _hStdErrorField = new Lazy<FieldInfo>(
            () => _startupInfoType.Value.GetField("hStdError", BindingFlags.Instance | BindingFlags.Public),
            LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<FieldInfo> _lpDesktopField = new Lazy<FieldInfo>(
            () => _startupInfoType.Value.GetField("lpDesktop", BindingFlags.Instance | BindingFlags.Public),
            LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<IntPtr> _oldConstructor = new Lazy<IntPtr>(() =>
        {
            var c = _startupInfoType.Value.GetConstructors().FirstOrDefault();
            RuntimeHelpers.PrepareMethod(c.MethodHandle);
            var ptr = c.MethodHandle.GetFunctionPointer();
            return ptr;
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<IntPtr> _newConstructor = new Lazy<IntPtr>(() =>
        {
            var c = typeof(SecureProcess).GetMethod("NewStartupInfo", BindingFlags.Static | BindingFlags.NonPublic);
            //RuntimeHelpers.PrepareMethod(c.MethodHandle);
            var ptr = c.MethodHandle.GetFunctionPointer();
            return ptr;
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private static byte[] _oldInstruction = null;
        [ThreadStatic]
        private static IntPtr? _handle = null;

        private readonly CancellationTokenSource _cancellationTokenSource;

        public SecureProcess() : base()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        internal bool StartSecure(bool prepare = true)
        {
            if (StartInfo.UseShellExecute)
                throw new NotSupportedException("SecureProcess.ProcessStartInfo.UseShellExecute");

            var ret = false;

            using (var mrs = new ManualResetEventSlim(false))
            {
                Task.Factory.StartNew(() =>
                {
                    /*if (prepare)
                        SecureDesktop.Instance.Prepare();*/

                    SecureDesktop.Instance.Run(lpDesktop =>
                    {
                        var handle = Marshal.SecureStringToGlobalAllocUnicode(lpDesktop);

                        if (_handle.HasValue)
                        {
                            Marshal.ZeroFreeGlobalAllocUnicode(_handle.Value);
                            _handle = null;
                        }
                        _handle = handle;

                        try
                        {
                            HijackMethod(_oldConstructor.Value, _newConstructor.Value);
                            ret = Start();
                        }
                        catch (Exception ex)
                        {
                            if (StartInfo.ErrorDialog)
                                Util.ShowErrorDialog(
                                    Util.FormatException(ex),
                                    null,
                                    string.Format("{0} - {1}", Util.KeePassRDP, KPRes.Error),
                                    VtdIcon.Error,
                                    Control.FromHandle(StartInfo.ErrorDialogParentHandle) as Form,
                                    null, 0, null, 0);
                        }
                        finally
                        {
                            RestoreMethod();
                            mrs.Set();

                            Marshal.ZeroFreeGlobalAllocUnicode(handle);
                            if (_handle.HasValue && _handle.Value == handle)
                                _handle = null;
                        }

                        if (!ret)
                            return SecureDesktop.CompletedTask;

                        try
                        {
                            Util.ProtectProcessWithDacl(this);
                        }
                        catch { }

                        return Task.Factory.StartNew(() =>
                        {
                            if (!WaitForInputIdle(30000) && !HasExited)
                                throw new TimeoutException("SecureProcess.WaitForInputIdle");
                        }, CancellationToken.None, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);
                    }, true);
                }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

                if (!mrs.Wait(TimeSpan.FromSeconds(90)))
                    throw new TimeoutException("SecureProcess.StartSecure");
            }

            return ret;
        }

        public bool Start(KprConfig config)
        {
            if (_handle.HasValue)
            {
                Marshal.FreeHGlobal(_handle.Value);
                _handle = null;
            }

            bool ret;

            if (config.MstscSecureDesktop)
                ret = StartSecure();
            else
            {
                if (!config.CredPickerSecureDesktop)
                    if (SecureDesktop.IsValueCreated && SecureDesktop.Instance.IsAlive && !SecureDesktop.Instance.IsCancellationRequested)
                        SecureDesktop.Instance.Cancel();

                ret = Start();
            }

            if (_handle.HasValue)
            {
                Marshal.FreeHGlobal(_handle.Value);
                _handle = null;
            }

            return ret;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                using (_cancellationTokenSource)
                    if (!_cancellationTokenSource.IsCancellationRequested)
                        _cancellationTokenSource.Cancel();

            base.Dispose(disposing);
        }

#pragma warning disable IDE0051
        private static void NewStartupInfo(object startupInfo)
#pragma warning restore IDE0051
        {
            /*
            public int cb;
            public IntPtr lpReserved = IntPtr.Zero;
            public IntPtr lpDesktop = IntPtr.Zero;
            public IntPtr lpTitle = IntPtr.Zero;
            public int dwX = 0;
            public int dwY = 0;
            public int dwXSize = 0;
            public int dwYSize = 0;
            public int dwXCountChars = 0;
            public int dwYCountChars = 0;
            public int dwFillAttribute = 0;
            public int dwFlags = 0;
            public short wShowWindow = 0;
            public short cbReserved2 = 0;
            public IntPtr lpReserved2 = IntPtr.Zero;
            public SafeFileHandle hStdInput = new SafeFileHandle(IntPtr.Zero, false);
            public SafeFileHandle hStdOutput = new SafeFileHandle(IntPtr.Zero, false);
            public SafeFileHandle hStdError = new SafeFileHandle(IntPtr.Zero, false);
            */

            /*var startupInfoType = startupInfo.GetType();
            startupInfoType.GetField("cb", BindingFlags.Instance | BindingFlags.Public).SetValue(startupInfo, Marshal.SizeOf(startupInfoType));
            startupInfoType.GetField("hStdInput", BindingFlags.Instance | BindingFlags.Public).SetValue(startupInfo, new SafeFileHandle(IntPtr.Zero, false));
            startupInfoType.GetField("hStdOutput", BindingFlags.Instance | BindingFlags.Public).SetValue(startupInfo, new SafeFileHandle(IntPtr.Zero, false));
            startupInfoType.GetField("hStdError", BindingFlags.Instance | BindingFlags.Public).SetValue(startupInfo, new SafeFileHandle(IntPtr.Zero, false));

            if (_handle.HasValue)
                startupInfoType.GetField("lpDesktop", BindingFlags.Instance | BindingFlags.Public).SetValue(startupInfo, _handle.Value);*/

            _cbField.Value.SetValue(startupInfo, Marshal.SizeOf(_startupInfoType.Value));
            _hStdInputField.Value.SetValue(startupInfo, new SafeFileHandle(IntPtr.Zero, false));
            _hStdOutputField.Value.SetValue(startupInfo, new SafeFileHandle(IntPtr.Zero, false));
            _hStdErrorField.Value.SetValue(startupInfo, new SafeFileHandle(IntPtr.Zero, false));

            if (_handle.HasValue)
                _lpDesktopField.Value.SetValue(startupInfo, _handle.Value);
        }

        private enum Protection : uint
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400,
            PAGE_TARGETS_INVALID = 0x40000000,
            PAGE_TARGETS_NO_UPDATE = 0x40000000
        }

        [DllImport("Kernel32.dll", EntryPoint = "VirtualProtect", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VirtualProtect([In] IntPtr lpAddress, [In] uint dwSize, [In][MarshalAs(UnmanagedType.U4)] Protection flNewProtect, [Out][MarshalAs(UnmanagedType.U4)] out Protection lpflOldProtect);

        private static Protection UnlockPage(IntPtr address)
        {
            Protection oldProtection;
            if (!VirtualProtect(address, (uint)(IntPtr.Size + 1), Protection.PAGE_EXECUTE_READWRITE, out oldProtection))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return oldProtection;
        }

        private static void HijackMethod(IntPtr sourceAddress, IntPtr targetAddress)
        {
            _rwLock.EnterUpgradeableReadLock();
            try
            {
                var offset = (
                    IntPtr.Size == sizeof(long) ?
                        targetAddress.ToInt64() - sourceAddress.ToInt64() :
                        targetAddress.ToInt32() - sourceAddress.ToInt32()
                    ) - 4 - 1; // Four bytes for relative address and one byte for opcode.

                var instruction = new byte[]
                {
                    0xE9 // Long jump relative instruction.
                }.Concat(BitConverter.GetBytes(offset)).ToArray();

                var oldInstruction = Enumerable.Range(0, instruction.Length).Select(i => Marshal.ReadByte(sourceAddress, i)).ToArray();

                if (!Enumerable.SequenceEqual(instruction, oldInstruction))
                {
                    _rwLock.EnterWriteLock();

                    if (_oldInstruction == null)
                        _oldInstruction = oldInstruction;

                    try
                    {
                        var oldProtection = UnlockPage(sourceAddress);
                        Marshal.Copy(instruction, 0, sourceAddress, instruction.Length);
                        if (!VirtualProtect(sourceAddress, (uint)(IntPtr.Size + 1), oldProtection, out oldProtection))
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    finally
                    {
                        _rwLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _rwLock.ExitUpgradeableReadLock();
            }
        }

        private static void RestoreMethod()
        {
            if (_oldInstruction == null)
                return;

            _rwLock.EnterUpgradeableReadLock();
            try
            {
                var sourceAddress = _oldConstructor.Value;
                var instruction = Enumerable.Range(0, _oldInstruction.Length).Select(i => Marshal.ReadByte(sourceAddress, i)).ToArray();
                if (!Enumerable.SequenceEqual(_oldInstruction, instruction))
                {
                    _rwLock.EnterWriteLock();
                    try
                    {
                        var oldProtection = UnlockPage(sourceAddress);
                        Marshal.Copy(_oldInstruction, 0, sourceAddress, _oldInstruction.Length);
                        if (!VirtualProtect(sourceAddress, (uint)(IntPtr.Size + 1), oldProtection, out oldProtection))
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    finally
                    {
                        _rwLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _rwLock.ExitUpgradeableReadLock();
            }
        }
    }
}
