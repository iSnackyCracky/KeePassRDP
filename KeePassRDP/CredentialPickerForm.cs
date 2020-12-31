/*
 *  Copyright (C) 2018-2020 iSnackyCracky
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
using System.Windows.Forms;
using System.Collections.Generic;

using KeePassLib;


namespace KeePassRDP
{
    public partial class CredentialPickerForm : Form
    {
        private readonly KprConfig _config;
        private readonly PwDatabase _db;

        public CredentialPickerForm(KprConfig config, PwDatabase db)
        {
            _config = config;
            _db = db;
            InitializeComponent();
        }

        // PwObjectList with all matching entries
        public KeePassLib.Collections.PwObjectList<PwEntry> rdpAccountEntries { get; set; }
        // PwEntry that contains the URL for the connection
        public PwEntry connPE { get; set; }
        // new PwEntry created for the connection (URL from connPE, username and password from selected rdpAccountEntry)
        public PwEntry returnPE { get; set; }
        
        private void CredentialPickerForm_Load(object sender, EventArgs e)
        {
            // set window size
            Width = Convert.ToInt32(_config.CredPickerWidth);
            Height = Convert.ToInt32(_config.CredPickerHeight);
            CenterToParent();

            loadListEntries();
            olvEntries.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void loadListEntries()
        {
            // create new list with AccountEntry-objects to show them in ObjectListView-element
            List<AccountEntry> listAccounts = new List<AccountEntry>();

            foreach (PwEntry account in rdpAccountEntries)
            {
                // get title, username, notes and a UUID-hash from the Account...
                int uidhash = account.Uuid.GetHashCode();

                string path, title, username, notes;
                path = account.ParentGroup.GetFullPath("\\", false);
                if(_config.KeePassShowResolvedReferences)
                {
                    title = Util.ResolveReferences(account, _db, PwDefs.TitleField);
                    username = Util.ResolveReferences(account, _db, PwDefs.UserNameField);
                    notes = Util.ResolveReferences(account, _db, PwDefs.NotesField);
                } else
                {
                    title = account.Strings.ReadSafe(PwDefs.TitleField);
                    username = account.Strings.ReadSafe(PwDefs.UserNameField);
                    notes = account.Strings.ReadSafe(PwDefs.NotesField);
                }

                // ...and add as new AccountEntry to the list
                AccountEntry accEntry = new AccountEntry(path, title, username, notes, uidhash);
                listAccounts.Add(accEntry);
            }
            // fill the ObjectListView-element with objects from the AccountEntry-list
            olvEntries.SetObjects(listAccounts);
            // select the first entry in the ObjectListView automatically (so user can just press enter for a quick connection)
            olvEntries.Items[0].Selected = true;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            // set returnPE to the selected account
            confirmDialog(); 

            // Form schließen
            Close();
        }

        // when double-clicking an entry in the ObjectListView
        private void olvEntries_ItemActivate(object sender, EventArgs e)
        {
            // set returnPE to the selected account
            confirmDialog();

            // return dialog result OK
            DialogResult = DialogResult.OK;
            // Form schließen
            Close();
        }

        private void confirmDialog()
        {
            // save window Size
            if (_config.CredPickerRememberSize)
            {
                _config.CredPickerWidth = Convert.ToUInt64(Width);
                _config.CredPickerHeight = Convert.ToUInt64(Height);
            }

            try
            {
                // create returnPwEntry
                returnPE = new PwEntry(true, true);

                // set the URL value
                returnPE.Strings.Set(PwDefs.UrlField, connPE.Strings.GetSafe(PwDefs.UrlField));

                // set username and password
                foreach (PwEntry account in rdpAccountEntries)
                {
                    // get UUID-Hash and use entry if it matches the selected custom entry
                    int uidhash = account.Uuid.GetHashCode();
                    if (uidhash == ((AccountEntry)olvEntries.SelectedObject).UidHash)
                    {
                        returnPE.Strings.Set(PwDefs.UserNameField, account.Strings.GetSafe(PwDefs.UserNameField));
                        returnPE.Strings.Set(PwDefs.PasswordField, account.Strings.GetSafe(PwDefs.PasswordField));
                    }
                }
            } catch
            {
                MessageBox.Show("You have to select an account first", "KeePassRDP");
                return;
            }
        }
    }
}
