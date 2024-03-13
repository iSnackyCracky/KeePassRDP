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

using KeePass.Resources;
using KeePass.UI;
using KeePassRDP.Commands;
using KeePassRDP.Generator;
using KeePassRDP.Utils;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace KeePassRDP
{
    public partial class KprOptionsForm
    {
        private static readonly Lazy<X509Chain> _x509Chain = new Lazy<X509Chain>(() => {
            var chain = new X509Chain();
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            return chain;
        }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private void Init_TabExecutable()
        {
            if (_tabExecutableInitialized)
                return;

            _tabExecutableInitialized = true;

            tabExecutable.UseWaitCursor = true;
            tabExecutable.SuspendLayout();

            KprResourceManager.Instance.TranslateMany(
                grpMstscParameters,
                chkMstscUseFullscreen,
                chkMstscUsePublic,
                chkMstscUseAdmin,
                chkMstscUseRestrictedAdmin,
                chkMstscUseRemoteGuard,
                chkMstscUseSpan,
                chkMstscUseMultimon,
                grpMstscAutomation,
                lblWidth,
                lblHeight,
                chkMstscReplaceTitle,
                chkMstscCleanupRegistry,
                chkMstscConfirmCertificate,
                chkMstscSignRdpFiles,
                txtMstscSignRdpFiles,
                lblMstscExecutable
            );

            // Set form elements to match previously saved options.
            chkMstscUseFullscreen.Checked = _config.MstscUseFullscreen;
            chkMstscUsePublic.Checked = _config.MstscUsePublic;
            chkMstscUseAdmin.Checked = _config.MstscUseAdmin;
            chkMstscUseRestrictedAdmin.Checked = _config.MstscUseRestrictedAdmin;
            chkMstscUseRemoteGuard.Checked = _config.MstscUseRemoteGuard;
            chkMstscUseSpan.Checked = _config.MstscUseSpan;
            chkMstscUseMultimon.Checked = _config.MstscUseMultimon;
            chkMstscReplaceTitle.Checked = _config.MstscReplaceTitle;
            chkMstscCleanupRegistry.Checked = _config.MstscCleanupRegistry;
            chkMstscConfirmCertificate.Checked = _config.MstscConfirmCertificate;
            if (!string.IsNullOrWhiteSpace(_config.MstscSignRdpFiles))
            {
                chkMstscSignRdpFiles.Checked = true;
                txtMstscSignRdpFiles.Text = _config.MstscSignRdpFiles;
            }

            numMstscWidth.Maximum = Screen.PrimaryScreen.Bounds.Width;
            numMstscWidth.Value = Math.Max(Math.Min(_config.MstscWidth, numMstscWidth.Maximum), numMstscWidth.Minimum);
            _config.MstscWidth = (int)numMstscWidth.Value;

            numMstscHeight.Maximum = Screen.PrimaryScreen.Bounds.Height;
            numMstscHeight.Value = Math.Max(Math.Min(_config.MstscHeight, numMstscHeight.Maximum), numMstscHeight.Minimum);
            _config.MstscHeight = (int)numMstscHeight.Value;

            chkMstsc_CheckedChanged(null, EventArgs.Empty);

            if (chkMstscSignRdpFiles.Checked &&
                !string.IsNullOrWhiteSpace(txtMstscSignRdpFiles.Text) &&
                txtMstscSignRdpFiles.Text != KprResourceManager.Instance["Click here to select a certificate from the user store."])
            {
                var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

                try
                {
                    store.Open(OpenFlags.ReadOnly);

                    var cert = store.Certificates.Find(X509FindType.FindByThumbprint, txtMstscSignRdpFiles.Text, false).OfType<X509Certificate2>().FirstOrDefault();

                    if (_x509Chain.IsValueCreated)
                        _x509Chain.Value.Reset();

                    txtMstscSignRdpFiles.BackColor = cert != null && (cert.Verify() || _x509Chain.Value.Build(cert)) ? Color.White : Color.LightCoral;
                }
                catch (CryptographicException)
                {
                }
                finally
                {
                    store.Close();
                }
            }

            cbMstscExecutable.Items.AddRange(new[] { typeof(MstscCommand).Name, typeof(FreeRdpCommand).Name });
            cbMstscExecutable.SelectedIndex = !string.IsNullOrWhiteSpace(_config.MstscExecutable) ? Math.Max(0, cbMstscExecutable.Items.IndexOf(_config.MstscExecutable.Split(new[] { ':' }, 2).FirstOrDefault())) : 0;
            cbMstscExecutable.Width = Math.Max(grpMstscAutomation.Width - lblMstscExecutable.Width + 3, 0);

            grpMstscParameters.Enabled = File.Exists(KeePassRDPExt.MstscPath);

            tabExecutable.ResumeLayout(false);
            tabExecutable.UseWaitCursor = false;
        }

        private void chkMstsc_CheckedChanged(object sender, EventArgs e)
        {
            lblWidth.Enabled =
                lblHeight.Enabled =
                numMstscWidth.Enabled =
                numMstscHeight.Enabled = !(chkMstscUseFullscreen.Checked || chkMstscUseMultimon.Checked || chkMstscUseSpan.Checked);
        }

        private void grpMstscAutomation_SizeChanged(object sender, EventArgs e)
        {
            txtMstscSignRdpFiles.Width = grpMstscAutomation.Width - 12;
        }

        private void chkMstscSignRdpFiles_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkMstscSignRdpFiles.Checked)
                txtMstscSignRdpFiles.Text = string.Empty;

            txtMstscSignRdpFiles.Visible =
                txtMstscSignRdpFiles.Enabled =
                cmdSelfSignedCertificate.Visible =
                cmdSelfSignedCertificate.Enabled = chkMstscSignRdpFiles.Checked;
        }

        private void txtMstscSignRdpFiles_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMstscSignRdpFiles.Text))
                txtMstscSignRdpFiles.Text = KprResourceManager.Instance["Click here to select a certificate from the user store."];
            else if(txtMstscSignRdpFiles.Text == KprResourceManager.Instance["Click here to select a certificate from the user store."])
                txtMstscSignRdpFiles.BackColor = default(Color);
        }

        private void txtMstscSignRdpFiles_Click(object sender, EventArgs e)
        {
            ResetActiveControl(txtMstscSignRdpFiles);

            var oid = CryptoConfig.MapNameToOID(RdpFile.RdpSignatureAlgorithmName);
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            try
            {
                store.Open(OpenFlags.ReadOnly);

                // pick a certificate from the store
                var cert = X509Certificate2UI.SelectFromCollection(
                    new X509Certificate2Collection(
                        store.Certificates
                            .OfType<X509Certificate2>()
                            .Where(x => x.HasPrivateKey && x.SignatureAlgorithm.Value == oid)
                            .OrderByDescending(x=> x.Issuer.StartsWith(Util.KeePassRDP) || x.Subject.StartsWith(Util.KeePassRDP) ? 1 : 2)
                            .ThenByDescending(x => x.Issuer, StringComparer.OrdinalIgnoreCase)
                            .ThenByDescending(x => x.Subject, StringComparer.OrdinalIgnoreCase)
                            .ToArray()),
                    Util.KeePassRDP,
                    KprResourceManager.Instance["Select a certificate from the list for signing .rdp files:"],
                    X509SelectionFlag.SingleSelection,
                    Handle)
                    .OfType<X509Certificate2>()
                    .FirstOrDefault(x => x.HasPrivateKey && x.SignatureAlgorithm.Value == oid);

                if (cert == null)
                    return;

                if (_x509Chain.IsValueCreated)
                    _x509Chain.Value.Reset();

                var thumbprint = cert.Thumbprint;
                txtMstscSignRdpFiles.Text = thumbprint;
                txtMstscSignRdpFiles.BackColor = cert.Verify() || _x509Chain.Value.Build(cert) ? Color.White : Color.LightCoral;
            }
            finally
            {
                store.Close();
            }
        }

        private void cmdSelfSignedCertificate_Click(object sender, EventArgs e)
        {
            if (VistaTaskDialog.ShowMessageBoxEx(
                KprResourceManager.Instance["This will create a self-signed certificate that can be used for signing .rdp files and save it into the current users \"my\" and \"root\" certificate store."],
                KprResourceManager.Instance["Continue?"],
                Util.KeePassRDP,
                VtdIcon.Information,
                this,
                KprResourceManager.Instance["&Yes"], 0,
                KprResourceManager.Instance["&No"], 1) != 0)
                return;

            try
            {
                var cngKeyExists = CngKey.Exists(Util.KeePassRDP, CngProvider.MicrosoftSoftwareKeyStorageProvider, CngKeyOpenOptions.UserKey);
                var cngKeyCaExists = CngKey.Exists(string.Format("{0} CA", Util.KeePassRDP), CngProvider.MicrosoftSoftwareKeyStorageProvider, CngKeyOpenOptions.UserKey);

                if (cngKeyExists || cngKeyCaExists)
                {
                    if (VistaTaskDialog.ShowMessageBoxEx(
                        KprResourceManager.Instance["Existing private keys will be overwritten, making previously generated certificates unusable for signing."],
                        KprResourceManager.Instance["Really continue?"],
                        Util.KeePassRDP,
                        VtdIcon.Warning,
                        this,
                        KprResourceManager.Instance["&Yes"], 0,
                        KprResourceManager.Instance["&No"], 1) != 0)
                        return;

                    if (cngKeyExists)
                        using (var cngKey = CngKey.Open(Util.KeePassRDP, CngProvider.MicrosoftSoftwareKeyStorageProvider))
                            cngKey.Delete();

                    if (cngKeyCaExists)
                        using (var cngKeyCa = CngKey.Open(string.Format("{0} CA", Util.KeePassRDP), CngProvider.MicrosoftSoftwareKeyStorageProvider))
                            cngKeyCa.Delete();
                }

                var keyParams = new CngKeyCreationParameters
                {
                    Provider = CngProvider.MicrosoftSoftwareKeyStorageProvider,
                    KeyUsage = CngKeyUsages.Signing,
                    KeyCreationOptions = CngKeyCreationOptions.None,
                    UIPolicy = new CngUIPolicy(CngUIProtectionLevels.None),
                    ExportPolicy = CngExportPolicies.AllowPlaintextExport,
                    ParentWindowHandle = Handle
                };
                keyParams.Parameters.Add(new CngProperty("Length", BitConverter.GetBytes(4096), CngPropertyOptions.None));

                using (var cngKey = CngKey.Create(new CngAlgorithm("RSA"), Util.KeePassRDP, keyParams))
                using (var cngKeyCa = CngKey.Create(new CngAlgorithm("RSA"), string.Format("{0} CA", Util.KeePassRDP), keyParams))
                {
                    var caCert = new X509Certificate2(
                        SelfSignedCertificate.CreateSelfSignedCertificate(
                            cngKeyCa,
                            true,
                            string.Format("CN={0} CA", Util.KeePassRDP),
                            SelfSignedCertificate.X509CertificateCreationOptions.None,
                            CryptoConfig.MapNameToOID(RdpFile.RdpSignatureAlgorithmName),
                            DateTime.UtcNow,
                            DateTime.UtcNow.AddYears(10),
                            new X509ExtensionCollection()
                            {
                                new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyCertSign, true),
                                new X509BasicConstraintsExtension(true, true, 1, true),
                                new X509EnhancedKeyUsageExtension(new OidCollection
                                {
                                    new Oid("1.3.6.1.5.5.7.3.1", "Server Authentication"),
                                    new Oid("1.3.6.1.5.5.7.3.2", "Client Authentication"),
                                    new Oid("1.3.6.1.5.5.7.3.3", "Code Signing"),
                                    new Oid("1.3.6.1.5.5.7.3.8", "Time Stamping")
                                }, true)
                            }).DangerousGetHandle());

                    var tempCert = new X509Certificate2(
                        SelfSignedCertificate.CreateSelfSignedCertificate(
                            cngKey,
                            true,
                            string.Format("CN={0}", Util.KeePassRDP),
                            SelfSignedCertificate.X509CertificateCreationOptions.None,
                            CryptoConfig.MapNameToOID(RdpFile.RdpSignatureAlgorithmName),
                            DateTime.UtcNow,
                            DateTime.UtcNow.AddYears(10),
                            new X509ExtensionCollection()
                            {
                                new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true),
                                new X509EnhancedKeyUsageExtension(new OidCollection
                                {
                                    new Oid("1.3.6.1.5.5.7.3.1", "Server Authentication"),
                                    new Oid("1.3.6.1.5.5.7.3.2", "Client Authentication"),
                                    new Oid("1.3.6.1.4.1.311.54.1.2", "Remote Desktop Authentication")
                                }, true)
                            }).DangerousGetHandle());

                    var cert = SelfSignedCertificate.SignCertificate(cngKey, tempCert, cngKeyCa, caCert, CryptoConfig.MapNameToOID(RdpFile.RdpSignatureAlgorithmName));
                    var ca = new X509Certificate2(caCert.Export(X509ContentType.Cert));
                    cngKeyCa.Delete();

                    var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);

                    try
                    {
                        store.Open(OpenFlags.ReadWrite);
                        store.Add(ca);
                    }
                    finally
                    {
                        store.Close();
                    }

                    store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

                    try
                    {
                        store.Open(OpenFlags.ReadWrite);
                        store.Add(cert);
                    }
                    finally
                    {
                        store.Close();
                    }
                }

                VistaTaskDialog.ShowMessageBoxEx(
                    KprResourceManager.Instance["Self-signed certificate created successfully."],
                    null,
                    Util.KeePassRDP,
                    VtdIcon.Information,
                    this,
                    null, 0, null, 0);
            }
            catch (Exception ex)
            {
                VistaTaskDialog.ShowMessageBoxEx(
                    string.Format(KprResourceManager.Instance["Failed to create self-signed certificate:{0}{1}"], Environment.NewLine, ex.ToString()),
                    null,
                    Util.KeePassRDP + " - " + KPRes.FatalError,
                    VtdIcon.Error,
                    this,
                    null, 0, null, 0);
            }
        }

        private void txtMstscSignRdpFiles_ShowToolTip(object sender, EventArgs e)
        {
            var timer = sender as Timer;
            timer.Enabled = false;

            var control = timer.Tag as Control;
            if (!string.IsNullOrEmpty(ttMstscAutomation.GetToolTip(control)))
                return;

            var point = control.PointToClient(Cursor.Position);
            var size = _cursorSize.Value;

            if (!size.IsEmpty)
                point.Y += size.Height / 2;

            point.X += 2;
            point.Y += 1;

            ttMstscAutomation.Show(
                KprResourceManager.Instance["Click here to select a certificate from the user store."],
                control,
                point,
                ttMstscAutomation.AutoPopDelay);
        }

        private void txtMstscSignRdpFiles_MouseEnter(object sender, EventArgs e)
        {
            _tooltipTimer.Tag = sender;
            _tooltipTimer.Tick += txtMstscSignRdpFiles_ShowToolTip;
            if (_tooltipTimer.Enabled)
                _tooltipTimer.Enabled = false;
            _tooltipTimer.Enabled = !_lastTooltipMousePosition.HasValue;
        }

        private void txtMstscSignRdpFiles_MouseLeave(object sender, EventArgs e)
        {
            _tooltipTimer.Tick -= txtMstscSignRdpFiles_ShowToolTip;
            if (_tooltipTimer.Enabled)
                _tooltipTimer.Enabled = false;
            ttMstscAutomation.Hide(sender as Control);
            _lastTooltipMousePosition = null;
        }

        private void txtMstscSignRdpFiles_MouseMove(object sender, MouseEventArgs e)
        {
            if (_lastTooltipMousePosition.HasValue && _lastTooltipMousePosition.Value == e.Location)
                return;
            _lastTooltipMousePosition = e.Location;
            if (_tooltipTimer.Enabled)
                _tooltipTimer.Enabled = false;
            _tooltipTimer.Enabled = true;
        }

        private void txtMstscSignRdpFiles_Enter(object sender, EventArgs e)
        {
            if (_tooltipTimer.Enabled)
                _tooltipTimer.Enabled = false;
            ttMstscAutomation.Hide(sender as Control);
        }

        private void txtMstscSignRdpFiles_Leave(object sender, EventArgs e)
        {
            if (_tooltipTimer.Enabled)
                _tooltipTimer.Enabled = false;
            ttMstscAutomation.Hide(sender as Control);
            _lastTooltipMousePosition = null;
        }
    }
}