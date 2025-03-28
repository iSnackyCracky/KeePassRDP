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

using System.ComponentModel;

namespace KeePassRDP.Generator
{
    public partial class RdpFile
    {
        private const string EmptyString = "";

        public enum KeyboardHooks
        {
            Local = 0,
            Remote = 1,
            FullScreen = 2,
            RemoteApp = 3
        }

        public enum AudioModes
        {
            Local = 0,
            Remote = 1,
            Mute = 2
        }

        public enum AudioCaptureModes
        {
            DoNotCapture = 0,
            SendToRemoteComputer = 1
        }

        public enum WindowStates : int
        {
            Normal = 1,
            Maximized = 3
        }

        public enum SessionBpps
        {
            Bpp_8 = 8,
            Bpp_15 = 15,
            Bpp_16 = 16,
            Bpp_24 = 24,
            Bpp_32 = 32
        }

        public enum DesktopSizes
        {
            Size_640_480 = 0,
            Size_800_600 = 1,
            Size_1024_768 = 2,
            Size_1280_1024 = 3,
            Size_1600x1200 = 4
        }

        public enum ScreenModes
        {
            Window = 1,
            FullScreen = 2
        }

        public enum ConnectionTypes
        {
            Modem = 1,
            LowSpeed = 2,
            Satellite = 3,
            HighSpeed = 4,
            WAN = 5,
            LAN = 6,
            Automatic = 7
        }

        public enum AuthenticationLevels
        {
            NoWarning = 0,
            DoNotConnect = 1,
            ShowWarning = 2,
            NotRequired = 3
        }

        public enum VideoCaptureEncodingQualities
        {
            High = 0,
            Medium = 1,
            Low = 2
        }

        public enum AudioQualityModes
        {
            Dynamic = 0,
            Medmium = 1,
            Uncompressed = 2
        }

        public enum GatewayProfileUsageMethods
        {
            Default = 0,
            Explicit = 1
        }

        public enum GatewayUsageMethods
        {
            DoNotUseGateway = 0,
            AlwaysUseGateway = 1,
            UseGateway = 2,
            UseDefaultGateway = 3,
            DoNotUseGatewayNoLocal = 4
        }

        public enum GatewayCredentialSources
        {
            AskForPassword = 0,
            UseSmartCard = 1,
            UseCurrentUser = 2,
            PromptUser = 3,
            SelectLater = 4,
            CookieBased = 5
        }

        public enum RemoteApplicationModes
        {
            Normal = 0,
            Launch = 1
        }

        public enum VideoPlaybackModes
        {
            Disabled = 0,
            Efficient = 1
        }

        public enum DesktopScaleFactors
        {
            Default = 0,
            Scale_100 = 100,
            Scale_125 = 125,
            Scale_150 = 150,
            Scale_175 = 175,
            Scale_200 = 200,
            Scale_250 = 250,
            Scale_300 = 300,
            Scale_400 = 400,
            Scale_500 = 500,
        }

        public struct RECT
        {
            public int Top;
            public int Left;
            public int Width;
            public int Height;
        }

        public struct WindowsPosition
        {
            public WindowStates WinState;
            public RECT Rect;
        }

        // https://learn.microsoft.com/en-us/windows-server/remote/remote-desktop-services/clients/rdp-files

        [RdpSetting("administrative session:i:0")]
        public bool AdministrativeSession { get; set; }

        [RdpSetting("allow desktop composition:i:0", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool AllowDesktopComposition { get; set; }

        [RdpSetting("allow font smoothing:i:0", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool AllowFontSmoothing { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("alternate full address:s:")]
        [RdpSignscope("Alternate Full Address")]
        public string AlternateFullAddress { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("alternate shell:s:")]
        [RdpSignscope("Alternate Shell")]
        public string AlternateShell { get; set; }

        [DefaultValue(AudioCaptureModes.DoNotCapture)]
        [RdpSetting("audiocapturemode:i:0", Category = RdpSettingAttribute.SettingCategory.AudioVideo)]
        public AudioCaptureModes Audiocapturemode { get; set; }

        [DefaultValue(AudioModes.Local)]
        [RdpSetting("audiomode:i:0", Category = RdpSettingAttribute.SettingCategory.AudioVideo)]
        [RdpSignscope("AudioMode")]
        public AudioModes Audiomode { get; set; }

        [DefaultValue(AudioQualityModes.Dynamic)]
        [RdpSetting("audioqualitymode:i:0", Category = RdpSettingAttribute.SettingCategory.AudioVideo)]
        public AudioQualityModes Audioqualitymode { get; set; }

        [DefaultValue(AuthenticationLevels.NotRequired)]
        [RdpSetting("authentication level:i:3", Category = RdpSettingAttribute.SettingCategory.Authentication)]
        [RdpSignscope("Authentication Level")]
        public AuthenticationLevels AuthenticationLevel { get; set; }

        [DefaultValue(true)]
        [RdpSetting("autoreconnection enabled:i:1", Category = RdpSettingAttribute.SettingCategory.Network)]
        [RdpSignscope("AutoReconnection Enabled")]
        public bool AutoreconnectionEnabled { get; set; }

        [DefaultValue(5)]
        [RdpSetting("autoreconnect max retries:i:5", Category = RdpSettingAttribute.SettingCategory.Network)]
        public int AutoreconnectMaxRetries { get; set; }

        [DefaultValue(true)]
        [RdpSetting("bandwidthautodetect:i:1", Category = RdpSettingAttribute.SettingCategory.Network)]
        public bool Bandwidthautodetect { get; set; }

        [DefaultValue(true)]
        [RdpSetting("bitmapcachepersistenable:i:1", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool Bitmapcachepersistenable { get; set; }

        [DefaultValue(1500)]
        [RdpSetting("bitmapcachesize:i:1500", Category = RdpSettingAttribute.SettingCategory.Display)]
        public int Bitmapcachesize { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("camerastoredirect:s:", Category = RdpSettingAttribute.SettingCategory.Redirect)]
        public string Camerastoredirect { get; set; }

        [DefaultValue(true)]
        [RdpSetting("compression:i:1", Category = RdpSettingAttribute.SettingCategory.Network)]
        public bool Compression { get; set; }

        [DefaultValue(ConnectionTypes.LowSpeed)]
        [RdpSetting("connection type:i:2", Category = RdpSettingAttribute.SettingCategory.Network)]
        public ConnectionTypes ConnectionType { get; set; }

        [RdpSetting("connect to console:i:0")]
        public bool ConnectToConsole { get; set; }

        [DefaultValue(600)]
        [RdpSetting("desktopheight:i:600", Category = RdpSettingAttribute.SettingCategory.Display)]
        public int Desktopheight { get; set; }

        [DefaultValue(DesktopSizes.Size_640_480)]
        [RdpSetting("desktop size id:i:0", Category = RdpSettingAttribute.SettingCategory.Display)]
        public DesktopSizes DesktopSizeId { get; set; }

        [DefaultValue(800)]
        [RdpSetting("desktopwidth:i:800", Category = RdpSettingAttribute.SettingCategory.Display)]
        public int Desktopwidth { get; set; }

        [DefaultValue("*")]
        [RdpSetting("devicestoredirect:s:*", Category = RdpSettingAttribute.SettingCategory.Redirect)]
        [RdpSignscope("DevicesToRedirect")]
        public string Devicestoredirect { get; set; }

        [RdpSetting("disableconnectionsharing:i:0", Category = RdpSettingAttribute.SettingCategory.Network)]
        [RdpSignscope("DisableConnectionSharing")]
        public bool Disableconnectionsharing { get; set; }

        [DefaultValue(true)]
        [RdpSetting("disable ctrl+alt+del:i:1")]
        public bool DisableCtrlAltDel { get; set; }

        [DefaultValue(true)]
        [RdpSetting("disable full window drag:i:1", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool DisableFullWindowDrag { get; set; }

        [DefaultValue(true)]
        [RdpSetting("disable menu anims:i:1", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool DisableMenuAnims { get; set; }

        [RdpSetting("disableremoteappcapscheck:i:0", Category = RdpSettingAttribute.SettingCategory.RemoteApp)]
        public bool Disableremoteappcapscheck { get; set; }

        [RdpSetting("disable themes:i:0", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool DisableThemes { get; set; }

        [DefaultValue(true)]
        [RdpSetting("disable wallpaper:i:1", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool DisableWallpaper { get; set; }

        [DefaultValue(true)]
        [RdpSetting("displayconnectionbar:i:1", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool Displayconnectionbar { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("domain:s:")]
        public string Domain { get; set; }

        [DefaultValue("*")]
        [RdpSetting("drivestoredirect:s:*", Category = RdpSettingAttribute.SettingCategory.Redirect)]
        [RdpSignscope("DrivesToRedirect")]
        public string Drivestoredirect { get; set; }

        [DefaultValue(true)]
        [RdpSetting("enablecredsspsupport:i:1", Category = RdpSettingAttribute.SettingCategory.Authentication)]
        [RdpSignscope("EnableCredSspSupport")]
        public bool Enablecredsspsupport { get; set; }

        [RdpSetting("enablesuperpan:i:0", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool Enablesuperpan { get; set; }

        [DefaultValue(true)]
        [RdpSetting("encode redirected video capture:i:1", Category = RdpSettingAttribute.SettingCategory.AudioVideo)]
        public bool EncodeRedirectedVideoCapture { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("full address:s:")]
        [RdpSignscope("Full Address")]
        public string FullAddress { get; set; }

        [DefaultValue(GatewayCredentialSources.AskForPassword)]
        [RdpSetting("gatewaycredentialssource:i:0", Category = RdpSettingAttribute.SettingCategory.Network)]
        [RdpSignscope("GatewayCredentialsSource")]
        public GatewayCredentialSources Gatewaycredentialssource { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("gatewayhostname:s:", Category = RdpSettingAttribute.SettingCategory.Network)]
        [RdpSignscope("GatewayHostname")]
        public string Gatewayhostname { get; set; }

        [DefaultValue(GatewayProfileUsageMethods.Default)]
        [RdpSetting("gatewayprofileusagemethod:i:0", Category = RdpSettingAttribute.SettingCategory.Network)]
        [RdpSignscope("GatewayProfileUsageMethod")]
        public GatewayProfileUsageMethods Gatewayprofileusagemethod { get; set; }

        [DefaultValue(GatewayUsageMethods.DoNotUseGateway)]
        [RdpSetting("gatewayusagemethod:i:0", Category = RdpSettingAttribute.SettingCategory.Network)]
        [RdpSignscope("GatewayUsageMethod")]
        public GatewayUsageMethods Gatewayusagemethod { get; set; }

        [DefaultValue(KeyboardHooks.FullScreen)]
        [RdpSetting("keyboardhook:i:2")]
        public KeyboardHooks Keyboardhook { get; set; }

        [DefaultValue(true)]
        [RdpSetting("negotiate security layer:i:1", Category = RdpSettingAttribute.SettingCategory.Authentication)]
        [RdpSignscope("Negotiate Security Layer")]
        public bool NegotiateSecurityLayer { get; set; }

        [DefaultValue(true)]
        [RdpSetting("networkautodetect:i:1", Category = RdpSettingAttribute.SettingCategory.Network)]
        public bool Networkautodetect { get; set; }

        [DefaultValue(null)]
        [RdpSetting("password 51:b:")]
        public byte[] Password51 { get; set; }

        [DefaultValue(true)]
        [RdpSetting("pinconnectionbar:i:1", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool Pinconnectionbar { get; set; }

        [DefaultValue(true)]
        [RdpSetting("promptcredentialonce:i:1", Category = RdpSettingAttribute.SettingCategory.Authentication)]
        [RdpSignscope("PromptCredentialOnce")]
        public bool Promptcredentialonce { get; set; }

        [RdpSetting("prompt for credentials:i:0", Category = RdpSettingAttribute.SettingCategory.Authentication)]
        [RdpSignscope("Prompt For Credentials")]
        public bool PromptForCredentials { get; set; }

        [RdpSetting("prompt for credentials on client:i:0", Category = RdpSettingAttribute.SettingCategory.Authentication)]
        public bool PromptForCredentialsOnClient { get; set; }

        [RdpSetting("public mode:i:0", Category = RdpSettingAttribute.SettingCategory.Authentication)]
        public bool PublicMode { get; set; }

        [DefaultValue(true)]
        [RdpSetting("redirectclipboard:i:1", Category = RdpSettingAttribute.SettingCategory.Redirect)]
        [RdpSignscope("RedirectClipboard")]
        public bool Redirectclipboard { get; set; }

        [DefaultValue(true)]
        [RdpSetting("redirectcomports:i:1", Category = RdpSettingAttribute.SettingCategory.Redirect)]
        [RdpSignscope("RedirectCOMPorts")]
        public bool Redirectcomports { get; set; }

        [DefaultValue(true)]
        [RdpSetting("redirectdirectx:i:1", Category = RdpSettingAttribute.SettingCategory.Redirect)]
        [RdpSignscope("RedirectDirectX")]
        public bool Redirectdirectx { get; set; }

        [RdpSetting("redirectdrives:i:0", Category = RdpSettingAttribute.SettingCategory.Redirect)]
        [RdpSignscope("RedirectDrives")]
        public bool Redirectdrives { get; set; }

        [DefaultValue(VideoCaptureEncodingQualities.High)]
        [RdpSetting("redirected video capture encoding quality:i:0", Category = RdpSettingAttribute.SettingCategory.AudioVideo)]
        public VideoCaptureEncodingQualities RedirectedVideoCaptureEncodingQuality { get; set; }

        [RdpSetting("redirectlocation:i:0", Category = RdpSettingAttribute.SettingCategory.Redirect)]
        public bool Redirectlocation { get; set; }

        [RdpSetting("redirectposdevices:i:0", Category = RdpSettingAttribute.SettingCategory.Redirect)]
        [RdpSignscope("RedirectPOSDevices")]
        public bool Redirectposdevices { get; set; }

        [DefaultValue(true)]
        [RdpSetting("redirectprinters:i:1", Category = RdpSettingAttribute.SettingCategory.Redirect)]
        [RdpSignscope("RedirectPrinters")]
        public bool Redirectprinters { get; set; }

        [DefaultValue(true)]
        [RdpSetting("redirectsmartcards:i:1", Category = RdpSettingAttribute.SettingCategory.Redirect)]
        [RdpSignscope("RedirectSmartCards")]
        public bool Redirectsmartcards { get; set; }

        [DefaultValue(true)]
        [RdpSetting("redirectwebauthn:i:1", Category = RdpSettingAttribute.SettingCategory.Redirect)]
        [RdpSignscope("RedirectWebAuthn")]
        public bool Redirectwebauthn { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("remoteapplicationcmdline:s:", Category = RdpSettingAttribute.SettingCategory.RemoteApp)]
        [RdpSignscope("RemoteApplicationCmdLine")]
        public string Remoteapplicationcmdline { get; set; }

        [DefaultValue(true)]
        [RdpSetting("remoteapplicationexpandcmdline:i:1", Category = RdpSettingAttribute.SettingCategory.RemoteApp)]
        [RdpSignscope("RemoteApplicationExpandCmdLine")]
        public bool Remoteapplicationexpandcmdline { get; set; }

        [DefaultValue(true)]
        [RdpSetting("remoteapplicationexpandworkingdir:i:1", Category = RdpSettingAttribute.SettingCategory.RemoteApp)]
        [RdpSignscope("RemoteApplicationExpandWorkingdir")]
        public bool Remoteapplicationexpandworkingdir { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("remoteapplicationfile:s:", Category = RdpSettingAttribute.SettingCategory.RemoteApp)]
        [RdpSignscope("RemoteApplicationFile")]
        public string Remoteapplicationfile { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("remoteapplicationfileextensions:s:", Category = RdpSettingAttribute.SettingCategory.RemoteApp)]
        [RdpSignscope("RemoteApplicationFileExtensions")]
        public string Remoteapplicationfileextensions { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("remoteapplicationguid:s:", Category = RdpSettingAttribute.SettingCategory.RemoteApp)]
        [RdpSignscope("RemoteApplicationGuid")]
        public string Remoteapplicationguid { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("remoteapplicationicon:s:", Category = RdpSettingAttribute.SettingCategory.RemoteApp)]
        [RdpSignscope("RemoteApplicationIcon")]
        public string Remoteapplicationicon { get; set; }

        [DefaultValue(RemoteApplicationModes.Normal)]
        [RdpSetting("remoteapplicationmode:i:0", Category = RdpSettingAttribute.SettingCategory.RemoteApp)]
        [RdpSignscope("RemoteApplicationMode")]
        public RemoteApplicationModes Remoteapplicationmode { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("remoteapplicationname:s:", Category = RdpSettingAttribute.SettingCategory.RemoteApp)]
        [RdpSignscope("RemoteApplicationName")]
        public string Remoteapplicationname { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("remoteapplicationprogram:s:", Category = RdpSettingAttribute.SettingCategory.RemoteApp)]
        [RdpSignscope("RemoteApplicationProgram")]
        public string Remoteapplicationprogram { get; set; }

        [DefaultValue(ScreenModes.FullScreen)]
        [RdpSetting("screen mode id:i:2", Category = RdpSettingAttribute.SettingCategory.Display)]
        public ScreenModes ScreenModeId { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("selectedmonitors:s:", Category = RdpSettingAttribute.SettingCategory.Display)]
        public string Selectedmonitors { get; set; }

        [DefaultValue(3389)]
        [RdpSetting("server port:i:3389")]
        [RdpSignscope("Server Port")]
        public int ServerPort { get; set; }

        [DefaultValue(SessionBpps.Bpp_32)]
        [RdpSetting("session bpp:i:32", Category = RdpSettingAttribute.SettingCategory.Display)]
        public SessionBpps SessionBpp { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("shell working directory:s:")]
        [RdpSignscope("Shell Working Directory")]
        public string ShellWorkingDirectory { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("signature:s:")]
        public string Signature { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("signscope:s:")]
        public string Signscope { get; set; }

        [RdpSetting("smart sizing:i:0", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool SmartSizing { get; set; }

        [RdpSetting("span monitors:i:0", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool SpanMonitors { get; set; }

        [DefaultValue(1)]
        [RdpSetting("superpanaccelerationfactor:i:1", Category = RdpSettingAttribute.SettingCategory.Display)]
        public int Superpanaccelerationfactor { get; set; }

        [DefaultValue("*")]
        [RdpSetting("usbdevicestoredirect:s:*", Category = RdpSettingAttribute.SettingCategory.Redirect)]
        public string Usbdevicestoredirect { get; set; }

        [DefaultValue(true)]
        [RdpSetting("use multimon:i:1", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool UseMultimon { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("username:s:")]
        public string Username { get; set; }

        [DefaultValue(VideoPlaybackModes.Efficient)]
        [RdpSetting("videoplaybackmode:i:1", Category = RdpSettingAttribute.SettingCategory.AudioVideo)]
        public VideoPlaybackModes Videoplaybackmode { get; set; }

        [DefaultValue("0,3,0,0,800,600")]
        [RdpSetting("winposstr:s:0,3,0,0,800,600", Category = RdpSettingAttribute.SettingCategory.Display)]
        public string Winposstr { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("workspaceid:s:")]
        public string Workspaceid { get; set; }

        [RdpSetting("maximizetocurrentdisplays:i:0", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool Maximizetocurrentdisplays { get; set; }

        [RdpSetting("singlemoninwindowedmode:i:0", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool Singlemoninwindowedmode { get; set; }

        [RdpSetting("dynamic resolution:i:0", Category = RdpSettingAttribute.SettingCategory.Display)]
        public bool DynamicResolution { get; set; }

        [DefaultValue(DesktopScaleFactors.Default)]
        [RdpSetting("desktopscalefactor:i:0", Category = RdpSettingAttribute.SettingCategory.Display)]
        public DesktopScaleFactors Desktopscalefactor { get; set; }

        [RdpSetting("disable cursor setting:i:0")]
        public bool DisableCursorSetting { get; set; }

        [RdpSetting("enableworkspacereconnect:i:0")]
        public bool Enableworkspacereconnect { get; set; }

        [RdpSetting("gatewaybrokeringtype:i:0", Category = RdpSettingAttribute.SettingCategory.Network)]
        public int Gatewaybrokeringtype { get; set; }

        [RdpSetting("use redirection server name:i:0", Category = RdpSettingAttribute.SettingCategory.Network)]
        [RdpSignscope("Use Redirection Server Name")]
        public bool UseRedirectionServerName { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("loadbalanceinfo:s:", Category = RdpSettingAttribute.SettingCategory.Network)]
        [RdpSignscope("LoadBalanceInfo")]
        public string Loadbalanceinfo { get; set; }

        [RdpSetting("rdgiskdcproxy:i:0", Category = RdpSettingAttribute.SettingCategory.Network)]
        [RdpSignscope("RDGIsKDCProxy")]
        public bool Rdgiskdcproxy { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("kdcproxyname:s:", Category = RdpSettingAttribute.SettingCategory.Network)]
        [RdpSignscope("KDCProxyName")]
        public string Kdcproxyname { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("pcb:s:")]
        [RdpSignscope("PCB")]
        public string Pcb { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("support url:s:")]
        [RdpSignscope("Support URL")]
        public string SupportUrl { get; set; }

        [RdpSetting("require pre-authentication:i:0", Category = RdpSettingAttribute.SettingCategory.Authentication)]
        [RdpSignscope("Require pre-authentication")]
        public bool RequirePreAuthentication { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("pre-authentication server address:s:", Category = RdpSettingAttribute.SettingCategory.Authentication)]
        [RdpSignscope("Pre-authentication server address")]
        public string PreAuthenticationServerAddress { get; set; }

        [DefaultValue(EmptyString)]
        [RdpSetting("eventloguploadaddress:s:")]
        [RdpSignscope("EventLogUploadAddress")]
        public string Eventloguploadaddress { get; set; }
    }
}
