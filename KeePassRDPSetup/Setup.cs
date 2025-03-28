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

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;
using WixSharp;
using WixSharp.CommonTasks;
using WixToolset.Dtf.WindowsInstaller;
using Assembly = System.Reflection.Assembly;
using File = System.IO.File;
using FileAttributes = System.IO.FileAttributes;

namespace KeePassRDP
{
    public class Setup
    {
#if DEBUG
        private const string Configuration = "Debug";
#else
        private const string Configuration = "Release";
#endif
        static void Main()
        {
            var rootDir = Path.GetDirectoryName(Environment.CurrentDirectory);
            var outDir = Path.Combine(Environment.CurrentDirectory, "bin", Configuration);

            // Convert license to RTF.
            var licenseFile = Path.Combine(outDir, "license.rtf");
            var licenseSourceFile = Path.Combine(rootDir, "COPYING");
            if (!File.Exists(licenseFile) || File.GetLastWriteTimeUtc(licenseFile) < File.GetLastWriteTimeUtc(licenseSourceFile))
                using (var richTextBox = new RichTextBox())
                {
                    richTextBox.LoadFile(licenseSourceFile, RichTextBoxStreamType.PlainText);
                    richTextBox.SaveFile(licenseFile, RichTextBoxStreamType.RichText);
                }

            // Create nice looking icon and bitmaps.
            var bannrbmpFile = Path.Combine(outDir, "bannrbmp.bmp");
            var dlgbmpFile = Path.Combine(outDir, "dlgbmp.bmp");
            var icoFile = Path.Combine(outDir, "icon.ico");
            if (!File.Exists(icoFile))
                using (var shield = SystemIcons.Shield)
                using (var icon = Icon.ExtractAssociatedIcon(Path.Combine(Environment.SystemDirectory, "mstsc.exe")))
                using (var tmpBmp = new Bitmap(48, 48, PixelFormat.Format32bppArgb))
                {
                    using (var graphics = Graphics.FromImage(tmpBmp))
                    {
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;

                        graphics.DrawIconUnstretched(icon, new Rectangle(new Point((tmpBmp.Width - icon.Width) / 2, (tmpBmp.Height - icon.Height) / 2), icon.Size));
                        graphics.DrawIcon(shield, new Rectangle(tmpBmp.Width - 27, tmpBmp.Height - 27, 22, 22));
                    }

                    using (var fs = File.Create(bannrbmpFile))
                    using (var bannrbmp = new Bitmap(493, 58, PixelFormat.Format32bppArgb))
                    {
                        using (var graphics = Graphics.FromImage(bannrbmp))
                        {
                            graphics.Clear(Color.White);
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.SmoothingMode = SmoothingMode.AntiAlias;

                            using (var imageAttributes = new ImageAttributes())
                            {
                                var colorMatrix = new ColorMatrix
                                {
                                    Matrix33 = .95f
                                };
                                imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                                var gap = (bannrbmp.Height - tmpBmp.Height) / 2;
                                graphics.DrawImage(tmpBmp, new Rectangle(new Point(bannrbmp.Width - tmpBmp.Width - gap, gap), tmpBmp.Size), 0, 0, tmpBmp.Width, tmpBmp.Height, GraphicsUnit.Pixel, imageAttributes);
                            }
                        }
                        bannrbmp.Save(fs, ImageFormat.Bmp);
                    }
                    using (var fs = File.Create(dlgbmpFile))
                    using (var dlgbmp = new Bitmap(493, 312, PixelFormat.Format32bppArgb))
                    {
                        using (var graphics = Graphics.FromImage(dlgbmp))
                        {
                            graphics.Clear(Color.White);
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                            using (var brush = new SolidBrush(Color.FromArgb(235, Color.RoyalBlue)))
                                graphics.FillRectangle(brush, new Rectangle(Point.Empty, new Size(150, dlgbmp.Height)));

                            graphics.TranslateTransform(0, dlgbmp.Height);
                            graphics.RotateTransform(-90);
                            using (var font = new Font(SystemFonts.DialogFont.OriginalFontName, 18, FontStyle.Regular))
                            using (var sf = new StringFormat
                            {
                                Alignment = StringAlignment.Near,
                                LineAlignment = StringAlignment.Near,
                                FormatFlags = StringFormatFlags.NoWrap,
                                Trimming = StringTrimming.None
                            })
                            {
                                var textSize = graphics.MeasureString("KeePassRDP", font, SizeF.Empty, sf);
                                using (var brush = new LinearGradientBrush(new RectangleF(new PointF(-210, 0), SizeF.Add(textSize, new SizeF(490, 0))), Color.Transparent, Color.Transparent, LinearGradientMode.Horizontal))
                                {
                                    brush.InterpolationColors = new ColorBlend
                                    {
                                        Colors = new[] { Color.FromArgb(230, Color.RoyalBlue), Color.FromArgb(250, Color.WhiteSmoke) },
                                        Positions = new[] { 0f, 1f }
                                    };
                                    graphics.DrawString("KeePassRDP", font, brush, new Point(-5, -5), sf);
                                }
                            }
                            graphics.ResetTransform();

                            using (var imageAttributes = new ImageAttributes())
                            {
                                var colorMatrix = new ColorMatrix
                                {
                                    Matrix33 = .85f
                                };
                                imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                                graphics.DrawImage(tmpBmp, new Rectangle(new Point(150 - tmpBmp.Width - 10, 10), tmpBmp.Size), 0, 0, tmpBmp.Width, tmpBmp.Height, GraphicsUnit.Pixel, imageAttributes);
                            }
                        }
                        dlgbmp.Save(fs, ImageFormat.Bmp);
                    }

                    using (var bmp = new Bitmap(tmpBmp.Width, tmpBmp.Height, tmpBmp.PixelFormat))
                    {
                        using (var graphics = Graphics.FromImage(bmp))
                        {
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            graphics.Clear(Color.Transparent);
                            using (var brush = new SolidBrush(Color.FromArgb(150, Color.DarkGray)))
                                graphics.FillEllipse(brush, new RectangleF(Point.Empty, bmp.Size));

                            using (var imageAttributes = new ImageAttributes())
                            {
                                var colorMatrix = new ColorMatrix
                                {
                                    Matrix33 = .9f
                                };
                                imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                                graphics.DrawImage(tmpBmp, new Rectangle(Point.Empty, tmpBmp.Size), 0, 0, tmpBmp.Width, tmpBmp.Height, GraphicsUnit.Pixel, imageAttributes);
                            }

                            using (var brush = new SolidBrush(Color.FromArgb(150, Color.WhiteSmoke)))
                            using (var pen = new Pen(brush, 1f))
                                graphics.DrawEllipse(pen, new RectangleF(new PointF(2 + pen.Width / 2, 2 + pen.Width / 2), new SizeF(bmp.Width - 2 - pen.Width, bmp.Height - 2 - pen.Width)));
                            using (var brush = new SolidBrush(Color.FromArgb(150, Color.Black)))
                            using (var pen = new Pen(brush, 2f))
                                graphics.DrawEllipse(pen, new RectangleF(new PointF(pen.Width / 2, pen.Width / 2), new SizeF(bmp.Width - pen.Width, bmp.Height - pen.Width)));
                        }

                        using (var finalBmp = new Bitmap(32, 32, bmp.PixelFormat))
                        {
                            using (var graphics = Graphics.FromImage(finalBmp))
                            {
                                graphics.CompositingQuality = CompositingQuality.HighQuality;
                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                                graphics.DrawImage(bmp, new Rectangle(Point.Empty, finalBmp.Size), new Rectangle(Point.Empty, bmp.Size), GraphicsUnit.Pixel);
                            }

                            using (var fs = File.Create(icoFile))
                            using (var ms = new MemoryStream())
                            {
                                finalBmp.Save(ms, ImageFormat.Png);
                                var png = ms.ToArray();
                                var pngiconheader = new byte[] { 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                                pngiconheader[6] = (byte)finalBmp.Width;
                                pngiconheader[7] = (byte)finalBmp.Height;
                                pngiconheader[14] = (byte)(png.Length & 255);
                                pngiconheader[15] = (byte)(png.Length / 256);
                                pngiconheader[18] = (byte)pngiconheader.Length;
                                fs.Write(pngiconheader, 0, pngiconheader.Length);
                                fs.Write(png, 0, png.Length);
                            }
                        }
                    }
                }

            var sourceDir = Path.Combine(rootDir, "KeePassRDP", "bin", Configuration);
            var version = FileVersionInfo.GetVersionInfo(Path.Combine(sourceDir, "KeePassRDP.dll")).ProductVersion;

            // Setup project.
            var launchCondition = new LaunchCondition("KEEPASS", "Please install KeePass from https://keepass.info/download.html first.");
            var setOptionalTextProperty = new SetPropertyAction(new Id("ExitDialogOptionalText"), "WIXUI_EXITDIALOGOPTIONALTEXT", "Thank you for installing [ProductName].", Return.check, When.After, Step.FindRelatedProducts, Condition.NOT_Installed, Sequence.InstallUISequence);
            var launchApplication = new LaunchApplicationFromExitDialog(string.Empty, "Launch KeePass");
            var project = new Project("KeePassRDP",
                new Dir(new Id("INSTALLDIR"), ".", new WixSharp.File("KeePassRDP.plgx"))
                {
                    IsInstallDir = true
                },
                launchCondition,
                new ManagedAction(new Id("SetupSession"), SetupSession, Return.check, When.Before, Step.AppSearch, Condition.Always, Sequence.InstallExecuteSequence),
                new ManagedAction(new Id("SetupSessionUi"), SetupSession, Return.check, When.Before, Step.LaunchConditions, Condition.Always, Sequence.InstallUISequence),
                new ManagedAction(new Id("Cleanup"), Cleanup, Return.check, When.Before, Step.InstallFiles, Condition.Always, Sequence.InstallExecuteSequence),
                new ManagedAction(new Id("CleanupUi"), Cleanup, Return.check, When.Before, Step.ExecuteAction, Condition.Always, Sequence.InstallUISequence),
                setOptionalTextProperty,
                new Property("WixAppFolder", "WixPerUserFolder"),
                new Property("DefaultWixAppFolder", "WixPerUserFolder"),
                new SetPropertyAction(new Id("WixSetPerUserFolderCustom"), "WixAppFolder", "WixPerUserFolder", Return.check, When.Before, Step.CostFinalize, new Condition(@"MSIINSTALLPERUSER = ""1"""), Sequence.InstallExecuteSequence),
                new SetPropertyAction(new Id("WixSetDefaultPerUserFolderCustom"), "DefaultWixAppFolder", "[WixAppFolder]", Return.check, When.After, new Step("WixSetPerUserFolderCustom"), new Condition(@"MSIINSTALLPERUSER = ""1"""), Sequence.InstallExecuteSequence),
                new SetPropertyAction(new Id("WixSetPerMachineFolderCustom"), "WixAppFolder", "WixPerMachineFolder", Return.check, When.Before, Step.CostFinalize, new Condition(@"MSIINSTALLPERUSER <> ""1"""), Sequence.InstallExecuteSequence),
                new SetPropertyAction(new Id("WixSetDefaultPerMachineFolderCustom"), "DefaultWixAppFolder", "[WixAppFolder]", Return.check, When.After, new Step("WixSetPerMachineFolderCustom"), new Condition(@"MSIINSTALLPERUSER <> ""1"""), Sequence.InstallExecuteSequence),
                new SetPropertyAction(new Id("WixSetPerUserFolderCustomUi"), "WixAppFolder", "WixPerUserFolder", Return.check, When.Before, Step.CostFinalize, new Condition(@"MSIINSTALLPERUSER = ""1"""), Sequence.InstallUISequence),
                new SetPropertyAction(new Id("WixSetDefaultPerUserFolderCustomUi"), "DefaultWixAppFolder", "[WixAppFolder]", Return.check, When.After, new Step("WixSetPerUserFolderCustomUi"), new Condition(@"MSIINSTALLPERUSER = ""1"""), Sequence.InstallUISequence),
                new SetPropertyAction(new Id("WixSetPerMachineFolderCustomUi"), "WixAppFolder", "WixPerMachineFolder", Return.check, When.Before, Step.CostFinalize, new Condition(@"MSIINSTALLPERUSER <> ""1"""), Sequence.InstallUISequence),
                new SetPropertyAction(new Id("WixSetDefaultPerMachineFolderCustomUi"), "DefaultWixAppFolder", "[WixAppFolder]", Return.check, When.After, new Step("WixSetPerMachineFolderCustomUi"), new Condition(@"MSIINSTALLPERUSER <> ""1"""), Sequence.InstallUISequence),
                launchApplication,
                new SetPropertyAction(new Id("WixSetShellExecTargetUi"), "WixShellExecTarget", "[KEEPASS]\\KeePass.exe", Return.check, When.Before, Step.CostFinalize, new Condition(@"KEEPASS <> ""1"" AND NOT Installed"), Sequence.InstallUISequence),
                new SetPropertyAction(new Id("WixSetShellExecTargetUiText"), "WIXUI_EXITDIALOGOPTIONALCHECKBOXTEXT", "{}", Return.check, When.Before, Step.CostFinalize, new Condition(@"KEEPASS = ""1"""), Sequence.InstallUISequence)
            )
            {
                GUID = new Guid("9EAFF850-2B28-4AF1-AE41-056F319393B5"),
                Platform = Platform.x64,
                UI = WUI.WixUI_InstallDir,
                ControlPanelInfo = new ProductInfo
                {
                    InstallLocation = "[INSTALLDIR]",
                    Manufacturer = "KeePassRDP",
                    Comments = "KeePass Plugin",
                    UrlInfoAbout = "https://github.com/iSnackyCracky/KeePassRDP",
                    ProductIcon = icoFile,
                    NoModify = true
                },
                Language = "en-US",
                Description = "KeePassRDP",
                Name = "KeePassRDP",
                Version = Version.Parse(version),
                MajorUpgrade = new MajorUpgrade
                {
                    AllowDowngrades = false,
                    AllowSameVersionUpgrades = false,
                    IgnoreRemoveFailure = true,
                    DowngradeErrorMessage = "A newer version of KeePassRDP is already installed."
                },
                BannerImage = bannrbmpFile,
                BackgroundImage = dlgbmpFile,
                ValidateBackgroundImage = false,
                LicenceFile = licenseFile,
                LocalizationFile = Path.Combine(Environment.CurrentDirectory, "WixUI_en-us.wxl"),
                SourceBaseDir = sourceDir,
                OutDir = outDir,
                OutFileName = string.Format("KeePassRDP_v{0}", version)
            };

            //project.SuppressRemovingFolderFor("INSTALLDIR");

            project.WixVariables.Add("virtual WixUISupportPerUser", "1");
            project.WixVariables.Add("virtual WixUISupportPerMachine", "1");

            project
                .AddXmlElement("Wix/Package/InstallUISequence", "Show", @"Dialog=LicenseAgreementDlgCustom; Before=ProgressDlg; Condition=(NOT Installed OR PATCH) AND LicenseAccepted <> ""1""")
                .AddXmlElement("Wix/Package/InstallUISequence", "Show", @"Dialog=VerifyReadyDlg; Before=ProgressDlg; Condition=Installed AND LicenseAccepted = ""1""")
                .AddXmlElement("Wix/Package/InstallUISequence", "Show", @"Dialog=override MaintenanceWelcomeDlg; Before=ProgressDlg; Condition=Installed AND NOT PATCH")
                .AddWixFragment("Wix", XElement.Parse(@"
        <UI Id=""WixUI_InstallDirCustom_X64"">
            <Publish Dialog=""LicenseAgreementDlgCustom"" Control=""Print"" Event=""DoAction"" Value=""WixUIPrintEula_X64"" />
            <Publish Dialog=""BrowseDlg"" Control=""OK"" Event=""DoAction"" Value=""WixUIValidatePath_X64"" Order=""3"" Condition=""NOT WIXUI_DONTVALIDATEPATH"" />
            <Publish Dialog=""InstallDirDlg"" Control=""Next"" Event=""DoAction"" Value=""WixUIValidatePath_X64"" Order=""2"" Condition=""NOT WIXUI_DONTVALIDATEPATH"" />
        </UI>
"), XElement.Parse(@"
        <UIRef Id=""WixUI_InstallDirCustom"" />
"))
                .AddWixFragment("Wix", XElement.Parse(@"
        <UI>
            <Dialog Id=""LicenseAgreementDlgCustom"" Width=""370"" Height=""270"" Title=""!(loc.LicenseAgreementDlg_Title)"">
                <Control Id=""BannerBitmap"" Type=""Bitmap"" X=""0"" Y=""0"" Width=""370"" Height=""44"" TabSkip=""no"" Text=""!(loc.LicenseAgreementDlgBannerBitmap)"" />
                <Control Id=""BannerLine"" Type=""Line"" X=""0"" Y=""44"" Width=""370"" Height=""0"" />
                <Control Id=""BottomLine"" Type=""Line"" X=""0"" Y=""234"" Width=""370"" Height=""0"" />
                <Control Id=""Description"" Type=""Text"" X=""25"" Y=""23"" Width=""340"" Height=""15"" Transparent=""yes"" NoPrefix=""yes"" Text=""!(loc.LicenseAgreementDlgDescription)"" />
                <Control Id=""Title"" Type=""Text"" X=""15"" Y=""6"" Width=""200"" Height=""15"" Transparent=""yes"" NoPrefix=""yes"" Text=""!(loc.LicenseAgreementDlgTitle)"" />
                <Control Id=""LicenseAcceptedCheckBox"" Type=""CheckBox"" X=""20"" Y=""207"" Width=""330"" Height=""18"" CheckBoxValue=""1"" Property=""LicenseAccepted"" Text=""!(loc.LicenseAgreementDlgLicenseAcceptedCheckBox)"" />
                <Control Id=""Print"" Type=""PushButton"" X=""112"" Y=""243"" Width=""56"" Height=""17"" Text=""!(loc.WixUIPrint)"" />
                <Control Id=""Back"" Type=""PushButton"" X=""180"" Y=""243"" Width=""56"" Height=""17"" Text=""!(loc.WixUIBack)"" Disabled=""yes"" />
                <Control Id=""Next"" Type=""PushButton"" X=""236"" Y=""243"" Width=""56"" Height=""17"" Default=""yes"" Text=""!(loc.WixUINext)"" DisableCondition=""LicenseAccepted &lt;&gt; &quot;1&quot;"" EnableCondition=""LicenseAccepted = &quot;1&quot;"">
                    <Publish Event=""SpawnWaitDialog"" Value=""WaitForCostingDlg"" Condition=""!(wix.WixUICostingPopupOptOut) OR CostingComplete = 1"" />
                </Control>
                <Control Id=""Cancel"" Type=""PushButton"" X=""304"" Y=""243"" Width=""56"" Height=""17"" Cancel=""yes"" Text=""!(loc.WixUICancel)"">
                    <Publish Event=""SpawnDialog"" Value=""CancelDlg"" />
                </Control>
                <Control Id=""LicenseText"" Type=""ScrollableText"" X=""20"" Y=""60"" Width=""330"" Height=""140"" Sunken=""yes"" TabSkip=""no"">
                    <Text SourceFile=""!(wix.WixUILicenseRtf=license.rtf)"" />
                </Control>
            </Dialog>

            <Dialog Id=""ExitDialogCustom"" Width=""370"" Height=""270"" Title=""!(loc.ExitDialog_Title)"">
                <Control Id=""Finish"" Type=""PushButton"" X=""236"" Y=""243"" Width=""56"" Height=""17"" Default=""yes"" Cancel=""yes"" Text=""!(loc.WixUIFinish)"" />
                <Control Id=""Cancel"" Type=""PushButton"" X=""304"" Y=""243"" Width=""56"" Height=""17"" Disabled=""yes"" Text=""!(loc.WixUICancel)"" />
                <Control Id=""Bitmap"" Type=""Bitmap"" X=""0"" Y=""0"" Width=""370"" Height=""234"" TabSkip=""no"" Text=""!(loc.ExitDialogBitmap)"" />
                <Control Id=""Back"" Type=""PushButton"" X=""180"" Y=""243"" Width=""56"" Height=""17"" Disabled=""yes"" Text=""!(loc.WixUIBack)"" />
                <Control Id=""BottomLine"" Type=""Line"" X=""0"" Y=""234"" Width=""370"" Height=""0"" />
                <Control Id=""Description"" Type=""Text"" X=""135"" Y=""70"" Width=""220"" Height=""40"" Transparent=""yes"" NoPrefix=""yes"" Text=""!(loc.ExitDialogDescription)"" />
                <Control Id=""Title"" Type=""Text"" X=""135"" Y=""20"" Width=""220"" Height=""60"" Transparent=""yes"" NoPrefix=""yes"" Text=""!(loc.ExitDialogTitle)"" />
                <Control Id=""OptionalText"" Type=""Text"" X=""135"" Y=""110"" Width=""220"" Height=""80"" Transparent=""yes"" NoPrefix=""yes"" Hidden=""yes"" Text=""[WIXUI_EXITDIALOGOPTIONALTEXT]"" ShowCondition=""WIXUI_EXITDIALOGOPTIONALTEXT AND NOT Installed"" />
                <Control Id=""OptionalCheckBox"" Type=""CheckBox"" X=""15"" Y=""244"" Width=""100"" Height=""17"" Hidden=""yes"" Property=""WIXUI_EXITDIALOGOPTIONALCHECKBOX"" CheckBoxValue=""1"" Text=""[WIXUI_EXITDIALOGOPTIONALCHECKBOXTEXT]"" ShowCondition=""WIXUI_EXITDIALOGOPTIONALCHECKBOXTEXT AND NOT Installed"" />
            </Dialog>

            <InstallUISequence>
                <Show Dialog=""virtual ExitDialogCustom"" OnExit=""success"" />
            </InstallUISequence>

            <AdminUISequence>
                <Show Dialog=""virtual ExitDialogCustom"" OnExit=""success"" />
            </AdminUISequence>
        </UI>
"))
                .AddWixFragment("Wix", XElement.Parse(@"
        <UI Id=""file WixUI_InstallDirCustom"">
            <TextStyle Id=""WixUI_Font_Normal"" FaceName=""Tahoma"" Size=""8"" />
            <TextStyle Id=""WixUI_Font_Bigger"" FaceName=""Tahoma"" Size=""12"" />
            <TextStyle Id=""WixUI_Font_Title"" FaceName=""Tahoma"" Size=""9"" Bold=""yes"" />

            <Property Id=""DefaultUIFont"" Value=""WixUI_Font_Normal"" />

            <DialogRef Id=""BrowseDlg"" />
            <DialogRef Id=""DiskCostDlg"" />
            <DialogRef Id=""ErrorDlg"" />
            <DialogRef Id=""FatalError"" />
            <DialogRef Id=""FilesInUse"" />
            <DialogRef Id=""MsiRMFilesInUse"" />
            <DialogRef Id=""PrepareDlg"" />
            <DialogRef Id=""ProgressDlg"" />
            <DialogRef Id=""ResumeDlg"" />
            <DialogRef Id=""UserExit"" />
            <Publish Dialog=""BrowseDlg"" Control=""OK"" Event=""SpawnDialog"" Value=""InvalidDirDlg"" Order=""4"" Condition=""NOT WIXUI_DONTVALIDATEPATH AND WIXUI_INSTALLDIR_VALID&lt;&gt;&quot;1&quot;"" />

            <Publish Dialog=""ExitDialogCustom"" Control=""Finish"" Event=""EndDialog"" Value=""Return"" Order=""999"" />

            <Publish Dialog=""LicenseAgreementDlgCustom"" Control=""Next"" Event=""NewDialog"" Value=""InstallScopeDlg"" Order=""1"" Condition=""LicenseAccepted = &quot;1&quot;"" />
            <Publish Dialog=""LicenseAgreementDlgCustom"" Control=""Next"" Event=""NewDialog"" Value=""VerifyReadyDlg"" Order=""2"" Condition=""Installed AND PATCH AND LicenseAccepted = &quot;1&quot;"" />

            <Publish Dialog=""InstallScopeDlg"" Control=""Back"" Event=""NewDialog"" Value=""LicenseAgreementDlgCustom"" />
            <!-- override default WixAppFolder of WixPerMachineFolder as standard user won't be shown the radio group to set WixAppFolder -->
            <Publish Dialog=""InstallScopeDlg"" Control=""Next"" Property=""WixAppFolder"" Value=""WixPerUserFolder"" Order=""1"" Condition=""!(wix.WixUISupportPerUser) AND NOT Privileged"" />
            <Publish Dialog=""InstallScopeDlg"" Control=""Next"" Property=""ALLUSERS"" Value=""{}"" Order=""2"" Condition=""WixAppFolder = &quot;WixPerUserFolder&quot;"" />
            <Publish Dialog=""InstallScopeDlg"" Control=""Next"" Property=""MSIINSTALLPERUSER"" Value=""1"" Order=""3"" Condition=""WixAppFolder = &quot;WixPerUserFolder&quot;"" />
            <Publish Dialog=""InstallScopeDlg"" Control=""Next"" Property=""MSIINSTALLPERUSER"" Value=""0"" Order=""4"" Condition=""WixAppFolder = &quot;WixPerMachineFolder&quot;"" />
            <Publish Dialog=""InstallScopeDlg"" Control=""Next"" Event=""NewDialog"" Value=""InstallDirDlg"" Order=""5"" Condition=""MSIINSTALLPERUSER = &quot;1&quot;""/>
            <Publish Dialog=""InstallScopeDlg"" Control=""Next"" Event=""NewDialog"" Value=""VerifyReadyDlg"" Order=""6"" Condition=""MSIINSTALLPERUSER &lt;&gt; &quot;1&quot;"" />

            <Publish Dialog=""InstallDirDlg"" Control=""Back"" Property=""WixAppFolder"" Value=""[DefaultWixAppFolder]"" Order=""1"" />
            <Publish Dialog=""InstallDirDlg"" Control=""Back"" Property=""ALLUSERS"" Value=""2"" Order=""2"" />
            <Publish Dialog=""InstallDirDlg"" Control=""Back"" Property=""MSIINSTALLPERUSER"" Value=""1"" Order=""3"" Condition=""WixAppFolder = &quot;WixPerUserFolder&quot;"" />
            <Publish Dialog=""InstallDirDlg"" Control=""Back"" Property=""MSIINSTALLPERUSER"" Value=""0"" Order=""4"" Condition=""WixAppFolder = &quot;WixPerMachineFolder&quot;"" />
            <Publish Dialog=""InstallDirDlg"" Control=""Back"" Event=""NewDialog"" Value=""InstallScopeDlg"" Order=""5"" />
            <Publish Dialog=""InstallDirDlg"" Control=""Next"" Event=""SetTargetPath"" Value=""[WIXUI_INSTALLDIR]"" Order=""1"" />
            <Publish Dialog=""InstallDirDlg"" Control=""Next"" Event=""SpawnDialog"" Value=""InvalidDirDlg"" Order=""2"" Condition=""NOT WIXUI_DONTVALIDATEPATH AND WIXUI_INSTALLDIR_VALID&lt;&gt;&quot;1&quot;"" />
            <Publish Dialog=""InstallDirDlg"" Control=""Next"" Event=""NewDialog"" Value=""VerifyReadyDlg"" Order=""3"" Condition=""WIXUI_DONTVALIDATEPATH OR WIXUI_INSTALLDIR_VALID=&quot;1&quot;"" />
            <Publish Dialog=""InstallDirDlg"" Control=""ChangeFolder"" Property=""_BrowseProperty"" Value=""[WIXUI_INSTALLDIR]"" Order=""1"" />
            <Publish Dialog=""InstallDirDlg"" Control=""ChangeFolder"" Event=""SpawnDialog"" Value=""BrowseDlg"" Order=""2"" />
            <Publish Dialog=""VerifyReadyDlg"" Control=""Back"" Event=""NewDialog"" Value=""InstallDirDlg"" Order=""1"" Condition=""MSIINSTALLPERUSER = &quot;1&quot; AND NOT Installed"" />
            <Publish Dialog=""VerifyReadyDlg"" Control=""Back"" Event=""NewDialog"" Value=""InstallScopeDlg"" Order=""2"" Condition=""MSIINSTALLPERUSER &lt;&gt; &quot;1&quot; AND NOT Installed"" />
            <Publish Dialog=""VerifyReadyDlg"" Control=""Back"" Event=""NewDialog"" Value=""MaintenanceTypeDlg"" Order=""3"" Condition=""Installed AND NOT PATCH"" />
            <Publish Dialog=""VerifyReadyDlg"" Control=""Back"" Event=""NewDialog"" Value=""LicenseAgreementDlgCustom"" Order=""4"" Condition=""Installed AND PATCH"" />

            <Publish Dialog=""MaintenanceWelcomeDlg"" Control=""Next"" Event=""NewDialog"" Value=""MaintenanceTypeDlg"" />

            <Publish Dialog=""MaintenanceTypeDlg"" Control=""RepairButton"" Event=""NewDialog"" Value=""VerifyReadyDlg"" />
            <Publish Dialog=""MaintenanceTypeDlg"" Control=""RemoveButton"" Event=""NewDialog"" Value=""VerifyReadyDlg"" />
            <Publish Dialog=""MaintenanceTypeDlg"" Control=""Back"" Event=""NewDialog"" Value=""MaintenanceWelcomeDlg"" />

            <Property Id=""ARPNOMODIFY"" Value=""1"" />
        </UI>
"), XElement.Parse(@"
        <UIRef Id=""WixUI_Common"" />
"));

            project.WixSourceGenerated += (XDocument doc) =>
            {
                doc.FindFirst("SummaryInformation").AddAttributes("Comments=KeePass Plugin; Description=KeePassRDP");
                doc.FindFirst("WixUI").SetAttribute("Id", "WixUI_InstallDirCustom");
                doc.XPathSelectElement("//*[name()='CustomAction' and @BinaryRef='Wix4UtilCA_X86']")
                    .SetAttribute("BinaryRef", "Wix4UtilCA_X64")
                    .SetAttribute("Impersonate", "yes");
                doc.XPathSelectElement("//*[name()='Property' and @Id='WIXUI_EXITDIALOGOPTIONALCHECKBOX']").Remove();
                doc.XPathSelectElement("//*[name()='Property' and @Id='WixShellExecTarget']").SetAttribute("Value", "KeePass.exe");
                doc.XPathSelectElement("//*[name()='Publish' and @Dialog='ExitDialog']").SetAttribute("Dialog", "ExitDialogCustom");
            };

            var msiFile = project.BuildMsi();
            project.Language = "de-DE";
            project.LocalizationFile = Path.Combine(Environment.CurrentDirectory, "WixUI_de-de.wxl");
            launchCondition.Message = "Bitte zuvor KeePass von https://keepass.info/download.html installieren.";
            setOptionalTextProperty.Value = "Vielen Dank für die Installation von [ProductName].";
            launchApplication.Description = "KeePass ausführen";
            var msiDe = project.BuildMsi(Path.Combine(outDir, "de-DE.msi"));
            var msiDeMst = Path.ChangeExtension(msiDe, ".mst");
            new ExternalTool
            {
                ExePath = "wix.exe",
                Arguments = string.Format("msi transform -p -t language \"{0}\" \"{1}\" -out \"{2}\"", msiFile, msiDe, msiDeMst)
            }.ConsoleRun();

            msiFile.EmbedTransform(msiDeMst);
            msiFile.SetPackageLanguages("en-US, de-DE".ToLcidList());

            File.Copy(msiFile, Path.Combine(rootDir, Path.GetFileName(msiFile)), true);

            foreach (var file in new[] { icoFile, licenseFile, bannrbmpFile, dlgbmpFile, msiDe, msiDeMst })
                if (File.Exists(file))
                    File.Delete(file);
        }

        [CustomAction]
        public static ActionResult SetupSession(Session session)
        {
            if (session.IsInstalling() && !session.IsUpgradingInstalledVersion())
            {
                var keePass = session.Property("KEEPASS");
                if (string.IsNullOrWhiteSpace(keePass))
                {
                    var targetDir = AppSearch.GetRegValue(
                        Registry.LocalMachine,
                        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\KeePassPasswordSafe2_is1",
                        "InstallLocation") as string;

                    if (string.IsNullOrWhiteSpace(targetDir))
                    {
                        //if (AppSearch.IsProductInstalled("{CD222A6D-6927-454E-BDBA-DEACD4E0B22C}"))
                        {
                            var len = 256;
                            var sb = new StringBuilder(len);
                            const int INSTALLSTATE_LOCAL = 3;
                            if (MsiGetComponentPath("{CD222A6D-6927-454E-BDBA-DEACD4E0B22C}", "{CD222A6D-6927-454E-BDBA-DEACD4E0B22C}", sb, ref len) >= INSTALLSTATE_LOCAL)
                                targetDir = sb.ToString().Substring(0, len).TrimEnd('\0');
                        }
                    }

                    if (string.IsNullOrWhiteSpace(targetDir))
                        return ActionResult.Failure;

                    var pluginsDir = Path.Combine(targetDir, "Plugins");

                    session.SetProperty("KEEPASS", targetDir);
                    session.SetProperty("INSTALLDIR", pluginsDir);

                    if (string.IsNullOrEmpty(session.Property("ALLUSERS")))
                        session.SetProperty("ALLUSERS", "2");

                    var isAdmin = IsAdmin();
                    var hasWriteAccess = isAdmin || HasWriteAccess(pluginsDir);

                    session.SetProperty("MSIINSTALLPERUSER", isAdmin || !hasWriteAccess ? "0" : "1");
                }
                else if (keePass != "1")
                {
                    var pluginsDir = Path.Combine(keePass, "Plugins");
                    if (!Directory.Exists(pluginsDir))
                        pluginsDir = keePass;

                    session.SetProperty("INSTALLDIR", pluginsDir);

                    if (string.IsNullOrEmpty(session.Property("ALLUSERS")))
                        session.SetProperty("ALLUSERS", "2");

                    var isAdmin = IsAdmin();
                    var hasWriteAccess = isAdmin || HasWriteAccess(pluginsDir);

                    session.SetProperty("MSIINSTALLPERUSER", isAdmin || !hasWriteAccess ? "0" : "1");
                }
                else // KEEPASS=1
                {
                    if (session.IsUISupressed() && string.IsNullOrWhiteSpace(session.Property("INSTALLDIR")))
                        return ActionResult.Failure;
                }
            }
            else
            {
                var keePass = session.Property("KEEPASS");
                if (string.IsNullOrWhiteSpace(keePass))
                    session.SetProperty("KEEPASS", "1");
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult Cleanup(Session session)
        {
            if (session.IsInstalled() && session.IsUninstalling())
            {
                // Cleanup KeePass plugin cache.
                try
                {
                    var installDir = session.Property("INSTALLDIR");
                    if (!string.IsNullOrWhiteSpace(installDir))
                    {
                        var targetDir = Path.GetDirectoryName(installDir.TrimEnd('\\'));
                        var cacheDir = GetCacheDirectory(targetDir);
                        if (!string.IsNullOrWhiteSpace(cacheDir) && Directory.Exists(cacheDir) && (IsAdmin() || HasWriteAccess(cacheDir)))
                            Directory.Delete(cacheDir, true);
                    }
                }
                catch
                {
                    //return ActionResult.Failure;
                }
            }

            return ActionResult.Success;
        }

        private static bool HasWriteAccess(string dir)
        {
            try
            {
                var f = Path.Combine(dir, Path.GetRandomFileName());
                /*using (File.Create(f, 1, FileOptions.DeleteOnClose))
                {
                    File.SetAttributes(f, File.GetAttributes(p) | FileAttributes.Temporary);
                }*/
                using(var sfh = new SafeFileHandle(CreateFile(
                    f,
                    FileAccess.ReadWrite,
                    FileShare.Read,
                    IntPtr.Zero,
                    FileMode.Create,
                    (uint)FileAttributes.Temporary | (uint)FileOptions.DeleteOnClose,
                    IntPtr.Zero), true))
                using (new FileStream(sfh, FileAccess.ReadWrite, 1, false))
                { }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsAdmin()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                    return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private static string GetCacheDirectory(string targetDir)
        {
            try
            {
                string baseFileName = null;
                byte[] guidBytes = null;

                var kprPlgx = Path.Combine(targetDir, "Plugins", "KeePassRDP.plgx");

                using (var fs = File.Open(kprPlgx, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var br = new BinaryReader(fs))
                {
                    br.ReadUInt32();
                    br.ReadUInt32();
                    br.ReadUInt32();

                    var bt = 0;
                    while ((bt = br.ReadUInt16()) != 0) // KeePass.Plugins.PlgxPlugin.PlgxEOF
                    {
                        var bl = br.ReadUInt32();
                        var bb = bl > 0 ? br.ReadBytes((int)bl) : null;
                        if (bb != null)
                        {
                            switch (bt)
                            {
                                case 1: // KeePass.Plugins.PlgxPlugin.PlgxFileUuid
                                    guidBytes = bb;
                                    break;
                                case 2: // KeePass.Plugins.PlgxPlugin.PlgxBaseFileName
                                    baseFileName = Encoding.UTF8.GetString(bb);
                                    break;
                            }

                            if (guidBytes != null && baseFileName != null)
                                break;
                        }
                    }
                }

                if (guidBytes == null || string.IsNullOrWhiteSpace(baseFileName))
                    return string.Empty;

                var keePassExe = Path.Combine(targetDir, "KeePass.exe");
                var asm = Assembly.LoadFile(keePassExe);

                var type = asm.GetType("KeePass.Plugins.PlgxCache");
                var method = type.GetMethod("GetCacheDirectory");

                type = asm.GetType("KeePass.Plugins.PlgxPluginInfo");
                var pluginInfo = Activator.CreateInstance(type, false, false, false);

                type.GetProperty("BaseFileName").SetValue(pluginInfo, baseFileName, null);
                type.GetProperty("FileUuid").SetValue(pluginInfo, Activator.CreateInstance(asm.GetType("KeePassLib.PwUuid"), guidBytes), null);

                return (string)method.Invoke(null, new[] { pluginInfo, false });
            }
            catch
            {
                return string.Empty;
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFile(string name, [MarshalAs(UnmanagedType.U4)] FileAccess accessMode, [MarshalAs(UnmanagedType.U4)] FileShare shareMode, IntPtr security, [MarshalAs(UnmanagedType.U4)] FileMode createMode, uint flags, IntPtr template);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        private static extern int MsiGetComponentPath(string szProduct, string szComponent, StringBuilder lpPathBuf, ref int pcchBuf);
    }
}