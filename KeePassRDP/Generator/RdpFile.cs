/*
 *  Copyright (C) 2018 - 2024 iSnackyCracky, NETertainer
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
using System.IO;
using System.Linq;
using System.Reflection;
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

        private static readonly Lazy<Tuple<PropertyInfo, RdpSettingAttribute>[]> _attributeCache = new Lazy<Tuple<PropertyInfo, RdpSettingAttribute>[]>(
            () => typeof(RdpFile)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(x => new Tuple<PropertyInfo, RdpSettingAttribute>(x, x.GetCustomAttributes(false).OfType<RdpSettingAttribute>().FirstOrDefault()))
                .Where(x => x.Item2 != null)
                .OrderBy(x => x.Item2.Template.Substring(0, Math.Max(0, x.Item2.Template.IndexOf(':'))), StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        [JsonIgnore]
        public static IEnumerable<Tuple<PropertyInfo, RdpSettingAttribute>> AttributeCache { get { return _attributeCache.Value.AsReadOnly(); } }

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

            if (createFile)
                new FileInfo(_path = Path.GetTempFileName()).Attributes = FileAttributes.Temporary;
            else
                _path = string.Empty;
        }

        public RdpFile(bool createFile)
        {
            foreach (var prop in AttributeCache)
            {
                var template = prop.Item2.Template.Split(new[] { ':' }, 3);
                var type = template[1];

                var propValue = prop.Item1.GetValue(this, null);
                object defaultValue = null;
                switch (type)
                {
                    case "s":
                        defaultValue = prop.Item1.PropertyType.IsEnum ?
                            Enum.Parse(prop.Item1.PropertyType, template.Length > 2 ? template[2] : string.Empty) :
                            template.Length > 2 ? template[2] : string.Empty;
                        break;
                    case "i":
                        defaultValue = prop.Item1.PropertyType.IsEnum ?
                            Enum.ToObject(prop.Item1.PropertyType, template.Length > 2 ? int.Parse(template[2]) : 0) :
                            template.Length > 2 ? int.Parse(template[2]) : 0;
                        break;
                }

                if (defaultValue != null)
                    prop.Item1.SetValue(this, Convert.ChangeType(defaultValue, prop.Item1.PropertyType), null);
            }

            if (createFile)
                new FileInfo(_path = Path.GetTempFileName()).Attributes = FileAttributes.Temporary;
            else
                _path = string.Empty;
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(_path) && File.Exists(_path))
                File.Delete(_path);
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(_path) && File.Exists(_path))
            {
                using (var fs = new FileStream(_path, FileMode.Truncate, FileAccess.Write, FileShare.Read, 4096))
                    using (var sw = new StreamWriter(fs, Encoding.UTF8, 4096))
                    foreach (var prop in AttributeCache)
                    {
                        var template = prop.Item2.Template.Split(new[] { ':' }, 3);
                        var key = template[0];
                        var type = template[1];

                        object defaultValue = null;
                        var targetType = typeof(string);
                        switch (type)
                        {
                            case "s":
                                defaultValue = template.Length > 2 ? template[2] : string.Empty;
                                break;
                            case "i":
                                defaultValue = template.Length > 2 ? int.Parse(template[2]) : 0;
                                targetType = typeof(int);
                                break;
                        }

                        var value = prop.Item1.GetValue(this, null) ?? defaultValue;
                        if (value != null)
                            sw.WriteLine(string.Format("{0}:{1}:{2}", key, type, Convert.ChangeType(value, targetType)));
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
