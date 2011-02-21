using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SwitchChecker
{
    public partial class TextForm : Form
    {
        public TextForm(string titleText, string messageText)
        {
            InitializeComponent();
            this.Text = titleText;
            this.richTextBox1.Text = messageText;
        }

        private void MacForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }
    }
}
