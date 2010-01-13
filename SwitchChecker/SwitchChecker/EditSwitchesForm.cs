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
    public partial class EditSwitchesForm : Form
    {
        bool update = false;
        MainForm _mainForm;

        public EditSwitchesForm(MainForm frm)
        {
            InitializeComponent();
            _mainForm = frm;
            populateListData();
        }

        private void populateListData()
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (SwitchInfo sw in MainForm.switches)
            {
                ListViewItem itm = new ListViewItem(new string[] { sw.Name, sw.Address, sw.UserName });
                listView1.Items.Add(itm);
            }
            listView1.EndUpdate();
        }

        private void EditSwitchesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (update)
            {
                _mainForm.populateTabData();
                _mainForm.switchUpdated = true;
            }
        }
            
        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems == null || listView1.SelectedItems.Count < 1)
                return;

            SwitchInfo sw = MainForm.getSwitch(listView1.SelectedItems[0].SubItems[0].Text);

            Form frm = new EditSwitchForm(sw);
            DialogResult dr = frm.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                update = true;
                populateListData();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            Form frm = new EditSwitchForm();
            DialogResult dr = frm.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                update = true;
                populateListData();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems == null || listView1.SelectedItems.Count < 1)
                return;

            DialogResult dr = MessageBox.Show("This will delete all data associated with the current switch.\r\n\r\nAre you sure you wish to continue?",
                "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dr != DialogResult.Yes)
                return;

            MainForm.deleteSwitch(MainForm.getSwitch(listView1.SelectedItems[0].SubItems[0].Text));
            update = true;
            populateListData();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("This will delete all existing switch data and cannot be undone.\r\n\r\nAre you sure you wish to continue?",
                "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dr != DialogResult.Yes)
                return;

            SwitchInfo[] switches = MainForm.switches.ToArray();

            foreach (SwitchInfo sw in switches)
                MainForm.deleteSwitch(sw);

            update = true;
            populateListData();
        }

    }
}
