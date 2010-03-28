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
    public partial class MacForm : Form
    {
        public MacForm(string titleText, string messageText)
        {
            InitializeComponent();
            this.Text = titleText;
            this.richTextBox1.Text = messageText;
        }
    }
}
