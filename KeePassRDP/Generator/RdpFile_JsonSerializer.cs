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
using System.Linq;
using System.Reflection;

namespace KeePassRDP.Generator
{
    public partial class RdpFile
    {
        private static readonly DefaultValueAttribute EmptyDefaultValue = new DefaultValueAttribute(false);

        private static object GetDefaultValue(string propertyName)
        {
            return (typeof(RdpFile).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public).GetCustomAttributes(typeof(DefaultValueAttribute), false).OfType<DefaultValueAttribute>().FirstOrDefault() ?? EmptyDefaultValue).Value;
        }

        public bool ShouldSerializeAdministrativeSession() { return !Equals(GetDefaultValue("AdministrativeSession"), AdministrativeSession); }
        public bool ShouldSerializeAllowDesktopComposition() { return !Equals(GetDefaultValue("AllowDesktopComposition"), AllowDesktopComposition); }
        public bool ShouldSerializeAllowFontSmoothing() { return !Equals(GetDefaultValue("AllowFontSmoothing"), AllowFontSmoothing); }
        public bool ShouldSerializeAlternateFullAddress() { return !Equals(GetDefaultValue("AlternateFullAddress"), (AlternateFullAddress ?? string.Empty).Trim()); }
        public bool ShouldSerializeAlternateShell() { return !Equals(GetDefaultValue("AlternateShell"), (AlternateShell ?? string.Empty).Trim()); }
        public bool ShouldSerializeAudiocapturemode() { return !Equals(GetDefaultValue("Audiocapturemode"), Audiocapturemode); }
        public bool ShouldSerializeAudiomode() { return !Equals(GetDefaultValue("Audiomode"), Audiomode); }
        public bool ShouldSerializeAudioqualitymode() { return !Equals(GetDefaultValue("Audioqualitymode"), Audioqualitymode); }
        public bool ShouldSerializeAuthenticationLevel() { return !Equals(GetDefaultValue("AuthenticationLevel"), AuthenticationLevel); }
        public bool ShouldSerializeAutoreconnectionEnabled() { return !Equals(GetDefaultValue("AutoreconnectionEnabled"), AutoreconnectionEnabled); }
        public bool ShouldSerializeAutoreconnectMaxRetries() { return !Equals(GetDefaultValue("AutoreconnectMaxRetries"), AutoreconnectMaxRetries); }
        public bool ShouldSerializeBandwidthautodetect() { return !Equals(GetDefaultValue("Bandwidthautodetect"), Bandwidthautodetect); }
        public bool ShouldSerializeBitmapcachepersistenable() { return !Equals(GetDefaultValue("Bitmapcachepersistenable"), Bitmapcachepersistenable); }
        public bool ShouldSerializeBitmapcachesize() { return !Equals(GetDefaultValue("Bitmapcachesize"), Bitmapcachesize); }
        public bool ShouldSerializeCamerastoredirect() { return !Equals(GetDefaultValue("Camerastoredirect"), (Camerastoredirect ?? string.Empty).Trim()); }
        public bool ShouldSerializeCompression() { return !Equals(GetDefaultValue("Compression"), Compression); }
        public bool ShouldSerializeConnectionType() { return !Bandwidthautodetect && !Equals(GetDefaultValue("ConnectionType"), ConnectionType); }
        public bool ShouldSerializeConnectToConsole() { return !Equals(GetDefaultValue("ConnectToConsole"), ConnectToConsole); }
        public bool ShouldSerializeDesktopheight() { return !Equals(GetDefaultValue("Desktopheight"), Desktopheight); }
        public bool ShouldSerializeDesktopSizeId() { return !Equals(GetDefaultValue("DesktopSizeId"), DesktopSizeId); }
        public bool ShouldSerializeDesktopwidth() { return !Equals(GetDefaultValue("Desktopwidth"), Desktopwidth); }
        public bool ShouldSerializeDevicestoredirect() { return !Equals(GetDefaultValue("Devicestoredirect"), (Devicestoredirect ?? string.Empty).Trim()); }
        public bool ShouldSerializeDisableconnectionsharing() { return !Equals(GetDefaultValue("Disableconnectionsharing"), Disableconnectionsharing); }
        public bool ShouldSerializeDisableCtrlAltDel() { return !Equals(GetDefaultValue("DisableCtrlAltDel"), DisableCtrlAltDel); }
        public bool ShouldSerializeDisableFullWindowDrag() { return !Equals(GetDefaultValue("DisableFullWindowDrag"), DisableFullWindowDrag); }
        public bool ShouldSerializeDisableMenuAnims() { return !Equals(GetDefaultValue("DisableMenuAnims"), DisableMenuAnims); }
        public bool ShouldSerializeDisableremoteappcapscheck() { return !Equals(GetDefaultValue("Disableremoteappcapscheck"), Disableremoteappcapscheck); }
        public bool ShouldSerializeDisableThemes() { return !Equals(GetDefaultValue("DisableThemes"), DisableThemes); }
        public bool ShouldSerializeDisableWallpaper() { return !Equals(GetDefaultValue("DisableWallpaper"), DisableWallpaper); }
        public bool ShouldSerializeDisplayconnectionbar() { return !Equals(GetDefaultValue("Displayconnectionbar"), Displayconnectionbar); }
        public bool ShouldSerializeDomain() { return !Equals(GetDefaultValue("Domain"), (Domain ?? string.Empty).Trim()); }
        public bool ShouldSerializeDrivestoredirect() { return !Equals(GetDefaultValue("Drivestoredirect"), (Drivestoredirect ?? string.Empty).Trim()); }
        public bool ShouldSerializeEnablecredsspsupport() { return !Equals(GetDefaultValue("Enablecredsspsupport"), Enablecredsspsupport); }
        public bool ShouldSerializeEnablesuperpan() { return !Equals(GetDefaultValue("Enablesuperpan"), Enablesuperpan); }
        public bool ShouldSerializeEncodeRedirectedVideoCapture() { return !Equals(GetDefaultValue("EncodeRedirectedVideoCapture"), EncodeRedirectedVideoCapture); }
        public bool ShouldSerializeFullAddress() { return !Equals(GetDefaultValue("FullAddress"), (FullAddress ?? string.Empty).Trim()); }
        public bool ShouldSerializeGatewaycredentialssource() { return !Equals(GetDefaultValue("Gatewaycredentialssource"), Gatewaycredentialssource); }
        public bool ShouldSerializeGatewayhostname() { return !Equals(GetDefaultValue("Gatewayhostname"), (Gatewayhostname ?? string.Empty).Trim()); }
        public bool ShouldSerializeGatewayprofileusagemethod() { return !Equals(GetDefaultValue("Gatewayprofileusagemethod"), Gatewayprofileusagemethod); }
        public bool ShouldSerializeGatewayusagemethod() { return !Equals(GetDefaultValue("Gatewayusagemethod"), Gatewayusagemethod); }
        public bool ShouldSerializeKeyboardhook() { return !Equals(GetDefaultValue("Keyboardhook"), Keyboardhook); }
        public bool ShouldSerializeNegotiateSecurityLayer() { return !Equals(GetDefaultValue("NegotiateSecurityLayer"), NegotiateSecurityLayer); }
        public bool ShouldSerializeNetworkautodetect() { return !Equals(GetDefaultValue("Networkautodetect"), Networkautodetect); }
        public bool ShouldSerializePassword51() { return Password51 != null && Password51.Length > 0 && !Password51.All(x => x == 0); }
        public bool ShouldSerializePinconnectionbar() { return !Equals(GetDefaultValue("Pinconnectionbar"), Pinconnectionbar); }
        public bool ShouldSerializePromptcredentialonce() { return !Equals(GetDefaultValue("Promptcredentialonce"), Promptcredentialonce); }
        public bool ShouldSerializePromptForCredentials() { return !Equals(GetDefaultValue("PromptForCredentials"), PromptForCredentials); }
        public bool ShouldSerializePromptForCredentialsOnClient() { return !Equals(GetDefaultValue("PromptForCredentialsOnClient"), PromptForCredentialsOnClient); }
        public bool ShouldSerializePublicMode() { return !Equals(GetDefaultValue("PublicMode"), PublicMode); }
        public bool ShouldSerializeRedirectclipboard() { return !Equals(GetDefaultValue("Redirectclipboard"), Redirectclipboard); }
        public bool ShouldSerializeRedirectcomports() { return !Equals(GetDefaultValue("Redirectcomports"), Redirectcomports); }
        public bool ShouldSerializeRedirectdirectx() { return !Equals(GetDefaultValue("Redirectdirectx"), Redirectdirectx); }
        public bool ShouldSerializeRedirectdrives() { return !Equals(GetDefaultValue("Redirectdrives"), Redirectdrives); }
        public bool ShouldSerializeRedirectedVideoCaptureEncodingQuality() { return !Equals(GetDefaultValue("RedirectedVideoCaptureEncodingQuality"), RedirectedVideoCaptureEncodingQuality); }
        public bool ShouldSerializeRedirectlocation() { return !Equals(GetDefaultValue("Redirectlocation"), Redirectlocation); }
        public bool ShouldSerializeRedirectposdevices() { return !Equals(GetDefaultValue("Redirectposdevices"), Redirectposdevices); }
        public bool ShouldSerializeRedirectprinters() { return !Equals(GetDefaultValue("Redirectprinters"), Redirectprinters); }
        public bool ShouldSerializeRedirectsmartcards() { return !Equals(GetDefaultValue("Redirectsmartcards"), Redirectsmartcards); }
        public bool ShouldSerializeRedirectwebauthn() { return !Equals(GetDefaultValue("Redirectwebauthn"), Redirectwebauthn); }
        public bool ShouldSerializeRemoteapplicationcmdline() { return !Equals(GetDefaultValue("Remoteapplicationcmdline"), (Remoteapplicationcmdline ?? string.Empty).Trim()); }
        public bool ShouldSerializeRemoteapplicationexpandcmdline() { return !Equals(GetDefaultValue("Remoteapplicationexpandcmdline"), Remoteapplicationexpandcmdline); }
        public bool ShouldSerializeRemoteapplicationexpandworkingdir() { return !Equals(GetDefaultValue("Remoteapplicationexpandworkingdir"), Remoteapplicationexpandworkingdir); }
        public bool ShouldSerializeRemoteapplicationfile() { return !Equals(GetDefaultValue("Remoteapplicationfile"), (Remoteapplicationfile ?? string.Empty).Trim()); }
        public bool ShouldSerializeRemoteapplicationfileextensions() { return !Equals(GetDefaultValue("Remoteapplicationfileextensions"), (Remoteapplicationfileextensions ?? string.Empty).Trim()); }
        public bool ShouldSerializeRemoteapplicationguid() { return !Equals(GetDefaultValue("Remoteapplicationguid"), (Remoteapplicationguid ?? string.Empty).Trim()); }
        public bool ShouldSerializeRemoteapplicationicon() { return !Equals(GetDefaultValue("Remoteapplicationicon"), (Remoteapplicationicon ?? string.Empty).Trim()); }
        public bool ShouldSerializeRemoteapplicationmode() { return !Equals(GetDefaultValue("Remoteapplicationmode"), Remoteapplicationmode); }
        public bool ShouldSerializeRemoteapplicationname() { return !Equals(GetDefaultValue("Remoteapplicationname"), (Remoteapplicationname ?? string.Empty).Trim()); }
        public bool ShouldSerializeRemoteapplicationprogram() { return !Equals(GetDefaultValue("Remoteapplicationprogram"), (Remoteapplicationprogram ?? string.Empty).Trim()); }
        public bool ShouldSerializeScreenModeId() { return !Equals(GetDefaultValue("ScreenModeId"), ScreenModeId); }
        public bool ShouldSerializeSelectedmonitors() { return !Equals(GetDefaultValue("Selectedmonitors"), (Selectedmonitors ?? string.Empty).Trim()); }
        public bool ShouldSerializeServerPort() { return !Equals(GetDefaultValue("ServerPort"), ServerPort); }
        public bool ShouldSerializeSessionBpp() { return !Equals(GetDefaultValue("SessionBpp"), SessionBpp); }
        public bool ShouldSerializeShellWorkingDirectory() { return !Equals(GetDefaultValue("ShellWorkingDirectory"), (ShellWorkingDirectory ?? string.Empty).Trim()); }
        public bool ShouldSerializeSignature() { return !Equals(GetDefaultValue("Signature"), (Signature ?? string.Empty).Trim()); }
        public bool ShouldSerializeSignscope() { return !Equals(GetDefaultValue("Signscope"), (Signscope ?? string.Empty).Trim()); }
        public bool ShouldSerializeSmartSizing() { return !Equals(GetDefaultValue("SmartSizing"), SmartSizing); }
        public bool ShouldSerializeSpanMonitors() { return !Equals(GetDefaultValue("SpanMonitors"), SpanMonitors); }
        public bool ShouldSerializeSuperpanaccelerationfactor() { return !Equals(GetDefaultValue("Superpanaccelerationfactor"), Superpanaccelerationfactor); }
        public bool ShouldSerializeUsbdevicestoredirect() { return !Equals(GetDefaultValue("Usbdevicestoredirect"), (Usbdevicestoredirect ?? string.Empty).Trim()); }
        public bool ShouldSerializeUseMultimon() { return !Equals(GetDefaultValue("UseMultimon"), UseMultimon); }
        public bool ShouldSerializeUsername() { return !Equals(GetDefaultValue("Username"), (Username ?? string.Empty).Trim()); }
        public bool ShouldSerializeVideoplaybackmode() { return !Equals(GetDefaultValue("Videoplaybackmode"), Videoplaybackmode); }
        public bool ShouldSerializeWinposstr() { return !Equals(GetDefaultValue("Winposstr"), (Winposstr ?? string.Empty).Trim()); }
        public bool ShouldSerializeWorkspaceid() { return !Equals(GetDefaultValue("Workspaceid"), (Workspaceid ?? string.Empty).Trim()); }
        public bool ShouldSerializeMaximizetocurrentdisplays() { return !Equals(GetDefaultValue("Maximizetocurrentdisplays"), Maximizetocurrentdisplays); }
        public bool ShouldSerializeSinglemoninwindowedmode() { return !Equals(GetDefaultValue("Singlemoninwindowedmode"), Singlemoninwindowedmode); }
        public bool ShouldSerializeDynamicResolution() { return !Equals(GetDefaultValue("DynamicResolution"), DynamicResolution); }
        public bool ShouldSerializeDesktopscalefactor() { return !Equals(GetDefaultValue("Desktopscalefactor"), Desktopscalefactor); }
        public bool ShouldSerializeDisableCursorSetting() { return !Equals(GetDefaultValue("DisableCursorSetting"), DisableCursorSetting); }
        public bool ShouldSerializeEnableworkspacereconnect() { return !Equals(GetDefaultValue("Enableworkspacereconnect"), Enableworkspacereconnect); }
        public bool ShouldSerializeGatewaybrokeringtype() { return !Equals(GetDefaultValue("Gatewaybrokeringtype"), Gatewaybrokeringtype); }
        public bool ShouldSerializeUseRedirectionServerName() { return !Equals(GetDefaultValue("UseRedirectionServerName"), UseRedirectionServerName); }
        public bool ShouldSerializeLoadbalanceinfo() { return !Equals(GetDefaultValue("Loadbalanceinfo"), (Loadbalanceinfo ?? string.Empty).Trim()); }
        public bool ShouldSerializeRdgiskdcproxy() { return !Equals(GetDefaultValue("Rdgiskdcproxy"), Rdgiskdcproxy); }
        public bool ShouldSerializeKdcproxyname() { return !Equals(GetDefaultValue("Kdcproxyname"), (Kdcproxyname ?? string.Empty).Trim()); }
        public bool ShouldSerializePcb() { return !Equals(GetDefaultValue("Pcb"), (Pcb ?? string.Empty).Trim()); }
        public bool ShouldSerializeSupportUrl() { return !Equals(GetDefaultValue("SupportUrl"), (SupportUrl ?? string.Empty).Trim()); }
        public bool ShouldSerializeRequirePreAuthentication() { return !Equals(GetDefaultValue("RequirePreAuthentication"), RequirePreAuthentication); }
        public bool ShouldSerializePreAuthenticationServerAddress() { return !Equals(GetDefaultValue("PreAuthenticationServerAddress"), (PreAuthenticationServerAddress ?? string.Empty).Trim()); }
        public bool ShouldSerializeEventloguploadaddress() { return !Equals(GetDefaultValue("Eventloguploadaddress"), (Eventloguploadaddress ?? string.Empty).Trim()); }
    }
}
