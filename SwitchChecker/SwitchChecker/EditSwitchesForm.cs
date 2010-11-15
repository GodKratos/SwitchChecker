using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace SwitchChecker
{
    public partial class EditSwitchesForm : Form
    {
        bool update = false;
        MainForm _mainForm;
        ListViewColumnSorter _lvwColumnSorter = new ListViewColumnSorter();

        public EditSwitchesForm(MainForm frm)
        {
            InitializeComponent();
            _mainForm = frm;
            populateListData();
            listView1.ListViewItemSorter = _lvwColumnSorter;
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

            DialogResult dr = MessageBox.Show("This will delete all data associated with the selected switch(es) and cannot be undone.\r\n\r\nAre you sure you wish to continue?",
                "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dr != DialogResult.Yes)
                return;

            foreach (ListViewItem itm in listView1.SelectedItems)
                MainForm.deleteSwitch(MainForm.getSwitch(itm.SubItems[0].Text));
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

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("If you have a lot of switches selected, updating switch data can take a very long time.\n\nAre you sure you wish to continue?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr != DialogResult.Yes)
                return;

            if (listView1.SelectedItems == null)
            {
                MessageBox.Show("Please select the switch(es) you want to update");
                return;
            }

            Collection<SwitchInfo> switchList = new Collection<SwitchInfo>();

            foreach (ListViewItem itm in listView1.SelectedItems)
            {
                SwitchInfo sw = MainForm.getSwitch(itm.SubItems[0].Text);
                if (sw == null)
                {
                    MessageBox.Show("Error occurred finding switch data for switch " + itm.SubItems[0].Text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }
                switchList.Add(sw);
            }

            _mainForm.progressBar1.Maximum = switchList.Count * 100;
            _mainForm.waitMode(true, "Gathering Data...", true);
            if (!_mainForm.switchUpdated) _mainForm.switchUpdated = true;
            _mainForm.worker.RunWorkerAsync(switchList);
            Close();
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == _lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (_lvwColumnSorter.Order == SortOrder.Ascending)
                    _lvwColumnSorter.Order = SortOrder.Descending;
                else
                    _lvwColumnSorter.Order = SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                _lvwColumnSorter.SortColumn = e.Column;
                _lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            listView1.Sort();
        }

    }
}
