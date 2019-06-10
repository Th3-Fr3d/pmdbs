﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pmdbs
{
    public partial class EditField : UserControl
    {
        private Color bgColor = Color.White;
        public event EventHandler<EventArgs> OnTextTextBoxChanged;
        public EditField()
        {
            InitializeComponent();
            advancedImageButton1.OnClickEvent += AdvancedImageButton1_Click;
            OnResized();
        }

        public Boolean IsEmpty
        {
            get { return advancedTextBox1.IsEmpty; }
        }

        public Boolean UseDefaultValue
        {
            get { return advancedTextBox1.UseDefaultValue; }
            set { advancedTextBox1.UseDefaultValue = value; }
        }

        public string DefaultValue
        {
            get { return advancedTextBox1.DefaultValue; }
            set { advancedTextBox1.DefaultValue = value; }
        }

        /*public char PasswordChar
        {
            get { return advancedTextBox1.PasswordChar; }
            set { advancedTextBox1.PasswordChar = value; }
        }*/

        public Boolean UseSystemPasswordChar
        {
            get { return advancedTextBox1.UseSystemPasswordChar; }
            set { advancedTextBox1.UseSystemPasswordChar = value; }
        }

        public Boolean UseColoredCaret
        {
            get { return advancedTextBox1.UseColoredCaret; }
            set { advancedTextBox1.UseColoredCaret = value; }
        }

        public Image ImageClearNormal
        {
            get { return advancedImageButton1.ImageNormal; }
            set { advancedImageButton1.ImageNormal = value; }
        }

        public Image ImageClearHover
        {
            get { return advancedImageButton1.ImageHover; }
            set { advancedImageButton1.ImageHover = value; }
        }

        public Font FontTitle
        {
            get { return textBox1.Font; }
            set { textBox1.Font = value; }
        }

        public Font FontTextBox
        {
            get { return advancedTextBox1.Font; }
            set { advancedTextBox1.Font = value; }
        }

        public String TextTitle
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        public String TextTextBox
        {
            get { return advancedTextBox1.TextValue; }
            set { advancedTextBox1.TextValue = value; }
        }

        public Color ColorTextBoxNormal
        {
            get { return advancedTextBox1.ColorNormal; }
            set { advancedTextBox1.ColorNormal = value; }
        }

        public Color ColorTextBoxFocus
        {
            get { return advancedTextBox1.ColorFocus; }
            set { advancedTextBox1.ColorFocus = value; }
        }

        public Color ColorTitle
        {
            get { return textBox1.ForeColor; }
            set { textBox1.ForeColor = value; }
        }

        public Color ForeColorTextBoxNormal
        {
            get { return advancedTextBox1.ForeColorNormal; }
            set { advancedTextBox1.ForeColorNormal = value; }
        }

        public Color ForeColorTextBoxFocus
        {
            get { return advancedTextBox1.ForeColorFocus; }
            set { advancedTextBox1.ForeColorFocus = value; }
        }

        public Color BackGroundColor
        {
            get { return bgColor; }
            set {
                bgColor = value;
                this.BackColor = value;
                textBox1.BackColor = value;
                advancedImageButton1.BackColor = value;
                advancedTextBox1.BackgroundColor = value;
            }
        }

        private void AdvancedTextBox1_SizeChanged(object sender, EventArgs e)
        {
            OnResized();
        }

        private void TextBox1_SizeChanged(object sender, EventArgs e)
        {
            OnResized();
        }

        private void OnResized()
        {
            this.Height = advancedTextBox1.Height + textBox1.Height + 30;
            tableLayoutPanel1.ColumnStyles[1].Width = advancedTextBox1.Height;
            advancedImageButton1.Size = new Size(advancedTextBox1.Height, advancedTextBox1.Height);
        }

        private void AdvancedImageButton1_Click(object sender, EventArgs e)
        {
            advancedTextBox1.TextValue = "";
            OnResized();
        }
    }
}
