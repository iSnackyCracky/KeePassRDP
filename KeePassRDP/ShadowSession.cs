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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeePassRDP
{
    internal class ShadowSession
    {
        public class ClientInfo
        {
            public ClientDetail Detail { get; private set; }

            public string Computer { get; private set; }
            public string UserName { get; internal set; }
            public string Domain { get; internal set; }

            // WTS_SESSION_INFO
            public int SessionId { get; private set; }
            public string WinStationName { get; internal set; }

            // WTSINFOEX_LEVEL1_W
            public WTS_CONNECTSTATE_CLASS SessionState { get; internal set; }
            public SessionFlags SessionFlags { get; internal set; }
            public DateTime LogonTime { get; internal set; }
            public DateTime ConnectTime { get; internal set; }
            public DateTime DisconnectTime { get; internal set; }
            public DateTime LastInputTime { get; internal set; }
            public DateTime CurrentTime { get; internal set; }

            public TimeSpan IdleTime
            {
                get
                {
                    return (SessionState == WTS_CONNECTSTATE_CLASS.WTSDisconnected ||
                        (DisconnectTime > LastInputTime && DisconnectTime > DateTime.MinValue)) ?
                            TimeSpan.FromSeconds(Math.Round((CurrentTime - DisconnectTime).TotalSeconds)) :
                            LastInputTime < CurrentTime && LastInputTime > DateTime.MinValue ?
                                TimeSpan.FromSeconds(Math.Round((CurrentTime - LastInputTime).TotalSeconds)) :
                                TimeSpan.Zero;
                }
            }

            public ClientInfo(string computer, int sessionId)
            {
                Computer = computer;
                SessionId = sessionId;
                Detail = new ClientDetail();
            }

            internal void FromWtsInfoEx(WTSINFOEX wtsInfoEx)
            {
                var wtsInfoExLevel1 = wtsInfoEx.Data.WTSInfoExLevel1;
                SessionState = wtsInfoExLevel1.SessionState;

                if (wtsInfoExLevel1.SessionState != WTS_CONNECTSTATE_CLASS.WTSListen &&
                    wtsInfoExLevel1.SessionState != WTS_CONNECTSTATE_CLASS.WTSConnected)
                {
                    SessionFlags = (SessionFlags)Enum.ToObject(typeof(SessionFlags), wtsInfoExLevel1.SessionFlags);
                    LogonTime = wtsInfoExLevel1.LogonTime == 0 ? DateTime.MinValue : DateTime.FromFileTimeUtc(Convert.ToInt64(wtsInfoExLevel1.LogonTime));
                    DisconnectTime = wtsInfoExLevel1.DisconnectTime == 0 ? DateTime.MinValue : DateTime.FromFileTimeUtc(Convert.ToInt64(wtsInfoExLevel1.DisconnectTime));
                    LastInputTime = wtsInfoExLevel1.LastInputTime == 0 ? DateTime.MinValue : DateTime.FromFileTimeUtc(Convert.ToInt64(wtsInfoExLevel1.LastInputTime));
                    CurrentTime = wtsInfoExLevel1.CurrentTime == 0 ? DateTime.MinValue : DateTime.FromFileTimeUtc(Convert.ToInt64(wtsInfoExLevel1.CurrentTime));
                    ConnectTime = wtsInfoExLevel1.ConnectTime == 0 ? DateTime.MinValue : DateTime.FromFileTimeUtc(Convert.ToInt64(wtsInfoExLevel1.ConnectTime));

                    if (string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(wtsInfoExLevel1.UserName))
                        UserName = wtsInfoExLevel1.UserName;
                    if (string.IsNullOrEmpty(Domain) && !string.IsNullOrEmpty(wtsInfoExLevel1.DomainName))
                        Domain = wtsInfoExLevel1.DomainName;

                    Detail.IncomingBytes = wtsInfoExLevel1.IncomingBytes;
                    Detail.OutgoingBytes = wtsInfoExLevel1.OutgoingBytes;
                    Detail.IncomingFrames = wtsInfoExLevel1.IncomingFrames;
                    Detail.OutgoingFrames = wtsInfoExLevel1.OutgoingFrames;
                    Detail.IncomingCompressedBytes = wtsInfoExLevel1.IncomingCompressedBytes;
                    Detail.OutgoingCompressedBytes = wtsInfoExLevel1.OutgoingCompressedBytes;
                }
            }

            internal void FromWtsClientW(WTSCLIENTW wtsClientInfo)
            {
                if (string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(wtsClientInfo.UserName))
                    UserName = wtsClientInfo.UserName;
                if (string.IsNullOrEmpty(Domain) && !string.IsNullOrEmpty(wtsClientInfo.Domain))
                    Domain = wtsClientInfo.Domain;
                Detail.ClientName = wtsClientInfo.ClientName;
                Detail.WorkDirectory = wtsClientInfo.WorkDirectory;
                Detail.InitialProgram = wtsClientInfo.InitialProgram;
                Detail.EncryptionLevel = wtsClientInfo.EncryptionLevel;
                try
                {
                    Detail.ClientAddressFamily = (AddressFamily)Enum.ToObject(typeof(AddressFamily), wtsClientInfo.ClientAddressFamily);
                    Detail.ClientAddress = new IPAddress(wtsClientInfo.ClientAddress.Take(Detail.ClientAddressFamily == AddressFamily.InterNetworkV6 ? 32 : 4).Select(x => Convert.ToByte(x)).ToArray());
                }
                catch { }
                Detail.HRes = wtsClientInfo.HRes;
                Detail.VRes = wtsClientInfo.VRes;
                Detail.ColorDepth = wtsClientInfo.ColorDepth;
                Detail.ClientDirectory = wtsClientInfo.ClientDirectory;
                Detail.ClientBuildNumber = wtsClientInfo.ClientBuildNumber;
                Detail.ClientHardwareId = wtsClientInfo.ClientHardwareId;
                Detail.ClientProductId = wtsClientInfo.ClientProductId;
                Detail.OutBufCountHost = wtsClientInfo.OutBufCountHost;
                Detail.OutBufCountClient = wtsClientInfo.OutBufCountClient;
                Detail.OutBufLength = wtsClientInfo.OutBufLength;
                Detail.DeviceId = wtsClientInfo.DeviceId;
            }

            internal void FromWtsConfigInfoW(WTSCONFIGINFOW wtsConfigInfo)
            {
                if (string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(wtsConfigInfo.LogonUserName))
                    UserName = wtsConfigInfo.LogonUserName;
                if (string.IsNullOrEmpty(Domain) && !string.IsNullOrEmpty(wtsConfigInfo.LogonDomain))
                    Domain = wtsConfigInfo.LogonDomain;
                //clientInfo.Detail.Version = Convert.ToInt32(wtsConfigInfo.version);
                Detail.ConnectClientDrivesAtLogon = wtsConfigInfo.fConnectClientDrivesAtLogon == 1;
                Detail.ConnectPrinterAtLogon = wtsConfigInfo.fConnectPrinterAtLogon == 1;
                Detail.DisablePrinterRedirection = wtsConfigInfo.fDisablePrinterRedirection == 1;
                Detail.DisableDefaultMainClientPrinter = wtsConfigInfo.fDisableDefaultMainClientPrinter == 1;
                Detail.ShadowSettings = (ShadowSettings)Enum.ToObject(typeof(ShadowSettings), wtsConfigInfo.ShadowSettings);
                Detail.ApplicationName = wtsConfigInfo.ApplicationName;
                if (string.IsNullOrEmpty(Detail.WorkDirectory) && !string.IsNullOrEmpty(wtsConfigInfo.WorkDirectory))
                    Detail.WorkDirectory = wtsConfigInfo.WorkDirectory;
                if (string.IsNullOrEmpty(Detail.InitialProgram) && !string.IsNullOrEmpty(wtsConfigInfo.InitialProgram))
                    Detail.InitialProgram = wtsConfigInfo.InitialProgram;
            }

            public override string ToString()
            {
                return string.Format(@"{0}

UserName: {1}       Domain: {2}
SessionId: {3}      WinStationName: {4}
SessionState: {5}   SessionFlags: {6}
LogonTime: {7}  ConnectTime: {8}    DisconnectTime: {9}
LastInputTime: {10} CurrentTime: {11}   IdleTime: {12}
{13}",
Computer,
UserName,
Domain,
SessionId,
WinStationName,
SessionState,
SessionFlags,
LogonTime,
ConnectTime,
DisconnectTime,
LastInputTime,
CurrentTime,
IdleTime,
Detail);
            }

            public class ClientDetail
            {
                // WTSINFOEX_LEVEL1_W
                public int IncomingBytes { get; internal set; }
                public int OutgoingBytes { get; internal set; }
                public int IncomingFrames { get; internal set; }
                public int OutgoingFrames { get; internal set; }
                public int IncomingCompressedBytes { get; internal set; }
                public int OutgoingCompressedBytes { get; internal set; }

                // WTSCLIENTW
                public string ClientName { get; internal set; }
                public string WorkDirectory { get; internal set; }
                public string InitialProgram { get; internal set; }
                public byte EncryptionLevel { get; internal set; }
                public AddressFamily ClientAddressFamily { get; internal set; }
                public IPAddress ClientAddress { get; internal set; }
                public ushort HRes { get; internal set; }
                public ushort VRes { get; internal set; }
                public ushort ColorDepth { get; internal set; }
                public string ClientDirectory { get; internal set; }
                public uint ClientBuildNumber { get; internal set; }
                public uint ClientHardwareId { get; internal set; }
                public ushort ClientProductId { get; internal set; }
                public ushort OutBufCountHost { get; internal set; }
                public ushort OutBufCountClient { get; internal set; }
                public ushort OutBufLength { get; internal set; }
                public string DeviceId { get; internal set; }

                // WTSCONFIGINFOW
                //public int Version { get; set; }
                public bool ConnectClientDrivesAtLogon { get; set; }
                public bool ConnectPrinterAtLogon { get; set; }
                public bool DisablePrinterRedirection { get; set; }
                public bool DisableDefaultMainClientPrinter { get; set; }
                public ShadowSettings ShadowSettings { get; set; }
                public string ApplicationName { get; set; }

                public short ClientProtocolType { get; set; }

                public override string ToString()
                {
                    return string.Format(@"
IncomingBytes: {0}      OutgoingBytes: {1}
IncomingFrames: {2}     OutgoingFrames: {3}
IncomingCompressedBytes: {4}    OutgoingCompressedBytes: {5}

ClientName: {6}
WorkDirectory: {7}
InitialProgram: {8}
EncryptionLevel: {9}
ClientAddressFamily: {10}   ClientAddress: {11}
HRes: {12}  VRes: {13}  ColorDepth: {14}
ClientDirectory: {15}
ClientBuildNumber: {16}
ClientHardwareId: {17}
ClientProductId: {18}
OutBufCountHost: {19}   OutBufCountClient: {20}     OutBufLength: {21}
DeviceId: {22}

ConnectClientDrivesAtLogon: {23}    ConnectPrinterAtLogon: {24}
DisablePrinterRedirection: {25}     DisableDefaultMainClientPrinter: {26}
ShadowSettings: {27}
ApplicationName: {28}

ClientProtocolType: {29}",
IncomingBytes,
OutgoingBytes,
IncomingFrames,
OutgoingFrames,
IncomingCompressedBytes,
OutgoingCompressedBytes,
ClientName,
WorkDirectory,
InitialProgram,
EncryptionLevel,
ClientAddressFamily,
ClientAddress,
HRes,
VRes,
ColorDepth,
ClientDirectory,
ClientBuildNumber,
ClientHardwareId,
ClientProductId,
OutBufCountHost,
OutBufCountClient,
OutBufLength,
DeviceId,
ConnectClientDrivesAtLogon,
ConnectPrinterAtLogon,
DisablePrinterRedirection,
DisableDefaultMainClientPrinter,
ShadowSettings,
ApplicationName,
ClientProtocolType);
                }
            }

        }

        internal enum ShadowSettings : uint
        {
            Disabled = 0,
            ConsentControl = 1,
            NoConsentControl = 2,
            Consent = 3,
            NoConsent = 4
        }

        internal enum SessionFlags : uint
        {
            Locked = 0,
            Unlocked = 1,
            Unknown = uint.MaxValue
        }

        internal enum WTS_CONNECTSTATE_CLASS : uint
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WTSINFOEX_LEVEL1_W
        {
            public int SessionId;
            public WTS_CONNECTSTATE_CLASS SessionState;
            public int SessionFlags; // 0 = locked, 1 = unlocked, ffffffff = unknown
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string WinStationName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
            public string UserName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
            public string DomainName;
            public ulong LogonTime;
            public ulong ConnectTime;
            public ulong DisconnectTime;
            public ulong LastInputTime;
            public ulong CurrentTime;
            public int IncomingBytes;
            public int OutgoingBytes;
            public int IncomingFrames;
            public int OutgoingFrames;
            public int IncomingCompressedBytes;
            public int OutgoingCompressedBytes;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WTSCONFIGINFOW
        {
            public uint version;
            public uint fConnectClientDrivesAtLogon;
            public uint fConnectPrinterAtLogon;
            public uint fDisablePrinterRedirection;
            public uint fDisableDefaultMainClientPrinter;
            public uint ShadowSettings;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
            public string LogonUserName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
            public string LogonDomain;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 261)]
            public string WorkDirectory;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 261)]
            public string InitialProgram;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 261)]
            public string ApplicationName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WTSCLIENTW
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
            public string ClientName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
            public string Domain;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
            public string UserName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 261)]
            public string WorkDirectory;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 261)]
            public string InitialProgram;
            public byte EncryptionLevel;
            public uint ClientAddressFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
            public ushort[] ClientAddress;
            public ushort HRes;
            public ushort VRes;
            public ushort ColorDepth;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 261)]
            public string ClientDirectory;
            public uint ClientBuildNumber;
            public uint ClientHardwareId;
            public ushort ClientProductId;
            public ushort OutBufCountHost;
            public ushort OutBufCountClient;
            public ushort OutBufLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 261)]
            public string DeviceId;
        }

        /*[StructLayout(LayoutKind.Sequential)]
        private struct WTS_CLIENT_DISPLAY
        {
            public uint HorizontalResolution;
            public uint VerticalResolution;
            public uint ColorDepth;
        }*/

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WTS_SESSION_INFO
        {
            public int SessionID;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pWinStationName;

            public WTS_CONNECTSTATE_CLASS State;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct WTSINFOEX_LEVEL_W
        { //Union
            [FieldOffset(0)]
            public WTSINFOEX_LEVEL1_W WTSInfoExLevel1;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WTSINFOEX
        {
            public int Level;
            public WTSINFOEX_LEVEL_W Data;
        }

        private enum WTS_INFO_CLASS : uint
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType,
            WTSIdleTime,
            WTSLogonTime,
            WTSIncomingBytes,
            WTSOutgoingBytes,
            WTSIncomingFrames,
            WTSOutgoingFrames,
            WTSClientInfo,
            WTSSessionInfo,
            WTSSessionInfoEx,
            WTSConfigInfo,
            WTSValidationInfo,   // Info Class value used to fetch Validation Information through the WTSQuerySessionInformation
            WTSSessionAddressV4,
            WTSIsRemoteSession
        }

        [Flags]
        private enum SendMessageStyle : uint
        {
            MB_OK = 0x00000000,
            MB_OKCANCEL = 0x00000001,
            MB_ABORTRETRYIGNORE = 0x00000002,
            MB_YESNOCANCEL = 0x00000003,
            MB_YESNO = 0x00000004,
            MB_RETRYCANCEL = 0x00000005,
            MB_CANCELTRYCONTINUE = 0x00000006,
            MB_HELP = 0x00004000
        }

        private enum SendMessageResponse : uint
        {
            IDNONE = 0,
            IDOK = 1,
            IDCANCEL = 2,
            IDABORT = 3,
            IDRETRY = 4,
            IDIGNORE = 5,
            IDYES = 6,
            IDNO = 7,
            IDTRYAGAIN = 10,
            IDCONTINUE = 11,
            IDTIMEOUT = 32000,
            IDASYNC = 32001
        }

        [Flags]
        private enum ShutdownFlags : uint
        {
            WTS_WSD_LOGOFF = 0x00000001,
            WTS_WSD_SHUTDOWN = 0x00000002,
            WTS_WSD_REBOOT = 0x00000004,
            WTS_WSD_POWEROFF = 0x00000008,
            WTS_WSD_FASTREBOOT = 0x00000010
        }

        [Flags]
        private enum WaitSystemEventFlags : uint
        {
            WTS_EVENT_NONE = 0x00000000,
            WTS_EVENT_CREATE = 0x00000001,
            WTS_EVENT_DELETE = 0x00000002,
            WTS_EVENT_RENAME = 0x00000004,
            WTS_EVENT_CONNECT = 0x00000008,
            WTS_EVENT_DISCONNECT = 0x00000010,
            WTS_EVENT_LOGON = 0x00000020,
            WTS_EVENT_LOGOFF = 0x00000040,
            WTS_EVENT_STATECHANGE = 0x00000080,
            WTS_EVENT_LICENSE = 0x00000100,
            WTS_EVENT_ALL = 0x7fffffff,
            WTS_EVENT_FLUSH = 0x80000000
        }

        /*private enum WTS_TYPE_CLASS : uint
        {
            WTS_PROCESS_INFO = 0,
            WTS_PROCESS_INFO_EX = 1,
            WTS_SESSION_INFO_1 = 2
        }*/

        [DllImport("wtsapi32.dll", EntryPoint = "WTSQuerySessionInformationW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WTSQuerySessionInformation(
                 IntPtr hServer,
                 int SessionId,
                 [MarshalAs(UnmanagedType.U4)] WTS_INFO_CLASS WTSInfoClass,
                 ref IntPtr ppSessionInfo,
                 ref int pBytesReturned);

        [DllImport("wtsapi32.dll", EntryPoint = "WTSEnumerateSessionsW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WTSEnumerateSessions(
                 IntPtr hServer,
                 [MarshalAs(UnmanagedType.U4)] int Reserved,
                 [MarshalAs(UnmanagedType.U4)] int Version,
                 ref IntPtr ppSessionInfo,
                 [MarshalAs(UnmanagedType.U4)] ref int pCount);

        [DllImport("wtsapi32.dll", EntryPoint = "WTSDisconnectSession", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WTSDisconnectSession([In] IntPtr hServer, [In][MarshalAs(UnmanagedType.I4)] int SessionId, [In] bool bWait);

        [DllImport("wtsapi32.dll", EntryPoint = "WTSLogoffSession", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WTSLogoffSession([In] IntPtr hServer, [In][MarshalAs(UnmanagedType.I4)] int SessionId, [In] bool bWait);

        [DllImport("wtsapi32.dll", EntryPoint = "WTSSendMessageW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WTSSendMessage(
          [In] IntPtr hServer,
          [In][MarshalAs(UnmanagedType.I4)] int SessionId,
          [In][MarshalAs(UnmanagedType.LPWStr)] string pTitle,
          [In][MarshalAs(UnmanagedType.U4)] int TitleLength,
          [In][MarshalAs(UnmanagedType.LPWStr)] string pMessage,
          [In][MarshalAs(UnmanagedType.U4)] int MessageLength,
          [In][MarshalAs(UnmanagedType.U4)] SendMessageStyle Style,
          [In][MarshalAs(UnmanagedType.U4)] int Timeout,
          [Out][MarshalAs(UnmanagedType.U4)] out SendMessageResponse pResponse,
          [In] bool bWait
        );

        [DllImport("wtsapi32.dll", EntryPoint = "WTSShutdownSystem", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WTSShutdownSystem([In] IntPtr hServer, [In][MarshalAs(UnmanagedType.U4)] ShutdownFlags ShutdownFlag);

        [DllImport("wtsapi32.dll", EntryPoint = "WTSWaitSystemEvent", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WTSWaitSystemEvent(
            [In] IntPtr hServer,
            [In][MarshalAs(UnmanagedType.U4)] WaitSystemEventFlags EventMask,
            [Out][MarshalAs(UnmanagedType.U4)] out WaitSystemEventFlags pEventFlags);

        [DllImport("wtsapi32.dll", EntryPoint = "WTSOpenServerW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPWStr)] string pServerName);

        [DllImport("wtsapi32.dll", EntryPoint = "WTSCloseServer", SetLastError = true)]
        private static extern void WTSCloseServer(IntPtr hServer);

        [DllImport("wtsapi32.dll", EntryPoint = "WTSFreeMemory", SetLastError = true)]
        private static extern void WTSFreeMemory(IntPtr pMemory);

        /*[DllImport("wtsapi32.dll", EntryPoint = "WTSFreeMemoryExW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern void WTSFreeMemoryEx([In][MarshalAs(UnmanagedType.U4)] WTS_TYPE_CLASS WTSTypeClass, [In] IntPtr pMemory, [In] ulong NumberOfEntries);*/

        public static IDictionary<int, string> SendMessages(string computer, IEnumerable<int> sessionIds, string title, string message, bool wait = false, int timeout = 0, CancellationToken? cancellationToken = null)
        {
            if (string.IsNullOrEmpty(computer))
                throw new ArgumentNullException("computer");

            if (!sessionIds.Any())
                throw new ArgumentNullException("sessionIds");

            var serverHandle = WTSOpenServer(computer);

            if (serverHandle == IntPtr.Zero)
                throw new Win32Exception("WTSOpenServer", new Win32Exception(Marshal.GetLastWin32Error()));

            var responses = new ConcurrentDictionary<int, string>(16, 0);

            if (string.IsNullOrEmpty(title))
                title = " ";

            try
            {
                Parallel.ForEach(sessionIds, new ParallelOptions
                {
                    MaxDegreeOfParallelism = 16,
                    TaskScheduler = TaskScheduler.Default,
                    CancellationToken = cancellationToken ?? CancellationToken.None
                },
                sessionId =>
                {
                    SendMessageResponse pResponse;
                    if (!WTSSendMessage(
                        serverHandle,
                        sessionId,
                        title,
                        Encoding.Unicode.GetByteCount(title),
                        message,
                        Encoding.Unicode.GetByteCount(message),
                        wait ? SendMessageStyle.MB_OKCANCEL : SendMessageStyle.MB_OK,
                        timeout,
                        out pResponse,
                        wait))
                        throw new Win32Exception("WTSSendMessage", new Win32Exception(Marshal.GetLastWin32Error()));

                    if (wait && pResponse == SendMessageResponse.IDNONE)
                        pResponse = SendMessageResponse.IDTIMEOUT;

                    responses[sessionId] = pResponse.ToString();
                });
            }
            catch (OperationCanceledException) { }
            finally
            {
                WTSCloseServer(serverHandle);
                serverHandle = IntPtr.Zero;
            }

            return responses;
        }

        public static void DisconnectSessions(string computer, IEnumerable<int> sessionIds, bool wait = false, CancellationToken? cancellationToken = null)
        {
            if (string.IsNullOrEmpty(computer))
                throw new ArgumentNullException("computer");

            if (!sessionIds.Any())
                throw new ArgumentNullException("sessionIds");

            var serverHandle = WTSOpenServer(computer);

            if (serverHandle == IntPtr.Zero)
                throw new Win32Exception("WTSOpenServer", new Win32Exception(Marshal.GetLastWin32Error()));

            try
            {
                Parallel.ForEach(sessionIds, new ParallelOptions
                {
                    MaxDegreeOfParallelism = 32,
                    TaskScheduler = TaskScheduler.Default,
                    CancellationToken = cancellationToken ?? CancellationToken.None
                },
                sessionId =>
                {
                    if (!WTSDisconnectSession(serverHandle, sessionId, wait))
                        throw new Win32Exception("WTSDisconnectSession", new Win32Exception(Marshal.GetLastWin32Error()));
                });
            }
            catch (OperationCanceledException) { }
            finally
            {
                WTSCloseServer(serverHandle);
                serverHandle = IntPtr.Zero;
            }
        }

        public static void LogoffSessions(string computer, IEnumerable<int> sessionIds, bool wait = false, CancellationToken? cancellationToken = null)
        {
            if (string.IsNullOrEmpty(computer))
                throw new ArgumentNullException("computer");

            if (!sessionIds.Any())
                throw new ArgumentNullException("sessionIds");

            var serverHandle = WTSOpenServer(computer);

            if (serverHandle == IntPtr.Zero)
                throw new Win32Exception("WTSOpenServer", new Win32Exception(Marshal.GetLastWin32Error()));

            try
            {
                Parallel.ForEach(sessionIds, new ParallelOptions
                {
                    MaxDegreeOfParallelism = 32,
                    TaskScheduler = TaskScheduler.Default,
                    CancellationToken = cancellationToken ?? CancellationToken.None
                },
                sessionId =>
                {
                    if (!WTSLogoffSession(serverHandle, sessionId, wait))
                        throw new Win32Exception("WTSLogoffSession", new Win32Exception(Marshal.GetLastWin32Error()));
                });
            }
            catch (OperationCanceledException) { }
            finally
            {
                WTSCloseServer(serverHandle);
                serverHandle = IntPtr.Zero;
            }
        }

        public static IDictionary<string, List<ClientInfo>> GetWTSSessionInformation()
        {
            return GetWTSSessionInformation(null);
        }

        public static IDictionary<string, List<ClientInfo>> GetWTSSessionInformation(IEnumerable<string> computers, CancellationToken? cancellationToken = null)
        {
            var datasize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));

            var dict = new ConcurrentDictionary<string, List<ClientInfo>>(4, 0, StringComparer.OrdinalIgnoreCase);

            Parallel.ForEach((computers ?? new string[] { Environment.MachineName }).Where(x => !string.IsNullOrEmpty(x)), new ParallelOptions
            {
                MaxDegreeOfParallelism = 4,
                TaskScheduler = TaskScheduler.Default,
                CancellationToken = cancellationToken ?? CancellationToken.None
            },
            computer =>
            {
                var list = new ConcurrentBag<ClientInfo>();

                var serverHandle = WTSOpenServer(computer);

                if (serverHandle == IntPtr.Zero)
                    throw new Win32Exception("WTSOpenServer", new Win32Exception(Marshal.GetLastWin32Error()));

                var ppSessionInfo = IntPtr.Zero;

                try
                {
                    var count = 0;
                    if (!WTSEnumerateSessions(serverHandle, 0, 1, ref ppSessionInfo, ref count))
                        throw new AggregateException(new Win32Exception("WTSEnumerateSessions"), new Win32Exception(Marshal.GetLastWin32Error()));

                    try
                    {
                        Parallel.For(0, count, new ParallelOptions
                        {
                            MaxDegreeOfParallelism = 4,
                            TaskScheduler = TaskScheduler.Default,
                            CancellationToken = cancellationToken ?? CancellationToken.None
                        },
                        index =>
                        {
                            var wtsSessionInfo = (WTS_SESSION_INFO)Marshal.PtrToStructure(new IntPtr((long)ppSessionInfo + (datasize * index)), typeof(WTS_SESSION_INFO));
                            if (wtsSessionInfo.SessionID == 0)
                                return;

                            var clientInfo = new ClientInfo(computer, wtsSessionInfo.SessionID)
                            {
                                WinStationName = wtsSessionInfo.pWinStationName
                            };

                            var ppQueryInfo = IntPtr.Zero;
                            var ppBytesReturned = 0;

                            try
                            {
                                if (!WTSQuerySessionInformation(serverHandle, clientInfo.SessionId, WTS_INFO_CLASS.WTSSessionInfoEx, ref ppQueryInfo, ref ppBytesReturned))
                                    throw new Win32Exception("WTSQuerySessionInformation.WTSSessionInfoEx", new Win32Exception(Marshal.GetLastWin32Error()));

                                if (ppQueryInfo != IntPtr.Zero)
                                {
                                    if (ppBytesReturned == Marshal.SizeOf(typeof(WTSINFOEX)))
                                    {
                                        var wtsInfoEx = (WTSINFOEX)Marshal.PtrToStructure(ppQueryInfo, typeof(WTSINFOEX));
                                        clientInfo.FromWtsInfoEx(wtsInfoEx);
                                    }

                                    WTSFreeMemory(ppQueryInfo);
                                    ppQueryInfo = IntPtr.Zero;
                                }

                                if (string.IsNullOrEmpty(clientInfo.UserName))
                                {
                                    if (!WTSQuerySessionInformation(serverHandle, clientInfo.SessionId, WTS_INFO_CLASS.WTSUserName, ref ppQueryInfo, ref ppBytesReturned))
                                        throw new Win32Exception("WTSQuerySessionInformation.WTSUserName", new Win32Exception(Marshal.GetLastWin32Error()));

                                    if (ppQueryInfo != IntPtr.Zero)
                                    {
                                        if (ppBytesReturned > 0)
                                        {
                                            var userName = Marshal.PtrToStringUni(ppQueryInfo);
                                            if (!string.IsNullOrEmpty(userName))
                                                clientInfo.UserName = userName;
                                        }

                                        WTSFreeMemory(ppQueryInfo);
                                        ppQueryInfo = IntPtr.Zero;
                                    }
                                }

                                if (string.IsNullOrEmpty(clientInfo.Domain))
                                {
                                    if (!WTSQuerySessionInformation(serverHandle, clientInfo.SessionId, WTS_INFO_CLASS.WTSDomainName, ref ppQueryInfo, ref ppBytesReturned))
                                        throw new Win32Exception("WTSQuerySessionInformation.WTSDomainName", new Win32Exception(Marshal.GetLastWin32Error()));

                                    if (ppQueryInfo != IntPtr.Zero)
                                    {
                                        if (ppBytesReturned > 0)
                                        {
                                            var domain = Marshal.PtrToStringUni(ppQueryInfo);
                                            if (!string.IsNullOrEmpty(domain))
                                                clientInfo.Domain = domain;
                                        }

                                        WTSFreeMemory(ppQueryInfo);
                                        ppQueryInfo = IntPtr.Zero;
                                    }
                                }

                                if (string.IsNullOrEmpty(clientInfo.WinStationName))
                                {
                                    if (!WTSQuerySessionInformation(serverHandle, clientInfo.SessionId, WTS_INFO_CLASS.WTSWinStationName, ref ppQueryInfo, ref ppBytesReturned))
                                        throw new Win32Exception("WTSQuerySessionInformation.WTSWinStationName", new Win32Exception(Marshal.GetLastWin32Error()));

                                    if (ppQueryInfo != IntPtr.Zero)
                                    {
                                        if (ppBytesReturned > 0)
                                        {
                                            var winStationName = Marshal.PtrToStringUni(ppQueryInfo);
                                            if (!string.IsNullOrEmpty(winStationName))
                                                clientInfo.WinStationName = winStationName;
                                        }

                                        WTSFreeMemory(ppQueryInfo);
                                        ppQueryInfo = IntPtr.Zero;
                                    }
                                }
                            }
                            catch (Win32Exception) { }
                            finally
                            {
                                if (ppQueryInfo != IntPtr.Zero)
                                {
                                    WTSFreeMemory(ppQueryInfo);
                                    ppQueryInfo = IntPtr.Zero;
                                }
                            }

                            list.Add(clientInfo);
                        });
                    }
                    finally
                    {
                        if (ppSessionInfo != IntPtr.Zero)
                        {
                            WTSFreeMemory(ppSessionInfo);
                            ppSessionInfo = IntPtr.Zero;
                        }
                    }
                }
                catch (OperationCanceledException) { }
                finally
                {
                    WTSCloseServer(serverHandle);
                    serverHandle = IntPtr.Zero;
                }

                dict[computer] = list.OrderBy(x => x.SessionId).ToList();
            });

            return dict;
        }

        public static void GetWTSClientConfigInformation(ClientInfo clientInfo, bool refreshSessionInformation = true)
        {
            var computer = clientInfo.Computer;

            var serverHandle = WTSOpenServer(computer);

            if (serverHandle == IntPtr.Zero)
                throw new Win32Exception("WTSOpenServer", new Win32Exception(Marshal.GetLastWin32Error()));

            var ppQueryInfo = IntPtr.Zero;
            var ppBytesReturned = 0;

            try
            {
                if (refreshSessionInformation)
                {
                    if (!WTSQuerySessionInformation(serverHandle, clientInfo.SessionId, WTS_INFO_CLASS.WTSSessionInfoEx, ref ppQueryInfo, ref ppBytesReturned))
                        throw new Win32Exception("WTSQuerySessionInformation.WTSSessionInfoEx", new Win32Exception(Marshal.GetLastWin32Error()));

                    if (ppQueryInfo != IntPtr.Zero)
                    {
                        if (ppBytesReturned == Marshal.SizeOf(typeof(WTSINFOEX)))
                        {
                            var wtsInfoEx = (WTSINFOEX)Marshal.PtrToStructure(ppQueryInfo, typeof(WTSINFOEX));
                            clientInfo.FromWtsInfoEx(wtsInfoEx);
                        }

                        WTSFreeMemory(ppQueryInfo);
                        ppQueryInfo = IntPtr.Zero;
                    }

                    if (string.IsNullOrEmpty(clientInfo.UserName))
                    {
                        if (!WTSQuerySessionInformation(serverHandle, clientInfo.SessionId, WTS_INFO_CLASS.WTSUserName, ref ppQueryInfo, ref ppBytesReturned))
                            throw new Win32Exception("WTSQuerySessionInformation.WTSUserName", new Win32Exception(Marshal.GetLastWin32Error()));

                        if (ppQueryInfo != IntPtr.Zero)
                        {
                            if (ppBytesReturned > 0)
                            {
                                var userName = Marshal.PtrToStringUni(ppQueryInfo);
                                if (!string.IsNullOrEmpty(userName))
                                    clientInfo.UserName = userName;
                            }

                            WTSFreeMemory(ppQueryInfo);
                            ppQueryInfo = IntPtr.Zero;
                        }
                    }

                    if (string.IsNullOrEmpty(clientInfo.Domain))
                    {
                        if (!WTSQuerySessionInformation(serverHandle, clientInfo.SessionId, WTS_INFO_CLASS.WTSDomainName, ref ppQueryInfo, ref ppBytesReturned))
                            throw new Win32Exception("WTSQuerySessionInformation.WTSDomainName", new Win32Exception(Marshal.GetLastWin32Error()));

                        if (ppQueryInfo != IntPtr.Zero)
                        {
                            if (ppBytesReturned > 0)
                            {
                                var domain = Marshal.PtrToStringUni(ppQueryInfo);
                                if (!string.IsNullOrEmpty(domain))
                                    clientInfo.Domain = domain;
                            }

                            WTSFreeMemory(ppQueryInfo);
                            ppQueryInfo = IntPtr.Zero;
                        }
                    }

                    if (string.IsNullOrEmpty(clientInfo.WinStationName))
                    {
                        if (!WTSQuerySessionInformation(serverHandle, clientInfo.SessionId, WTS_INFO_CLASS.WTSWinStationName, ref ppQueryInfo, ref ppBytesReturned))
                            throw new Win32Exception("WTSQuerySessionInformation.WTSWinStationName", new Win32Exception(Marshal.GetLastWin32Error()));

                        if (ppQueryInfo != IntPtr.Zero)
                        {
                            if (ppBytesReturned > 0)
                            {
                                var winStationName = Marshal.PtrToStringUni(ppQueryInfo);
                                if (!string.IsNullOrEmpty(winStationName))
                                    clientInfo.WinStationName = winStationName;
                            }

                            WTSFreeMemory(ppQueryInfo);
                            ppQueryInfo = IntPtr.Zero;
                        }
                    }
                }

                if (!WTSQuerySessionInformation(serverHandle, clientInfo.SessionId, WTS_INFO_CLASS.WTSClientInfo, ref ppQueryInfo, ref ppBytesReturned))
                    throw new Win32Exception("WTSQuerySessionInformation.WTSClientInfo", new Win32Exception(Marshal.GetLastWin32Error()));

                if (ppQueryInfo != IntPtr.Zero)
                {
                    if (ppBytesReturned == Marshal.SizeOf(typeof(WTSCLIENTW)))
                    {
                        var wtsClientInfo = (WTSCLIENTW)Marshal.PtrToStructure(ppQueryInfo, typeof(WTSCLIENTW));
                        clientInfo.FromWtsClientW(wtsClientInfo);
                    }

                    WTSFreeMemory(ppQueryInfo);
                    ppQueryInfo = IntPtr.Zero;
                }

                if (!WTSQuerySessionInformation(serverHandle, clientInfo.SessionId, WTS_INFO_CLASS.WTSConfigInfo, ref ppQueryInfo, ref ppBytesReturned))
                    throw new Win32Exception("WTSQuerySessionInformation.WTSConfigInfo", new Win32Exception(Marshal.GetLastWin32Error()));

                if (ppQueryInfo != IntPtr.Zero)
                {
                    if (ppBytesReturned == Marshal.SizeOf(typeof(WTSCONFIGINFOW)))
                    {
                        var wtsConfigInfo = (WTSCONFIGINFOW)Marshal.PtrToStructure(ppQueryInfo, typeof(WTSCONFIGINFOW));
                        clientInfo.FromWtsConfigInfoW(wtsConfigInfo);
                    }

                    WTSFreeMemory(ppQueryInfo);
                    ppQueryInfo = IntPtr.Zero;
                }

                var clientProtocolType = ushort.MaxValue;
                if (!WTSQuerySessionInformation(serverHandle, clientInfo.SessionId, WTS_INFO_CLASS.WTSClientProtocolType, ref ppQueryInfo, ref ppBytesReturned))
                    throw new Win32Exception("WTSQuerySessionInformation.WTSClientProtocolType", new Win32Exception(Marshal.GetLastWin32Error()));

                if (ppQueryInfo != IntPtr.Zero)
                {
                    if (ppBytesReturned == Marshal.SizeOf(clientProtocolType.GetType()))
                        clientInfo.Detail.ClientProtocolType = (short)(ushort)Marshal.PtrToStructure(ppQueryInfo, clientProtocolType.GetType());

                    WTSFreeMemory(ppQueryInfo);
                    ppQueryInfo = IntPtr.Zero;
                }
            }
            finally
            {
                if (ppQueryInfo != IntPtr.Zero)
                {
                    WTSFreeMemory(ppQueryInfo);
                    ppQueryInfo = IntPtr.Zero;
                }

                WTSCloseServer(serverHandle);
                serverHandle = IntPtr.Zero;
            }
        }
    }
}
