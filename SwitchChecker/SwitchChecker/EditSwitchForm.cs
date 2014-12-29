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
    public partial class EditSwitchForm : Form
    {
        SwitchInfo editingSwitch = null;

        public EditSwitchForm()
        {
            InitializeComponent();
        }

        public EditSwitchForm(SwitchInfo sw)
        {
            InitializeComponent();
            editingSwitch = sw;
            txtName.Text = sw.Name;
            txtAddress.Text = sw.Address;
            txtUserName.Text = sw.UserName;
            txtPassword.Text = sw.Password;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                MessageBox.Show("The name field cannot be empty, please try again.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(txtAddress.Text.Trim()))
            {
                MessageBox.Show("The address field cannot be empty, please try again.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool result = false;
            if (editingSwitch != null)
                result = editSwitch();
            else
                result = addSwitch();

            if (result)
            {
                this.DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool editSwitch()
        {
            if (!editingSwitch.Name.Equals(txtName.Text) || !editingSwitch.Address.Equals(txtAddress.Text))
            {
                foreach (SwitchInfo sw in MainForm.switches)
                {
                    if (!editingSwitch.Name.Equals(txtName.Text) && sw.Name.ToLower().Equals(txtName.Text.ToLower().Trim()))
                    {
                        MessageBox.Show("That switch name is already in use, please try something else.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    if (!editingSwitch.Address.Equals(txtAddress.Text) && sw.Address.ToLower().Equals(txtAddress.Text.ToLower().Trim()))
                    {
                        MessageBox.Show("That switch address is already in use, please try something else.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
            }

            editingSwitch.Name = txtName.Text.Trim();
            editingSwitch.Address = txtAddress.Text.Trim();
            editingSwitch.UserName = txtUserName.Text.Trim();
            editingSwitch.Password = txtPassword.Text;
            return true;
        }

        private bool addSwitch()
        {
            foreach (SwitchInfo sw in MainForm.switches)
            {
                if (sw.Name.ToLower().Equals(txtName.Text.ToLower().Trim()))
                {
                    MessageBox.Show("That switch name is already in use, please try something else.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (sw.Address.ToLower().Equals(txtAddress.Text.ToLower().Trim()))
                {
                    MessageBox.Show("That switch address is already in use, please try something else.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            SwitchInfo swtch = new SwitchInfo(txtName.Text, txtAddress.Text, txtUserName.Text, Functions.EncryptPassword(txtPassword.Text));
            MainForm.switches.Add(swtch);
            return true;
        }
    }
}
