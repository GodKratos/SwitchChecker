namespace SwitchChecker
{
    partial class SwitchForm
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
            this.dgvSwitch = new System.Windows.Forms.DataGridView();
            this.columnInterface = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnVlan = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnSpeed = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnDuplex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rightClickMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mACAddressesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSwitch)).BeginInit();
            this.rightClickMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvSwitch
            // 
            this.dgvSwitch.AllowUserToAddRows = false;
            this.dgvSwitch.AllowUserToDeleteRows = false;
            this.dgvSwitch.AllowUserToResizeRows = false;
            this.dgvSwitch.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSwitch.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSwitch.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnInterface,
            this.columnDescription,
            this.columnVlan,
            this.columnSpeed,
            this.columnDuplex});
            this.dgvSwitch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSwitch.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvSwitch.Location = new System.Drawing.Point(0, 0);
            this.dgvSwitch.Name = "dgvSwitch";
            this.dgvSwitch.ReadOnly = true;
            this.dgvSwitch.RowHeadersVisible = false;
            this.dgvSwitch.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSwitch.Size = new System.Drawing.Size(494, 458);
            this.dgvSwitch.TabIndex = 3;
            this.dgvSwitch.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvSwitch_CellMouseClick);
            this.dgvSwitch.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvSwitch_CellDoubleClick);
            // 
            // columnInterface
            // 
            this.columnInterface.FillWeight = 5F;
            this.columnInterface.HeaderText = "Interface";
            this.columnInterface.Name = "columnInterface";
            this.columnInterface.ReadOnly = true;
            // 
            // columnDescription
            // 
            this.columnDescription.FillWeight = 20F;
            this.columnDescription.HeaderText = "Description";
            this.columnDescription.Name = "columnDescription";
            this.columnDescription.ReadOnly = true;
            // 
            // columnVlan
            // 
            this.columnVlan.FillWeight = 5F;
            this.columnVlan.HeaderText = "Vlan";
            this.columnVlan.Name = "columnVlan";
            this.columnVlan.ReadOnly = true;
            // 
            // columnSpeed
            // 
            this.columnSpeed.FillWeight = 5F;
            this.columnSpeed.HeaderText = "Speed";
            this.columnSpeed.Name = "columnSpeed";
            this.columnSpeed.ReadOnly = true;
            // 
            // columnDuplex
            // 
            this.columnDuplex.FillWeight = 5F;
            this.columnDuplex.HeaderText = "Duplex";
            this.columnDuplex.Name = "columnDuplex";
            this.columnDuplex.ReadOnly = true;
            // 
            // rightClickMenu
            // 
            this.rightClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mACAddressesToolStripMenuItem});
            this.rightClickMenu.Name = "rightClickMenu1";
            this.rightClickMenu.Size = new System.Drawing.Size(158, 26);
            // 
            // mACAddressesToolStripMenuItem
            // 
            this.mACAddressesToolStripMenuItem.Name = "mACAddressesToolStripMenuItem";
            this.mACAddressesToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.mACAddressesToolStripMenuItem.Text = "MAC Addresses";
            this.mACAddressesToolStripMenuItem.Click += new System.EventHandler(this.mACAddressesToolStripMenuItem_Click);
            // 
            // SwitchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 458);
            this.Controls.Add(this.dgvSwitch);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SwitchForm";
            this.Text = "SwitchForm";
            ((System.ComponentModel.ISupportInitialize)(this.dgvSwitch)).EndInit();
            this.rightClickMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridViewTextBoxColumn columnInterface;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnVlan;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnSpeed;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnDuplex;
        private System.Windows.Forms.ContextMenuStrip rightClickMenu;
        private System.Windows.Forms.ToolStripMenuItem mACAddressesToolStripMenuItem;
        public System.Windows.Forms.DataGridView dgvSwitch;
    }
}