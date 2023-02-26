/*
 *  Copyright (C) 2018 - 2023 iSnackyCracky, NETertainer
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
using System.Drawing;
using System.Windows.Forms;

namespace KeePassRDP
{
    [ToolboxBitmap(typeof(ToolTip))]
    internal class KprToolTip : ToolTip
    {
        new public string ToolTipTitle
        {
            get
            {
                return base.ToolTipTitle;
            }
            set
            {
                base.ToolTipTitle = KprResourceManager.Instance[value];
            }
        }

        public KprToolTip(IContainer container) : base(container)
        {
        }

        public KprToolTip() : base()
        {
        }

        new public void SetToolTip(Control control, string caption)
        {
            base.SetToolTip(control, KprResourceManager.Instance[caption]);
        }
    }
}