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
using KeePass.Plugins;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Security;
using KeePassRDP.Extensions;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using VisualStyles = System.Windows.Forms.VisualStyles;

namespace KeePassRDP.Utils
{
    public static class Util
    {
        #region Constants
        public const string KeePassRDP = "KeePassRDP";
        public const string UpdateUrl = "https://raw.githubusercontent.com/iSnackyCracky/KeePassRDP/master/KeePassRDP.ver";
        public const string WebsiteUrl = "https://github.com/iSnackyCracky/KeePassRDP";
        public const string LicenseUrl = "https://github.com/iSnackyCracky/KeePassRDP/blob/master/COPYING";
        public const string IgnoreEntryString = "rdpignore";
        public const string KprCpIgnoreField = IgnoreEntryString;
        public const string KprEntrySettingsField = "KeePassRDP Settings";
        public const string DefaultTriggerGroup = "RDP";
        public const string DefaultCredPickRegExPre = "domain|domänen|local|lokaler|windows";
        public const string DefaultCredPickRegExPost = "admin|user|administrator|benutzer|nutzer";
        public const string DefaultMstscPath = @"%SystemRoot%\System32\mstsc.exe";
        public const string DefaultRdpScheme = "rdp";
        public const int DefaultRdpPort = 3389;
        #endregion

        internal static readonly string GroupSeperator = Version.Parse(PwDefs.VersionString) >= Version.Parse("2.53") ? " \u2192 " : " - ";

        private static readonly Lazy<JsonSerializerSettings> _jsonSerializerSettings = new Lazy<JsonSerializerSettings>(() => new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = true
                }
            }
        });

        public static JsonSerializerSettings JsonSerializerSettings { get { return _jsonSerializerSettings.Value; } }

        public static void EnableDoubleBuffered(params Control[] controls)
        {
            foreach (var control in controls)
                control.SetDoubleBuffered();
        }

        public static bool ClickButtonOnEnter(Button button, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.None)
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        if (button != null)
                            button.PerformClick();
                        e.SuppressKeyPress = e.Handled = true;
                        return true;
                }
            return false;
        }

        public static bool ResetTextOnEscape(TextBox textBox, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.None)
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        textBox.ResetText();
                        e.SuppressKeyPress = e.Handled = true;
                        return true;
                }
            return false;
        }

        //public static bool HotkeyIsFocused { get { return FindFocusedControl(Form.ActiveForm) is KprHotkeyBox; } }

        public static Control FindFocusedControl(Control control)
        {
            var container = control as ContainerControl;
            while (container != null)
            {
                control = container.ActiveControl;
                container = control as ContainerControl;
            }
            return control;
        }

        /// <summary>
        /// Check if action is a valid operation.
        /// </summary>
        /// <param name="host"><see cref="IPluginHost"/> to check.</param>
        /// <param name="showMsg">Switch to enable/disable showing error messages with <see cref="VistaTaskDialog"/>.</param>
        /// <returns><see langword="true"/> on sucess, <see langword="false"/> otherwise.</returns>
        public static bool IsValid(IPluginHost host, bool showMsg = true)
        {
            if (!host.Database.IsOpen)
            {
                if (showMsg)
                    VistaTaskDialog.ShowMessageBoxEx(
                        KprResourceManager.Instance["Please open a KeePass database first."],
                        null,
                        KeePassRDP,
                        VtdIcon.Information,
                        host.MainWindow,
                        null, 0, null, 0);

                return false;
            }

            if (host.MainWindow.GetSelectedEntriesCount() < 1)
            {
                if (showMsg)
                    VistaTaskDialog.ShowMessageBoxEx(
                        KprResourceManager.Instance["Please select an entry first."],
                        null,
                        KeePassRDP,
                        VtdIcon.Information,
                        host.MainWindow,
                        null, 0, null, 0);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the name of the parent group of <paramref name="pe"/> is equal to <paramref name="groupName"/>.
        /// </summary>
        /// <param name="pe"><see cref="PwEntry"/> to check.</param>
        /// <param name="groupName">Name of group to compare.</param>
        /// <returns><see langword="true"/> on sucess, <see langword="false"/> otherwise.</returns>
        public static bool InRdpSubgroup(PwEntry pe, string groupName = DefaultTriggerGroup, bool recursive = true)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                groupName = DefaultTriggerGroup;

            var pg = pe.ParentGroup;
            var isInGroup = string.Equals(pg.Name, groupName, StringComparison.Ordinal);

            if (recursive)
                while (pg != null)
                {
                    if (isInGroup = string.Equals(pg.Name, groupName, StringComparison.Ordinal))
                        break;
                    pg = pg.ParentGroup;
                }

            return isInGroup;
        }

        /// <summary>
        /// Checks if <paramref name="pe"/> has the "rdpignore-flag" set.
        /// </summary>
        /// <param name="pe"><see cref="PwEntry"/> to check.</param>
        /// <returns><see langword="true"/> if ignored, <see langword="false"/> otherwise.</returns>
        public static bool IsEntryIgnored(PwEntry pe)
        {
            if (pe == null)
                return false;

            // Does a CustomField "rdpignore" exist and is the value NOT set to "false"?
            if (pe.Strings.Exists(KprCpIgnoreField) && !string.Equals(pe.Strings.ReadSafe(KprCpIgnoreField), bool.FalseString, StringComparison.OrdinalIgnoreCase))
                return true;

            using (var entrySettings = pe.GetKprSettings(true) ?? KprEntrySettings.Empty)
                return entrySettings.Ignore;
        }

        /// <summary>
        /// Tries to parse <paramref name="url"/> as <see cref="Uri"/>.
        /// </summary>
        /// <param name="url">URL to parse.</param>
        /// <returns><see cref="Uri"/> on success, <see langword="null"/> on failure.</returns>
        public static Uri ParseURL(string url)
        {
            Uri uri;

            // Try to parse entry URL as URI.
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) ||
                !Uri.TryCreate(url, UriKind.Absolute, out uri) ||
                (uri.HostNameType == UriHostNameType.Unknown && !UriParser.IsKnownScheme(uri.Scheme)))
            {
                try
                {
                    // Second try to parse entry URL as URI with UriBuilder and fallback scheme.
                    uri = new UriBuilder(DefaultRdpScheme, url).Uri;
                }
                catch (UriFormatException)
                {
                    uri = null;
                }

                if (uri == null || (uri.HostNameType == UriHostNameType.Unknown && !UriParser.IsKnownScheme(uri.Scheme)))
                {
                    // Third try to parse entry URL as URI with fallback scheme.
                    if (/*!Uri.IsWellFormedUriString(url, UriKind.Absolute) ||*/
                        !Uri.TryCreate(string.Format("{0}{1}{2}", DefaultRdpScheme, Uri.SchemeDelimiter, url), UriKind.Absolute, out uri) ||
                        uri.HostNameType == UriHostNameType.Unknown)
                    {
                        try
                        {
                            // Fourth try to parse entry URL as URI with UriBuilder.
                            uri = new UriBuilder(url).Uri;
                        }
                        catch (UriFormatException)
                        {
                            uri = null;
                        }

                        if (uri == null || (uri.HostNameType == UriHostNameType.Unknown && !UriParser.IsKnownScheme(uri.Scheme)))
                            uri = null;
                    }
                }
            }

            return uri;
        }

        /// <summary>
        /// Replaces domain in <paramref name="username"/> with local computer name.
        /// </summary>
        /// <param name="username"><see cref="ProtectedString"/> to modify.</param>
        /// <param name="host">Hostname to replace.</param>
        /// <returns>Modified original <see cref="ProtectedString"/>.</returns>
        public static ProtectedString ForceLocalUser(this ProtectedString username, string host = null)
        {
            if (username.IsEmpty)
                return username;

            var chars = username.ReadChars();

            if (chars.Length >= 2 && chars[0] == '.' && chars[1] == '\\')
                return username;

            var seperatorIndex = Array.FindIndex(chars, x => x == '\\');

            if (seperatorIndex >= 0 &&
                (string.IsNullOrEmpty(host) ||
                    (host.Length >= seperatorIndex &&
                    (host.Length == seperatorIndex || host[seperatorIndex] == '.') &&
                    host.Take(seperatorIndex)
                        .Select(x => char.ToLowerInvariant(x))
                        .SequenceEqual(chars.Take(seperatorIndex).Select(x => char.ToLowerInvariant(x))))))
            {
                username = username.Remove(0, seperatorIndex);
                username = username.Insert(0, Environment.MachineName);
            }

            MemoryUtil.SecureZeroMemory(chars);

            return username;
        }

        /// <summary>
        /// Converts from HOST.NAME\USERNAME to username@host.name
        /// </summary>
        /// <param name="username"><see cref="ProtectedString"/> to modify.</param>
        /// <returns>Modified original <see cref="ProtectedString"/>.</returns>
        public static ProtectedString ForceUPN(this ProtectedString username)
        {
            if (username.IsEmpty)
                return username;

            var user = new StringBuilder(NativeMethods.CREDUI_MAX_USERNAME_LENGTH + 1);
            var domain = new StringBuilder(NativeMethods.CREDUI_MAX_DOMAIN_TARGET_LENGTH + 1);

            var currentUsername = username.ReadString();
            if (NativeMethods.CredUIParseUserName(currentUsername, user, user.Capacity, domain, domain.Capacity) == 0 &&
                user.Length > 0 &&
                domain.Length > 0)
            {
                var newUser = user.ToString().TrimEnd(char.MinValue);
                var newDomain = domain.ToString().TrimEnd(char.MinValue);

                user.Clear();
                domain.Clear();

                if (!string.IsNullOrWhiteSpace(newUser) && !string.IsNullOrWhiteSpace(newDomain) && newDomain.Contains('.') && newDomain != ".")
                {
                    var newUsername = string.Format("{0}@{1}", newUser, newDomain);
                    if (!string.Equals(newUsername, currentUsername, StringComparison.OrdinalIgnoreCase))
                    {
                        var newUsernameLower = newUsername.ToLowerInvariant();
                        if (newUsername != newUsernameLower)
                            MemoryUtil.SecureZeroMemory(newUsername);
                        username = username.Remove(0, username.Length);
                        username = username.Insert(0, newUsernameLower);
                    }
                }

                MemoryUtil.SecureZeroMemory(newUser);
                MemoryUtil.SecureZeroMemory(newDomain);
            }

            return username;
        }

        internal static void RemoveHintFromRegistry(string host)
        {
            if (string.IsNullOrEmpty(host))
                return;

            try
            {
                using (var tsc = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Terminal Server Client", true))
                using (var srv = tsc.OpenSubKey("Servers", true))
                    if (srv != null /*&& srv.GetSubKeyNames().Contains(host)*/)
                        srv.DeleteSubKey(host, false);
            }
            catch { }
        }

        internal static bool IsRdpScheme(this Uri uri)
        {
            return DefaultRdpScheme.Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase);
        }

        private class KprToolStripMenuItem : ToolStripMenuItem
        {
            private readonly string _text;

            public KprToolStripMenuItem(string text) : base(text)
            {
                _text = text;
            }

            protected override void OnAvailableChanged(EventArgs e)
            {
                base.OnAvailableChanged(e);
                if (!Available)
                {
                    AutoSize = false;
                    Text = string.Empty;
                }
                else
                {
                    Text = _text;
                    AutoSize = true;
                }
            }

            protected override void OnOwnerChanged(EventArgs e)
            {
                base.OnOwnerChanged(e);
                if (!Available)
                {
                    AutoSize = false;
                    Text = string.Empty;
                }
                else
                {
                    Text = _text;
                    AutoSize = true;
                }
            }
        }

        internal static ToolStripMenuItem CreateToolStripMenuItem(KprMenu.MenuItem menuItem, Keys keyCode = Keys.None)
        {
            var tsmi = new KprToolStripMenuItem(menuItem.GetText())
            {
                AutoToolTip = false,
                ShowShortcutKeys = true,
                Name = menuItem.ToString(),
                AutoSize = false,
                Visible = false,
                Available = false,
                Enabled = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ImageScaling = ToolStripItemImageScaling.SizeToFit,
                Margin = Padding.Empty,
                Padding = new Padding(1)
            };

            // Silently ignore inacceptable shortcuts.
            //UIUtil.AssignShortcut(tsmi, Enum.IsDefined(typeof(Shortcut), (int)keyCode) ? keyCode : Keys.None);
            UIUtil.AssignShortcut(tsmi, ToolStripManager.IsValidShortcut(keyCode) ? keyCode : Keys.None);

            return tsmi;
        }

        internal static string FormatException(Exception ex)
        {
            var message = ex.Message;

            if (ex is AggregateException)
                (ex as AggregateException).Flatten().Handle(x =>
                {
                    message = string.Format("{0} {1}", message, x.Message).Trim();
                    return true;
                });

            ex = ex.GetBaseException();

            return string.Format("{0}{1}{2}",
                !string.IsNullOrWhiteSpace(message) ?
                    string.Format(KprResourceManager.Instance["{0}: '{1}' at {2}"], ex.GetType().Name, message, ex.TargetSite) :
                        string.Format(KprResourceManager.Instance["{0} at {1}"], ex.GetType().Name, ex.TargetSite),
                Environment.NewLine,
                string.Join(
                    Environment.NewLine,
                    Environment.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .SkipWhile(x => !x.Contains("KeePassRDP"))
                    .Skip(1)
                    .TakeWhile(x => x.Contains("KeePassRDP")))).Trim();
        }

        private static int MulDiv(int number, int numerator, int denominator)
        {
            return (int)(((long)number * numerator + (denominator >> 1)) / denominator);
        }

        internal static int ShowErrorDialog(string strContent, string strMainInstruction, string strWindowTitle, VtdIcon vtdIcon, Form fParent, string strButton1, int iResult1, string strButton2, int iResult2)
        {
            var vistaTaskDialog = new VistaTaskDialog
            {
                CommandLinks = false
            };

            if (strContent != null)
            {
                vistaTaskDialog.Content = strContent;
            }

            if (strMainInstruction != null)
            {
                vistaTaskDialog.MainInstruction = strMainInstruction;
            }

            if (strWindowTitle != null)
            {
                vistaTaskDialog.WindowTitle = strWindowTitle;
            }

            vistaTaskDialog.SetIcon(vtdIcon);
            var flag = false;

            if (!string.IsNullOrEmpty(strButton1))
            {
                vistaTaskDialog.AddButton(iResult1, strButton1, null);
                flag = true;
            }

            if (!string.IsNullOrEmpty(strButton2))
            {
                vistaTaskDialog.AddButton(iResult2, strButton2, null);
                flag = true;
            }

            try
            {
                var mCfgField = vistaTaskDialog.GetType().GetField("m_cfg", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var mCfg = mCfgField.GetValue(vistaTaskDialog);

                /*var dwFlags = mCfg.GetType().GetField("dwFlags", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                dwFlags.SetValue(mCfg, ((int)dwFlags.GetValue(mCfg)) | 0x01000000); // TDF_SIZE_TO_CONTENT;*/

                var baseUnitsX = 0;
                var width = 450;
                using (var g = Graphics.FromHwnd((fParent ?? Program.MainForm).Handle))
                {
                    var renderer = new VisualStyles.VisualStyleRenderer(VisualStyles.VisualStyleElement.Window.Dialog.Normal);
                    var metrics = renderer.GetTextMetrics(g);
                    baseUnitsX = metrics.AverageCharWidth;

                    using (var sf = new StringFormat(StringFormat.GenericTypographic)
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Near,
                        FormatFlags = StringFormatFlags.NoWrap,
                        Trimming = StringTrimming.None,
                        HotkeyPrefix = HotkeyPrefix.None
                    })
                        width = Math.Min(900, Math.Max(width, Size.Ceiling(g.MeasureString(strContent, SystemFonts.DialogFont, SizeF.Empty, sf)).Width + 32));
                }

                var cxWidth = mCfg.GetType().GetField("cxWidth", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                cxWidth.SetValue(mCfg, Convert.ToUInt32(MulDiv(width, 4, baseUnitsX)));

                mCfgField.SetValue(vistaTaskDialog, mCfg);
            }
            catch { }

            if (!vistaTaskDialog.ShowDialog(fParent))
            {
                return -1;
            }

            if (!flag)
            {
                return 0;
            }

            return vistaTaskDialog.Result;
        }

        internal static bool CheckCredentialGuard()
        {
            try
            {
                var scope = new ManagementScope(@"\\.\root\Microsoft\Windows\DeviceGuard", new ConnectionOptions
                {
                    Impersonation = ImpersonationLevel.Impersonate,
                    Authentication = AuthenticationLevel.Connect
                });

                scope.Connect();

                var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT SecurityServicesConfigured FROM Win32_DeviceGuard"));
                var result = searcher.Get().OfType<ManagementObject>().Select(x => x["SecurityServicesConfigured"]).FirstOrDefault();

                if (result != null)
                    return Array.Exists((uint[])result, x => x == 1);
            }
            catch { }

            return false;
        }

        internal static void ProtectProcessWithDacl(Process process = null)
        {
            var dispose = false;
            if (process == null)
            {
                process = Process.GetCurrentProcess();
                dispose = true;
            }

            try
            {
                var hProcess = process.Handle;
                var dacl = NativeMethods.GetProcessSecurityDescriptor(hProcess);
                for (var i = dacl.DiscretionaryAcl.Count - 1; i > 0; i--)
                    dacl.DiscretionaryAcl.RemoveAce(i);
                //dacl.DiscretionaryAcl.InsertAce(0, new CommonAce(AceFlags.None, AceQualifier.AccessDenied, (int)NativeMethods.ProcessAccessRights.PROCESS_ALL_ACCESS, new SecurityIdentifier(WellKnownSidType.WorldSid, null), false, null));
                dacl.DiscretionaryAcl.InsertAce(0, new CommonAce(
                    AceFlags.None,
                    AceQualifier.AccessAllowed,
                    (int)(NativeMethods.ProcessAccessRights.SYNCHRONIZE | NativeMethods.ProcessAccessRights.PROCESS_QUERY_LIMITED_INFORMATION | NativeMethods.ProcessAccessRights.PROCESS_TERMINATE),
                    WindowsIdentity.GetCurrent(TokenAccessLevels.Query).User,
                    false, null));
                NativeMethods.SetProcessSecurityDescriptor(hProcess, dacl);
            }
            finally
            {
                if (dispose)
                    process.Dispose();
            }
        }

        /*internal enum OidGroup
        {
            AllGroups = 0,
            HashAlgorithm = 1,                              // CRYPT_HASH_ALG_OID_GROUP_ID
            EncryptionAlgorithm = 2,                        // CRYPT_ENCRYPT_ALG_OID_GROUP_ID
            PublicKeyAlgorithm = 3,                         // CRYPT_PUBKEY_ALG_OID_GROUP_ID
            SignatureAlgorithm = 4,                         // CRYPT_SIGN_ALG_OID_GROUP_ID
            Attribute = 5,                                  // CRYPT_RDN_ATTR_OID_GROUP_ID
            ExtensionOrAttribute = 6,                       // CRYPT_EXT_OR_ATTR_OID_GROUP_ID
            EnhancedKeyUsage = 7,                           // CRYPT_ENHKEY_USAGE_OID_GROUP_ID
            Policy = 8,                                     // CRYPT_POLICY_OID_GROUP_ID
            Template = 9,                                   // CRYPT_TEMPLATE_OID_GROUP_ID
            KeyDerivationFunction = 10,                     // CRYPT_KDF_OID_GROUP_ID

            // This can be ORed into the above groups to turn off an AD search
            DisableSearchDS = unchecked((int)0x80000000)    // CRYPT_OID_DISABLE_SEARCH_DS_FLAG
        }

        internal enum OidKeyType
        {
            Oid = 1,                                        // CRYPT_OID_INFO_OID_KEY
            Name = 2,                                       // CRYPT_OID_INFO_NAME_KEY
            AlgorithmID = 3,                                // CRYPT_OID_INFO_ALGID_KEY
            SignatureID = 4,                                // CRYPT_OID_INFO_SIGN_KEY
            CngAlgorithmID = 5,                             // CRYPT_OID_INFO_CNG_ALGID_KEY
            CngSignatureID = 6,                             // CRYPT_OID_INFO_CNG_SIGN_KEY
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CRYPTOAPI_BLOB
        {
            internal int cbData;

            internal IntPtr pbData; // BYTE*
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CRYPT_OID_INFO
        {
            internal int cbSize;

            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszOID;

            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pwszName;

            internal OidGroup dwGroupId;

            // Really a union of dwValue, dwLength, or ALG_ID Algid
            internal int dwValue;

            internal CRYPTOAPI_BLOB ExtraInfo;

            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pwszCNGAlgid;

            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pwszCNGExtraAlgid;
        }

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("crypt32.dll", SetLastError = true)]
        private static extern IntPtr CryptFindOIDInfo(OidKeyType dwKeyType, IntPtr pvKey, OidGroup dwGroupId);

        // https://learn.microsoft.com/de-de/windows/win32/seccrypto/alg-id
        //private const int CALG_SHA_256 = 0x0000800c;

        private static CRYPT_OID_INFO OIDInfo(OidKeyType keyType, string key)
        {
            IntPtr rawKey;
            if (keyType == OidKeyType.Oid)
                rawKey = Marshal.StringToCoTaskMemAnsi(key);
            else
                rawKey = Marshal.StringToCoTaskMemUni(key);
            var poidinfo = CryptFindOIDInfo(keyType, rawKey, OidGroup.AllGroups);
            if (poidinfo == IntPtr.Zero)
                return new CRYPT_OID_INFO();
            var coinfo = (CRYPT_OID_INFO)Marshal.PtrToStructure(poidinfo, typeof(CRYPT_OID_INFO));
            if (rawKey != IntPtr.Zero)
                Marshal.FreeCoTaskMem(rawKey);
            return coinfo;
        }*/
    }

    public static class MemoryUtil
    {
        /*[DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
        private static extern void RtlZeroMemory(IntPtr dest, IntPtr size);*/

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("KeePassRDP.unmanaged.dll", EntryPoint = "KprSecureZeroMemory", SetLastError = false)]
        private static extern IntPtr RtlSecureZeroMemory([In] IntPtr dest, [In] IntPtr size);

        [SecurityCritical]
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static void SafeSecureZeroMemory(IntPtr dest, long size)
        {
            if (dest == IntPtr.Zero)
                return;

            try
            {
                RtlSecureZeroMemory(dest, (IntPtr)size);
            }
            catch
            {
                KeePassLib.Utility.MemUtil.ZeroMemory(dest, size);
            }
        }

        [SecurityCritical]
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static void SecureZeroMemory(string memory)
        {
            var handle = GCHandle.Alloc(memory, GCHandleType.Pinned);
            SafeSecureZeroMemory(handle.AddrOfPinnedObject(), Encoding.Unicode.GetByteCount(memory));
            handle.Free();
        }

        [SecurityCritical]
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static void SecureZeroMemory(char[] memory)
        {
            var handle = GCHandle.Alloc(memory, GCHandleType.Pinned);
            SafeSecureZeroMemory(handle.AddrOfPinnedObject(), memory.Length * sizeof(char));
            handle.Free();
        }

        [SecurityCritical]
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static void SecureZeroMemory(byte[] memory)
        {
            var handle = GCHandle.Alloc(memory, GCHandleType.Pinned);
            SafeSecureZeroMemory(handle.AddrOfPinnedObject(), memory.Length);
            handle.Free();
        }
    }

    public static class IconUtil
    {
        [DllImport("Shell32.dll", EntryPoint = "SHDefExtractIconW", SetLastError = false)]
        private static extern int SHDefExtractIcon([MarshalAs(UnmanagedType.LPWStr)] string pszIconFile, int iIndex, int uFlags, out IntPtr phiconLarge, out IntPtr phiconSmall, uint nIconSize);

        [DllImport("User32.dll", EntryPoint = "DestroyIcon", SetLastError = false)]
        internal static extern int DestroyIcon(IntPtr hIcon);

        /*[DllImport("Shell32.dll", EntryPoint = "SHCreateFileExtractIconW")]
        private static extern int SHCreateFileExtractIcon([In][MarshalAs(UnmanagedType.LPWStr)] string pszFile, [In] int dwFileAttributes, [In] Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        [ComImport()]
        [Guid("000214eb-0000-0000-c000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IExtractIconW
        {
            [PreserveSig]
            int Extract([MarshalAs(UnmanagedType.LPWStr)] string pszFile, uint nIconIndex, out IntPtr phiconLarge, out IntPtr phiconSmall, uint nIconSize);
        }*/

        private const int GIL_DONTCACHE = 0x0010;

        /// <summary>
        /// Extract <see cref="Icon"/> from file with specified size.
        /// </summary>
        /// <param name="filename">File to extract icon from.</param>
        /// <param name="index">Index of icon to request.</param>
        /// <param name="largeSize">Size of large icon to request.</param>
        /// <returns><see cref="Icon"/>, <see langword="null"/></returns>
        public static Icon ExtractIcon(string filename, int index = 0, int largeSize = 256)
        {
            // Round to next power of 2.
            /*var v = Math.Max(16, largeSize);
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            largeSize = Math.Min(256, v);*/

            // Round to previous power of 2.
            /*var v = Math.Max(16, largeSize);
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v -= v >> 1;
            largeSize = Math.Min(256, v);*/

            var v = Math.Max(16, largeSize);
            v -= v % 8;
            largeSize = Math.Min(256, v);

            var smallSize = 16;
            var largeAndSmallSize = (uint)((smallSize << 16) | (largeSize & 0xFFFF));
            IntPtr hLrgIcon;
            IntPtr hSmlIcon;

            if (SHDefExtractIcon(filename, index, GIL_DONTCACHE, out hLrgIcon, out hSmlIcon, largeAndSmallSize) != 0)
                return null;

            /*object extractIcon;
            SHCreateFileExtractIcon(filename, 0x80, typeof(IExtractIcon).GUID, out extractIcon);
            ((IExtractIcon)extractIcon).Extract(filename, (uint)index, out hLrgIcon, out hSmlIcon, largeAndSmallSize);*/

            var handle = hLrgIcon == IntPtr.Zero ? hSmlIcon : hLrgIcon;
            if (handle == IntPtr.Zero)
                return null;

            if (handle == hLrgIcon && hSmlIcon != IntPtr.Zero)
                DestroyIcon(hSmlIcon);
            if (handle == hSmlIcon && hLrgIcon != IntPtr.Zero)
                DestroyIcon(hLrgIcon);

            Icon icon = null;
            using (var tempIcon = Icon.FromHandle(handle))
                icon = tempIcon.Clone() as Icon;
            DestroyIcon(handle);

            return icon;
        }
    }

    public static class ScrollbarUtil
    {
        private const int WS_VSCROLL = 0x00200000;
        private const int WS_HSCROLL = 0x00100000;

        public static ScrollBars GetVisibleScrollbars(Control ctl)
        {
            if (!ctl.IsHandleCreated)
                return ScrollBars.None;

            var wndStylePtr = NativeMethods.GetWindowLongPtr(ctl.Handle, NativeMethods.GWL_STYLE);
            var wndStyle = IntPtr.Size == 4 ? wndStylePtr.ToInt32() : wndStylePtr.ToInt64();

            if ((wndStyle & WS_HSCROLL) != 0)
                return (wndStyle & WS_VSCROLL) != 0 ? ScrollBars.Both : ScrollBars.Horizontal;

            return (wndStyle & WS_VSCROLL) != 0 ? ScrollBars.Vertical : ScrollBars.None;
        }
    }

    public static class CursorUtil
    {
        [DllImport("User32.dll", EntryPoint = "GetCursorInfo", SetLastError = true)]
        private static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("User32.dll", EntryPoint = "GetIconInfo", SetLastError = true)]
        private static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

        [DllImport("User32.dll", EntryPoint = "CopyIcon", SetLastError = true)]
        private static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport("Gdi32.dll", EntryPoint = "DeleteObject", SetLastError = false)]
        private static extern int DeleteObject(IntPtr hObject);

        private const int CURSOR_SHOWING = 0x00000001;

        [StructLayout(LayoutKind.Sequential)]
        private struct ICONINFO
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CURSORINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }

        /// <summary>
        /// Get real pixel size of <see cref="Cursor.Current"/>.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static bool GetIconSize(out Size size)
        {
            IntPtr hicon;
            ICONINFO icInfo;
#pragma warning disable IDE0059
            var ci = new CURSORINFO
            {
                cbSize = Marshal.SizeOf(typeof(CURSORINFO))
            };
#pragma warning restore IDE0059
            if (GetCursorInfo(out ci) && ci.flags == CURSOR_SHOWING)
            {
                hicon = CopyIcon(ci.hCursor);
                if (GetIconInfo(hicon, out icInfo))
                {
                    var x = 0;
                    var y = 0;

                    using (var bmp = Image.FromHbitmap(icInfo.hbmMask))
                    {
                        var offset = icInfo.hbmColor == IntPtr.Zero ? bmp.Height / 2 : 0;
                        var bits = bmp.LockBits(
                            offset == 0 ?
                                new Rectangle(Point.Empty, bmp.Size) :
                                new Rectangle(new Point(0, offset), new Size(bmp.Width, bmp.Height - offset)),
                            ImageLockMode.ReadOnly,
                            bmp.PixelFormat);
                        var ptr = bits.Scan0;
                        var perPixel = Image.GetPixelFormatSize(bits.PixelFormat) / 8;
                        var stride = bits.Stride;
                        var width = bits.Width;
                        var height = bits.Height;
                        //var offset = icInfo.hbmColor == IntPtr.Zero ? bits.Height / 2 : 0;
                        var foundX = false;
                        var foundY = false;
                        for (var i = 0; i < width; i++)
                        {
                            var allblack = true;
                            //for (var j = offset; j < height; j++)
                            for (var j = 0; j < height; j++)
                            {
                                var off = i * perPixel + j * stride;
                                if (!(
                                    Marshal.ReadByte(ptr, off++) == 0 &&
                                    Marshal.ReadByte(ptr, off++) == 0 &&
                                    Marshal.ReadByte(ptr, off) == 0))
                                {
                                    allblack = false;
                                    if (i > x)
                                    {
                                        if (foundX)
                                            x = i;
                                        else
                                            foundX = true;
                                    }
                                    //if (j - offset > y)
                                    if (j > y)
                                    {
                                        if (foundY)
                                            y = j; //y = j - offset;
                                        else
                                            foundY = true;
                                    }
                                }
                            }
                            if (allblack && foundX && foundY)
                                break;
                        }
                        x = x == 0 ? width : x + 1;
                        y = y == 0 ? (offset > 0 ? offset : height) : y + 1;
                        bmp.UnlockBits(bits);
                    }

                    if (hicon != IntPtr.Zero)
                        IconUtil.DestroyIcon(hicon);
                    if (icInfo.hbmColor != IntPtr.Zero)
                        DeleteObject(icInfo.hbmColor);
                    if (icInfo.hbmMask != IntPtr.Zero)
                        DeleteObject(icInfo.hbmMask);
                    if (ci.hCursor != IntPtr.Zero)
                        DeleteObject(ci.hCursor);

                    size = new Size(x, y);
                    return true;
                }

                if (hicon != IntPtr.Zero)
                    IconUtil.DestroyIcon(hicon);
                if (icInfo.hbmColor != IntPtr.Zero)
                    DeleteObject(icInfo.hbmColor);
                if (icInfo.hbmMask != IntPtr.Zero)
                    DeleteObject(icInfo.hbmMask);
            }

            if (ci.hCursor != IntPtr.Zero)
                DeleteObject(ci.hCursor);

            size = Size.Empty;
            return false;
        }
    }

    /*public static class TempFileUtil
    {
        [DllImport("Kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        private const uint OPEN_EXISTING = 3;
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_ATTRIBUTE_TEMPORARY = 0x100;
        private const uint FILE_FLAG_DELETE_ON_CLOSE = 0x04000000;

        public static FileStream OpenTempFile(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = Path.GetTempFileName();

            var handle = CreateFile(
                path,
                GENERIC_READ | GENERIC_WRITE,
                (uint)(FileShare.Read | FileShare.Delete),
                IntPtr.Zero,
                OPEN_EXISTING,
                FILE_ATTRIBUTE_TEMPORARY | FILE_FLAG_DELETE_ON_CLOSE,
                IntPtr.Zero);

            if (handle.IsInvalid)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

            return new FileStream(handle, FileAccess.ReadWrite, 1024);
        }
    }*/
}