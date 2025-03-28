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

using KeePassRDP.Extensions;
using KeePassRDP.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace KeePassRDP.Generator
{
    public partial class RdpFile : IDisposable
    {
        private readonly string _path;

        [JsonIgnore]
        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(Regex.Replace(
                    JsonConvert.SerializeObject(this, Util.JsonSerializerSettings),
                    "^{}$",
                    string.Empty,
                    RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase));
            }
        }

        private static readonly Lazy<ReadOnlyCollection<PropertyInfo>> _propertyCache = new Lazy<ReadOnlyCollection<PropertyInfo>>(() =>
            typeof(RdpFile)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToArray()
                .AsReadOnly(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<ReadOnlyCollection<Tuple<PropertyInfo, RdpSignscopeAttribute, RdpSettingAttribute>>> _signscopeCache = new Lazy<ReadOnlyCollection<Tuple<PropertyInfo, RdpSignscopeAttribute, RdpSettingAttribute>>>(() =>
            _propertyCache.Value
                .Select(x =>
                {
                    var customAttributes = x.GetCustomAttributes(false);
                    var signscopeAttribute = customAttributes.OfType<RdpSignscopeAttribute>().FirstOrDefault();
                    if (signscopeAttribute == null)
                        return null;
                    return new Tuple<PropertyInfo, RdpSignscopeAttribute, RdpSettingAttribute>(x, signscopeAttribute, customAttributes.OfType<RdpSettingAttribute>().FirstOrDefault());
                })
                .Where(x => x != null && x.Item2 != null && x.Item3 != null)
                .OrderBy(x => x.Item3.Template.Substring(0, Math.Max(0, x.Item3.Template.IndexOf(':'))), StringComparer.OrdinalIgnoreCase)
                .ToArray()
                .AsReadOnly(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<SortedDictionary<RdpSettingAttribute.SettingCategory, ReadOnlyCollection<Tuple<PropertyInfo, RdpSettingAttribute>>>> _settingsCache = new Lazy<SortedDictionary<RdpSettingAttribute.SettingCategory, ReadOnlyCollection<Tuple<PropertyInfo, RdpSettingAttribute>>>>(() =>
            new SortedDictionary<RdpSettingAttribute.SettingCategory, ReadOnlyCollection<Tuple<PropertyInfo, RdpSettingAttribute>>>(
                _propertyCache.Value
                .Select(x =>
                {
                    var settingAttribute = x.GetCustomAttributes(false).OfType<RdpSettingAttribute>().FirstOrDefault();
                    if (settingAttribute == null)
                        return null;
                    return new Tuple<PropertyInfo, RdpSettingAttribute>(x, settingAttribute);
                })
                .Where(x => x != null && x.Item2 != null)
                .GroupBy(x => x.Item2.Category)
                .ToDictionary(x => x.Key, x => x.OrderBy(y => y.Item2.Template.Substring(0, Math.Max(0, y.Item2.Template.IndexOf(':'))), StringComparer.OrdinalIgnoreCase).ToArray().AsReadOnly()),
                Comparer<RdpSettingAttribute.SettingCategory>.Default),
            LazyThreadSafetyMode.ExecutionAndPublication);

        [JsonIgnore]
        public static IDictionary<string, ReadOnlyCollection<Tuple<PropertyInfo, RdpSettingAttribute>>> SettingsCache { get { return _settingsCache.Value.ToDictionary(x => x.Key.ToString(), x => x.Value); } }

        public const string RdpSignatureAlgorithmName = "sha256RSA";
        public const string RdpSignatureDigestAlgorithmName = "sha256";

        private static readonly byte[] _signatureHeader = { 0x1, 0x0, 0x1, 0x0, 0x1, 0x0, 0x0, 0x0 };

        public RdpFile() : this(false)
        {
        }

        public RdpFile(RdpFile rdpFile, bool createFile = true)
        {
            AdministrativeSession = rdpFile.AdministrativeSession;
            AllowDesktopComposition = rdpFile.AllowDesktopComposition;
            AllowFontSmoothing = rdpFile.AllowFontSmoothing;
            AlternateFullAddress = rdpFile.AlternateFullAddress;
            AlternateShell = rdpFile.AlternateShell;
            Audiocapturemode = rdpFile.Audiocapturemode;
            Audiomode = rdpFile.Audiomode;
            Audioqualitymode = rdpFile.Audioqualitymode;
            AuthenticationLevel = rdpFile.AuthenticationLevel;
            AutoreconnectionEnabled = rdpFile.AutoreconnectionEnabled;
            AutoreconnectMaxRetries = rdpFile.AutoreconnectMaxRetries;
            Bandwidthautodetect = rdpFile.Bandwidthautodetect;
            Bitmapcachepersistenable = rdpFile.Bitmapcachepersistenable;
            Bitmapcachesize = rdpFile.Bitmapcachesize;
            Camerastoredirect = rdpFile.Camerastoredirect;
            Compression = rdpFile.Compression;
            ConnectionType = rdpFile.ConnectionType;
            ConnectToConsole = rdpFile.ConnectToConsole;
            Desktopheight = rdpFile.Desktopheight;
            DesktopSizeId = rdpFile.DesktopSizeId;
            Desktopwidth = rdpFile.Desktopwidth;
            Devicestoredirect = rdpFile.Devicestoredirect;
            Disableconnectionsharing = rdpFile.Disableconnectionsharing;
            DisableCtrlAltDel = rdpFile.DisableCtrlAltDel;
            DisableFullWindowDrag = rdpFile.DisableFullWindowDrag;
            DisableMenuAnims = rdpFile.DisableMenuAnims;
            Disableremoteappcapscheck = rdpFile.Disableremoteappcapscheck;
            DisableThemes = rdpFile.DisableThemes;
            DisableWallpaper = rdpFile.DisableWallpaper;
            Displayconnectionbar = rdpFile.Displayconnectionbar;
            Domain = rdpFile.Domain;
            Drivestoredirect = rdpFile.Drivestoredirect;
            Enablecredsspsupport = rdpFile.Enablecredsspsupport;
            Enablesuperpan = rdpFile.Enablesuperpan;
            EncodeRedirectedVideoCapture = rdpFile.EncodeRedirectedVideoCapture;
            FullAddress = rdpFile.FullAddress;
            Gatewaycredentialssource = rdpFile.Gatewaycredentialssource;
            Gatewayhostname = rdpFile.Gatewayhostname;
            Gatewayprofileusagemethod = rdpFile.Gatewayprofileusagemethod;
            Gatewayusagemethod = rdpFile.Gatewayusagemethod;
            Keyboardhook = rdpFile.Keyboardhook;
            NegotiateSecurityLayer = rdpFile.NegotiateSecurityLayer;
            Networkautodetect = rdpFile.Networkautodetect;
            Password51 = rdpFile.Password51;
            Pinconnectionbar = rdpFile.Pinconnectionbar;
            Promptcredentialonce = rdpFile.Promptcredentialonce;
            PromptForCredentials = rdpFile.PromptForCredentials;
            PromptForCredentialsOnClient = rdpFile.PromptForCredentialsOnClient;
            PublicMode = rdpFile.PublicMode;
            Redirectclipboard = rdpFile.Redirectclipboard;
            Redirectcomports = rdpFile.Redirectcomports;
            Redirectdirectx = rdpFile.Redirectdirectx;
            Redirectdrives = rdpFile.Redirectdrives;
            RedirectedVideoCaptureEncodingQuality = rdpFile.RedirectedVideoCaptureEncodingQuality;
            Redirectlocation = rdpFile.Redirectlocation;
            Redirectposdevices = rdpFile.Redirectposdevices;
            Redirectprinters = rdpFile.Redirectprinters;
            Redirectsmartcards = rdpFile.Redirectsmartcards;
            Redirectwebauthn = rdpFile.Redirectwebauthn;
            Remoteapplicationcmdline = rdpFile.Remoteapplicationcmdline;
            Remoteapplicationexpandcmdline = rdpFile.Remoteapplicationexpandcmdline;
            Remoteapplicationexpandworkingdir = rdpFile.Remoteapplicationexpandworkingdir;
            Remoteapplicationfile = rdpFile.Remoteapplicationfile;
            Remoteapplicationfileextensions = rdpFile.Remoteapplicationfileextensions;
            Remoteapplicationguid = rdpFile.Remoteapplicationguid;
            Remoteapplicationicon = rdpFile.Remoteapplicationicon;
            Remoteapplicationmode = rdpFile.Remoteapplicationmode;
            Remoteapplicationname = rdpFile.Remoteapplicationname;
            Remoteapplicationprogram = rdpFile.Remoteapplicationprogram;
            ScreenModeId = rdpFile.ScreenModeId;
            Selectedmonitors = rdpFile.Selectedmonitors;
            ServerPort = rdpFile.ServerPort;
            SessionBpp = rdpFile.SessionBpp;
            ShellWorkingDirectory = rdpFile.ShellWorkingDirectory;
            Signature = rdpFile.Signature;
            Signscope = rdpFile.Signscope;
            SmartSizing = rdpFile.SmartSizing;
            SpanMonitors = rdpFile.SpanMonitors;
            Superpanaccelerationfactor = rdpFile.Superpanaccelerationfactor;
            Usbdevicestoredirect = rdpFile.Usbdevicestoredirect;
            UseMultimon = rdpFile.UseMultimon;
            Username = rdpFile.Username;
            Videoplaybackmode = rdpFile.Videoplaybackmode;
            Winposstr = rdpFile.Winposstr;
            Workspaceid = rdpFile.Workspaceid;
            Maximizetocurrentdisplays = rdpFile.Maximizetocurrentdisplays;
            Singlemoninwindowedmode = rdpFile.Singlemoninwindowedmode;
            DynamicResolution = rdpFile.DynamicResolution;
            Desktopscalefactor = rdpFile.Desktopscalefactor;
            DisableCursorSetting = rdpFile.DisableCursorSetting;
            Enableworkspacereconnect = rdpFile.Enableworkspacereconnect;
            Gatewaybrokeringtype = rdpFile.Gatewaybrokeringtype;
            UseRedirectionServerName = rdpFile.UseRedirectionServerName;
            Loadbalanceinfo = rdpFile.Loadbalanceinfo;
            Rdgiskdcproxy = rdpFile.Rdgiskdcproxy;
            Kdcproxyname = rdpFile.Kdcproxyname;
            Pcb = rdpFile.Pcb;
            SupportUrl = rdpFile.SupportUrl;
            RequirePreAuthentication = rdpFile.RequirePreAuthentication;
            PreAuthenticationServerAddress = rdpFile.PreAuthenticationServerAddress;
            Eventloguploadaddress = rdpFile.Eventloguploadaddress;

            if (createFile)
                new FileInfo(_path = Path.GetTempFileName()).Attributes = FileAttributes.Temporary;
            else
                _path = string.Empty;
        }

        public RdpFile(bool createFile)
        {
            Reset();

            if (createFile)
                new FileInfo(_path = Path.GetTempFileName()).Attributes = FileAttributes.Temporary;
            else
                _path = string.Empty;
        }

        public void Reset()
        {
            foreach (var prop in _settingsCache.Value.Values.SelectMany(x => x))
                SetProperty(prop);
        }

        public void Sign(X509Certificate2 certificate)
        {
            if (!certificate.HasPrivateKey)
                throw new ArgumentOutOfRangeException("certificate");

            if (certificate.SignatureAlgorithm.Value != CryptoConfig.MapNameToOID(RdpSignatureAlgorithmName))
                throw new CryptographicException("certificate");

            if (!string.IsNullOrWhiteSpace(FullAddress) && string.IsNullOrWhiteSpace(AlternateFullAddress))
                AlternateFullAddress = FullAddress;

            var signscope = string.Join(",", _signscopeCache.Value.Select(x => x.Item2.Scope));

            var cms = new SignedCms(
                new ContentInfo(
                    Encoding.Unicode.GetBytes(
                        string.Format(
                            "{0}{1}signscope:s:{2}{3}{4}",
                            string.Join(
                                Environment.NewLine,
                                _signscopeCache.Value.Select(x => TransformProperty(new Tuple<PropertyInfo, RdpSettingAttribute>(x.Item1, x.Item3)))),
                            Environment.NewLine,
                            signscope,
                            Environment.NewLine,
                            char.MinValue))), true);

            cms.ComputeSignature(new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificate)
            {
                IncludeOption = X509IncludeOption.WholeChain,
                DigestAlgorithm = new Oid(CryptoConfig.MapNameToOID(RdpSignatureDigestAlgorithmName), RdpSignatureDigestAlgorithmName)
            }, true);

            var pkcs7 = cms.Encode();
            var signatureBase64 = Convert.ToBase64String(_signatureHeader.Concat(BitConverter.GetBytes(Convert.ToUInt32(pkcs7.Length))).Concat(pkcs7).ToArray());

            var signatureBuilder = new StringBuilder();
            for (var i = 0; i < signatureBase64.Length; i += 64)
            {
                if (i + 64 > signatureBase64.Length)
                    signatureBuilder.Append(signatureBase64.Substring(i));
                else
                    signatureBuilder.Append(signatureBase64.Substring(i, 64));

                signatureBuilder.Append("  ");
            }

            Signature = signatureBuilder.ToString();
            Signscope = signscope;
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(_path) && File.Exists(_path))
                File.Delete(_path);
        }

        internal void SetProperty(Tuple<PropertyInfo, RdpSettingAttribute> tuple, string[] template = null)
        {
            var property = tuple.Item1;
            var attribute = tuple.Item2;

            if (template != null && template.Length < 3)
            {
                var attrTemplate = attribute.Template.Split(new[] { ':' }, 3);
                if (attrTemplate.Length > 2)
                    template = template.Concat(new[] { attrTemplate[2] }).ToArray();
            }
            template = template ?? attribute.Template.Split(new[] { ':' }, 3);
            var type = template[1];

            object defaultValue = null;
            switch (type)
            {
                case "s":
                    defaultValue = property.PropertyType.IsEnum ?
                        template.Length > 2 ?
                            !string.IsNullOrEmpty(template[2]) ?
                                Enum.Parse(property.PropertyType, template[2]) :
                                Enum.ToObject(property.PropertyType, 0) :
                            string.Empty :
                        template.Length > 2 && !string.IsNullOrEmpty(template[2]) ? template[2] : string.Empty;
                    break;
                case "i":
                    defaultValue = property.PropertyType.IsEnum ?
                        template.Length > 2 ?
                            Enum.ToObject(property.PropertyType, !string.IsNullOrEmpty(template[2]) ? int.Parse(template[2]) : 0) :
                            Enum.ToObject(property.PropertyType, 0) :
                        template.Length > 2 && !string.IsNullOrEmpty(template[2]) ? int.Parse(template[2]) : 0;
                    break;
            }

            if (defaultValue != null)
                property.SetValue(this, Convert.ChangeType(defaultValue, property.PropertyType), null);
            else
                property.SetValue(this, null, null);
        }

        internal string TransformProperty(Tuple<PropertyInfo, RdpSettingAttribute> tuple)
        {
            var property = tuple.Item1;
            var attribute = tuple.Item2;

            var template = attribute.Template.Split(new[] { ':' }, 3);
            var key = template[0];
            var type = template[1];

            object defaultValue = null;
            var targetType = typeof(string);
            switch (type)
            {
                case "s":
                    defaultValue = property.PropertyType.IsEnum ?
                        template.Length > 2 ?
                            !string.IsNullOrEmpty(template[2]) ?
                                Enum.Parse(property.PropertyType, template[2]) :
                                Enum.ToObject(property.PropertyType, 0) :
                            string.Empty :
                        template.Length > 2 && !string.IsNullOrEmpty(template[2]) ? template[2] : string.Empty;
                    break;
                case "i":
                    defaultValue = property.PropertyType.IsEnum ?
                        template.Length > 2 ?
                            Enum.ToObject(property.PropertyType, !string.IsNullOrEmpty(template[2]) ? int.Parse(template[2]) : 0) :
                            Enum.ToObject(property.PropertyType, 0) :
                        template.Length > 2 && !string.IsNullOrEmpty(template[2]) ? int.Parse(template[2]) : 0;
                    targetType = typeof(int);
                    break;
            }

            var value = property.GetValue(this, null) ?? defaultValue;
            return value == null ? string.Empty : string.Format("{0}:{1}:{2}", key, type, Convert.ChangeType(value, targetType));
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(_path) && File.Exists(_path))
            {
                using (var fs = new FileStream(_path, FileMode.Truncate, FileAccess.Write, FileShare.Read, 4096))
                using (var sw = new StreamWriter(fs, Encoding.Unicode, 4096))
                {
                    var signscope = string.Empty;
                    var signature = string.Empty;
                    foreach (var line in _settingsCache.Value.Values.SelectMany(x => x).Select(TransformProperty).Where(x => !string.IsNullOrEmpty(x)))
                    {
                        if (line.StartsWith("signscope:s:", StringComparison.OrdinalIgnoreCase))
                        {
                            signscope = line.Substring(12);
                            continue;
                        }
                        if (line.StartsWith("signature:s:", StringComparison.OrdinalIgnoreCase))
                        {
                            signature = line.Substring(12);
                            continue;
                        }
                        sw.WriteLine(line);
                    }
                    if (!string.IsNullOrWhiteSpace(signscope))
                        sw.WriteLine(string.Format("signscope:s:{0}", signscope));
                    if (!string.IsNullOrWhiteSpace(signature))
                        sw.WriteLine(string.Format("signature:s:{0}", signature));
                }
            }

            return _path;
        }

        public static implicit operator string(RdpFile rdpFile)
        {
            return rdpFile.ToString();
        }
    }
}
