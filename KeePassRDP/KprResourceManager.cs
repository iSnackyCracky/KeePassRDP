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

using System;
using System.Windows.Forms;

namespace KeePassRDP
{
    internal class KprResourceManager
    {
        private static readonly Lazy<KprResourceManager> _instance = new Lazy<KprResourceManager>(
            () => new KprResourceManager(),
            System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        public static KprResourceManager Instance { get { return _instance.Value; } }

        public string this[string key]
        {
            get { return Resources.Resources.ResourceManager.GetString(key, Resources.Resources.Culture) ?? key; }
        }

        private KprResourceManager()
        {
        }

        public void Translate(Control control)
        {
            control.SuspendLayout();
            control.Text = this[control.Text];
            control.ResumeLayout(false);
        }

        public void TranslateMany(params Control[] controls)
        {
            foreach (var control in controls)
                Translate(control);
        }
    }
}