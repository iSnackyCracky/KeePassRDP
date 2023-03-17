/*
 *  Copyright (C) 2018 - 2023 iSnackyCracky, NETertainer
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

using KeePassRDP.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;

namespace KeePassRDP
{
    /// <summary>
    /// Methods and classes for reading and writing <see cref="NativeCredential"/> and <see cref="Credential"/> from and to Windows vault.
    /// </summary>
    internal static class NativeCredentials
    {
        /// <summary>
        /// Translate from unmanaged <see cref="NativeCredential"/> to managed <see cref="Credential"/>.
        /// </summary>
        /// <param name="pCred"></param>
        /// <returns><see cref="Credential"/></returns>
        private static Credential TranslateFromNativeCred(NativeCredential ncred)
        {
            var cred = new Credential(ncred);
            return cred;
        }

        /// <summary>
        /// Translate from managed <see cref="Credential"/> to unmanaged <see cref="NativeCredential"/>.
        /// </summary>
        /// <param name="cred"></param>
        /// <returns><see cref="NativeCredential"/></returns>
        private static NativeCredential TranslateToNativeCred(Credential cred)
        {
            var lastWritten = cred.LastWritten != DateTime.MinValue ? cred.LastWritten.ToFileTime() : 0;

            var ncred = new NativeCredential
            {
                Type = (uint)cred.Type,
                Flags = (uint)cred.Flags,
                Persist = (uint)cred.Persist,
                LastWritten = lastWritten > 0 ? new System.Runtime.InteropServices.ComTypes.FILETIME
                {
                    dwHighDateTime = (int)((lastWritten >> 32) & 0xFFFFFFFFL),
                    dwLowDateTime = (int)(lastWritten & 0xFFFFFFFFL)
                } : new System.Runtime.InteropServices.ComTypes.FILETIME(),
                UserName = cred.UserName, // Marshal.StringToCoTaskMemUni(ncred.UserName);
                TargetName = cred.TargetName, // Marshal.StringToCoTaskMemUni(ncred.TargetName);
                TargetAlias = cred.TargetAlias, // Marshal.StringToCoTaskMemUni(ncred.TargetAlias);
                Comment = cred.Comment, // Marshal.StringToCoTaskMemUni(ncred.Comment);
                CredentialBlobSize = (uint)Encoding.Unicode.GetByteCount(cred.CredentialBlob),
                AttributeCount = (uint)cred.Attributes.Count
            };

            if (ncred.CredentialBlobSize > 0)
                ncred.CredentialBlob = Marshal.StringToCoTaskMemUni(cred.CredentialBlob);

            if (ncred.AttributeCount > 0)
            {
                var attribSize = Marshal.SizeOf(typeof(CREDENTIAL_ATTRIBUTE));
                var rawData = Marshal.AllocHGlobal(cred.Attributes.Count * attribSize); // new byte[nativeAttribs.Length * attribSize];
                var ptr = rawData; //IntPtr.Size == 8 ? rawData.ToInt64() : rawData.ToInt32();
                var formatter = new BinaryFormatter();

                //var i = 0;
                foreach (var attribute in cred.Attributes)
                {
                    using (var stream = new MemoryStream())
                    {
                        formatter.Serialize(stream, attribute.Value);
                        var value = stream.ToArray();

                        var pinnedArray = GCHandle.Alloc(value, GCHandleType.Pinned);
                        var attrib = new CREDENTIAL_ATTRIBUTE
                        {
                            Keyword = attribute.Key,
                            ValueSize = (uint)value.Length,
                            Value = pinnedArray.AddrOfPinnedObject() //Marshal.UnsafeAddrOfPinnedArrayElement(value, 0) //Marshal.AllocHGlobal(value.Length)
                        };
                        //Marshal.Copy(value, 0, attrib.Value, value.Length);
                        pinnedArray.Free();
                        Marshal.StructureToPtr(attrib, ptr, false); //new IntPtr(ptr + i * attribSize), false);
                        ptr = IntPtr.Add(ptr, attribSize);
                    }
                    //i++;
                }
                ncred.Attributes = rawData;
            }

            /*Marshal.FreeCoTaskMem(cred.UserName);
            Marshal.FreeCoTaskMem(cred.TargetName);
            Marshal.FreeCoTaskMem(cred.TargetAlias);
            Marshal.FreeCoTaskMem(cred.Comment);*/

            return ncred;
        }

        /// <summary>
        /// Read single <see cref="Credential"/> from Windows vault.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <param name="credential"></param>
        /// <returns><see langword="true"/> on sucess, <see langword="false"/> otherwise.</returns>
        /// <exception cref="System.ComponentModel.Win32Exception"></exception>
        public static bool CredRead(string target, CRED_TYPE type, out Credential credential)
        {
            IntPtr pCredential;
            if (!CredRead(target, type, 0, out pCredential))
            {
                var error = Marshal.GetHRForLastWin32Error();
                if ((CRED_ERRORS)error == CRED_ERRORS.ERROR_NOT_FOUND)
                {
                    credential = null;
                    return false;
                }
                throw new System.ComponentModel.Win32Exception(error);
            }

            var credHandle = new CriticalCredentialHandle(pCredential);
            return credHandle.GetCredential(out credential);
        }

        /// <summary>
        /// Write single <see cref="Credential"/> to Windows vault.
        /// </summary>
        /// <param name="userCredential"></param>
        /// <returns><see langword="true"/> on sucess, <see langword="false"/> otherwise.</returns>
        /// <exception cref="System.ComponentModel.Win32Exception"></exception>
        public static bool CredWrite(Credential userCredential)
        {
            var nativeCredential = TranslateToNativeCred(userCredential);

            try
            {
                if (!CredWrite(ref nativeCredential, 0))
                    throw new System.ComponentModel.Win32Exception(Marshal.GetHRForLastWin32Error());
            }
            finally
            {
                nativeCredential.ZeroMemory();
            }

            return true;
        }

        /// <summary>
        /// Delete single <see cref="Credential"/> from Windows vault.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <returns><see langword="true"/> on sucess, <see langword="false"/> otherwise.</returns>
        /// <exception cref="System.ComponentModel.Win32Exception"></exception>
        public static bool CredDelete(string target, CRED_TYPE type)
        {
            if (!CredDelete(target, type, 0))
                throw new System.ComponentModel.Win32Exception(Marshal.GetHRForLastWin32Error());

            return true;
        }

        /// <summary>
        /// Enumerate credentials stored in Windows vault.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="credentials"></param>
        /// <returns><see langword="true"/> on sucess, <see langword="false"/> otherwise.</returns>
        /// <exception cref="System.ComponentModel.Win32Exception"></exception>
        public static bool CredEnumerate(string filter, out Credential[] credentials)
        {
            var flags = (uint)CRED_ENUMERATE.NONE;
            if (string.IsNullOrEmpty(filter) || filter == "*")
            {
                filter = null;
                if (Environment.OSVersion.Version.Major >= 6)
                    flags = (uint)CRED_ENUMERATE.ALL_CREDENTIALS;
            }

            uint count;
            IntPtr pCredentials;
            if (!CredEnumerate(filter, flags, out count, out pCredentials))
                throw new System.ComponentModel.Win32Exception(Marshal.GetHRForLastWin32Error());

            if (pCredentials == null || pCredentials == IntPtr.Zero)
            {
                credentials = null;
                return false;
            }

            var credHandle = new CriticalCredentialHandle(pCredentials);
            return credHandle.GetCredentials(count, out credentials);
        }

        /// <summary>
        /// Test if credentials can be persisted in Windows vault.
        /// </summary>
        /// <param name="type"></param>
        /// <returns><see langword="true"/> on sucess, <see langword="false"/> otherwise.</returns>
        /// <exception cref="System.ComponentModel.Win32Exception"></exception>
        public static bool CredCanPersist(CRED_TYPE type)
        {
            var count = (uint)CRED_TYPE.MAXIMUM;
            var arr = new CRED_PERSIST[count];

            if (!CredGetSessionTypes(count, arr))
                throw new System.ComponentModel.Win32Exception(Marshal.GetHRForLastWin32Error());

            var persist = arr[(int)type];

            // If the maximum allowed is anything less than "local machine" then cannot persist credentials.
            return persist >= CRED_PERSIST.SESSION;
        }

        public static bool CredReadDomainCredentials(string targetName, out Credential[] credentials)
        {
            IntPtr pTargetInfo;
            if (!CredGetTargetInfo(targetName, 0, out pTargetInfo))
            {
                var error = Marshal.GetHRForLastWin32Error();
                if ((CRED_ERRORS)error == CRED_ERRORS.ERROR_NOT_FOUND)
                {
                    credentials = null;
                    return false;
                }
                throw new System.ComponentModel.Win32Exception(error);
            }

            if (pTargetInfo == null || pTargetInfo == IntPtr.Zero)
            {
                credentials = null;
                return false;
            }

            var targetInfo = (CREDENTIAL_TARGET_INFORMATION)Marshal.PtrToStructure(pTargetInfo, typeof(CREDENTIAL_TARGET_INFORMATION));

            uint count;
            IntPtr pCredentials;
            if (!CredReadDomainCredentials(targetInfo, 0, out count, out pCredentials))
            {
                var error = Marshal.GetHRForLastWin32Error();
                if ((CRED_ERRORS)error == CRED_ERRORS.ERROR_NOT_FOUND)
                {
                    credentials = null;
                    return false;
                }
                throw new System.ComponentModel.Win32Exception(error);
            }

            if (pCredentials == null || pCredentials == IntPtr.Zero)
            {
                credentials = null;
                return false;
            }

            var ptr = pCredentials; //IntPtr.Size == 8 ? pCredentials.ToInt64() : pCredentials.ToInt32();
            var size = Marshal.SizeOf(typeof(NativeCredential));

            credentials = new Credential[count];

            for (var i = 0; i < credentials.Length; i++)
            {
                var ncred = (NativeCredential)Marshal.PtrToStructure(ptr, typeof(NativeCredential)); //new IntPtr(ptr + i * size), typeof(NativeCredential));
                ptr = IntPtr.Add(ptr, size);
                credentials[i] = new Credential(ncred);
            }

            return true;
        }

        #region Native windows functions
        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool CredRead([In] string targetName, [In] CRED_TYPE type, [In] uint flags, out IntPtr credential);

        [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool CredWrite([In] ref NativeCredential userCredential, [In] uint flags);

        [DllImport("Advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool CredDelete([In] string target, [In] CRED_TYPE type, [In] uint flags);

        [DllImport("Advapi32.dll", EntryPoint = "CredEnumerateW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool CredEnumerate([In] string filter, [In] uint flags, out uint count, out IntPtr credential);

        [DllImport("Advapi32.dll", EntryPoint = "CredGetSessionTypes", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool CredGetSessionTypes(uint maximumPersistCount, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] CRED_PERSIST[] maximumPersist);

        [DllImport("Advapi32.dll", EntryPoint = "CredGetTargetInfoW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool CredGetTargetInfo([In] string targetName, [In] uint flags, out IntPtr targetInfo);

        [DllImport("Advapi32.dll", EntryPoint = "CredReadDomainCredentialsW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool CredReadDomainCredentials([In] CREDENTIAL_TARGET_INFORMATION TargetInfo, [In] uint flags, out uint count, out IntPtr credential);

        [DllImport("Advapi32.dll", EntryPoint = "CredFree", ExactSpelling = true, SetLastError = false)]
        private static extern void CredFree([In] IntPtr credential);
        #endregion

        private class CriticalCredentialHandle : Microsoft.Win32.SafeHandles.CriticalHandleZeroOrMinusOneIsInvalid
        {
            public CriticalCredentialHandle(IntPtr preexistingHandle)
            {
                SetHandle(preexistingHandle);
            }

            public bool GetCredential(out Credential cred)
            {
                if (IsInvalid)
                    throw new InvalidOperationException("Invalid CriticalHandle!");

                var ncred = (NativeCredential)Marshal.PtrToStructure(handle, typeof(NativeCredential));
                cred = new Credential(ncred);

                return true;
            }

            public bool GetCredentials(uint count, out Credential[] credentials)
            {
                if (IsInvalid)
                    throw new InvalidOperationException("Invalid CriticalHandle!");

                credentials = new Credential[count];

                for (var i = 0; i < credentials.Length; i++)
                {
                    var ncred = (NativeCredential)Marshal.PtrToStructure(Marshal.ReadIntPtr(handle, i * IntPtr.Size), typeof(NativeCredential));
                    credentials[i] = new Credential(ncred);
                }

                return true;
            }

            protected override bool ReleaseHandle()
            {
                if (IsInvalid)
                    return false;

                CredFree(handle);
                SetHandleAsInvalid();

                return true;
            }
        }

        /// <summary>
        /// Managed wrapper class for <see cref="NativeCredential"/>.
        /// </summary>
        internal class Credential
        {
            public CRED_FLAGS Flags { get; protected set; }
            public CRED_TYPE Type { get; protected set; }
            public string TargetName { get; protected set; }
            public string Comment { get; protected set; }
            public DateTime LastWritten { get; protected set; }
            public string CredentialBlob { get; protected set; }
            public CRED_PERSIST Persist { get; protected set; }
            public Dictionary<string, object> Attributes { get; protected set; }
            public string TargetAlias { get; protected set; }
            public string UserName { get; protected set; }

            protected Credential()
            {
            }

            [SecurityCritical]
            [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
            internal Credential(NativeCredential ncred)
            {
                Type = (CRED_TYPE)ncred.Type;
                Flags = (CRED_FLAGS)ncred.Flags;
                Persist = (CRED_PERSIST)ncred.Persist;
                LastWritten = DateTime.FromFileTime((long)((ulong)ncred.LastWritten.dwHighDateTime << 32 | (uint)ncred.LastWritten.dwLowDateTime));
                UserName = ncred.UserName; //Marshal.PtrToStringUni(ncred.UserName),
                TargetName = ncred.TargetName; //Marshal.PtrToStringUni(ncred.TargetName),
                TargetAlias = ncred.TargetAlias; //Marshal.PtrToStringUni(ncred.TargetAlias),
                Comment = ncred.Comment; //Marshal.PtrToStringUni(ncred.Comment),
                Attributes = new Dictionary<string, object>();

                if (ncred.CredentialBlobSize > 0)
                {
                    CredentialBlob = Marshal.PtrToStringUni(ncred.CredentialBlob, (int)ncred.CredentialBlobSize / 2);
                    MemoryUtil.SafeSecureZeroMemory(ncred.CredentialBlob, ncred.CredentialBlobSize);
                }

                if (ncred.AttributeCount > 0)
                {
                    var formatter = new BinaryFormatter();
                    var size = Marshal.SizeOf(typeof(CREDENTIAL_ATTRIBUTE));
                    var ptr = ncred.Attributes; //IntPtr.Size == 8 ? ncred.Attributes.ToInt64() : ncred.Attributes.ToInt32();
                    for (var i = 0; i < ncred.AttributeCount; i++)
                    {
                        var attr = (CREDENTIAL_ATTRIBUTE)Marshal.PtrToStructure(ptr, typeof(CREDENTIAL_ATTRIBUTE)); //new IntPtr(ptr + i * size), typeof(CREDENTIAL_ATTRIBUTE));
                        ptr = IntPtr.Add(ptr, size);

                        if (attr.ValueSize > 0)
                        {
                            var val = new byte[attr.ValueSize];
                            Marshal.Copy(attr.Value, val, 0, val.Length);
                            try
                            {
                                using (var stream = new MemoryStream(val, false))
                                    Attributes[attr.Keyword] = formatter.Deserialize(stream);
                            }
                            catch (InvalidCastException)
                            {
                                Attributes[attr.Keyword] = val;
                            }
                            catch (FormatException)
                            {
                                Attributes[attr.Keyword] = val;
                            }
                            catch (OverflowException)
                            {
                                Attributes[attr.Keyword] = val;
                            }
                            catch (OutOfMemoryException)
                            {
                                Attributes[attr.Keyword] = val;
                            }
                            catch (NullReferenceException)
                            {
                                Attributes[attr.Keyword] = val;
                            }
                            catch (IndexOutOfRangeException)
                            {
                                Attributes[attr.Keyword] = val;
                            }
                            catch (SerializationException)
                            {
                                Attributes[attr.Keyword] = val;
                            }
                            catch (DecoderFallbackException)
                            {
                                Attributes[attr.Keyword] = val;
                            }
                            catch (Exception ex)
                            {
                                Attributes[attr.Keyword] = ex.Message;
                            }
                        }
                        else
                            Attributes[attr.Keyword] = null;
                    }
                }
            }

            /// <summary>
            /// Zero out memory used by <see cref="UserName"/> and <see cref="CredentialBlob"/>.
            /// </summary>
            [SecurityCritical]
            [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
            public void ZeroMemory()
            {
                if (!string.IsNullOrEmpty(UserName))
                {
                    MemoryUtil.SecureZeroMemory(UserName);
                    UserName = string.Empty;
                }

                if (!string.IsNullOrEmpty(CredentialBlob))
                {
                    MemoryUtil.SecureZeroMemory(CredentialBlob);
                    CredentialBlob = string.Empty;
                }
            }
        }

        #region Native enums and structs.
        [Flags]
        internal enum CRED_FLAGS : uint
        {
            NONE = 0x0,
            PROMPT_NOW = 0x2,
            USERNAME_TARGET = 0x4,
            //UNDOCUMENTED_RUNAS = 0x2004 //8196
        }

        [Flags]
        internal enum CRED_ENUMERATE : uint
        {
            NONE = 0x0,
            ALL_CREDENTIALS = 0x1
        }

        internal enum CRED_ERRORS : uint
        {
            ERROR_SUCCESS = 0x0,
            ERROR_INVALID_PARAMETER = 0x80070057,
            ERROR_INVALID_FLAGS = 0x800703EC,
            ERROR_NOT_FOUND = 0x80070490,
            ERROR_NO_SUCH_LOGON_SESSION = 0x80070520,
            ERROR_BAD_USERNAME = 0x8007089A
        }

        internal enum CRED_PERSIST : uint
        {
            NONE = 0,
            SESSION = 1,
            LOCAL_MACHINE = 2,
            ENTERPRISE = 3
        }

        internal enum CRED_TYPE : uint
        {
            GENERIC = 1,
            DOMAIN_PASSWORD = 2,
            DOMAIN_CERTIFICATE = 3,
            DOMAIN_VISIBLE_PASSWORD = 4,
            GENERIC_CERTIFICATE = 5,
            DOMAIN_EXTENDED = 6,
            MAXIMUM = 7,
            MAXIMUM_EX = 1007
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL_ATTRIBUTE
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Keyword;
            public uint Flags;
            public uint ValueSize;
            public IntPtr Value;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL_TARGET_INFORMATION
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string TargetName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string NetbiosServerName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string DnsServerName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string NetbiosDomainName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string DnsDomainName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string DnsTreeName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string PackageName;
            public uint Flags;
            public uint CredTypeCount;
            public IntPtr CredTypes;
        }

        /// <summary>
        /// Unmanaged credentials struct wrapped by <see cref="Credential"/>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct NativeCredential
        {
            public uint Flags;
            public uint Type;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string TargetName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string TargetAlias;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string UserName;
        }
        #endregion

        /// <summary>
        /// Zero out memory used by <see cref="NativeCredential.CredentialBlob"/> and free unmanaged resources.
        /// </summary>
        [SecurityCritical]
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static void ZeroMemory(this NativeCredential nativeCredential)
        {
            // Free unmanaged resources
            if (nativeCredential.CredentialBlobSize > 0)
            {
                MemoryUtil.SafeSecureZeroMemory(nativeCredential.CredentialBlob, nativeCredential.CredentialBlobSize);
                Marshal.FreeCoTaskMem(nativeCredential.CredentialBlob);
            }

            if (nativeCredential.AttributeCount > 0)
            {
                /*var size = Marshal.SizeOf(typeof(CREDENTIAL_ATTRIBUTE));
                var ptr = nativeCredential.Attributes.ToInt32();
                for (var i = 0; i < nativeCredential.AttributeCount; i++)
                {
                    var attr = (CREDENTIAL_ATTRIBUTE)Marshal.PtrToStructure(new IntPtr(ptr + i * size), typeof(CREDENTIAL_ATTRIBUTE));
                    //Marshal.FreeHGlobal(attr.Value);
                }*/
                Marshal.FreeHGlobal(nativeCredential.Attributes);
            }
        }
    }
}