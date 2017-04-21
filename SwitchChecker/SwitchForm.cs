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
    public partial class SwitchForm : Form
    {
        private SwitchInfo sw;
        
        public SwitchForm(SwitchInfo switchInfo)
        {
            InitializeComponent();
            
            if (switchInfo == null)
                return;

            sw = switchInfo;
            this.Text = sw.Name;
            AddData();
        }

        public void AddData()
        {
            if (sw != null)
            {
                dgvSwitch.Rows.Clear();
                foreach (SwitchPort prt in sw.Ports)
                    dgvSwitch.Rows.Add(new string[] { prt.Name, prt.Description, prt.Status, prt.Vlan, prt.Speed, prt.Duplex });
            }
        }

        private void dgvSwitch_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            SwitchPort prt = sw.getPort(dgv.Rows[e.RowIndex].Cells[0].Value.ToString());
            showMacs(prt);
        }

        private void dgvSwitch_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && sender is DataGridView)
            {
                DataGridView dgv = (DataGridView)sender;
                int row = e.RowIndex;
                if (row < 0) return;
                dgv.ClearSelection();
                dgv.Rows[row].Selected = true;
                Rectangle rec = dgv.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                rightClickMenu.Show((Control)sender, rec.X + e.X, rec.Y + e.Y);
            }
        }

        private void mACAddressesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SwitchPort prt = sw.getPort(dgvSwitch.SelectedRows[0].Cells[0].Value.ToString());
            showMacs(prt);
        }

        private void showMacs(SwitchPort prt)
        {
            string formTitle = "MAC Addresses for port " + prt.Name + ":";
            string formMessage = "";
            foreach (string s in prt.getMacs())
            {
                formMessage += s + "\n";
            }
            Form macForm = new TextForm(formTitle, formMessage);
            macForm.ShowDialog(this);
        }
    }
}
