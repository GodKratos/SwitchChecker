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
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
            SetValues();
        }

        private void SetValues()
        {
            txtTimeout.Text = Properties.Settings.Default.Timeout.ToString();
            chkExcludeTrunks.Checked = Properties.Settings.Default.ExcludeTrunksSearch;
            txtUserName.Text = Properties.Settings.Default.DefaultUserName;
            txtPassword.Text = Functions.DecryptPassword(Properties.Settings.Default.DefaultEncryptedPassword);
        }

        private void StoreValues()
        {
            int timeout;
            if (!int.TryParse(txtTimeout.Text, out timeout))
            {
                MessageBox.Show("Telnet Timeout must be a number value");
                return;
            }

            Properties.Settings.Default.Timeout = Math.Max(timeout, 1);
            Properties.Settings.Default.ExcludeTrunksSearch = chkExcludeTrunks.Checked;
            Properties.Settings.Default.DefaultUserName = txtUserName.Text;
            Properties.Settings.Default.DefaultEncryptedPassword = Functions.EncryptPassword(txtPassword.Text);
            Properties.Settings.Default.Save();

            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            StoreValues();
        }
    }
}
