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
    public partial class SearchForm : Form
    {
        public SearchForm()
        {
            InitializeComponent();
        }

        private void mACAddressesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dgvSearch.SelectedRows[0];
            SwitchInfo sw = MainForm.getSwitch(row.Cells[1].Value.ToString());
            if (sw == null)
                return;
            SwitchPort prt = sw.getPort(row.Cells[2].Value.ToString());
            if (prt == null)
                return;
            showMacs(prt);
        }

        private void gotoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((MainForm)MdiParent).gotoSwitch(dgvSearch.SelectedRows[0].Cells[1].Value.ToString(),
                dgvSearch.SelectedRows[0].Cells[2].Value.ToString());
        }

        private void showMacs(SwitchPort prt)
        {
            string formTitle = "MAC Addresses for port " + prt.Name + ":";
            string formMessage = "";
            foreach (string s in prt.getMacs())
            {
                formMessage += s + "\n";
            }
            Form macForm = new MacForm(formTitle, formMessage);
            macForm.ShowDialog(this);
        }

        private void dgvSearch_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ((MainForm)MdiParent).gotoSwitch(dgvSearch.Rows[e.RowIndex].Cells[1].Value.ToString(),
                dgvSearch.Rows[e.RowIndex].Cells[2].Value.ToString());
        }

        private void dgvSearch_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && sender is DataGridView)
            {
                int row = e.RowIndex;
                if (row < 0) return;
                dgvSearch.ClearSelection();
                dgvSearch.Rows[row].Selected = true;
                Rectangle rec = dgvSearch.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                rightClickMenu.Show((Control)sender, rec.X + e.X, rec.Y + e.Y);
            }
        }

    }
}
