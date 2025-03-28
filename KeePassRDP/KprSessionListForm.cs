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

using KeePass.Forms;
using KeePass.Resources;
using KeePass.UI;
using KeePassRDP.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassRDP
{
    public partial class KprSessionListForm : Form, IGwmWindow
    {
        private static readonly Lazy<Icon> _icon = new Lazy<Icon>(
            () => IconUtil.ExtractIcon(KeePassRDPExt.MstscPath, 10, UIUtil.GetSmallIconSize().Height),
            LazyThreadSafetyMode.ExecutionAndPublication);

        public class KprSessionListResult
        {
            public string Shadow { get; set; }
            public bool Control { get; set; }
            public bool NoConsentPrompt { get; set; }
        }

        public bool CanCloseWithoutDataLoss { get { return true; } }

        public IList<string> Computers { get { return _computers; } }
        public IList<KprSessionListResult> Results { get { return _results; } }

        private readonly KprConfig _config;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly List<string> _computers;
        private readonly List<KprSessionListResult> _results;

        public KprSessionListForm(KprConfig config)
        {
            _config = config;

            _cancellationTokenSource = new CancellationTokenSource();
            _computers = new List<string>();
            _results = new List<KprSessionListResult>();

            InitializeComponent();

            KprResourceManager.Instance.TranslateMany(
                checkBox1,
                checkBox2,
                button1,
                button2,
                button3
            );

            KprResourceManager.Instance.TranslateMany(
                toolStripMenuItem1,
                toolStripMenuItem2,
                toolStripMenuItem3,
                toolStripMenuItem4
            );

            Util.EnableDoubleBuffered(
                tableLayoutPanel1,
                listView1
            );

            contextMenuStrip1.ImageList = kprImageList;
            toolStripMenuItem1.ImageKey = "UserFeedback";
            toolStripMenuItem2.ImageKey = "Disconnect";
            toolStripMenuItem3.ImageKey = "LoginScreen";
            toolStripMenuItem4.ImageKey = "User";

            listView1.HandleCreated += listView1_HandleCreated;

            Icon = _icon.Value;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (_config.CredPickerSecureDesktop)
                NativeMethods.SetWindowDisplayAffinity(Handle, NativeMethods.WDA_EXCLUDEFROMCAPTURE);
        }

        private void listView1_HandleCreated(object sender, EventArgs e)
        {
            try
            {
                if (UIUtil.VistaStyleListsSupported)
                    UIUtil.SetExplorerTheme(listView1.Handle);
            }
            catch { }
        }

        private void KprSessionListForm_Load(object sender, EventArgs e)
        {
            GlobalWindowManager.AddWindow(this);

            SuspendLayout();

            if (_config.CredPickerSecureDesktop)
                CenterToScreen();
            else
                CenterToParent();
            LoadSessions();
        }

        private void LoadSessions()
        {
            if (UseWaitCursor)
                return;

            UseWaitCursor = true;

            listView1.BeginUpdate();
            listView1.Items.Clear();
            listView1.Groups.Clear();
            listView1.Columns.Clear();

            Task.Factory.StartNew(() =>
            {
                using (var cts = new CancellationTokenSource())
                using (var lcts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _cancellationTokenSource.Token))
                {
                    // Cancel after 90 seconds.
                    Task.Factory.StartNew(() =>
                    {
                        using (var mrs = new ManualResetEventSlim(false))
                        {
                            for (var i = 0; i < 90; i++)
                            {
                                if (lcts.IsCancellationRequested)
                                    break;
                                try
                                {
                                    mrs.Wait(TimeSpan.FromSeconds(1), lcts.Token);
                                }
                                catch { }
                            }
                            try
                            {
                                if (!lcts.IsCancellationRequested)
                                    cts.Cancel();
                            }
                            catch { }
                        }
                    }, lcts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                    try
                    {
                        var groups = new ListViewGroup[_computers.Count];

                        var computers = ShadowSession.GetWTSSessionInformation(_computers, lcts.Token)
                            .SelectMany((x, i) =>
                            {
                                var group = groups[i] = new ListViewGroup(x.Key);
                                return x.Value.Select(y =>
                                {
                                    var lvi = new ListViewItem(string.Empty, group);
                                    lvi.SubItems.AddRange(new ListViewItem.ListViewSubItem[]
                                    {
                                        new ListViewItem.ListViewSubItem(lvi, y.SessionId.ToString()),
                                        new ListViewItem.ListViewSubItem(lvi, string.Format(@"{0}\{1}", string.IsNullOrEmpty(y.Domain) ? "." : y.Domain, y.UserName)),
                                        new ListViewItem.ListViewSubItem(lvi, y.WinStationName),
                                        new ListViewItem.ListViewSubItem(lvi, y.IdleTime.ToString()),
                                        new ListViewItem.ListViewSubItem(lvi, y.ConnectTime == DateTime.MinValue ? string.Empty : y.ConnectTime.ToString()),
                                        new ListViewItem.ListViewSubItem(lvi, y.LogonTime == DateTime.MinValue ? string.Empty : y.LogonTime.ToString())
                                    });
                                    return lvi;
                                });
                            })
                            .ToArray();

                        if (!lcts.IsCancellationRequested && !cts.IsCancellationRequested)
                            cts.Cancel();

                        Task.Factory.FromAsync(listView1.BeginInvoke(new Action(() =>
                        {
                            listView1.Columns.AddRange(new ColumnHeader[]
                            {
                                new ColumnHeader
                                {
                                    Width = 0,
                                    Text = string.Empty
                                },
                                new ColumnHeader
                                {
                                    Name = "SessionId",
                                    Text = "SessionId",
                                    TextAlign = HorizontalAlignment.Right
                                },
                                new ColumnHeader
                                {
                                    Name = "UserName",
                                    Text = "UserName"
                                },
                                new ColumnHeader
                                {
                                    Name = "WinStationName",
                                    Text = "WinStationName"
                                },
                                new ColumnHeader
                                {
                                    Name = "IdleTime",
                                    Text = "IdleTime"
                                },
                                new ColumnHeader
                                {
                                    Name = "ConnectTime",
                                    Text = "ConnectTime"
                                },
                                new ColumnHeader
                                {
                                    Name = "LogonTime",
                                    Text = "LogonTime"
                                }
                            });
                            listView1.Groups.AddRange(groups);
                            listView1.ShowGroups = listView1.Groups.Count > 1;
                            listView1.Items.AddRange(computers);

                            listView1.EndUpdate();
                            ResumeLayout(false);
                            UseWaitCursor = false;

                            Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                            {
                                listView1_SizeChanged(null, EventArgs.Empty);
                            })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                        })), endInvoke => listView1.EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    }
                    catch (Exception ex)
                    {
                        Task.Factory.FromAsync(listView1.BeginInvoke(new Action(() =>
                        {
                            UseWaitCursor = false;
                            listView1.EndUpdate();
                        })), endInvoke => listView1.EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

                        Util.ShowErrorDialog(
                            Util.FormatException(ex),
                            null,
                            string.Format("{0} - {1}", Util.KeePassRDP, KPRes.Error),
                            VtdIcon.Error,
                            this,
                            null, 0, null, 0);
                    }
                    finally
                    {
                        if (!lcts.IsCancellationRequested && !cts.IsCancellationRequested)
                            cts.Cancel();
                    }
                }
            }, _cancellationTokenSource.Token, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
    }

        private void KprSessionListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel)
                return;

            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();

            _results.Clear();

            if (DialogResult != DialogResult.OK)
                return;

            if (listView1.SelectedItems.Count != 1)
            {
                VistaTaskDialog.ShowMessageBoxEx(
                    KprResourceManager.Instance["Please select exactly one session to shadow."],
                    null,
                    string.Format("{0} - {1}", Util.KeePassRDP, KPRes.Error),
                    VtdIcon.Error,
                    this,
                    null, 0, null, 0);

                e.Cancel = true;
                return;
            }

            _results.AddRange(listView1.SelectedItems.OfType<ListViewItem>().Select(lvi => new KprSessionListResult
            {
                Shadow = lvi == null ? string.Empty : lvi.SubItems[1].Text,
                Control = checkBox1.Checked,
                NoConsentPrompt = checkBox2.Checked
            }));
        }

        private void KprSessionListForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalWindowManager.RemoveWindow(this);
        }

        private void KprSessionListForm_Deactivate(object sender, EventArgs e)
        {
            if (contextMenuStrip1.Visible)
                contextMenuStrip1.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadSessions();

            if (ActiveControl == sender)
                ActiveControl = null;
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (contextMenuStrip1.Visible)
                contextMenuStrip1.Close();
            if (e.Button == MouseButtons.Right && listView1.GetItemAt(e.X, e.Y) is ListViewItem)
                contextMenuStrip1.Show(Cursor.Position);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            listView1_ItemSelectionChanged(null, null);
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var enableItems = listView1.SelectedItems.OfType<ListViewItem>().Any(x => x.SubItems[2].Text != ".\\");
            toolStripMenuItem3.Enabled = toolStripMenuItem4.Enabled = enableItems;
            var enableDisconnect = enableItems && listView1.SelectedItems.OfType<ListViewItem>().Any(x => !string.IsNullOrEmpty(x.SubItems[3].Text));
            toolStripMenuItem1.Enabled = toolStripMenuItem2.Enabled = enableDisconnect;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (UseWaitCursor)
                return;

            var message = string.Empty;
            var wait = false;
            var timeout = 0;

            var selectedItems = listView1.SelectedItems.OfType<ListViewItem>().Where(x => x.SubItems[2].Text != ".\\" && !string.IsNullOrEmpty(x.SubItems[3].Text));

            using (var icon = Icon.ToBitmap())
            using (var slef = new SingleLineEditForm())
            {
                slef.Load += (s, ee) =>
                {
                    slef.BackColor = BackColor;
                    slef.Icon = Icon;
                    slef.Text = Text;
                    var flp = new FlowLayoutPanel
                    {
                        FlowDirection = FlowDirection.LeftToRight,
                        Margin = Padding.Empty,
                        Padding = Padding.Empty,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        Location = new Point(12, 140)
                    };
                    var checkbox = new CheckBox
                    {
                        Text = KprResourceManager.Instance["Wait for respones"],
                        AutoSize = true,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Anchor = AnchorStyles.None,
                        Margin = Padding.Empty,
                        Padding = Padding.Empty
                    };
                    var numericUpDown = new NumericUpDown
                    {
                        Value = 30,
                        Minimum = 0,
                        Maximum = int.MaxValue,
                        DecimalPlaces = 0,
                        Increment = 1,
                        ThousandsSeparator = false,
                        BorderStyle = BorderStyle.FixedSingle,
                        Width = 40,
                        Anchor = AnchorStyles.None,
                        Margin = Padding.Empty,
                        Padding = Padding.Empty,
                        Enabled = false
                    };
                    var label = new Label
                    {
                        Text = "s",
                        AutoSize = true,
                        Anchor = AnchorStyles.None,
                        Margin = Padding.Empty,
                        Padding = Padding.Empty
                    };
                    checkbox.CheckedChanged += (ss, eee) =>
                    {
                        numericUpDown.Enabled = checkbox.Checked;
                    };
                    flp.Controls.AddRange(new Control[] { checkbox, numericUpDown, label });
                    slef.Controls.Add(flp);
                    foreach (Control c in slef.Controls)
                        c.Font = listView1.Font;
                };
                var filteredItems = selectedItems.Select(x => x.SubItems[2].Text).Distinct(StringComparer.OrdinalIgnoreCase);
                var longDesc = string.Join(", ", filteredItems.Take(100));
                slef.InitEx(
                    KprResourceManager.Instance["Send message"],
                    KprResourceManager.Instance["Type in a message to be sent to the selected users."],
                    string.Format("{0}{1}",
                        longDesc.Length > 1000 ? longDesc.Substring(0, 1000) : longDesc,
                        longDesc.Length > 1000 || filteredItems.Count() > 100 ? " ..." : string.Empty),
                    icon,
                    string.Empty,
                    null);
                if (slef.ShowDialog(this) != DialogResult.OK)
                    return;

                message = slef.ResultString;
                var panel = slef.Controls[slef.Controls.Count - 1] as FlowLayoutPanel;
                wait = (panel.Controls[0] as CheckBox).Checked;
                timeout = (int)(panel.Controls[1] as NumericUpDown).Value;
            }

            UseWaitCursor = true;

            Task.Factory.StartNew(() =>
            {
                var responses = new ConcurrentDictionary<string, IDictionary<int, string>>(4, 0);

                using (var cts = new CancellationTokenSource())
                using (var lcts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _cancellationTokenSource.Token))
                {
                    // Cancel after 90 seconds.
                    Task.Factory.StartNew(() =>
                    {
                        using (var mrs = new ManualResetEventSlim(false))
                        {
                            var waitTimeout = Math.Max(90, wait ? timeout + 5 : 0);
                            for (var i = 0; i < waitTimeout; i++)
                            {
                                if (lcts.IsCancellationRequested)
                                    break;
                                try
                                {
                                    mrs.Wait(TimeSpan.FromSeconds(1), lcts.Token);
                                }
                                catch { }
                            }
                            try
                            {
                                if (!lcts.IsCancellationRequested && !cts.IsCancellationRequested)
                                    cts.Cancel();
                            }
                            catch { }
                        }
                    }, lcts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                    try
                    {
                        Parallel.ForEach(selectedItems.GroupBy(x => x.Group.Header), new ParallelOptions
                        {
                            MaxDegreeOfParallelism = 4,
                            CancellationToken = lcts.Token,
                            TaskScheduler = TaskScheduler.Default
                        },
                        group =>
                        {
                            responses[group.Key] = ShadowSession.SendMessages(group.Key, group.Select(x => Convert.ToInt32(x.SubItems[1].Text)), string.Empty, message, wait, wait ? timeout : 0, lcts.Token);
                        });
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        Util.ShowErrorDialog(
                            Util.FormatException(ex),
                            null,
                            string.Format("{0} - {1}", Util.KeePassRDP, KPRes.Error),
                            VtdIcon.Error,
                            this,
                            null, 0, null, 0);
                    }
                    finally
                    {
                        if (!lcts.IsCancellationRequested && !cts.IsCancellationRequested)
                            cts.Cancel();

                        if (wait)
                        {
                            VistaTaskDialog.ShowMessageBoxEx(
                                string.Join(Environment.NewLine, responses.Select(x => string.Format("{0}{1}{2}", x.Key, Environment.NewLine, string.Join(", ", x.Value.Select(y => string.Format("{0} -> {1}", y.Key, y.Value)))))),
                                null,
                                Text,
                                VtdIcon.Information,
                                this,
                                null, 0, null, 0);
                        }

                        Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                        {
                            UseWaitCursor = false;
                        })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    }
                }
            }, _cancellationTokenSource.Token, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (UseWaitCursor)
                return;

            UseWaitCursor = true;

            var reload = true;

            var selectedItems = listView1.SelectedItems.OfType<ListViewItem>().Where(x => x.SubItems[2].Text != ".\\" && !string.IsNullOrEmpty(x.SubItems[3].Text));

            Task.Factory.StartNew(() =>
            {
                using (var cts = new CancellationTokenSource())
                using (var lcts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _cancellationTokenSource.Token))
                {
                    // Cancel after 90 seconds.
                    Task.Factory.StartNew(() =>
                    {
                        using (var mrs = new ManualResetEventSlim(false))
                        {
                            for (var i = 0; i < 90; i++)
                            {
                                if (lcts.IsCancellationRequested)
                                    break;
                                try
                                {
                                    mrs.Wait(TimeSpan.FromSeconds(1), lcts.Token);
                                }
                                catch { }
                            }
                            try
                            {
                                if (!lcts.IsCancellationRequested && !cts.IsCancellationRequested)
                                    cts.Cancel();
                            }
                            catch { }
                        }
                    }, lcts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                    try
                    {
                        Parallel.ForEach(selectedItems.GroupBy(x => x.Group.Header), new ParallelOptions
                        {
                            MaxDegreeOfParallelism = 4,
                            CancellationToken = lcts.Token,
                            TaskScheduler = TaskScheduler.Default
                        },
                        group =>
                        {
                            ShadowSession.DisconnectSessions(group.Key, group.Select(x => Convert.ToInt32(x.SubItems[1].Text)), true, lcts.Token);
                        });
                    }
                    catch (OperationCanceledException)
                    {
                        reload = false;
                    }
                    catch (Exception ex)
                    {
                        reload = false;
                        Util.ShowErrorDialog(
                            Util.FormatException(ex),
                            null,
                            string.Format("{0} - {1}", Util.KeePassRDP, KPRes.Error),
                            VtdIcon.Error,
                            this,
                            null, 0, null, 0);
                    }
                    finally
                    {
                        Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                        {
                            UseWaitCursor = false;
                            if (reload)
                                LoadSessions();
                        })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

                        if (!lcts.IsCancellationRequested && !cts.IsCancellationRequested)
                            cts.Cancel();
                    }
                }
            }, _cancellationTokenSource.Token, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (UseWaitCursor)
                return;

            UseWaitCursor = true;

            var reload = true;

            var selectedItems = listView1.SelectedItems.OfType<ListViewItem>().Where(x => x.SubItems[2].Text != ".\\");

            Task.Factory.StartNew(() =>
            {
                using (var cts = new CancellationTokenSource())
                using (var lcts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _cancellationTokenSource.Token))
                {
                    // Cancel after 90 seconds.
                    Task.Factory.StartNew(() =>
                    {
                        using (var mrs = new ManualResetEventSlim(false))
                        {
                            for (var i = 0; i < 90; i++)
                            {
                                if (lcts.IsCancellationRequested)
                                    break;
                                try
                                {
                                    mrs.Wait(TimeSpan.FromSeconds(1), lcts.Token);
                                }
                                catch { }
                            }
                            try
                            {
                                if (!lcts.IsCancellationRequested && !cts.IsCancellationRequested)
                                    cts.Cancel();
                            }
                            catch { }
                        }
                    }, lcts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                    try
                    {
                        Parallel.ForEach(selectedItems.GroupBy(x => x.Group.Header), new ParallelOptions
                        {
                            MaxDegreeOfParallelism = 4,
                            CancellationToken = lcts.Token,
                            TaskScheduler = TaskScheduler.Default
                        },
                        group =>
                        {
                            ShadowSession.LogoffSessions(group.Key, group.Select(x => Convert.ToInt32(x.SubItems[1].Text)), true, lcts.Token);
                        });
                    }
                    catch (OperationCanceledException)
                    {
                        reload = false;
                    }
                    catch (Exception ex)
                    {
                        reload = false;
                        Util.ShowErrorDialog(
                            Util.FormatException(ex),
                            null,
                            string.Format("{0} - {1}", Util.KeePassRDP, KPRes.Error),
                            VtdIcon.Error,
                            this,
                            null, 0, null, 0);
                    }
                    finally
                    {
                        Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                        {
                            UseWaitCursor = false;
                            if (reload)
                                LoadSessions();
                        })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

                        if (!lcts.IsCancellationRequested && !cts.IsCancellationRequested)
                            cts.Cancel();
                    }
                }
            }, _cancellationTokenSource.Token, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (UseWaitCursor)
                return;

            UseWaitCursor = true;

            //var reload = true;

            var selectedItems = listView1.SelectedItems.OfType<ListViewItem>().Where(x => x.SubItems[2].Text != ".\\");

            Task.Factory.StartNew(() =>
            {
                var clientInfos = new ConcurrentBag<ShadowSession.ClientInfo>();

                using (var cts = new CancellationTokenSource())
                using (var lcts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _cancellationTokenSource.Token))
                {
                    // Cancel after 90 seconds.
                    Task.Factory.StartNew(() =>
                    {
                        using (var mrs = new ManualResetEventSlim(false))
                        {
                            for (var i = 0; i < 90; i++)
                            {
                                if (lcts.IsCancellationRequested)
                                    break;
                                try
                                {
                                    mrs.Wait(TimeSpan.FromSeconds(1), lcts.Token);
                                }
                                catch { }
                            }
                            try
                            {
                                if (!lcts.IsCancellationRequested && !cts.IsCancellationRequested)
                                    cts.Cancel();
                            }
                            catch { }
                        }
                    }, lcts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                    try
                    {
                        Parallel.ForEach(selectedItems.GroupBy(x => x.Group.Header), new ParallelOptions
                        {
                            MaxDegreeOfParallelism = 4,
                            CancellationToken = _cancellationTokenSource.Token,
                            TaskScheduler = TaskScheduler.Default
                        },
                        group =>
                        {
                            Parallel.ForEach(group, new ParallelOptions
                            {
                                MaxDegreeOfParallelism = 4,
                                CancellationToken = _cancellationTokenSource.Token,
                                TaskScheduler = TaskScheduler.Default
                            },
                            lvi =>
                            {
                                var clientInfo = new ShadowSession.ClientInfo(group.Key, Convert.ToInt32(lvi.SubItems[1].Text));
                                ShadowSession.GetWTSClientConfigInformation(clientInfo);
                                clientInfos.Add(clientInfo);
                            });
                        });
                    }
                    catch (OperationCanceledException)
                    {
                        //reload = false;
                    }
                    catch (Exception ex)
                    {
                        //reload = false;
                        Util.ShowErrorDialog(
                            Util.FormatException(ex),
                            null,
                            string.Format("{0} - {1}", Util.KeePassRDP, KPRes.Error),
                            VtdIcon.Error,
                            this,
                            null, 0, null, 0);
                    }
                    finally
                    {
                        Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                        {
                            UseWaitCursor = false;
                            /*if (reload)
                                LoadSessions();*/
                        })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

                        if (!lcts.IsCancellationRequested && !cts.IsCancellationRequested)
                            cts.Cancel();
                    }
                }

                foreach (var clientInfo in clientInfos.OrderBy(x => x.Computer, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.SessionId))
                    VistaTaskDialog.ShowMessageBoxEx(
                        clientInfo.ToString(),
                        null,
                        Text,
                        VtdIcon.None,
                        this,
                        null, 0, null, 0);
            }, _cancellationTokenSource.Token, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            button1.PerformClick();
        }

        private volatile bool _lvResizing = false;
        private void listView1_SizeChanged(object sender, EventArgs e)
        {
            if (_lvResizing || (listView1.UseWaitCursor && sender != null))
                return;

            _lvResizing = true;

            var columnsCount = listView1.Columns.Count - 1;

            if (columnsCount >= 0)
            {
                var wasActiveControl = ActiveControl == listView1;
                listView1.Visible = false;
                listView1.SuspendLayout();
                listView1.BeginUpdate();

                if (sender == null)
                {
                    if (listView1.Items.Count > 0)
                    {
                        listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                        var minWidths = listView1.Columns.OfType<ColumnHeader>().Select(x => x.Width).ToArray();

                        listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                        for (var i = columnsCount; i >= 0; i--)
                        {
                            var width = listView1.Columns[i].Width;
                            if (width < minWidths[i])
                                listView1.Columns[i].Width = minWidths[i];
                        }
                    }
                    else
                        listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                    listView1.Columns[columnsCount].Width = -2;
                }

                listView1.Columns[0].Width = 0;
                listView1.Columns.Add(string.Empty, 0);

                var column = listView1.Columns[columnsCount];
                column.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                var minWidth = column.Width;
                column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                column.Width = Math.Max(minWidth, column.Width);

                listView1.Columns.RemoveAt(listView1.Columns.Count - 1);

                column = listView1.Columns[2];
                column.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                minWidth = column.Width;
                column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                var oldWidth = Math.Max(minWidth, column.Width);
                var allWidth = listView1.Columns.OfType<ColumnHeader>().Sum(x => x.Width);
                column.Width = Math.Max(oldWidth, listView1.Width - (allWidth - column.Width));

                listView1.Columns[columnsCount].Width = -2;

                if (listView1.Items.Count == 0)
                {
                    allWidth = listView1.Columns.OfType<ColumnHeader>().Sum(x => x.Width);
                    if (allWidth > listView1.Width)
                        column.Width -= Math.Min(allWidth - listView1.Width, column.Width);
                }

                if (ScrollbarUtil.GetVisibleScrollbars(listView1) >= ScrollBars.Vertical)
                    column.Width = Math.Max(oldWidth, column.Width - UIUtil.GetVScrollBarWidth());

                listView1.EndUpdate();
                listView1.ResumeLayout(false);
                listView1.Visible = true;
                if (wasActiveControl)
                    ActiveControl = listView1;
            }

            _lvResizing = false;
        }

        private void listView1_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            if (_lvResizing || listView1.UseWaitCursor)
                return;

            if (e.ColumnIndex == 0)
            {
                e.Cancel = true;
                e.NewWidth = 0;
            }
        }

        private void listView1_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (_lvResizing || listView1.UseWaitCursor)
                return;

            listView1_SizeChanged(sender, EventArgs.Empty);
            listView1.Invalidate();
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            if (e.ColumnIndex == 0)
                return;

            e.DrawDefault = true;
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            //e.DrawDefault = true;
        }

        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ColumnIndex == 0)
                return;

            e.DrawDefault = true;
        }

        public new DialogResult ShowDialog(IWin32Window owner)
        {
            var control = owner as Control;
            if (control != null && control.InvokeRequired)
                ToolStripManager.Renderer = (ToolStripRenderer)control.Invoke(new Func<object>(() =>
                {
                    return ToolStripManager.Renderer;
                }));

            return base.ShowDialog(owner);
        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                listView1.HandleCreated -= listView1_HandleCreated;

                using (_cancellationTokenSource)
                    if (!_cancellationTokenSource.IsCancellationRequested)
                        _cancellationTokenSource.Cancel();
            }

            base.Dispose(disposing);
        }
    }
}
