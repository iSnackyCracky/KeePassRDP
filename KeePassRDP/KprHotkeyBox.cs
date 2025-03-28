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

using KeePass.Resources;
using KeePass.UI;
using KeePassRDP.Utils;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace KeePassRDP
{
    [DefaultEvent("HotkeyChanged")]
    [DefaultBindingProperty("Hotkey")]
    [DefaultProperty("Hotkey")]
    [TypeConverter(typeof(KprHotkeyBoxConverter))]
    [ToolboxItem(typeof(KprHotkeyBoxToolboxItem))]
    [ToolboxBitmap(typeof(TextBox))]
    [ToolboxItemFilter("System.Windows.Forms")]
    public class KprHotkeyBox : TextBox
    {
        [Category("Hotkey")]
        public event EventHandler HotkeyChanged;

        [ReadOnly(true)]
        [Browsable(false)]
        public new string[] Lines { get { return new string[] { Text }; } private set { base.Lines = value; } }

        [ReadOnly(true)]
        [Browsable(false)]
        public override bool Multiline { get { return false; } }

        [Browsable(false)]
        public new char PasswordChar { get; set; }

        [Browsable(false)]
        public new ScrollBars ScrollBars { get; set; }

        [ReadOnly(true)]
        [Browsable(false)]
        public override bool ShortcutsEnabled { get { return false; } }

        [ReadOnly(true)]
        [Browsable(false)]
        public new bool CanUndo { get { return false; } }

        [ReadOnly(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsFocused { get { return Util.FindFocusedControl(Form.ActiveForm) == this; } }

        [ReadOnly(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue("None")]
        public new string Text { get { return base.Text; } private set { base.Text = value; } }

        [ReadOnly(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool WordWrap { get { return false; } }

        [ReadOnly(true)]
        [Browsable(false)]
        public Keys KeyCode
        {
            get { return _hotkey.KeyCode; }
            private set { _hotkey.KeyCode = value; }
        }

        [ReadOnly(true)]
        [Browsable(false)]
        public bool Windows
        {
            get { return _hotkey.Windows; }
            private set { _hotkey.Windows = value; }
        }

        [ReadOnly(true)]
        [Browsable(false)]
        public bool Control
        {
            get { return _hotkey.Control; }
            private set { _hotkey.Control = value; }
        }

        [ReadOnly(true)]
        [Browsable(false)]
        public bool Alt
        {
            get { return _hotkey.Alt; }
            private set { _hotkey.Alt = value; }
        }

        [ReadOnly(true)]
        [Browsable(false)]
        public bool Shift
        {
            get { return _hotkey.Shift; }
            private set { _hotkey.Shift = value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Hotkey Hotkey
        {
            get { return _hotkey.Clone(); }
            set
            {
                _hotkey.KeyCode = value.KeyCode;
                _hotkey.Windows = value.Windows;
                _hotkey.Control = value.Control;
                _hotkey.Alt = value.Alt;
                _hotkey.Shift = value.Shift;
                RefreshText();
            }
        }

        private readonly Hotkey _hotkey;
        private int _keysPressed;
        private Keys _lastKeyCode;

        public KprHotkeyBox() : base()
        {
            AcceptsReturn = true;
            AcceptsTab = true;

            _hotkey = new Hotkey();
            _keysPressed = 0;
            _lastKeyCode = Keys.None;

            if (DesignMode)
            {
                Text = "None";
                return;
            }

            RefreshText();
        }

        public void Reset()
        {
            KeyCode = Keys.None;
            Windows = Control = Alt = Shift = false;
        }

        private void RefreshText()
        {
            Text = _hotkey;
        }

        protected virtual void OnHotkeyChanged(EventArgs e)
        {
            if (HotkeyChanged != null)
                HotkeyChanged.Invoke(this, e);
        }

        // We swallow it all.
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            e.IsInputKey = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            e.Handled = e.SuppressKeyPress = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = e.SuppressKeyPress = true;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            e.Handled = true;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button == MouseButtons.Left && SelectionStart != Text.Length)
                SelectAll();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left && SelectionLength != Text.Length)
                SelectAll();
        }

        /*public override bool PreProcessMessage(ref Message msg)
        {
            if (DesignMode)
                return base.PreProcessMessage(ref msg);
            return true;
        }*/

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (DesignMode)
                return base.ProcessCmdKey(ref msg, keyData);
            return true;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (DesignMode)
                return base.ProcessDialogKey(keyData);
            return true;
        }

        protected override bool ProcessKeyPreview(ref Message m)
        {
            if (DesignMode)
                return base.ProcessKeyPreview(ref m);
            return true;
        }

        protected override bool ProcessKeyEventArgs(ref Message m)
        {
            if (DesignMode)
                return base.ProcessKeyEventArgs(ref m);
            return true;
        }

        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_CHAR = 0x102;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;
        private const int WM_SYSCHAR = 0x106;
        private const int WM_IME_CHAR = 0x286;

        protected override bool ProcessKeyMessage(ref Message m)
        {
            if (DesignMode)
                return base.ProcessKeyMessage(ref m);

            switch (m.Msg)
            {
                case WM_KEYUP:
                case WM_SYSKEYUP:
                    if (_keysPressed == 0 || --_keysPressed == 0)
                    {
                        _lastKeyCode = Keys.None;
                        OnHotkeyChanged(EventArgs.Empty);
                    }
                    return true;
                case WM_CHAR:
                case WM_SYSCHAR:
                case WM_IME_CHAR:
                    return true;
            }

            var e = new KeyEventArgs(((Keys)(int)(long)m.WParam) | ModifierKeys);

            switch (e.KeyCode)
            {
                case Keys.Delete:
                case Keys.Back:
                    Reset();
                    break;
                default:
                    if (m.Msg == WM_KEYDOWN || m.Msg == WM_SYSKEYDOWN || e.KeyCode == Keys.PrintScreen)
                    {
                        if (e.KeyCode == _lastKeyCode)
                            return true;
                        _lastKeyCode = e.KeyCode;

                        if (_keysPressed++ == 0 || SelectionLength > 0)
                            Reset();

                        Control = e.Control;
                        Shift = e.Shift;
                        Alt = e.Alt;

                        switch (e.KeyCode)
                        {
                            case Keys.Apps:
                                return true;
                            case Keys.ShiftKey:
                            case Keys.ControlKey:
                            case Keys.Menu:
                                KeyCode = Keys.None;
                                break;
                            case Keys.LWin:
                            case Keys.RWin:
                                KeyCode = Keys.None;
                                Windows = true;
                                break;
                            default:
                                KeyCode = e.KeyCode &~ Keys.Modifiers;
                                _keysPressed--;
                                break;
                        }
                    }
                    break;
            }

            RefreshText();
            Select(TextLength, 0);

            return true;
        }
    }

    /// <summary>
    /// Class containing single captured hotkey from <see cref="KprHotkeyBox"/>.
    /// </summary>
    public sealed class Hotkey
    {
        /// <summary>
        /// KeyCode without modifiers.
        /// </summary>
        public Keys KeyCode { get; set; }

        /// <summary>
        /// Windows modifier key.
        /// </summary>
        public bool Windows { get; set; }

        /// <summary>
        /// Control modifier key.
        /// </summary>
        public bool Control { get; set; }

        /// <summary>
        /// Alt modifier key.
        /// </summary>
        public bool Alt { get; set; }

        /// <summary>
        /// Shift modifier key.
        /// </summary>
        public bool Shift { get; set; }

        public Hotkey()
        {
            KeyCode = Keys.None;
            Windows = Control = Alt = Shift = false;
        }

        public Hotkey(Keys keyCode) : this()
        {
            if (keyCode == Keys.None)
                return;

            KeyCode = keyCode & ~Keys.Modifiers;
            Windows = keyCode.HasFlag(Keys.LWin) || keyCode.HasFlag(Keys.RWin);
            Control = keyCode.HasFlag(Keys.Control);
            Alt = keyCode.HasFlag(Keys.Alt);
            Shift = keyCode.HasFlag(Keys.Shift);
        }

        public Keys ToKeyCode()
        {
            var keyCode = KeyCode;

            if (Control)
                keyCode |= Keys.Control;
            if (Alt)
                keyCode |= Keys.Alt;
            if (Shift)
                keyCode |= Keys.Shift;
            if (Windows)
                keyCode |= Keys.LWin;

            return keyCode;
        }

        public Hotkey Clone()
        {
            return new Hotkey
            {
                KeyCode = KeyCode,
                Windows = Windows,
                Control = Control,
                Alt = Alt,
                Shift = Shift
            };
        }

        public override string ToString()
        {
            var keyCode = ToKeyCode();
            return keyCode == Keys.None ? KPRes.None : UIUtil.GetKeysName(keyCode);
        }

        public static implicit operator string(Hotkey hotkey)
        {
            return hotkey.ToString();
        }

        public static implicit operator Hotkey(Keys keycode)
        {
            return new Hotkey(keycode);
        }

        public static implicit operator Keys(Hotkey hotkey)
        {
            return hotkey.ToKeyCode();
        }
    }

    /// <summary>
    /// <see cref="ToolboxItem"/> for <see cref="KprHotkeyBox"/>.
    /// </summary>
    [Serializable]
    internal class KprHotkeyBoxToolboxItem : ToolboxItem
    {
        public KprHotkeyBoxToolboxItem(Type toolType) : base(toolType)
        {
        }

        KprHotkeyBoxToolboxItem(SerializationInfo info, StreamingContext context)
        {
            Deserialize(info, context);
        }

        protected override IComponent[] CreateComponentsCore(IDesignerHost host)
        {
            return new IComponent[] { (KprHotkeyBox)host.CreateComponent(typeof(KprHotkeyBox)) };
        }
    }

    /// <summary>
    /// <see cref="ComponentConverter"/> for <see cref="KprHotkeyBox"/>.
    /// </summary>
    internal class KprHotkeyBoxConverter : ComponentConverter
    {
        public KprHotkeyBoxConverter() : base(typeof(KprHotkeyBox))
        {
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}