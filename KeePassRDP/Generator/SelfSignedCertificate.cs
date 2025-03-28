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

using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using _FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace KeePassRDP.Generator
{
    /*
     * Partly taken from https://github.com/microsoftarchive/clrsecurity/tree/master/Security.Cryptography/src/X509Certificates
     * MIT License https://github.com/microsoftarchive/clrsecurity/blob/master/LICENSE
     * Copyright (c) 2017 Microsoft
     */
    internal class SelfSignedCertificate
    {
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public sealed class SafeCertContextHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeCertContextHandle() : base(true)
            {
            }

            internal SafeCertContextHandle(IntPtr handle) : base(true)
            {
                SetHandle(handle);
            }

            [DllImport("crypt32.dll")]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CertFreeCertificateContext(IntPtr pCertContext);

            protected override bool ReleaseHandle()
            {
                return CertFreeCertificateContext(handle);
            }
        }

        internal sealed class SafeLocalAllocHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeLocalAllocHandle() : base(true) { }

            // 0 is an Invalid Handle
            internal SafeLocalAllocHandle(IntPtr handle) : base(true)
            {
                SetHandle(handle);
            }

            internal static SafeLocalAllocHandle InvalidHandle
            {
                get { return new SafeLocalAllocHandle(IntPtr.Zero); }
            }

            [DllImport("kernel32.dll", SetLastError = true),
             SuppressUnmanagedCodeSecurity,
             ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            private static extern IntPtr LocalFree(IntPtr handle);

            override protected bool ReleaseHandle()
            {
                return LocalFree(handle) == IntPtr.Zero;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class UnsafeNativeMethods
        {
            [DllImport("crypt32.dll", SetLastError = true)]
            internal static extern SafeCertContextHandle CertCreateSelfSignCertificate([In] SafeNCryptKeyHandle hCryptProvOrNCryptKey,
                                                                                       [In] ref CapiNative.CRYPTOAPI_BLOB pSubjectIssuerBlob,
                                                                                       [In] X509CertificateCreationOptions dwFlags,
                                                                                       [In] ref CRYPT_KEY_PROV_INFO pKeyProvInfo,
                                                                                       [In] ref CapiNative.CRYPT_ALGORITHM_IDENTIFIER pSignatureAlgorithm,
                                                                                       [In] ref Win32Native.SYSTEMTIME pStartTime,
                                                                                       [In] ref Win32Native.SYSTEMTIME pEndTime,
                                                                                       [In] ref CERT_EXTENSIONS pExtensions);

            // Overload of CertSetCertificateContextProperty for setting CERT_KEY_CONTEXT_PROP_ID
            [DllImport("crypt32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CertSetCertificateContextProperty([In] SafeCertContextHandle pCertContext,
                                                                          [In] CertificateProperty dwPropId,
                                                                          [In] CertificatePropertySetFlags dwFlags,
                                                                          [In] ref CERT_KEY_CONTEXT pvData);

            [DllImport("crypt32.dll", SetLastError = true, EntryPoint = "CertStrToNameW", CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CertStrToName([In] uint dwCertEncodingType,
                                                      [In] string pszX500,
                                                      [In] uint dwStrType,
                                                      [In] IntPtr pvReserved,
                                                      [In, Out] byte[] pbEncoded,
                                                      [In, Out] ref uint pcbEncoded,
                                                      [In, Out] IntPtr ppszError);

            [DllImport("crypt32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CryptSignAndEncodeCertificate(SafeNCryptKeyHandle hCryptProvOrNCryptKey,
                                                                    KeySpec dwKeySpec,
                                                                    uint dwCertEncodingType,
                                                                    ulong lpszStructType,
                                                                    IntPtr pvStructInfo,
                                                                    ref CapiNative.CRYPT_ALGORITHM_IDENTIFIER pSignatureAlgorithm,
                                                                    IntPtr pvHashAuxInfo,
                                                                    byte[] pbEncoded,
                                                                    ref uint pcbEncoded);

            [DllImport("crypt32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CertSetCertificateContextProperty([In] SafeCertContextHandle pCertContext,
                                                                          [In] CertificateProperty dwPropId,
                                                                          [In] CertificatePropertySetFlags dwFlags,
                                                                          [In] SafeLocalAllocHandle pvData);

            /*[DllImport("crypt32.dll", SetLastError = true)]
            internal static extern bool CertGetCertificateContextProperty(
                [In] SafeCertContextHandle pCertContext,
                [In] uint dwPropId,
                [In, Out] SafeLocalAllocHandle pvData,
                [In, Out] ref uint pcbData);*/
        }

        internal static class Win32Native
        {
            [StructLayout(LayoutKind.Sequential)]
            internal struct SYSTEMTIME
            {
                internal ushort wYear;
                internal ushort wMonth;
                internal ushort wDayOfWeek;
                internal ushort wDay;
                internal ushort wHour;
                internal ushort wMinute;
                internal ushort wSecond;
                internal ushort wMilliseconds;

                internal SYSTEMTIME(DateTime time)
                {
                    wYear = (ushort)time.Year;
                    wMonth = (ushort)time.Month;
                    wDayOfWeek = (ushort)time.DayOfWeek;
                    wDay = (ushort)time.Day;
                    wHour = (ushort)time.Hour;
                    wMinute = (ushort)time.Minute;
                    wSecond = (ushort)time.Second;
                    wMilliseconds = (ushort)time.Millisecond;
                }
            }
        }

        internal static class CapiNative
        {
            [StructLayout(LayoutKind.Sequential)]
            internal struct CRYPT_ALGORITHM_IDENTIFIER
            {
                [MarshalAs(UnmanagedType.LPStr)]
                internal string pszObjId;

                internal CRYPTOAPI_BLOB Parameters;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct CRYPTOAPI_BLOB
            {
                internal int cbData;

                internal IntPtr pbData; // BYTE*
            }
        }

        /// <summary>
        ///     The X509CertificateCreationOptions enumeration provides a set of flags for use when creating a new
        ///     X509 certificate.
        /// </summary>
        [Flags]
        public enum X509CertificateCreationOptions
        {
            /// <summary>
            ///     Do not set any flags when creating the certificate
            /// </summary>
            None = 0x00000000,

            /// <summary>
            ///     Create an unsigned certificate.  This maps to the CERT_CREATE_SELFSIGN_NO_SIGN flag.
            /// </summary>
            DoNotSignCertificate = 0x00000001,

            /// <summary>
            ///     By default, certificates will reference their private keys by setting the
            ///     CERT_KEY_PROV_INFO_PROP_ID; the DoNotLinkKeyInformation flag causes the certificate to
            ///     instead contain the private key direclty rather than by reference.  This maps to the
            ///     CERT_CREATE_SELFSIGN_NO_KEY_INFO flag.
            /// </summary>
            DoNotLinkKeyInformation = 0x00000002,
        }

        /// <summary>
        ///     Well known certificate property IDs
        /// </summary>
        internal enum CertificateProperty
        {
            KeyProviderInfo = 2,    // CERT_KEY_PROV_INFO_PROP_ID
            KeyContext = 5,    // CERT_KEY_CONTEXT_PROP_ID
        }

        /// <summary>
        ///     Flags for the CertSetCertificateContextProperty API
        /// </summary>
        [Flags]
        internal enum CertificatePropertySetFlags
        {
            None = 0x00000000,
            NoCryptRelease = 0x00000001,   // CERT_STORE_NO_CRYPT_RELEASE_FLAG
        }

        /// <summary>
        ///     KeySpec for CERT_KEY_CONTEXT structures
        /// </summary>
        internal enum KeySpec
        {
            NCryptKey = unchecked((int)0xffffffff)    // CERT_NCRYPT_KEY_SPEC
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CERT_EXTENSION
        {
            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszObjId;

            [MarshalAs(UnmanagedType.Bool)]
            internal bool fCritical;

            internal CapiNative.CRYPTOAPI_BLOB Value;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CERT_EXTENSIONS
        {
            internal int cExtension;

            internal IntPtr rgExtension;                // CERT_EXTENSION[cExtension]
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CERT_KEY_CONTEXT
        {
            internal int cbSize;
            internal IntPtr hNCryptKey;
            internal KeySpec dwKeySpec;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CRYPT_KEY_PROV_INFO
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pwszContainerName;

            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pwszProvName;

            internal int dwProvType;

            internal int dwFlags;

            internal int cProvParam;

            internal IntPtr rgProvParam;        // PCRYPT_KEY_PROV_PARAM

            internal int dwKeySpec;
        }

        /// <summary>
        ///     Create a self signed certificate around a CNG key
        /// </summary>
        [SecurityCritical]
        internal static SafeCertContextHandle CreateSelfSignedCertificate(CngKey key,
                                                                          bool takeOwnershipOfKey,
                                                                          string subjectName,
                                                                          X509CertificateCreationOptions creationOptions,
                                                                          string signatureAlgorithmOid,
                                                                          DateTime startTime,
                                                                          DateTime endTime,
                                                                          X509ExtensionCollection extensions)
        {
            Debug.Assert(key != null, "key != null");
            Debug.Assert(subjectName != null, "subjectName != null");
            Debug.Assert(!String.IsNullOrEmpty(signatureAlgorithmOid), "!String.IsNullOrEmpty(signatureAlgorithmOid)");
            Debug.Assert(extensions != null, "extensions != null");

            // Create an algorithm identifier structure for the signature algorithm
            CapiNative.CRYPT_ALGORITHM_IDENTIFIER nativeSignatureAlgorithm = new CapiNative.CRYPT_ALGORITHM_IDENTIFIER();
            nativeSignatureAlgorithm.pszObjId = signatureAlgorithmOid;
            nativeSignatureAlgorithm.Parameters = new CapiNative.CRYPTOAPI_BLOB();
            nativeSignatureAlgorithm.Parameters.cbData = 0;
            nativeSignatureAlgorithm.Parameters.pbData = IntPtr.Zero;

            // Convert the begin and expire dates to system time structures
            Win32Native.SYSTEMTIME nativeStartTime = new Win32Native.SYSTEMTIME(startTime);
            Win32Native.SYSTEMTIME nativeEndTime = new Win32Native.SYSTEMTIME(endTime);

            // Map the extensions into CERT_EXTENSIONS.  This involves several steps to get the
            // CERT_EXTENSIONS ready for interop with the native APIs.
            //   1. Build up the CERT_EXTENSIONS structure in managed code
            //   2. For each extension, create a managed CERT_EXTENSION structure; this requires allocating
            //      native memory for the blob pointer in the CERT_EXTENSION. These extensions are stored in
            //      the nativeExtensionArray variable.
            //   3. Get a block of native memory that can hold a native array of CERT_EXTENSION structures.
            //      This is the block referenced by the CERT_EXTENSIONS structure.
            //   4. For each of the extension structures created in step 2, marshal the extension into the
            //      native buffer allocated in step 3.
            CERT_EXTENSIONS nativeExtensions = new CERT_EXTENSIONS();
            nativeExtensions.cExtension = extensions.Count;
            CERT_EXTENSION[] nativeExtensionArray = new CERT_EXTENSION[extensions.Count];

            // Run this in a CER to ensure that we release any native memory allocated for the certificate
            // extensions.
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                // Copy over each extension into a native extension structure, including allocating native
                // memory for its blob if necessary.
                for (int i = 0; i < extensions.Count; ++i)
                {
                    nativeExtensionArray[i] = new CERT_EXTENSION();
                    nativeExtensionArray[i].pszObjId = extensions[i].Oid.Value;
                    nativeExtensionArray[i].fCritical = extensions[i].Critical;

                    nativeExtensionArray[i].Value = new CapiNative.CRYPTOAPI_BLOB();
                    nativeExtensionArray[i].Value.cbData = extensions[i].RawData.Length;
                    if (nativeExtensionArray[i].Value.cbData > 0)
                    {
                        nativeExtensionArray[i].Value.pbData =
                            Marshal.AllocCoTaskMem(nativeExtensionArray[i].Value.cbData);
                        Marshal.Copy(extensions[i].RawData,
                                     0,
                                     nativeExtensionArray[i].Value.pbData,
                                     nativeExtensionArray[i].Value.cbData);
                    }
                }

                // Now that we've built up the extension array, create a block of native memory to marshal
                // them into.
                if (nativeExtensionArray.Length > 0)
                {
                    checked
                    {
                        // CERT_EXTENSION structures end with a pointer field, which means on all supported
                        // platforms they won't require any padding between elements of the array.
                        nativeExtensions.rgExtension =
                            Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(CERT_EXTENSION)) * nativeExtensionArray.Length);

                        for (int i = 0; i < nativeExtensionArray.Length; ++i)
                        {
                            ulong offset = (uint)i * (uint)Marshal.SizeOf(typeof(CERT_EXTENSION));
                            ulong next = offset + (ulong)nativeExtensions.rgExtension.ToInt64();
                            IntPtr nextExtensionAddr = new IntPtr((long)next);

                            Marshal.StructureToPtr(nativeExtensionArray[i], nextExtensionAddr, false);
                        }
                    }
                }

                // Setup a CRYPT_KEY_PROV_INFO for the key
                CRYPT_KEY_PROV_INFO keyProvInfo = new CRYPT_KEY_PROV_INFO();
                keyProvInfo.pwszContainerName = key.UniqueName;
                keyProvInfo.pwszProvName = key.Provider.Provider;
                keyProvInfo.dwProvType = 0;     // NCRYPT
                keyProvInfo.dwFlags = 0;
                keyProvInfo.cProvParam = 0;
                keyProvInfo.rgProvParam = IntPtr.Zero;
                keyProvInfo.dwKeySpec = 0;

                //
                // Now that all of the needed data structures are setup, we can create the certificate
                //

                SafeCertContextHandle selfSignedCertHandle = null;

                const uint X509_ASN_ENCODING = 0x00000001;
                const uint CERT_X500_NAME_STR = 3;

                byte[] encodedName = null;
                uint cbName = 0;
                if (UnsafeNativeMethods.CertStrToName(X509_ASN_ENCODING, subjectName, CERT_X500_NAME_STR, IntPtr.Zero, null, ref cbName, IntPtr.Zero))
                {
                    encodedName = new byte[cbName];
                    UnsafeNativeMethods.CertStrToName(X509_ASN_ENCODING, subjectName, CERT_X500_NAME_STR, IntPtr.Zero, encodedName, ref cbName, IntPtr.Zero);
                }

                GCHandle subjectNameHandle = GCHandle.Alloc(encodedName, GCHandleType.Pinned);
                // Create a CRYPTOAPI_BLOB for the subject of the cert
                CapiNative.CRYPTOAPI_BLOB nativeSubjectName = new CapiNative.CRYPTOAPI_BLOB();
                nativeSubjectName.cbData = encodedName.Length;
                nativeSubjectName.pbData = subjectNameHandle.AddrOfPinnedObject(); //new IntPtr(pSubjectName);

                // Now that we've converted all the inputs to native data structures, we can generate
                // the self signed certificate for the input key.
                using (SafeNCryptKeyHandle keyHandle = key.Handle)
                {
                    selfSignedCertHandle =
                        UnsafeNativeMethods.CertCreateSelfSignCertificate(keyHandle,
                                                                          ref nativeSubjectName,
                                                                          creationOptions,
                                                                          ref keyProvInfo,
                                                                          ref nativeSignatureAlgorithm,
                                                                          ref nativeStartTime,
                                                                          ref nativeEndTime,
                                                                          ref nativeExtensions);
                }

                subjectNameHandle.Free();
                Debug.Assert(selfSignedCertHandle != null, "selfSignedCertHandle != null");

                if (selfSignedCertHandle.IsInvalid)
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }

                // Attach a key context to the certificate which will allow Windows to find the private key
                // associated with the certificate if the NCRYPT_KEY_HANDLE is ephemeral.
                // is done.
                using (SafeNCryptKeyHandle keyHandle = key.Handle)
                {
                    CERT_KEY_CONTEXT keyContext = new CERT_KEY_CONTEXT();
                    keyContext.cbSize = Marshal.SizeOf(typeof(CERT_KEY_CONTEXT));
                    keyContext.hNCryptKey = keyHandle.DangerousGetHandle();
                    keyContext.dwKeySpec = KeySpec.NCryptKey;

                    bool attachedProperty = false;
                    int setContextError = 0;

                    // Run in a CER to ensure accurate tracking of the transfer of handle ownership
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try { }
                    finally
                    {
                        CertificatePropertySetFlags flags = CertificatePropertySetFlags.None;
                        if (!takeOwnershipOfKey)
                        {
                            // If the certificate is not taking ownership of the key handle, then it should
                            // not release the handle when the context is released.
                            flags |= CertificatePropertySetFlags.NoCryptRelease;
                        }

                        attachedProperty =
                            UnsafeNativeMethods.CertSetCertificateContextProperty(selfSignedCertHandle,
                                                                                  CertificateProperty.KeyContext,
                                                                                  flags,
                                                                                  ref keyContext);
                        setContextError = Marshal.GetLastWin32Error();

                        // If we succesfully transferred ownership of the key to the certificate,
                        // then we need to ensure that we no longer release its handle.
                        if (attachedProperty && takeOwnershipOfKey)
                        {
                            keyHandle.SetHandleAsInvalid();
                        }
                    }

                    if (!attachedProperty)
                    {
                        throw new CryptographicException(setContextError);
                    }
                }

                return selfSignedCertHandle;
            }
            finally
            {
                //
                // In order to release all resources held by the CERT_EXTENSIONS we need to do three things
                //   1. Destroy each structure marshaled into the native CERT_EXTENSION array
                //   2. Release the memory used for the CERT_EXTENSION array
                //   3. Release the memory used in each individual CERT_EXTENSION
                //

                // Release each extension marshaled into the native buffer as well
                if (nativeExtensions.rgExtension != IntPtr.Zero)
                {
                    for (int i = 0; i < nativeExtensionArray.Length; ++i)
                    {
                        ulong offset = (uint)i * (uint)Marshal.SizeOf(typeof(CERT_EXTENSION));
                        ulong next = offset + (ulong)nativeExtensions.rgExtension.ToInt64();
                        IntPtr nextExtensionAddr = new IntPtr((long)next);

                        Marshal.DestroyStructure(nextExtensionAddr, typeof(CERT_EXTENSION));
                    }

                    Marshal.FreeCoTaskMem(nativeExtensions.rgExtension);
                }

                // If we allocated memory for any extensions, make sure to free it now
                for (int i = 0; i < nativeExtensionArray.Length; ++i)
                {
                    if (nativeExtensionArray[i].Value.pbData != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(nativeExtensionArray[i].Value.pbData);
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CERT_CONTEXT
        {
            internal uint dwCertEncodingType;
            internal IntPtr pbCertEncoded;
            internal uint cbCertEncoded;
            internal IntPtr pCertInfo;
            internal IntPtr hCertStore;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CRYPT_BIT_BLOB
        {
            internal uint cbData;
            internal IntPtr pbData;
            internal uint cUnusedBits;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CERT_PUBLIC_KEY_INFO
        {
            internal CapiNative.CRYPT_ALGORITHM_IDENTIFIER Algorithm;
            internal CRYPT_BIT_BLOB PublicKey;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CERT_INFO
        {
            internal uint dwVersion;
            internal CapiNative.CRYPTOAPI_BLOB SerialNumber;
            internal CapiNative.CRYPT_ALGORITHM_IDENTIFIER SignatureAlgorithm;
            internal CapiNative.CRYPTOAPI_BLOB Issuer;
            internal _FILETIME NotBefore;
            internal _FILETIME NotAfter;
            internal CapiNative.CRYPTOAPI_BLOB Subject;
            internal CERT_PUBLIC_KEY_INFO SubjectPublicKeyInfo;
            internal CRYPT_BIT_BLOB IssuerUniqueId;
            internal CRYPT_BIT_BLOB SubjectUniqueId;
            internal uint cExtension;
            internal IntPtr rgExtension; // PCERT_EXTENSION
        }

        [SecurityCritical]
        internal static X509Certificate2 SignCertificate(CngKey key, X509Certificate2 certificateToSign, CngKey caKey, X509Certificate2 caCert, string signatureAlgorithmOid)
        {
            RuntimeHelpers.PrepareConstrainedRegions();

            // Get CA cert into CERT_CONTEXT
            // Get CA cert into CERT_INFO from context.pCertInfo

            CERT_CONTEXT CAContext = (CERT_CONTEXT)Marshal.PtrToStructure(caCert.Handle, typeof(CERT_CONTEXT));
            CERT_INFO CACertInfo = (CERT_INFO)Marshal.PtrToStructure(CAContext.pCertInfo, typeof(CERT_INFO));

            CapiNative.CRYPT_ALGORITHM_IDENTIFIER signatureAlgo = new CapiNative.CRYPT_ALGORITHM_IDENTIFIER
            {
                pszObjId = signatureAlgorithmOid,
                Parameters = new CapiNative.CRYPTOAPI_BLOB
                {
                    cbData = 0,
                    pbData = IntPtr.Zero
                }
            };

            // Get subordinate cert into CERT_CONTEXT
            // Get subordinate cert into CERT_INFO from context.pCertInfo

            CERT_CONTEXT subordinateCertContext = (CERT_CONTEXT)Marshal.PtrToStructure(certificateToSign.Handle, typeof(CERT_CONTEXT));
            CERT_INFO subordinateCertInfo = (CERT_INFO)Marshal.PtrToStructure(subordinateCertContext.pCertInfo, typeof(CERT_INFO));

            subordinateCertInfo.cExtension = certificateToSign.Extensions == null ? 0 : (uint)certificateToSign.Extensions.Count;
            CERT_EXTENSION[] nativeExtensionArray = new CERT_EXTENSION[subordinateCertInfo.cExtension];

            // Copy over each extension into a native extension structure, including allocating native
            // memory for its blob if necessary.
            for (int i = 0; i < nativeExtensionArray.Length; ++i)
            {
                nativeExtensionArray[i] = new CERT_EXTENSION();
                nativeExtensionArray[i].pszObjId = certificateToSign.Extensions[i].Oid.Value;
                nativeExtensionArray[i].fCritical = certificateToSign.Extensions[i].Critical;

                nativeExtensionArray[i].Value = new CapiNative.CRYPTOAPI_BLOB();
                nativeExtensionArray[i].Value.cbData = certificateToSign.Extensions[i].RawData.Length;
                if (nativeExtensionArray[i].Value.cbData > 0)
                {
                    nativeExtensionArray[i].Value.pbData =
                        Marshal.AllocCoTaskMem(nativeExtensionArray[i].Value.cbData);
                    Marshal.Copy(certificateToSign.Extensions[i].RawData,
                                 0,
                                 nativeExtensionArray[i].Value.pbData,
                                 nativeExtensionArray[i].Value.cbData);
                }
            }

            // Now that we've built up the extension array, create a block of native memory to marshal
            // them into.
            if (nativeExtensionArray.Length > 0)
            {
                checked
                {
                    // CERT_EXTENSION structures end with a pointer field, which means on all supported
                    // platforms they won't require any padding between elements of the array.
                    subordinateCertInfo.rgExtension =
                        Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(CERT_EXTENSION)) * nativeExtensionArray.Length);

                    for (int i = 0; i < nativeExtensionArray.Length; ++i)
                    {
                        ulong offset = (uint)i * (uint)Marshal.SizeOf(typeof(CERT_EXTENSION));
                        ulong next = offset + (ulong)subordinateCertInfo.rgExtension.ToInt64();
                        IntPtr nextExtensionAddr = new IntPtr((long)next);

                        Marshal.StructureToPtr(nativeExtensionArray[i], nextExtensionAddr, false);
                    }
                }
            }

            byte[] pbEncodedCert = null;
            IntPtr subordinateCertInfoAllocPtr = IntPtr.Zero;

            try
            {

                subordinateCertInfo.SignatureAlgorithm = signatureAlgo;
                subordinateCertInfo.Issuer = CACertInfo.Subject;

                subordinateCertInfoAllocPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CERT_INFO)));
                Marshal.StructureToPtr(subordinateCertInfo, subordinateCertInfoAllocPtr, false);

                UInt32 pbEncodedCertLength = 0;

                const uint PKCS_7_ASN_ENCODING = 0x00010000;
                const uint X509_ASN_ENCODING = 0x00000001;
                const ulong X509_CERT_TO_BE_SIGNED = 2;

                using (SafeNCryptKeyHandle caKeyHandle = caKey.Handle)
                {
                    if (!UnsafeNativeMethods.CryptSignAndEncodeCertificate(caKeyHandle,
                                                                  KeySpec.NCryptKey,
                                                                  PKCS_7_ASN_ENCODING | X509_ASN_ENCODING,
                                                                  X509_CERT_TO_BE_SIGNED,
                                                                  subordinateCertInfoAllocPtr,
                                                                  ref signatureAlgo,
                                                                  IntPtr.Zero,
                                                                  pbEncodedCert,
                                                                  ref pbEncodedCertLength))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    pbEncodedCert = new byte[pbEncodedCertLength];

                    if (!UnsafeNativeMethods.CryptSignAndEncodeCertificate(caKeyHandle,
                                                                    KeySpec.NCryptKey,
                                                                    PKCS_7_ASN_ENCODING | X509_ASN_ENCODING,
                                                                    X509_CERT_TO_BE_SIGNED,
                                                                    subordinateCertInfoAllocPtr,
                                                                    ref signatureAlgo,
                                                                    IntPtr.Zero,
                                                                    pbEncodedCert,
                                                                    ref pbEncodedCertLength))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
            }
            finally
            {
                // Release each extension marshaled into the native buffer as well
                if (subordinateCertInfo.rgExtension != IntPtr.Zero)
                {
                    for (int i = 0; i < nativeExtensionArray.Length; ++i)
                    {
                        ulong offset = (uint)i * (uint)Marshal.SizeOf(typeof(CERT_EXTENSION));
                        ulong next = offset + (ulong)subordinateCertInfo.rgExtension.ToInt64();
                        IntPtr nextExtensionAddr = new IntPtr((long)next);

                        Marshal.DestroyStructure(nextExtensionAddr, typeof(CERT_EXTENSION));
                    }

                    Marshal.FreeCoTaskMem(subordinateCertInfo.rgExtension);
                }

                // If we allocated memory for any extensions, make sure to free it now
                for (int i = 0; i < nativeExtensionArray.Length; ++i)
                {
                    if (nativeExtensionArray[i].Value.pbData != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(nativeExtensionArray[i].Value.pbData);
                    }
                }
            }

            var newCert = new X509Certificate2(pbEncodedCert, string.Empty, X509KeyStorageFlags.UserKeySet);
            var takeOwnershipOfKey = true;

            // Attach a key context to the certificate which will allow Windows to find the private key
            // associated with the certificate if the NCRYPT_KEY_HANDLE is ephemeral.
            // is done.
            using (SafeNCryptKeyHandle keyHandle = key.Handle)
            {
                CERT_KEY_CONTEXT keyContext = new CERT_KEY_CONTEXT();
                keyContext.cbSize = Marshal.SizeOf(typeof(CERT_KEY_CONTEXT));
                keyContext.hNCryptKey = keyHandle.DangerousGetHandle();
                keyContext.dwKeySpec = KeySpec.NCryptKey;

                CRYPT_KEY_PROV_INFO keyProvInfo = new CRYPT_KEY_PROV_INFO();
                keyProvInfo.pwszContainerName = key.UniqueName;
                keyProvInfo.pwszProvName = key.Provider.Provider;
                keyProvInfo.dwProvType = 0;     // NCRYPT
                keyProvInfo.dwFlags = 0;
                keyProvInfo.cProvParam = 0;
                keyProvInfo.rgProvParam = IntPtr.Zero;
                keyProvInfo.dwKeySpec = 0;

                IntPtr keyProvInfoAllocPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CRYPT_KEY_PROV_INFO)));
                Marshal.StructureToPtr(keyProvInfo, keyProvInfoAllocPtr, false);

                bool attachedProperty = false;
                int setContextError = 0;

                // Run in a CER to ensure accurate tracking of the transfer of handle ownership
                RuntimeHelpers.PrepareConstrainedRegions();
                try { }
                finally
                {
                    CertificatePropertySetFlags flags = CertificatePropertySetFlags.None;
                    if (!takeOwnershipOfKey)
                    {
                        // If the certificate is not taking ownership of the key handle, then it should
                        // not release the handle when the context is released.
                        flags |= CertificatePropertySetFlags.NoCryptRelease;
                    }


                    using (var safeCertContextHandle = new SafeCertContextHandle(newCert.Handle))
                    {
                        using (var safeLocalAllocHandle = new SafeLocalAllocHandle(keyProvInfoAllocPtr))
                            if (!UnsafeNativeMethods.CertSetCertificateContextProperty(safeCertContextHandle,
                                                                                  CertificateProperty.KeyProviderInfo,
                                                                                  CertificatePropertySetFlags.None,
                                                                                  safeLocalAllocHandle))
                                throw new Win32Exception(Marshal.GetLastWin32Error());

                        attachedProperty =
                                UnsafeNativeMethods.CertSetCertificateContextProperty(safeCertContextHandle,
                                                                                      CertificateProperty.KeyContext,
                                                                                      flags,
                                                                                      ref keyContext);
                        setContextError = Marshal.GetLastWin32Error();
                    }

                    // If we succesfully transferred ownership of the key to the certificate,
                    // then we need to ensure that we no longer release its handle.
                    if (attachedProperty && takeOwnershipOfKey)
                    {
                        keyHandle.SetHandleAsInvalid();
                    }
                }

                Marshal.DestroyStructure(keyProvInfoAllocPtr, typeof(CRYPT_KEY_PROV_INFO));
                //Marshal.FreeHGlobal(keyProvInfoAllocPtr);

                if (!attachedProperty)
                {
                    throw new CryptographicException(setContextError);
                }
            }

            Marshal.DestroyStructure(subordinateCertInfoAllocPtr, typeof(CERT_INFO));
            //Marshal.FreeHGlobal(subordinateCertInfoAllocPtr);

            return newCert;
        }
    }
}
