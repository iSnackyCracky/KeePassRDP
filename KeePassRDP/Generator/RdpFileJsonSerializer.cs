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

namespace KeePassRDP.Generator
{
    public partial class RdpFile
    {
        public bool ShouldSerializeAdministrativeSession() { return AdministrativeSession != false; }
        public bool ShouldSerializeAllowDesktopComposition() { return AllowDesktopComposition != false; }
        public bool ShouldSerializeAllowFontSmoothing() { return AllowFontSmoothing != false; }
        public bool ShouldSerializeAlternateFullAddress() { return !string.IsNullOrWhiteSpace(AlternateFullAddress); }
        public bool ShouldSerializeAlternateShell() { return !string.IsNullOrWhiteSpace(AlternateShell); }
        public bool ShouldSerializeAudiocapturemode() { return Audiocapturemode != AudioCaptureModes.DoNotCapture; }
        public bool ShouldSerializeAudiomode() { return Audiomode != AudioModes.Local; }
        public bool ShouldSerializeAudioqualitymode() { return Audioqualitymode != AudioQualityModes.Dynamic; }
        public bool ShouldSerializeAuthenticationLevel() { return AuthenticationLevel != AuthenticationLevels.ShowWarning; }
        public bool ShouldSerializeAutoreconnectionEnabled() { return AutoreconnectionEnabled != true; }
        public bool ShouldSerializeAutoreconnectMaxRetries() { return AutoreconnectMaxRetries != 20; }
        public bool ShouldSerializeBandwidthautodetect() { return Bandwidthautodetect != true; }
        public bool ShouldSerializeBitmapcachepersistenable() { return Bitmapcachepersistenable != true; }
        public bool ShouldSerializeBitmapcachesize() { return Bitmapcachesize != 1500; }
        public bool ShouldSerializeCamerastoredirect() { return !string.IsNullOrWhiteSpace(Camerastoredirect); }
        public bool ShouldSerializeCompression() { return Compression != true; }
        public bool ShouldSerializeConnectionType() { return !Bandwidthautodetect && ConnectionType != ConnectionTypes.LowSpeed; }
        public bool ShouldSerializeConnectToConsole() { return ConnectToConsole != false; }
        public bool ShouldSerializeDesktopheight() { return Desktopheight != 600; }
        public bool ShouldSerializeDesktopSizeId() { return DesktopSizeId != DesktopSizes.Size_640_480; }
        public bool ShouldSerializeDesktopwidth() { return Desktopwidth != 800; }
        public bool ShouldSerializeDevicestoredirect() { return !string.IsNullOrWhiteSpace(Devicestoredirect); }
        public bool ShouldSerializeDisableconnectionsharing() { return Disableconnectionsharing != false; }
        public bool ShouldSerializeDisableCtrlAltDel() { return DisableCtrlAltDel != true; }
        public bool ShouldSerializeDisableFullWindowDrag() { return DisableFullWindowDrag != true; }
        public bool ShouldSerializeDisableMenuAnims() { return DisableMenuAnims != true; }
        public bool ShouldSerializeDisableremoteappcapscheck() { return Disableremoteappcapscheck != false; }
        public bool ShouldSerializeDisableThemes() { return DisableThemes != false; }
        public bool ShouldSerializeDisableWallpaper() { return DisableWallpaper != true; }
        public bool ShouldSerializeDisplayconnectionbar() { return Displayconnectionbar != true; }
        public bool ShouldSerializeDomain() { return !string.IsNullOrWhiteSpace(Domain); }
        public bool ShouldSerializeDrivestoredirect() { return !string.IsNullOrWhiteSpace(Drivestoredirect); }
        public bool ShouldSerializeEnablecredsspsupport() { return Enablecredsspsupport != true; }
        public bool ShouldSerializeEnablesuperpan() { return Enablesuperpan != false; }
        public bool ShouldSerializeEncodeRedirectedVideoCapture() { return EncodeRedirectedVideoCapture != true; }
        public bool ShouldSerializeFullAddress() { return !string.IsNullOrWhiteSpace(FullAddress); }
        public bool ShouldSerializeGatewaycredentialssource() { return Gatewaycredentialssource != GatewayCredentialSources.SelectLater; }
        public bool ShouldSerializeGatewayhostname() { return !string.IsNullOrWhiteSpace(Gatewayhostname); }
        public bool ShouldSerializeGatewayprofileusagemethod() { return Gatewayprofileusagemethod != GatewayProfileUsageMethods.Default; }
        public bool ShouldSerializeGatewayusagemethod() { return Gatewayusagemethod != GatewayUsageMethods.DoNotUseGatewayNoLocal; }
        public bool ShouldSerializeKeyboardhook() { return Keyboardhook != KeyboardHooks.FullScreen; }
        public bool ShouldSerializeNegotiateSecurityLayer() { return NegotiateSecurityLayer != true; }
        public bool ShouldSerializeNetworkautodetect() { return Networkautodetect != true; }
        public bool ShouldSerializePassword51() { return Password51 != null && Password51.Length > 0; }
        public bool ShouldSerializePinconnectionbar() { return Pinconnectionbar != true; }
        public bool ShouldSerializePromptcredentialonce() { return Promptcredentialonce != true; }
        public bool ShouldSerializePromptForCredentials() { return PromptForCredentials != false; }
        public bool ShouldSerializePromptForCredentialsOnClient() { return PromptForCredentialsOnClient != false; }
        public bool ShouldSerializePublicMode() { return PublicMode != false; }
        public bool ShouldSerializeRedirectclipboard() { return Redirectclipboard != true; }
        public bool ShouldSerializeRedirectcomports() { return Redirectcomports != false; }
        public bool ShouldSerializeRedirectdirectx() { return Redirectdirectx != true; }
        public bool ShouldSerializeRedirectdrives() { return Redirectdrives != false; }
        public bool ShouldSerializeRedirectedVideoCaptureEncodingQuality() { return RedirectedVideoCaptureEncodingQuality != VideoCaptureEncodingQualities.High; }
        public bool ShouldSerializeRedirectlocation() { return Redirectlocation != false; }
        public bool ShouldSerializeRedirectposdevices() { return Redirectposdevices != false; }
        public bool ShouldSerializeRedirectprinters() { return Redirectprinters != true; }
        public bool ShouldSerializeRedirectsmartcards() { return Redirectsmartcards != true; }
        public bool ShouldSerializeRedirectwebauthn() { return Redirectwebauthn != true; }
        public bool ShouldSerializeRemoteapplicationcmdline() { return !string.IsNullOrWhiteSpace(Remoteapplicationcmdline); }
        public bool ShouldSerializeRemoteapplicationexpandcmdline() { return Remoteapplicationexpandcmdline != true; }
        public bool ShouldSerializeRemoteapplicationexpandworkingdir() { return Remoteapplicationexpandworkingdir != true; }
        public bool ShouldSerializeRemoteapplicationfile() { return !string.IsNullOrWhiteSpace(Remoteapplicationfile); }
        public bool ShouldSerializeRemoteapplicationicon() { return !string.IsNullOrWhiteSpace(Remoteapplicationicon); }
        public bool ShouldSerializeRemoteapplicationmode() { return Remoteapplicationmode != RemoteApplicationModes.Normal; }
        public bool ShouldSerializeRemoteapplicationname() { return !string.IsNullOrWhiteSpace(Remoteapplicationname); }
        public bool ShouldSerializeRemoteapplicationprogram() { return !string.IsNullOrWhiteSpace(Remoteapplicationprogram); }
        public bool ShouldSerializeScreenModeId() { return ScreenModeId != ScreenModes.FullScreen; }
        public bool ShouldSerializeSelectedmonitors() { return !string.IsNullOrWhiteSpace(Selectedmonitors); }
        public bool ShouldSerializeServerPort() { return ServerPort != 3389; }
        public bool ShouldSerializeSessionBpp() { return SessionBpp != SessionBpps.Bpp_32; }
        public bool ShouldSerializeShellWorkingDirectory() { return !string.IsNullOrWhiteSpace(ShellWorkingDirectory); }
        public bool ShouldSerializeSignature() { return !string.IsNullOrWhiteSpace(Signature); }
        public bool ShouldSerializeSignscope() { return !string.IsNullOrWhiteSpace(Signscope); }
        public bool ShouldSerializeSmartSizing() { return SmartSizing != false; }
        public bool ShouldSerializeSpanMonitors() { return SpanMonitors != false; }
        public bool ShouldSerializeSuperpanaccelerationfactor() { return Superpanaccelerationfactor != 1; }
        public bool ShouldSerializeUsbdevicestoredirect() { return !string.IsNullOrWhiteSpace(Usbdevicestoredirect); }
        public bool ShouldSerializeUseMultimon() { return UseMultimon != false; }
        public bool ShouldSerializeUsername() { return !string.IsNullOrWhiteSpace(Username); }
        public bool ShouldSerializeVideoplaybackmode() { return Videoplaybackmode != VideoPlaybackModes.Efficient; }
        public bool ShouldSerializeWinposstr() { return !string.IsNullOrWhiteSpace(Winposstr) && Winposstr != "0,3,0,0,800,600"; }
        public bool ShouldSerializeWorkspaceid() { return !string.IsNullOrWhiteSpace(Workspaceid); }
    }
}
