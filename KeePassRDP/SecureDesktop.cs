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
using KeePassLib.Cryptography;
using KeePassLib.Utility;
using KeePassRDP.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassRDP
{
    internal class SecureDesktop : IDisposable
    {
        private static readonly Lazy<Task> _completedTask = new Lazy<Task>(() =>
        {
            var task = new Task(() => { }, CancellationToken.None, TaskCreationOptions.None);
            task.RunSynchronously(TaskScheduler.Default);
            return task;
        }, LazyThreadSafetyMode.ExecutionAndPublication);
        public static Task CompletedTask { get { return _completedTask.Value; } }

        private static readonly Lazy<SecureDesktop> _instance = new Lazy<SecureDesktop>(() => new SecureDesktop(), LazyThreadSafetyMode.ExecutionAndPublication);
        public static SecureDesktop Instance { get { return _instance.Value; } }
        public static bool IsValueCreated { get { return _instance.IsValueCreated; } }

        private static readonly ManualResetEventSlim _cleanupLock = new ManualResetEventSlim(true);

        public bool IsActive { get { return _activeLock.IsSet; } }
        public bool IsLocked { get { return _lockedLock.IsSet && !_unlockedLock.IsSet; } }
        public bool IsAlive
        {
            get
            {
                try
                {
                    return _workerThread != null && _workerThread.IsAlive;
                }
                catch
                {
                    return false;
                }
            }
        }
        public bool IsCancellationRequested
        {
            get
            {
                try
                {
                    return _cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested;
                }
                catch
                {
                    return true;
                }
            }
        }

        private static ToolStripRenderer _tsr = null;

        internal static void SetToolStripRenderer(ToolStripRenderer tsr)
        {
            _tsr = tsr;
        }

        private static volatile uint _threadId;

        private readonly ManualResetEventSlim _workerLock;
        private readonly ManualResetEventSlim _resultLock;
        private readonly ManualResetEventSlim _activeLock;
        private readonly ManualResetEventSlim _lockedLock;
        private readonly ManualResetEventSlim _unlockedLock;
        private CancellationTokenSource _cancellationTokenSource;
        private Thread _workerThread;
        private KprSecureDesktopToolBar _toolBar;

        public delegate Task Runner(SecureString lpDesktop = null);
        private volatile Runner _delegate;
        private volatile bool _withDesktop;

        private SecureDesktop()
        {
            _workerLock = new ManualResetEventSlim(false);
            _resultLock = new ManualResetEventSlim(true);
            _activeLock = new ManualResetEventSlim(false);
            _lockedLock = new ManualResetEventSlim(false);
            _unlockedLock = new ManualResetEventSlim(true);
            _cancellationTokenSource = new CancellationTokenSource();
            _delegate = null;
            _withDesktop = false;
            _workerThread = new Thread(SecureDesktopThread)
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture,
                IsBackground = true
            };
            _threadId = 0;
            _toolBar = null;
        }

        public void Cancel()
        {
            try
            {
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                    _cancellationTokenSource.Cancel();
            }
            catch { }
        }

        public void Run(Runner run = null)
        {
            Run(run, false);
        }

        internal void Run(Runner run, bool withDesktop = false)
        {
            if (_workerLock.IsSet)
            {
                if (!_workerLock.Wait(TimeSpan.FromSeconds(1), _cancellationTokenSource.Token))
                    _workerLock.Reset();
            }

            if (_resultLock.IsSet)
                _resultLock.Reset();

            _withDesktop = withDesktop;
            _delegate = run;

            var localCancellationTokenSource = _cancellationTokenSource;
            if (localCancellationTokenSource == null || localCancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (localCancellationTokenSource != null)
                        using (localCancellationTokenSource)
                            if (!localCancellationTokenSource.IsCancellationRequested)
                                localCancellationTokenSource.Cancel();
                }
                catch { }
                _cancellationTokenSource = new CancellationTokenSource();
                _workerThread = new Thread(SecureDesktopThread)
                {
                    CurrentCulture = _workerThread.CurrentCulture,
                    CurrentUICulture = _workerThread.CurrentUICulture,
                    IsBackground = true
                };
            }

            if (!_workerThread.IsAlive)
            {
                _workerThread.TrySetApartmentState(ApartmentState.STA);
                _workerThread.Start(_cancellationTokenSource);
            }

            _workerLock.Set();
        }

        public void Wait(int seconds = 0)
        {
            if (seconds > 0)
                Wait(TimeSpan.FromSeconds(seconds));
            else
                Wait(TimeSpan.FromMilliseconds(Timeout.Infinite));
        }

        private void Wait(TimeSpan timeout)
        {
            if (!_resultLock.IsSet && !_resultLock.Wait(timeout, _cancellationTokenSource.Token))
                throw new TimeoutException();
        }

        internal void ToolBarBringToFront()
        {
            var tempToolBar = _toolBar;
            if (tempToolBar == null)
                return;

            var invoke = tempToolBar.BeginInvoke(new Action(() =>
            {
                var localToolBar = _toolBar;

                if (localToolBar == null)
                    return;

                var hNewDesktop = NativeMethods.GetThreadDesktop(NativeMethods.GetCurrentThreadId());
                if (string.Equals(GetInputDesktopName(), GetDesktopName(hNewDesktop), StringComparison.Ordinal))
                    localToolBar.BringToFront();
            }));

            if (!invoke.IsCompleted)
                Task.Factory.FromAsync(
                    invoke,
                    endinvoke => tempToolBar.EndInvoke(endinvoke),
                    TaskCreationOptions.AttachedToParent,
                    TaskScheduler.Default);
            else
                tempToolBar.EndInvoke(invoke);

        }

        internal void SwitchDesktop(bool defaultOnly = false)
        {
            if (!defaultOnly && (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested))
                return;

            if (defaultOnly || Thread.CurrentThread.ManagedThreadId == _workerThread.ManagedThreadId)
                Task.Factory.FromAsync(Program.MainForm.BeginInvoke(new Action(() =>
                {
                    var hOldDesktop = NativeMethods.GetThreadDesktop(NativeMethods.GetCurrentThreadId());

                    if (hOldDesktop == IntPtr.Zero)
                        throw new ArgumentNullException("hOldDesktop");

                    if (defaultOnly || !string.Equals(GetInputDesktopName(), GetDesktopName(hOldDesktop), StringComparison.Ordinal))
                    {
                        var localToolBar = _toolBar;
                        if (localToolBar == null && !IsCancellationRequested)
                            localToolBar = new KprSecureDesktopToolBar(_tsr);

                        if (!NativeMethods.SwitchDesktop(hOldDesktop) && !IsInput(hOldDesktop))
                        {
                            if (!defaultOnly)
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                        }

                        if (_activeLock.IsSet)
                            _activeLock.Reset();

                        try
                        {
                            if (_toolBar != localToolBar)
                            {
                                if (_toolBar != null)
                                    using (_toolBar)
                                    {
                                        _toolBar.Hide();
                                        _toolBar.Close();
                                    }
                            }
                        }
                        catch { }

                        if (localToolBar != null)
                        {
                            if (!localToolBar.Visible)
                                localToolBar.Show();
                            if (Form.ActiveForm != localToolBar)
                                localToolBar.Activate();

                            _toolBar = localToolBar;
                        }
                    }
                })), endInvoke => Program.MainForm.EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            else
                Task.Factory.StartNew(() =>
                {
                    var hNewDesktop = NativeMethods.GetThreadDesktop(_threadId);

                    if (hNewDesktop == IntPtr.Zero)
                        throw new ArgumentNullException("hNewDesktop");

                    if (!string.Equals(GetInputDesktopName(), GetDesktopName(hNewDesktop), StringComparison.Ordinal))
                    {
                        if (!NativeMethods.SwitchDesktop(hNewDesktop) && !IsInput(hNewDesktop))
                            throw new Win32Exception(Marshal.GetLastWin32Error());

                        if (!_activeLock.IsSet)
                            _activeLock.Set();

                        Run(_ => CompletedTask, true);

                        var tempToolBar = _toolBar;
                        if (tempToolBar != null)
                            Task.Factory.FromAsync(Program.MainForm.BeginInvoke(new Action(() =>
                            {
                                var localToolBar = _toolBar;

                                if (localToolBar != null && tempToolBar == localToolBar)
                                {
                                    try
                                    {
                                        using (localToolBar)
                                        {
                                            localToolBar.Hide();
                                            localToolBar.Close();
                                        }
                                    }
                                    catch { }

                                    if (_toolBar == localToolBar)
                                        _toolBar = null;
                                }
                            })), endInvoke => Program.MainForm.EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    }
                }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        internal static bool IsInput(IntPtr? hDesk = null)
        {
            if (!hDesk.HasValue && _threadId != 0)
                hDesk = NativeMethods.GetThreadDesktop(_threadId);

            if (!hDesk.HasValue || hDesk.Value == IntPtr.Zero)
                return false;

            const int UOI_IO = 6;

            var isIo = false;
            var needed = Marshal.SizeOf(typeof(bool));
            var ptr = Marshal.AllocHGlobal(needed);

            try
            {
                if (!NativeMethods.GetUserObjectInformation(hDesk.Value, UOI_IO, ptr, needed, ref needed) && needed > 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                isIo = (bool)Marshal.PtrToStructure(ptr, typeof(bool));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return isIo;
        }

        internal static string GetDesktopName(IntPtr hDesk)
        {
            const int UOI_NAME = 2;

            if (hDesk == IntPtr.Zero)
                throw new ArgumentNullException("hDesk");

            var name = string.Empty;
            var needed = 0;

            if (!NativeMethods.GetUserObjectInformation(hDesk, UOI_NAME, IntPtr.Zero, 0, ref needed) && needed > 0)
            {
                var ptr = Marshal.AllocHGlobal(needed);

                try
                {
                    var oldNeeded = needed;
                    if (!NativeMethods.GetUserObjectInformation(hDesk, UOI_NAME, ptr, needed, ref needed) && needed > 0)
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    name = Marshal.PtrToStringUni(ptr);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }

            return string.IsNullOrEmpty(name) ? string.Empty : name.TrimEnd(char.MinValue);
        }

        internal static string GetInputDesktopName()
        {
            var name = string.Empty;
            var hCurDesk = NativeMethods.OpenInputDesktop(0, false, NativeMethods.DesktopAccess.DesktopReadObjects);

            try
            {
                name = GetDesktopName(hCurDesk);
            }
            catch (ArgumentNullException)
            {
                return string.Empty;
            }
            finally
            {
                NativeMethods.CloseDesktop(hCurDesk);
            }

            return name;
        }

        private void SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (!IsAlive || IsCancellationRequested)
                return;

            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    if (_unlockedLock.IsSet)
                        _unlockedLock.Reset();
                    if (!_lockedLock.IsSet)
                        _lockedLock.Set();
                    break;
                case SessionSwitchReason.SessionUnlock:
                    if (_lockedLock.IsSet)
                        _lockedLock.Reset();
                    if (!_unlockedLock.IsSet)
                        _unlockedLock.Set();
                    break;
            }
        }

        private static void CleanupTempTasks(IList<Tuple<Task, SingleThreadTaskScheduler>> tempTasks)
        {
            foreach (var tempTask in tempTasks.Where(x => x.Item1.IsCompleted).ToList())
            {
                CleanupTempTask(tempTask);
                tempTasks.Remove(tempTask);
            }
        }

        private static void CleanupTempTask(Tuple<Task, SingleThreadTaskScheduler> tempTask)
        {
            try
            {
                tempTask.Item1.Dispose();
            }
            catch { }
            try
            {
                tempTask.Item2.Dispose();
            }
            catch { }
        }

        /*[StructLayout(LayoutKind.Sequential)]
        private struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bInheritHandle;
        }*/

        private void SecureDesktopThread(object cts)
        {
            var threadId = _threadId = NativeMethods.GetCurrentThreadId();
            var hOldDesktop = NativeMethods.GetThreadDesktop(threadId);

            SynchronizationContext.SetSynchronizationContext(null);

            using (var secureName = new SecureString())
            {
                var strName = StrUtil.AlphaNumericOnly(string.Format("D{0}", Convert.ToBase64String(CryptoRandom.Instance.GetRandomBytes(16))));
                if (strName.Length > 15)
                    strName = strName.Substring(0, 15);

                foreach (var c in strName)
                    secureName.AppendChar(c);
                secureName.MakeReadOnly();

                using (var cancellationTokenSource = (CancellationTokenSource)cts)
                    try
                    {
                        var dwFlags =
                            //NativeMethods.DesktopAccess.DesktopHookControl |
                            NativeMethods.DesktopAccess.DesktopReadObjects |
                            NativeMethods.DesktopAccess.DesktopCreateWindow |
                            NativeMethods.DesktopAccess.DesktopCreateMenu |
                            NativeMethods.DesktopAccess.DesktopWriteObjects |
                            NativeMethods.DesktopAccess.DesktopSwitchDesktop;
                        /*var sAtt = new SECURITY_ATTRIBUTES
                        {
                            nLength = Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES)),
                            bInheritHandle = true
                        };
                        var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(sAtt));
                        Marshal.StructureToPtr(sAtt, ptr, false);*/
                        var hNewDesktop = NativeMethods.CreateDesktop(strName, IntPtr.Zero, IntPtr.Zero, 0, dwFlags, IntPtr.Zero); // ptr);
                        //Marshal.FreeHGlobal(ptr);

                        if (!NativeMethods.SetThreadDesktop(hNewDesktop))
                        {
                            NativeMethods.CloseDesktop(hNewDesktop);
                            throw new InvalidOperationException("SetThreadDesktop");
                        }

                        var oldDesktopName = GetDesktopName(hOldDesktop);

                        try
                        {
                            using (var syncContext = new WindowsFormsSynchronizationContext())
                            using (var runnerLock = new ManualResetEventSlim(false))
                            using (var bc = new BlockingCollection<Task>())
                            {
                                SynchronizationContext.SetSynchronizationContext(syncContext);

                                var runnerThread = new Thread(() =>
                                {
                                    if (!NativeMethods.SetThreadDesktop(hNewDesktop))
                                        throw new InvalidOperationException("SetThreadDesktop");

                                    SynchronizationContext.SetSynchronizationContext(syncContext);

                                    var _lock = new object();
                                    var tempTasks = new List<Tuple<Task, SingleThreadTaskScheduler>>();
                                    var cleanupTask = new Action(() =>
                                    {
                                        if (runnerLock.IsSet)
                                            runnerLock.Reset();

                                        try
                                        {
                                            Task.WaitAll(tempTasks.Where(x => !x.Item1.IsCompleted).Select(x => x.Item1).ToArray(), cancellationTokenSource.Token);
                                        }
                                        finally
                                        {
                                            if (tempTasks.All(x => x.Item1.IsCompleted))
                                            {
                                                lock (_lock)
                                                {
                                                    CleanupTempTasks(tempTasks);
                                                    if (tempTasks.All(x => x.Item1.IsCompleted))
                                                        tempTasks.Clear();
                                                }

                                                if (bc.Count == 0 && tempTasks.Count == 0 && !runnerLock.IsSet)
                                                    runnerLock.Set();
                                            }
                                            else
                                            {
                                                if (cancellationTokenSource.IsCancellationRequested)
                                                {
                                                    if (!runnerLock.IsSet)
                                                        runnerLock.Set();
                                                    lock (_lock)
                                                    {
                                                        CleanupTempTasks(tempTasks);
                                                        if (tempTasks.All(x => x.Item1.IsCompleted))
                                                            tempTasks.Clear();
                                                    }
                                                }
                                                else
                                                    lock (_lock)
                                                        foreach (var tempTask in tempTasks.Where(x => x.Item1.IsCompleted))
                                                        {
                                                            CleanupTempTask(tempTask);
                                                            tempTasks.Remove(tempTask);
                                                        }
                                            }

                                            if (bc.Count == 0 && tempTasks.Count == 0 && !runnerLock.IsSet)
                                                runnerLock.Set();
                                        }
                                    });
                                    try
                                    {
                                        Task task;
                                        while (!cancellationTokenSource.IsCancellationRequested)
                                        {
                                            try
                                            {
                                                if (bc.Count == 0 && tempTasks.Count == 0 && !runnerLock.IsSet)
                                                    runnerLock.Set();

                                                while (!cancellationTokenSource.IsCancellationRequested && bc.TryTake(out task, -1, cancellationTokenSource.Token))
                                                    try
                                                    {
                                                        if (runnerLock.IsSet)
                                                            runnerLock.Reset();

                                                        if (task.Status == TaskStatus.Created)
                                                        {
                                                            var stss = new SingleThreadTaskScheduler(cancellationTokenSource.Token, strName);
                                                            task.Start(stss);
                                                            lock (_lock)
                                                                if (task is Task<Task>)
                                                                    tempTasks.Add(new Tuple<Task, SingleThreadTaskScheduler>((task as Task<Task>).Unwrap(), stss));
                                                                else
                                                                    tempTasks.Add(new Tuple<Task, SingleThreadTaskScheduler>(task, stss));
                                                        }
                                                        else
                                                            task.Wait(cancellationTokenSource.Token);
                                                    }
                                                    catch { }
                                                    finally
                                                    {
                                                        lock (_lock)
                                                            foreach (var tempTask in tempTasks.Where(x => x.Item1.IsCompleted))
                                                            {
                                                                CleanupTempTask(tempTask);
                                                                tempTasks.Remove(tempTask);
                                                            }

                                                        if (cancellationTokenSource.IsCancellationRequested || tempTasks.All(x => x.Item1.IsCompleted))
                                                        {
                                                            lock (_lock)
                                                            {
                                                                CleanupTempTasks(tempTasks);
                                                                if (tempTasks.All(x => x.Item1.IsCompleted))
                                                                    tempTasks.Clear();
                                                            }

                                                            if (cancellationTokenSource.IsCancellationRequested || (bc.Count == 0 && tempTasks.Count == 0 && !runnerLock.IsSet))
                                                                runnerLock.Set();
                                                        }
                                                        else if (tempTasks.Count > 0 && bc.Count == 0)
                                                            Task.Factory.StartNew(cleanupTask, cancellationTokenSource.Token, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                                                    }
                                            }
                                            catch { }
                                            finally
                                            {
                                                try
                                                {
                                                    if (cancellationTokenSource.IsCancellationRequested || tempTasks.All(x => x.Item1.IsCompleted))
                                                    {
                                                        lock (_lock)
                                                        {
                                                            CleanupTempTasks(tempTasks);
                                                            if (tempTasks.All(x => x.Item1.IsCompleted))
                                                                tempTasks.Clear();
                                                        }

                                                        if (cancellationTokenSource.IsCancellationRequested || (bc.Count == 0 && tempTasks.Count == 0 && !runnerLock.IsSet))
                                                            runnerLock.Set();
                                                    }
                                                    else if (tempTasks.Count > 0 && bc.Count == 0)
                                                        Task.Factory.StartNew(cleanupTask, cancellationTokenSource.Token, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        if (cancellationTokenSource.IsCancellationRequested || (bc.Count == 0 && tempTasks.Count == 0 && !runnerLock.IsSet))
                                            runnerLock.Set();

                                        NativeMethods.SetThreadDesktop(hOldDesktop);
                                    }
                                })
                                {
                                    CurrentCulture = Thread.CurrentThread.CurrentCulture,
                                    CurrentUICulture = Thread.CurrentThread.CurrentUICulture,
                                    IsBackground = true
                                };

                                using (var shellWindow = new KprSecureShellWindow(_tsr))
                                {
                                    var watcherThread = new Thread(() =>
                                    {
                                        if (!NativeMethods.SetThreadDesktop(hNewDesktop))
                                            throw new InvalidOperationException("SetThreadDesktop");

                                        SynchronizationContext.SetSynchronizationContext(syncContext);

                                        if (_lockedLock.IsSet)
                                            _lockedLock.Reset();
                                        if (!_unlockedLock.IsSet)
                                            _unlockedLock.Set();

                                        SystemEvents.SessionSwitch += SessionSwitch;

                                        try
                                        {
                                            while (!cancellationTokenSource.IsCancellationRequested)
                                            {
                                                try
                                                {
                                                    if (_unlockedLock.IsSet)
                                                        _lockedLock.Wait(cancellationTokenSource.Token);
                                                }
                                                catch (OperationCanceledException)
                                                {
                                                    break;
                                                }

                                                try
                                                {
                                                    if (!_unlockedLock.IsSet && _activeLock.IsSet && !string.Equals(GetInputDesktopName(), strName, StringComparison.Ordinal))
                                                        Run(_ =>
                                                        {
                                                            shellWindow.Lock();
                                                            return CompletedTask;
                                                        });
                                                }
                                                catch { }

                                                try
                                                {
                                                    if (_lockedLock.IsSet)
                                                        _unlockedLock.Wait(cancellationTokenSource.Token);
                                                }
                                                catch (OperationCanceledException)
                                                {
                                                    break;
                                                }

                                                try
                                                {
                                                    if (!_lockedLock.IsSet && _activeLock.IsSet && !_workerLock.IsSet && string.Equals(GetInputDesktopName(), strName, StringComparison.Ordinal))
                                                        Run(_ =>
                                                        {
                                                            shellWindow.Unlock();
                                                            return CompletedTask;
                                                        }, true);
                                                }
                                                catch { }
                                            }
                                        }
                                        finally
                                        {
                                            SystemEvents.SessionSwitch -= SessionSwitch;

                                            if (_lockedLock.IsSet)
                                                _lockedLock.Reset();
                                            if (!_unlockedLock.IsSet)
                                                _unlockedLock.Set();

                                            NativeMethods.SetThreadDesktop(hOldDesktop);
                                        }
                                    })
                                    {
                                        CurrentCulture = Thread.CurrentThread.CurrentCulture,
                                        CurrentUICulture = Thread.CurrentThread.CurrentUICulture,
                                        IsBackground = true
                                    };

                                    runnerThread.TrySetApartmentState(ApartmentState.MTA);
                                    runnerThread.Start();
                                    watcherThread.TrySetApartmentState(ApartmentState.MTA);
                                    watcherThread.Start();

                                    while (!cancellationTokenSource.IsCancellationRequested)
                                    {
                                        try
                                        {
                                            if (!_workerLock.Wait(TimeSpan.FromSeconds(1), cancellationTokenSource.Token))
                                            {
                                                if (string.Equals(GetInputDesktopName(), strName, StringComparison.Ordinal))
                                                    Application.DoEvents();
                                                if (!_workerLock.IsSet || cancellationTokenSource.IsCancellationRequested)
                                                {
                                                    //Application.RaiseIdle(EventArgs.Empty);
                                                    continue;
                                                }
                                            }

                                            var oldWithDesktop = _withDesktop;
                                            _withDesktop = false;
                                            var oldDelegate = _delegate;
                                            _delegate = null;

                                            if (_resultLock.IsSet)
                                                _resultLock.Reset();
                                            if (_workerLock.IsSet)
                                                _workerLock.Reset();

                                            Application.DoEvents();

                                            if (oldDelegate != null)
                                            {
                                                if (string.Equals(GetInputDesktopName(), oldDesktopName, StringComparison.Ordinal))
                                                    if (!NativeMethods.SwitchDesktop(hNewDesktop) && !IsInput(hNewDesktop))
                                                        throw new InvalidOperationException("SecureDesktopThread.SwitchDesktop");

                                                if (!_activeLock.IsSet)
                                                    _activeLock.Set();

                                                if (!oldWithDesktop)
                                                {
                                                    shellWindow.Desktop.BringToFront();
                                                    shellWindow.ToolBar.SendToBack();
                                                    shellWindow.SendToBack();
                                                }

                                                Application.DoEvents();

                                                var task = oldDelegate(oldWithDesktop ? secureName/*.Copy()*/ : null);

                                                if (!runnerLock.IsSet && !_lockedLock.IsSet)
                                                {
                                                    if (!shellWindow.ToolBar.Visible)
                                                        shellWindow.ToolBar.Show(shellWindow.Empty);

                                                    shellWindow.BringToFront();
                                                    shellWindow.ToolBar.BringToFront();
                                                    shellWindow.Desktop.SendToBack();

                                                    if (Form.ActiveForm != shellWindow.ToolBar)
                                                        shellWindow.ToolBar.Activate();

                                                    Application.DoEvents();
                                                }

                                                if (!task.IsCompleted)
                                                {
                                                    if (runnerLock.IsSet)
                                                        runnerLock.Reset();
                                                    bc.Add(task);

                                                    if (!cancellationTokenSource.IsCancellationRequested && !runnerLock.IsSet)
                                                    {
                                                        if (!shellWindow.ToolBar.Visible)
                                                            shellWindow.ToolBar.Show(shellWindow.Empty);

                                                        shellWindow.BringToFront();
                                                        shellWindow.ToolBar.BringToFront();
                                                        shellWindow.Desktop.SendToBack();

                                                        if (Form.ActiveForm != shellWindow.ToolBar)
                                                            shellWindow.ToolBar.Activate();

                                                        Application.DoEvents();
                                                    }

                                                    while (!cancellationTokenSource.IsCancellationRequested && !runnerLock.IsSet && (_workerLock.IsSet || !runnerLock.Wait(125, cancellationTokenSource.Token)))
                                                    {
                                                        if (_workerLock.IsSet)
                                                        {
                                                            oldWithDesktop = _withDesktop;
                                                            _withDesktop = false;
                                                            oldDelegate = _delegate;
                                                            _delegate = null;

                                                            if (_resultLock.IsSet)
                                                                _resultLock.Reset();
                                                            if (_workerLock.IsSet)
                                                                _workerLock.Reset();

                                                            Application.DoEvents();

                                                            if (oldDelegate != null)
                                                            {
                                                                if (string.Equals(GetInputDesktopName(), oldDesktopName, StringComparison.Ordinal))
                                                                    if (!NativeMethods.SwitchDesktop(hNewDesktop) && !IsInput(hNewDesktop))
                                                                        throw new InvalidOperationException("SecureDesktopThread.SwitchDesktop");

                                                                if (!_activeLock.IsSet)
                                                                    _activeLock.Set();

                                                                if (!oldWithDesktop)
                                                                {
                                                                    shellWindow.Desktop.BringToFront();
                                                                    shellWindow.ToolBar.SendToBack();
                                                                    shellWindow.SendToBack();
                                                                }

                                                                Application.DoEvents();

                                                                task = oldDelegate(oldWithDesktop ? secureName/*.Copy()*/ : null);

                                                                if (!runnerLock.IsSet && !_lockedLock.IsSet)
                                                                {
                                                                    if (!shellWindow.ToolBar.Visible)
                                                                        shellWindow.ToolBar.Show(shellWindow.Empty);

                                                                    shellWindow.BringToFront();
                                                                    shellWindow.ToolBar.BringToFront();
                                                                    shellWindow.Desktop.SendToBack();

                                                                    if (Form.ActiveForm != shellWindow.ToolBar)
                                                                        shellWindow.ToolBar.Activate();

                                                                    Application.DoEvents();
                                                                }

                                                                if (!task.IsCompleted)
                                                                {
                                                                    if (runnerLock.IsSet)
                                                                        runnerLock.Reset();
                                                                    bc.Add(task);

                                                                    if (!cancellationTokenSource.IsCancellationRequested && !runnerLock.IsSet)
                                                                    {
                                                                        if (!shellWindow.ToolBar.Visible)
                                                                            shellWindow.ToolBar.Show(shellWindow.Empty);

                                                                        shellWindow.BringToFront();
                                                                        shellWindow.ToolBar.BringToFront();
                                                                        shellWindow.Desktop.SendToBack();

                                                                        if (Form.ActiveForm != shellWindow.ToolBar)
                                                                            shellWindow.ToolBar.Activate();

                                                                        Application.DoEvents();
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (!_workerLock.IsSet && runnerLock.IsSet)
                                                                    {
                                                                        if (string.Equals(GetInputDesktopName(), strName, StringComparison.Ordinal))
                                                                            if (!NativeMethods.SwitchDesktop(hOldDesktop) && !IsInput(hOldDesktop))
                                                                                throw new InvalidOperationException("SecureDesktopThread.SwitchDesktop");

                                                                        if (_activeLock.IsSet)
                                                                            _activeLock.Reset();
                                                                    }

                                                                    if (!_resultLock.IsSet)
                                                                        _resultLock.Set();
                                                                }
                                                            }
                                                            else if (!runnerLock.IsSet)
                                                            {
                                                                if (shellWindow.ToolBar.Visible)
                                                                {
                                                                    shellWindow.BringToFront();
                                                                    shellWindow.ToolBar.BringToFront();

                                                                    if (Form.ActiveForm != shellWindow.ToolBar)
                                                                        shellWindow.ToolBar.Activate();
                                                                }
                                                            }
                                                        }

                                                        if (!cancellationTokenSource.IsCancellationRequested && !runnerLock.IsSet)
                                                            Application.DoEvents();
                                                    }
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            if (!_workerLock.IsSet && runnerLock.IsSet)
                                            {
                                                if (string.Equals(GetInputDesktopName(), strName, StringComparison.Ordinal))
                                                    if (!NativeMethods.SwitchDesktop(hOldDesktop) && !IsInput(hOldDesktop))
                                                        throw new InvalidOperationException("SecureDesktopThread.SwitchDesktop");

                                                if (_activeLock.IsSet)
                                                    _activeLock.Reset();
                                            }

                                            if (!_resultLock.IsSet)
                                                _resultLock.Set();

                                            if (!_workerLock.IsSet)
                                            {
                                                if (_toolBar != null)
                                                {
                                                    Task.Factory.FromAsync(Program.MainForm.BeginInvoke(new Action(() =>
                                                    {
                                                        var localToolBar = _toolBar;

                                                        if (localToolBar != null)
                                                        {
                                                            try
                                                            {
                                                                using (localToolBar)
                                                                {
                                                                    localToolBar.Hide();
                                                                    localToolBar.Close();
                                                                }
                                                            }
                                                            catch { }

                                                            if (_toolBar == localToolBar)
                                                                _toolBar = null;
                                                        }
                                                    })), endInvoke => Program.MainForm.EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                                                }

                                                if (runnerLock.IsSet && !cancellationTokenSource.IsCancellationRequested)
                                                {
                                                    shellWindow.ToolBar.Location = Point.Empty;
                                                    shellWindow.ToolBar.Hide();
                                                }
                                            }

                                            if (!cancellationTokenSource.IsCancellationRequested)
                                                Application.DoEvents();
                                        }
                                    }

                                    if (shellWindow.ToolBar.Visible)
                                        shellWindow.ToolBar.Hide();
                                    shellWindow.ToolBar.Close();

                                    if (string.Equals(GetInputDesktopName(), strName, StringComparison.Ordinal))
                                        NativeMethods.SwitchDesktop(hOldDesktop);

                                    if (_activeLock.IsSet)
                                        _activeLock.Reset();
                                    if (_lockedLock.IsSet)
                                        _lockedLock.Reset();
                                    if (!_unlockedLock.IsSet)
                                        _unlockedLock.Set();
                                    if (!_resultLock.IsSet)
                                        _resultLock.Set();
                                    if (_workerLock.IsSet)
                                        _workerLock.Reset();

                                    _withDesktop = false;
                                    _delegate = null;

                                    if (!bc.IsAddingCompleted)
                                        bc.CompleteAdding();

                                    try
                                    {
                                        if (!cancellationTokenSource.IsCancellationRequested)
                                            cancellationTokenSource.Cancel();
                                    }
                                    catch { }

                                    Parallel.Invoke(new ParallelOptions
                                    {
                                        MaxDegreeOfParallelism = 2,
                                        CancellationToken = CancellationToken.None,
                                        TaskScheduler = TaskScheduler.Default
                                    }, () =>
                                    {
                                        try
                                        {
                                            if (runnerThread.IsAlive)
                                                if (!runnerThread.Join(TimeSpan.FromSeconds(5)))
                                                    runnerThread.Abort();
                                        }
                                        catch { }
                                    }, () =>
                                    {
                                        try
                                        {
                                            if (watcherThread.IsAlive)
                                                if (!watcherThread.Join(TimeSpan.FromSeconds(5)))
                                                    watcherThread.Abort();
                                        }
                                        catch { }
                                    });

                                    Application.DoEvents();
                                }

                                Application.DoEvents();
                            }

                            if (cancellationTokenSource == _cancellationTokenSource)
                                _cancellationTokenSource = null;
                        }
                        finally
                        {
                            Cleanup();

                            Application.DoEvents();

                            //Application.UnregisterMessageLoop();
                            NativeMethods.CloseDesktop(hNewDesktop);
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        if (string.Equals(GetInputDesktopName(), strName, StringComparison.Ordinal))
                            NativeMethods.SwitchDesktop(hOldDesktop);

                        try
                        {
                            if (!cancellationTokenSource.IsCancellationRequested)
                                cancellationTokenSource.Cancel();
                        }
                        catch { }

                        if (_activeLock.IsSet)
                            _activeLock.Reset();
                        if (_lockedLock.IsSet)
                            _lockedLock.Reset();
                        if (!_unlockedLock.IsSet)
                            _unlockedLock.Set();
                        if (!_resultLock.IsSet)
                            _resultLock.Set();

                        Task.Factory.FromAsync(Program.MainForm.BeginInvoke(new Action(() =>
                        {
                            var localToolBar = _toolBar;
                            if (localToolBar != null)
                            {
                                try
                                {
                                    using (localToolBar)
                                    {
                                        localToolBar.Hide();
                                        localToolBar.Close();
                                    }
                                }
                                catch
                                { }

                                if (_toolBar == localToolBar)
                                    _toolBar = null;
                            }

                            Util.ShowErrorDialog(
                                Util.FormatException(ex),
                                null,
                                string.Format("{0} - {1}", Util.KeePassRDP, KPRes.FatalError),
                                VtdIcon.Error,
                                Program.MainForm,
                                null, 0, null, 0);
                        })), endInvoke => Program.MainForm.EndInvoke(endInvoke), TaskCreationOptions.None, TaskScheduler.Default);
                    }
                    finally
                    {
                        var localCancellationTokenSource = _cancellationTokenSource;
                        if (cts == localCancellationTokenSource)
                        {
                            try
                            {
                                using (localCancellationTokenSource)
                                    if (!localCancellationTokenSource.IsCancellationRequested)
                                        localCancellationTokenSource.Cancel();
                            }
                            catch { }

                            if (localCancellationTokenSource == _cancellationTokenSource)
                                _cancellationTokenSource = null;
                        }

                        NativeMethods.SetThreadDesktop(hOldDesktop);
                    }
            }

            if (_threadId == threadId)
                _threadId = 0;
        }

        [Flags]
        private enum SnapshotFlags : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            NoHeaps = 0x40000000,
            Inherit = 0x80000000,
            All = HeapList | Module | Process | Thread
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct PROCESSENTRY32
        {
            private const int MAX_PATH = 260;
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public UIntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szExeFile;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct THREADENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ThreadID;
            public uint th32OwnerProcessID;
            public uint tpBasePri;
            public uint tpDeltaPri;
            public uint dwFlags;
        }

        [DllImport("Kernel32.dll", EntryPoint = "CreateToolhelp32Snapshot", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot([In][MarshalAs(UnmanagedType.U4)] SnapshotFlags dwFlags, [In] uint th32ProcessID);

        [DllImport("Kernel32.dll", EntryPoint = "Process32First", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool Process32First([In] IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("Kernel32.dll", EntryPoint = "Process32Next", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool Process32Next([In] IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        /*[DllImport("Kernel32.dll", EntryPoint = "Thread32First", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool Thread32First([In] IntPtr hSnapshot, ref THREADENTRY32 lpte);

        [DllImport("Kernel32.dll", EntryPoint = "Thread32Next", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool Thread32Next([In] IntPtr hSnapshot, ref THREADENTRY32 lpte);*/

        [DllImport("Kernel32.dll", EntryPoint = "CloseHandle", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle([In] IntPtr hObject);

        private static void Cleanup()
        {
            _cleanupLock.Reset();

            try
            {
                var hDesk = NativeMethods.GetThreadDesktop(NativeMethods.GetCurrentThreadId());
                var lpDesktop = GetDesktopName(hDesk);

                var pid = 0u;
                using (var p = Process.GetCurrentProcess())
                    pid = Convert.ToUInt32(p.Id);

                var failedPids = new HashSet<uint>();
                var processIds = new HashSet<uint>();
                var threadIds = new HashSet<uint>();
                var windowHandles = new HashSet<IntPtr>();
                var killProcessIds = new HashSet<uint>();

                var snapshot = CreateToolhelp32Snapshot(SnapshotFlags.Process /*| SnapshotFlags.Thread*/, 0);

                var proc = new PROCESSENTRY32
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32))
                };

                if (Process32First(snapshot, ref proc))
                    do
                    {
                        if (proc.th32ProcessID != pid && proc.th32ParentProcessID == pid)
                            processIds.Add(proc.th32ProcessID);
                    } while (Process32Next(snapshot, ref proc));

                /*var proct = new THREADENTRY32
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(THREADENTRY32))
                };

                if (Thread32First(snapshot, ref proct))
                    do
                    {
                        if (!processIds.Contains(proct.th32OwnerProcessID) || killProcessIds.Contains(proct.th32OwnerProcessID))
                            continue;

                        try
                        {
                            var oldDesktopName = GetDesktopName(NativeMethods.GetThreadDesktop(proct.th32ThreadID));
                            if (string.Equals(oldDesktopName, lpDesktop, StringComparison.Ordinal))
                                killProcessIds.Add(proct.th32OwnerProcessID);
                        }
                        catch (ArgumentNullException)
                        {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }
                    } while (Thread32Next(snapshot, ref proct));*/

                CloseHandle(snapshot);

                NativeMethods.EnumDesktopWindows(hDesk, (hWnd, lParam) =>
                {
                    uint procId;
                    var threadId = NativeMethods.GetWindowThreadProcessId(hWnd, out procId);

                    if (procId == pid || killProcessIds.Contains(procId) || failedPids.Contains(procId))
                        return true;

                    threadIds.Add(threadId);

                    try
                    {
                        using (var p = Process.GetProcessById(Convert.ToInt32(procId)))
                        {
                            if (p.HasExited)
                                return true;

                            using (var main = p.MainModule)
                                if (main == null)
                                    throw new AccessViolationException();

                            if (p.MainWindowHandle == hWnd)
                            {
                                windowHandles.Add(hWnd);
                                processIds.Add(procId);
                            }
                            else if (processIds.Contains(procId))
                                killProcessIds.Add(procId);
                            else
                                processIds.Add(procId);
                        }
                    }
                    catch
                    {
                        failedPids.Add(procId);
                    }

                    return true;
                }, IntPtr.Zero);

                processIds.UnionWith(failedPids);
                var failedPidsBag = new ConcurrentBag<uint>();

                Parallel.ForEach(processIds, new ParallelOptions
                {
                    MaxDegreeOfParallelism = 10,
                    TaskScheduler = TaskScheduler.Default
                },
                id =>
                {
                    try
                    {
                        using (var p = Process.GetProcessById(Convert.ToInt32(id)))
                        {
                            if (p.HasExited)
                                return;

                            try
                            {
                                if (failedPids.Contains(id))
                                {
                                    const int SC_CLOSE = 0xF060;
                                    NativeMethods.PostMessage(p.MainWindowHandle, NativeMethods.WM_SYSCOMMAND, SC_CLOSE, 0);

                                    p.CloseMainWindow();

                                    if (!p.HasExited)
                                        p.Kill();
                                }
                                else
                                {
                                    if (killProcessIds.Contains(id) || windowHandles.Contains(p.MainWindowHandle))
                                    {
                                        p.CloseMainWindow();

                                        using (var main = p.MainModule)
                                            if (main != null && string.Equals(Path.GetFileName(main.FileName), "mstsc.exe", StringComparison.OrdinalIgnoreCase))
                                                return;

                                        if (!p.HasExited)
                                            p.Kill();
                                    }
                                    else if (p.Threads.OfType<ProcessThread>().Any(x =>
                                    {
                                        try
                                        {
                                            using (x)
                                                return threadIds.Contains(Convert.ToUInt32(x.Id)) ||
                                                    string.Equals(GetDesktopName(NativeMethods.GetThreadDesktop(Convert.ToUInt32(x.Id))), lpDesktop, StringComparison.Ordinal);
                                        }
                                        catch (ArgumentNullException)
                                        {
                                            return false;
                                        }
                                    }))
                                    {
                                        p.CloseMainWindow();

                                        if (!p.HasExited)
                                            p.Kill();
                                    }
                                }
                            }
                            finally
                            {
                                try
                                {
                                    if (!p.HasExited && !p.WaitForExit(1000) && !p.HasExited)
                                        failedPidsBag.Add(id);
                                }
                                catch { }
                            }
                        }
                    }
                    catch
                    {
                        failedPidsBag.Add(id);
                    }
                });

                var failedPidsLeft = failedPidsBag.Where(x =>
                {
                    try
                    {
                        using (var p = Process.GetProcessById(Convert.ToInt32(x)))
                            try
                            {
                                using (var main = p.MainModule)
                                    if (main == null)
                                        throw new AccessViolationException();

                                return !p.HasExited;
                            }
                            catch
                            {
                                return true;
                            }
                    }
                    catch { }

                    return false;
                }).ToList();

                if (failedPidsLeft.Count > 0)
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var vtd = new VistaTaskDialog
                            {
                                Content = string.Format(
                                    KprResourceManager.Instance[@"The following PIDs could not be stopped when closing the secure desktop:
'{0}'
Please confirm to try killing them with administrator privileges."], string.Join(", ", failedPidsLeft)),
                                WindowTitle = string.Format("{0} - {1}", Util.KeePassRDP, KPRes.Warning)
                            };
                            vtd.SetIcon(VtdIcon.Warning);
                            vtd.AddButton(1, KprResourceManager.Instance[KPRes.Ok], null);
                            vtd.AddButton(0, KprResourceManager.Instance[KPRes.Cancel], null);
                            var showDialog = vtd.GetType().GetMethod("InternalShowDialog", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                            showDialog.Invoke(vtd, new object[] { null });
                            if (vtd.Result == 1)
                            {
                                try
                                {
                                    using (var p = Process.Start(new ProcessStartInfo
                                    {
                                        FileName = Path.Combine(Environment.SystemDirectory, "taskkill.exe"),
                                        Arguments = string.Format("/PID {0}", string.Join(" /PID", failedPidsLeft)),
                                        ErrorDialog = false,
                                        CreateNoWindow = true,
                                        LoadUserProfile = false,
                                        WorkingDirectory = Path.GetTempPath(),
                                        UseShellExecute = true,
                                        Verb = "runas",
                                        WindowStyle = ProcessWindowStyle.Hidden,
                                    }))
                                        if (!p.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds))
                                            throw new TimeoutException();
                                }
                                catch { }
                            }
                        }
                        finally
                        {
                            if (!_cleanupLock.IsSet)
                                _cleanupLock.Set();
                        }
                    }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
                else if (!_cleanupLock.IsSet)
                    _cleanupLock.Set();
            }
            catch
            {
                if (!_cleanupLock.IsSet)
                    _cleanupLock.Set();
            }
        }

        public void Dispose()
        {
            Cancel();

            try
            {
                if (_workerThread != null && _workerThread.IsAlive)
                    if (!_workerThread.Join(TimeSpan.FromSeconds(5)))
                        _workerThread.Abort();
            }
            catch { }

            try
            {
                if (!_cleanupLock.IsSet)
                    _cleanupLock.Wait(TimeSpan.FromSeconds(30));
            }
            catch { }

            try
            {
                if (_cancellationTokenSource != null)
                    _cancellationTokenSource.Dispose();
            }
            catch { }
            _cancellationTokenSource = null;

            _cleanupLock.Dispose();
            _workerLock.Dispose();
            _resultLock.Dispose();
            _activeLock.Dispose();
            _lockedLock.Dispose();
            _unlockedLock.Dispose();

            if (_toolBar != null)
                try
                {
                    using (_toolBar)
                    {
                        if (_toolBar.Visible)
                            _toolBar.Hide();
                        _toolBar.Close();
                    }
                }
                catch { }
            _toolBar = null;
        }

        private sealed class SingleThreadTaskScheduler : TaskScheduler, IDisposable
        {
            private static readonly SynchronizationContext _defaultSynchronizationContext = new SynchronizationContext();

            [ThreadStatic]
            private static volatile bool _isExecuting;

            private readonly CancellationTokenSource _cancellationTokenSource;

            private readonly BlockingCollection<Task> _taskQueue;

            private readonly Lazy<Thread> _singleThread;

            private readonly string _lpDesktop;

            private readonly SynchronizationContext _synchronizationContext;

            public override int MaximumConcurrencyLevel { get { return 1; } }

            public SingleThreadTaskScheduler(CancellationToken cancellationToken, string lpDesktop, SynchronizationContext synchronizationContext = null) : base()
            {
                _synchronizationContext = synchronizationContext ?? _defaultSynchronizationContext;
                _lpDesktop = lpDesktop;
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _taskQueue = new BlockingCollection<Task>();
                _singleThread = new Lazy<Thread>(() => new Thread(RunOnSingleThread)
                {
                    CurrentCulture = Thread.CurrentThread.CurrentCulture,
                    CurrentUICulture = Thread.CurrentThread.CurrentUICulture,
                    IsBackground = true
                }, LazyThreadSafetyMode.ExecutionAndPublication);
            }

            private void RunOnSingleThread(object state)
            {
                var lpDesktop = (string)state;
                var threadDesktop = string.IsNullOrEmpty(lpDesktop);
                var hNewDesktop = threadDesktop ?
                    NativeMethods.GetThreadDesktop(NativeMethods.GetCurrentThreadId()) :
                    NativeMethods.OpenDesktop(lpDesktop, 0, false, NativeMethods.DesktopAccess.DesktopReadObjects);

                try
                {
                    if (!NativeMethods.SetThreadDesktop(hNewDesktop))
                        throw new InvalidOperationException("SetThreadDesktop");
                }
                finally
                {
                    if (!threadDesktop)
                        NativeMethods.CloseDesktop(hNewDesktop);
                }

                SynchronizationContext.SetSynchronizationContext(_synchronizationContext);

                _isExecuting = true;

                try
                {
                    Task task;
                    while (!_cancellationTokenSource.IsCancellationRequested && _taskQueue.TryTake(out task, -1, _cancellationTokenSource.Token))
                        TryExecuteTask(task);
                }
                catch (ArgumentNullException) { }
                catch (OperationCanceledException) { }
                finally
                {
                    _isExecuting = false;
                }
            }

            protected override IEnumerable<Task> GetScheduledTasks() { return _taskQueue.ToList(); }

            protected override void QueueTask(Task task)
            {
                if (!_singleThread.IsValueCreated || !_singleThread.Value.IsAlive)
                {
                    _singleThread.Value.TrySetApartmentState(ApartmentState.MTA);
                    _singleThread.Value.Start(_lpDesktop);
                }

                try
                {
                    _taskQueue.Add(task, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException) { }
                catch (InvalidOperationException) { }
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                if (taskWasPreviouslyQueued)
                    return false;

                return _isExecuting && TryExecuteTask(task);
            }

            public void Dispose()
            {
                _isExecuting = false;

                if (!_cancellationTokenSource.IsCancellationRequested)
                    _cancellationTokenSource.Cancel();

                if (!_taskQueue.IsAddingCompleted)
                    _taskQueue.CompleteAdding();

                if (_singleThread.IsValueCreated && _singleThread.Value.IsAlive)
                {
                    if (!_singleThread.Value.Join(TimeSpan.FromSeconds(5)))
                        _singleThread.Value.Abort();
                }

                _cancellationTokenSource.Dispose();
                _taskQueue.Dispose();
            }
        }
    }
}
