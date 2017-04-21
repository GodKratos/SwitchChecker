namespace SwitchChecker
{
    partial class SearchForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dgvSearch = new System.Windows.Forms.DataGridView();
            this.columnMac = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnSwitch = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnInt = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rightClickMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.gotoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mACAddressesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSearch)).BeginInit();
            this.rightClickMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvSearch
            // 
            this.dgvSearch.AllowUserToAddRows = false;
            this.dgvSearch.AllowUserToDeleteRows = false;
            this.dgvSearch.AllowUserToResizeRows = false;
            this.dgvSearch.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSearch.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSearch.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnMac,
            this.columnSwitch,
            this.columnInt,
            this.columnDescription});
            this.dgvSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSearch.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvSearch.Location = new System.Drawing.Point(0, 0);
            this.dgvSearch.Name = "dgvSearch";
            this.dgvSearch.ReadOnly = true;
            this.dgvSearch.RowHeadersVisible = false;
            this.dgvSearch.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSearch.Size = new System.Drawing.Size(605, 497);
            this.dgvSearch.TabIndex = 2;
            this.dgvSearch.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvSearch_CellMouseClick);
            this.dgvSearch.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvSearch_CellDoubleClick);
            // 
            // columnMac
            // 
            this.columnMac.FillWeight = 10F;
            this.columnMac.HeaderText = "MAC";
            this.columnMac.Name = "columnMac";
            this.columnMac.ReadOnly = true;
            // 
            // columnSwitch
            // 
            this.columnSwitch.FillWeight = 10F;
            this.columnSwitch.HeaderText = "Switch";
            this.columnSwitch.Name = "columnSwitch";
            this.columnSwitch.ReadOnly = true;
            // 
            // columnInt
            // 
            this.columnInt.FillWeight = 10F;
            this.columnInt.HeaderText = "Interface";
            this.columnInt.Name = "columnInt";
            this.columnInt.ReadOnly = true;
            // 
            // columnDescription
            // 
            this.columnDescription.FillWeight = 30F;
            this.columnDescription.HeaderText = "Description";
            this.columnDescription.Name = "columnDescription";
            this.columnDescription.ReadOnly = true;
            // 
            // rightClickMenu
            // 
            this.rightClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gotoToolStripMenuItem,
            this.mACAddressesToolStripMenuItem});
            this.rightClickMenu.Name = "rightClickMenu1";
            this.rightClickMenu.Size = new System.Drawing.Size(158, 70);
            // 
            // gotoToolStripMenuItem
            // 
            this.gotoToolStripMenuItem.Name = "gotoToolStripMenuItem";
            this.gotoToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.gotoToolStripMenuItem.Text = "Goto";
            this.gotoToolStripMenuItem.Click += new System.EventHandler(this.gotoToolStripMenuItem_Click);
            // 
            // mACAddressesToolStripMenuItem
            // 
            this.mACAddressesToolStripMenuItem.Name = "mACAddressesToolStripMenuItem";
            this.mACAddressesToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.mACAddressesToolStripMenuItem.Text = "MAC Addresses";
            this.mACAddressesToolStripMenuItem.Click += new System.EventHandler(this.mACAddressesToolStripMenuItem_Click);
            // 
            // SearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(605, 497);
            this.Controls.Add(this.dgvSearch);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SearchForm";
            this.Text = "Search Results";
            ((System.ComponentModel.ISupportInitialize)(this.dgvSearch)).EndInit();
            this.rightClickMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridViewTextBoxColumn columnMac;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnSwitch;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnInt;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnDescription;
        public System.Windows.Forms.DataGridView dgvSearch;
        private System.Windows.Forms.ContextMenuStrip rightClickMenu;
        private System.Windows.Forms.ToolStripMenuItem gotoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mACAddressesToolStripMenuItem;
    }
}