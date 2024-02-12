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

using System.ComponentModel;

namespace KeePassRDP.Generator
{
    public partial class RdpFile
    {
        public enum KeyboardHooks
        {
            Local = 0,
            Remote = 1,
            FullScreen = 2
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
            SelectLater = 4
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

        [RdpSetting(Template = "administrative session:i:0")]
        public bool AdministrativeSession { get; set; }

        [RdpSetting(Template = "allow desktop composition:i:0")]
        public bool AllowDesktopComposition { get; set; }

        [RdpSetting(Template = "allow font smoothing:i:0")]
        public bool AllowFontSmoothing { get; set; }

        [RdpSetting(Template = "alternate full address:s:")]
        public string AlternateFullAddress { get; set; }

        [RdpSetting(Template = "alternate shell:s:")]
        public string AlternateShell { get; set; }

        [DefaultValue(AudioCaptureModes.DoNotCapture)]
        [RdpSetting(Template = "audiocapturemode:i:0")]
        public AudioCaptureModes Audiocapturemode { get; set; }

        [DefaultValue(AudioModes.Local)]
        [RdpSetting(Template = "audiomode:i:0")]
        public AudioModes Audiomode { get; set; }

        [DefaultValue(AudioQualityModes.Dynamic)]
        [RdpSetting(Template = "audioqualitymode:i:0")]
        public AudioQualityModes Audioqualitymode { get; set; }

        [DefaultValue(AuthenticationLevels.ShowWarning)]
        [RdpSetting(Template = "authentication level:i:2")]
        public AuthenticationLevels AuthenticationLevel { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "autoreconnection enabled:i:1")]
        public bool AutoreconnectionEnabled { get; set; }

        [DefaultValue(20)]
        [RdpSetting(Template = "autoreconnect max retries:i:20")]
        public int AutoreconnectMaxRetries { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "bandwidthautodetect:i:1")]
        public bool Bandwidthautodetect { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "bitmapcachepersistenable:i:1")]
        public bool Bitmapcachepersistenable { get; set; }

        [DefaultValue(1500)]
        [RdpSetting(Template = "bitmapcachesize:i:1500")]
        public int Bitmapcachesize { get; set; }

        [RdpSetting(Template = "camerastoredirect:s:")]
        public string Camerastoredirect { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "compression:i:1")]
        public bool Compression { get; set; }

        [DefaultValue(ConnectionTypes.LowSpeed)]
        [RdpSetting(Template = "connection type:i:2")]
        public ConnectionTypes ConnectionType { get; set; }

        [RdpSetting(Template = "connect to console:i:0")]
        public bool ConnectToConsole { get; set; }

        [DefaultValue(600)]
        [RdpSetting(Template = "desktopheight:i:600")]
        public int Desktopheight { get; set; }

        [RdpSetting(Template = "desktop size id:i:0")]
        public DesktopSizes DesktopSizeId { get; set; }

        [DefaultValue(800)]
        [RdpSetting(Template = "desktopwidth:i:800")]
        public int Desktopwidth { get; set; }

        [RdpSetting(Template = "devicestoredirect:s:")]
        public string Devicestoredirect { get; set; }

        [RdpSetting(Template = "disableconnectionsharing:i:0")]
        public bool Disableconnectionsharing { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "disable ctrl+alt+del:i:1")]
        public bool DisableCtrlAltDel { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "disable full window drag:i:1")]
        public bool DisableFullWindowDrag { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "disable menu anims:i:1")]
        public bool DisableMenuAnims { get; set; }

        [RdpSetting(Template = "disableremoteappcapscheck:i:0")]
        public bool Disableremoteappcapscheck { get; set; }

        [RdpSetting(Template = "disable themes:i:0")]
        public bool DisableThemes { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "disable wallpaper:i:1")]
        public bool DisableWallpaper { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "displayconnectionbar:i:1")]
        public bool Displayconnectionbar { get; set; }

        [RdpSetting(Template = "domain:s:")]
        public string Domain { get; set; }

        [RdpSetting(Template = "drivestoredirect:s:")]
        public string Drivestoredirect { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "enablecredsspsupport:i:1")]
        public bool Enablecredsspsupport { get; set; }

        [RdpSetting(Template = "enablesuperpan:i:0")]
        public bool Enablesuperpan { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "encode redirected video capture:i:1")]
        public bool EncodeRedirectedVideoCapture { get; set; }

        [RdpSetting(Template = "full address:s:")]
        public string FullAddress { get; set; }

        [DefaultValue(GatewayCredentialSources.SelectLater)]
        [RdpSetting(Template = "gatewaycredentialssource:i:4")]
        public GatewayCredentialSources Gatewaycredentialssource { get; set; }

        [RdpSetting(Template = "gatewayhostname:s:")]
        public string Gatewayhostname { get; set; }

        [DefaultValue(GatewayProfileUsageMethods.Default)]
        [RdpSetting(Template = "gatewayprofileusagemethod:i:0")]
        public GatewayProfileUsageMethods Gatewayprofileusagemethod { get; set; }

        [DefaultValue(GatewayUsageMethods.DoNotUseGatewayNoLocal)]
        [RdpSetting(Template = "gatewayusagemethod:i:4")]
        public GatewayUsageMethods Gatewayusagemethod { get; set; }

        [DefaultValue(KeyboardHooks.FullScreen)]
        [RdpSetting(Template = "keyboardhook:i:2")]
        public KeyboardHooks Keyboardhook { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "negotiate security layer:i:1")]
        public bool NegotiateSecurityLayer { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "networkautodetect:i:1")]
        public bool Networkautodetect { get; set; }

        [RdpSetting(Template = "password 51:b:")]
        public byte[] Password51 { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "pinconnectionbar:i:1")]
        public bool Pinconnectionbar { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "promptcredentialonce:i:1")]
        public bool Promptcredentialonce { get; set; }

        [RdpSetting(Template = "prompt for credentials:i:0")]
        public bool PromptForCredentials { get; set; }

        [RdpSetting(Template = "prompt for credentials on client:i:0")]
        public bool PromptForCredentialsOnClient { get; set; }

        [RdpSetting(Template = "public mode:i:0")]
        public bool PublicMode { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "redirectclipboard:i:1")]
        public bool Redirectclipboard { get; set; }

        [RdpSetting(Template = "redirectcomports:i:0")]
        public bool Redirectcomports { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "redirectdirectx:i:1")]
        public bool Redirectdirectx { get; set; }

        [RdpSetting(Template = "redirectdrives:i:0")]
        public bool Redirectdrives { get; set; }

        [DefaultValue(VideoCaptureEncodingQualities.High)]
        [RdpSetting(Template = "redirected video capture encoding quality:i:0")]
        public VideoCaptureEncodingQualities RedirectedVideoCaptureEncodingQuality { get; set; }

        [RdpSetting(Template = "redirectlocation:i:0")]
        public bool Redirectlocation { get; set; }

        [RdpSetting(Template = "redirectposdevices:i:0")]
        public bool Redirectposdevices { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "redirectprinters:i:1")]
        public bool Redirectprinters { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "redirectsmartcards:i:1")]
        public bool Redirectsmartcards { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "redirectwebauthn:i:1")]
        public bool Redirectwebauthn { get; set; }

        [RdpSetting(Template = "remoteapplicationcmdline:s:")]
        public string Remoteapplicationcmdline { get; set; }

        [DefaultValue(true)]
        [RdpSetting(Template = "remoteapplicationexpandcmdline:i:1")]
        public bool Remoteapplicationexpandcmdline { get; set; }

        [RdpSetting(Template = "remoteapplicationexpandworkingdir:i:0")]
        public bool Remoteapplicationexpandworkingdir { get; set; }

        [RdpSetting(Template = "remoteapplicationfile:s:")]
        public string Remoteapplicationfile { get; set; }

        [RdpSetting(Template = "remoteapplicationicon:s:")]
        public string Remoteapplicationicon { get; set; }

        [DefaultValue(RemoteApplicationModes.Normal)]
        [RdpSetting(Template = "remoteapplicationmode:i:0")]
        public RemoteApplicationModes Remoteapplicationmode { get; set; }

        [RdpSetting(Template = "remoteapplicationname:s:")]
        public string Remoteapplicationname { get; set; }

        [RdpSetting(Template = "remoteapplicationprogram:s:")]
        public string Remoteapplicationprogram { get; set; }

        [DefaultValue(ScreenModes.FullScreen)]
        [RdpSetting(Template = "screen mode id:i:2")]
        public ScreenModes ScreenModeId { get; set; }

        [RdpSetting(Template = "selectedmonitors:s:")]
        public string Selectedmonitors { get; set; }

        [DefaultValue(3389)]
        [RdpSetting(Template = "server port:i:3389")]
        public int ServerPort { get; set; }

        [DefaultValue(SessionBpps.Bpp_32)]
        [RdpSetting(Template = "session bpp:i:32")]
        public SessionBpps SessionBpp { get; set; }

        [RdpSetting(Template = "shell working directory:s:")]
        public string ShellWorkingDirectory { get; set; }

        [RdpSetting(Template = "signature:s:")]
        public string Signature { get; set; }

        [RdpSetting(Template = "signscope:s:")]
        public string Signscope { get; set; }

        [RdpSetting(Template = "smart sizing:i:0")]
        public bool SmartSizing { get; set; }

        [RdpSetting(Template = "span monitors:i:0")]
        public bool SpanMonitors { get; set; }

        [DefaultValue(1)]
        [RdpSetting(Template = "superpanaccelerationfactor:i:1")]
        public int Superpanaccelerationfactor { get; set; }

        [RdpSetting(Template = "usbdevicestoredirect:s:")]
        public string Usbdevicestoredirect { get; set; }

        [RdpSetting(Template = "use multimon:i:0")]
        public bool UseMultimon { get; set; }

        [RdpSetting(Template = "username:s:")]
        public string Username { get; set; }

        [DefaultValue(VideoPlaybackModes.Efficient)]
        [RdpSetting(Template = "videoplaybackmode:i:1")]
        public VideoPlaybackModes Videoplaybackmode { get; set; }

        [DefaultValue("0,3,0,0,800,600")]
        [RdpSetting(Template = "winposstr:s:0,3,0,0,800,600")]
        public string Winposstr { get; set; }

        [RdpSetting(Template = "workspaceid:s:")]
        public string Workspaceid { get; set; }
    }
}
