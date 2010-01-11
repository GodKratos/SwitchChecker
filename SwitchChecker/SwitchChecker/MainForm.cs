using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Xml;
using System.Threading;
using System.Data.OleDb;

namespace SwitchChecker
{
    public partial class MainForm : Form
    {
        private string DATA_PATH = "SwitchData\\";
        public static Collection<SwitchInfo> switches = new Collection<SwitchInfo>();
        private NameValueCollection ipList = new NameValueCollection();
        private SearchForm searchForm;
        BackgroundWorker worker = new BackgroundWorker();
        public bool switchUpdated = false;

        public MainForm()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            if (!Directory.Exists(DATA_PATH))
                Directory.CreateDirectory(DATA_PATH);
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            loadData();
        }

        private void updateSwitchDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Updating all switch data can take a very long time.\n\nAre you sure you wish to continue?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr != DialogResult.Yes)
                return;
            
            ipList = new NameValueCollection();

            this.progressBar1.Maximum = switches.Count * 100;
            waitMode(true, "Gathering Data...", true);
            if (!switchUpdated) switchUpdated = true;
            worker.RunWorkerAsync(null);
        }

        public static SwitchInfo getSwitch(string name)
        {
            foreach (SwitchInfo s in switches)
            {
                if (s.Name == name)
                    return s;
            }
            return null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            performSearch();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                performSearch();
            }
        }

        private void performSearch()
        {
            string searchText = textBox1.Text.ToLower();

            //MessageBox.Show("\"" + Regex.Replace(searchText, @"[^0-9a-zA-Z]", "") + "\"");
            //return;
            
            if (searchText.Length < 1)
                return;

            string[] macs = null;

            treeView1.SelectedNode = treeView1.Nodes[0];

            searchForm.dgvSearch.Rows.Clear();
            // comboBox1.SelectedIndex : 0 - All, 1 - Description, 2 - IP Address, 3 - MAC Address
            switch (comboBox1.SelectedIndex)
            {
                case 0: // All
                    if (ipList.Count > 0)
                        macs = ipList.GetValues(searchText);

                    if (macs == null)
                    {
                        if (searchText.Length >= 17 && Regex.IsMatch(searchText.Substring(0, 17), @"([0-9a-f][0-9a-f][:-]){5}([0-9a-f][0-9a-f])"))
                            macs = new string[] { searchText.Substring(0, 2) + searchText.Substring(3, 2) + "." + searchText.Substring(6, 2) +
                        searchText.Substring(9, 2) + "." + searchText.Substring(12, 2) + searchText.Substring(15, 2) };
                        else if (searchText.Length >= 14 && Regex.IsMatch(searchText.Substring(0, 14), @"([0-9a-f]{4}\.){2}[0-9a-f]{4}"))
                            macs = new string[] { searchText.Substring(0, 14) };
                    }
                    break;
                case 1: // Description
                    break;
                case 2: // IP Address
                    Collection<string> tempMacs = new Collection<string>();
                    foreach (string key in ipList.Keys)
                    {
                        if (key.IndexOf(searchText) >= 0)
                        {
                            foreach (string mac in ipList.GetValues(key))
                                tempMacs.Add(mac);
                        }
                    }
                    macs = tempMacs.ToArray();
                    break;
                case 3: // MAC Address
                    foreach (SwitchInfo sw in switches)
                    {
                        foreach (SwitchPort prt in sw.Ports)
                        {
                            foreach (string prtMac in prt.getMacs())
                            {
                                if (Regex.Replace(prtMac, @"[^0-9a-z]", "").IndexOf(Regex.Replace(searchText, @"[^0-9a-z]", "")) >= 0)
                                {
                                    searchForm.dgvSearch.Rows.Add(new string[] { prtMac, sw.Name, prt.Name, prt.Description });
                                }
                            }
                        }
                    }
                    return;
                default:
                    break;
            }

            foreach (SwitchInfo sw in switches)
            {
                foreach (SwitchPort prt in sw.Ports)
                {
                    if (macs != null)
                    {
                        foreach (string str in macs)
                        {
                            if (prt.getMacs().Contains(str))
                            {
                                searchForm.dgvSearch.Rows.Add(new string[] { str, sw.Name, prt.Name, prt.Description });
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (prt.Description.ToLower().IndexOf(searchText.ToLower()) > -1)
                            searchForm.dgvSearch.Rows.Add(new string[] { "", sw.Name, prt.Name, prt.Description });
                    }
                }
            }
        }

        public void gotoSwitch(string swtch, string port)
        {
            SwitchInfo sw = getSwitch(swtch);
            if (sw == null)
                return;
            SwitchPort prt = sw.getPort(port);
            if (prt == null)
                return;

            treeView1.SelectedNode = null;
            treeView1.SelectedNode = treeView1.Nodes[sw.Name];

            foreach (Form frm in MdiChildren)
            {
                if (frm.Text.Equals(sw.Name))
                {
                    SwitchForm swFrm = (SwitchForm)frm;
                    for (int j = 0; j < swFrm.dgvSwitch.RowCount; j++)
                    {
                        if (swFrm.dgvSwitch.Rows[j].Cells[0].Value.ToString() == prt.Name)
                        {
                            swFrm.dgvSwitch.ClearSelection();
                            swFrm.dgvSwitch.Rows[j].Selected = true;
                            swFrm.dgvSwitch.FirstDisplayedScrollingRowIndex = j;
                            break;
                        }
                    }
                    break;
                }
            }
        }

        private void switchTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            SwitchInfo sw = getSwitch(dgv.Name);
            SwitchPort prt = sw.getPort(dgv.Rows[e.RowIndex].Cells[0].Value.ToString());
            showMacs(prt);
        }

        private void showMacs(SwitchPort prt)
        {
            string message = "MAC Addresses for port " + prt.Name + ":\n\n";
            foreach (string s in prt.getMacs())
            {
                message += s + "\n";
            }
            MessageBox.Show(message);
        }

        private void saveSwitchDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveSwitchData();
        }

        private void saveSwitchData()
        {
            if (switches.Count < 1 && ipList.Count < 1)
                return;

            foreach (string file in Directory.GetFiles(DATA_PATH))
            {
                if (file.EndsWith(".xml"))
                    File.Delete(file);
            }

            using (StreamWriter sw = new StreamWriter(DATA_PATH + "IP_List.txt", false))
            {
                foreach (string ip in ipList)
                {
                    foreach (string mac in ipList.GetValues(ip))
                        sw.WriteLine(ip + "\t" + mac);
                }
            }
            
            XmlDocument xd = new XmlDocument();
            foreach (SwitchInfo si in switches)
            {
                string filename = DATA_PATH + si.Name + ".xml";
                using (XmlTextWriter xw = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    xw.Formatting = Formatting.Indented;
                    xw.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
                    xw.WriteStartElement("Switch");
                }
                xd.Load(filename);
                XmlElement swtch = xd.DocumentElement;
                swtch.SetAttribute("Name", si.Name);
                swtch.SetAttribute("Address", si.Address);
                swtch.SetAttribute("UserName", si.UserName);
                swtch.SetAttribute("Password", si.EncryptedPassword);
                foreach (SwitchPort prt in si.Ports)
                {
                    XmlElement port = xd.CreateElement("port");
                    port.SetAttribute("Name", prt.Name);
                    port.SetAttribute("Description", prt.Description);
                    port.SetAttribute("Vlan", prt.Vlan);
                    port.SetAttribute("Speed", prt.Speed);
                    port.SetAttribute("Duplex", prt.Duplex);
                    port.SetAttribute("Type", prt.Type);
                    foreach (string mac in prt.getMacs())
                    {
                        XmlElement macAdd = xd.CreateElement("mac");
                        macAdd.SetAttribute("Address", mac);
                        port.AppendChild(macAdd);
                    }
                    swtch.AppendChild(port);
                }
                xd.Save(filename);
            }
            switchUpdated = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void loadData()
        {
            if (!Directory.Exists(DATA_PATH))
                return;
            if (!File.Exists(DATA_PATH + "IP_List.txt"))
                return;

            waitMode(true, "Gathering Data...");

            using (StreamReader sr = new StreamReader(DATA_PATH + "IP_List.txt"))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    string[] parts = line.Split("\t".ToCharArray());

                    bool found = false;
                    foreach (string ip in ipList)
                    {
                        if (ip.Equals(parts[0]))
                        {
                            foreach (string mac in ipList.GetValues(ip))
                            {
                                if (mac.Equals(parts[1]))
                                    found = true;
                            }
                        }
                    }

                    if (!found)
                        ipList.Add(parts[0], parts[1]);
                }
            }
            
            XmlDocument xd = new XmlDocument();
            foreach (string filename in Directory.GetFiles(DATA_PATH, "*.xml"))
            {
                xd.Load(filename);
                XmlNode swtch = xd.DocumentElement;
                SwitchInfo sw = new SwitchInfo(swtch.Attributes["Name"].Value, swtch.Attributes["Address"].Value, swtch.Attributes["UserName"].Value, swtch.Attributes["Password"].Value);
                for (XmlNode port = swtch.FirstChild; port != null; port = port.NextSibling)
                {
                    SwitchPort prt = new SwitchPort(port.Attributes["Name"].Value, port.Attributes["Description"].Value);
                    prt.Vlan = port.Attributes["Vlan"].Value;
                    prt.Speed = port.Attributes["Speed"].Value;
                    prt.Duplex = port.Attributes["Duplex"].Value;
                    prt.Type = port.Attributes["Type"].Value;
                    for (XmlNode macAdd = port.FirstChild; macAdd != null; macAdd = macAdd.NextSibling)
                    {
                        prt.addMac(macAdd.Attributes["Address"].Value);
                    }
                    sw.addPort(prt);
                }
                switches.Add(sw);
            }

            populateTabData();

            waitMode(false);
        }

        public void populateTabData()
        {
            while (treeView1.Nodes.Count > 1)
                treeView1.Nodes.RemoveAt(1);
            //treeView1.Nodes..Clear();
            //treeView1.Nodes.Add(treeNode1);

            foreach (SwitchInfo sw in switches)
            {
                System.Windows.Forms.TreeNode tn = new System.Windows.Forms.TreeNode(sw.Name);
                tn.Name = sw.Name;
                tn.ToolTipText = sw.Address;
                
                this.treeView1.Nodes.Add(tn);
            }
        }

        private void updateSwitch(SwitchInfo sw)
        {
            TelnetConnection tc = new TelnetConnection(sw.Address, 23);
            try
            {
                if (InvokeRequired)
                    Invoke(new UpdateStatus(updateStatus), "Connecting to " + sw.Name + "...");
                Exception ex = null;
                if ((ex = tc.Connect()) != null)
                {
                    MessageBox.Show("Failed to connect to switch " + sw.Name + "\r\n\r\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                sw.Ports = new Collection<SwitchPort>();

                //If no username specified use defaults for username and password
                string username = (string.IsNullOrEmpty(sw.UserName) ? Properties.Settings.Default.DefaultUserName : sw.UserName);
                string password = (string.IsNullOrEmpty(sw.UserName) ? Properties.Settings.Default.DefaultPassword : sw.Password);

                tc.WaitAndSend(new byte[] { 255, 251, 1 }, new byte[] { 255, 253, 1 });
                tc.WaitAndSend(new byte[] { 255, 251, 3 }, new byte[] { 255, 253, 3, 255, 251, 24, 255, 251, 31 });
                tc.WaitAndSend(new byte[] { 255, 253, 31 }, new byte[] { 255, 250, 31, 0, 200, 0, 0, 255, 240 });
                //tc.WaitAndSend(new byte[] { 255, 253, 24 }, new byte[] { 255, 250, 24, 0, 65, 78, 83, 73, 255, 240 }); //ANSI terminal
                //tc.WaitAndSend(new byte[] { 255, 253, 24 }, new byte[] { 255, 250, 24, 0, 86, 84, 50, 50, 48, 255, 240 }); // VT220 terminal
                tc.WaitAndSend("Username:", username);
                tc.WaitAndSend("Password:", password);

                tc.WaitFor(tc.PROMPT);
                string reply = tc.SendCommand("show interface description");
                string[] lines = reply.Split("\r".ToCharArray());
                for (int i = 2; i < lines.Length - 1; i++)
                {
                    char[] lineChars = lines[i].ToCharArray();
                    string strInt = "";
                    string strDesc = "";
                    for (int j = 0; j < lineChars.Length; j++)
                    {
                        if (j < 31)
                            strInt += lineChars[j];
                        if (j > 54)
                            strDesc += lineChars[j];
                    }
                    strInt = strInt.Trim();
                    strDesc = strDesc.Trim();
                    if (strInt.Length > 0)
                    {
                        sw.addPort(new SwitchPort(strInt, strDesc));
                    }
                }

                reply = tc.SendCommand("show interface status");
                lines = reply.Split("\r".ToCharArray());
                for (int i = 3; i < lines.Length - 1; i++)
                {
                    char[] lineChars = lines[i].ToCharArray();
                    string strInt = "";
                    string strVlan = "";
                    string strDuplex = "";
                    string strSpeed = "";
                    string strType = "";
                    for (int j = 0; j < lineChars.Length; j++)
                    {
                        if (j < 10)
                            strInt += lineChars[j];
                        if (j >= 43 && j < 54)
                            strVlan += lineChars[j];
                        if (j >= 54 && j < 61)
                            strDuplex += lineChars[j];
                        if (j >= 61 && j < 68)
                            strSpeed += lineChars[j];
                        if (j >= 68)
                            strType += lineChars[j];
                    }
                    strInt = strInt.Trim();
                    strVlan = strVlan.Trim();
                    strDuplex = strDuplex.Trim();
                    strSpeed = strSpeed.Trim();
                    strType = strType.Trim();
                    if (strInt.Length > 0)
                    {
                        SwitchPort prt = sw.getPort(strInt);
                        if (prt != null)
                        {
                            prt.Vlan = strVlan;
                            prt.Duplex = strDuplex;
                            prt.Speed = strSpeed;
                            prt.Type = strType;
                        }
                    }
                }

                reply = tc.SendCommand("show ip arp");
                lines = reply.Split("\r".ToCharArray());
                for (int i = 2; i < lines.Length - 1; i++)
                {
                    char[] lineChars = lines[i].ToCharArray();
                    string strIp = "";
                    string strMac = "";
                    for (int j = 0; j < lineChars.Length; j++)
                    {
                        if (j >= 10 && j < 27)
                            strIp += lineChars[j];
                        if (j >= 38 && j < 54)
                            strMac += lineChars[j];
                    }
                    strIp = strIp.Trim();
                    strMac = strMac.Trim();
                    if (strIp.Length > 6)
                    {
                        bool found = false;
                        foreach (string ip in ipList)
                        {
                            if (ip.Equals(strIp))
                            {
                                foreach (string mac in ipList.GetValues(ip))
                                {
                                    if (mac.Equals(strMac))
                                        found = true;
                                }
                            }
                        }
                        if (!found)
                            ipList.Add(strIp, strMac);
                    }
                }
                //MessageBox.Show(reply);
                if (InvokeRequired)
                    Invoke(new UpdateStatus(updateStatus), "Gathering data for " + sw.Name + "...");
                foreach (SwitchPort prt in sw.Ports)
                {
                    reply = tc.SendCommand("show mac address interface " + prt.Name);
                    lines = reply.Split("\r".ToCharArray());
                    if (lines.Length > 6)
                    {
                        for (int i = 6; i < lines.Length - 2; i++)
                        {
                            if (lines[i].Length > 21)
                                prt.addMac(lines[i].Substring(9, 14));
                        }
                    }
                    if (InvokeRequired)
                        Invoke(new UpdateProgress(updateProgress), Math.Max(100/sw.Ports.Count,1), true);
                }
                
            }
            finally
            {
                tc.Disconnect();
            }
        }

        private void updateCurrentSwitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null || treeView1.SelectedNode == treeView1.Nodes[0])
            {
                MessageBox.Show("Please select the switch you want to update");
                return;
            }

            SwitchInfo sw = getSwitch(treeView1.SelectedNode.Name);
            if (sw == null)
            {
                MessageBox.Show("Error occurred finding switch data!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.progressBar1.Maximum = 100;
            waitMode(true, "Gathering Data...", true);
            if (!switchUpdated) switchUpdated = true;
            worker.RunWorkerAsync(sw);
        }
        
        /// <summary>
        /// Starts or stops wait mode processing of status bar and mouse icon
        /// </summary>
        /// <param name="start">True to begin wait mode, False to end it.</param>
        /// <param name="message">Message to display in status bar.</param>
        /// <param name="showProgress">True to show progress bar.</param>
        private void waitMode(bool start, string message, bool showProgress)
        {
            if (start)
            {
                this.menuStrip1.Enabled = false;
                this.statusBarPanel1.Text = "  " + message;
                if (showProgress)
                {
                    this.progressBar1.Value = 0;
                    this.progressBar1.Visible = true;
                }
                this.Cursor = Cursors.WaitCursor;
            }
            else
            {
                this.menuStrip1.Enabled = true;
                this.statusBarPanel1.Text = "";
                this.progressBar1.Visible = false;
                this.Cursor = Cursors.Default;
            }
            this.progressBar1.Refresh();
        }

        /// <summary>
        /// Starts or stops wait mode processing of status bar and mouse icon
        /// </summary>
        /// <param name="start">True to begin wait mode, False to end it.</param>
        /// <param name="message">Message to display in status bar.</param>
        private void waitMode(bool start, string message)
        {
            waitMode(start, message, false);
        }

        /// <summary>
        /// Starts or stops wait mode processing of status bar and mouse icon
        /// </summary>
        /// <param name="start">True to begin wait mode, False to end it.</param>
        private void waitMode(bool start)
        {
            waitMode(start, "", false);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Collection<SwitchInfo> switchList = new Collection<SwitchInfo>();
            if (e.Argument == null)
                switchList = switches;
            else
                switchList.Add((SwitchInfo)e.Argument);
            
            int i = 0;
            foreach (SwitchInfo sw in switchList)
            {
                updateSwitch(sw);
                if (InvokeRequired)
                    Invoke(new UpdateProgress(updateProgress), ++i * 100, false);
            }
            e.Result = e.Argument;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            treeView1.SelectedNode = treeView1.Nodes[0];
            if (e.Result == null)
                populateTabData();
            else
            {
                SwitchInfo sw = (SwitchInfo)e.Result;
                SwitchForm swFrm = null;
                foreach (Form frm in MdiChildren)
                {
                    if (frm.Name.Equals(sw.Name))
                    {
                        swFrm = (SwitchForm)frm;
                        break;
                    }
                }

                if (swFrm != null)
                    swFrm.AddData();

                treeView1.SelectedNode = treeView1.Nodes[sw.Name];
            }
            waitMode(false);
        }

        private delegate void UpdateProgress(int value, bool increment);
        private void updateProgress(int value, bool increment)
        {
            if (increment)
                value += progressBar1.Value;
            progressBar1.Value = value > progressBar1.Maximum ? progressBar1.Maximum : value;
        }

        private delegate void UpdateStatus(string message);
        private void updateStatus(string message)
        {
            waitMode(true, message);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 abt = new AboutBox1();
            abt.ShowDialog(this);
        }
        
        private void outputMACDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (switches.Count < 1 && ipList.Count < 1)
                return;

            saveFileDialog1.FileName = "MacList.xls";

            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            waitMode(true, "Saving data...");

            if (File.Exists(saveFileDialog1.FileName))
                File.Delete(saveFileDialog1.FileName);

            using (OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + saveFileDialog1.FileName + ";Extended Properties='Excel 8.0;HDR=Yes'"))
            {
                conn.Open();

                OleDbCommand cmd;

                cmd = new OleDbCommand("CREATE TABLE [IP List] ([IP Address] string, [MAC Address] string)", conn);
                cmd.ExecuteNonQuery();

                foreach (string ip in ipList)
                {
                    foreach (string mac in ipList.GetValues(ip))
                    {
                        cmd = new OleDbCommand("INSERT INTO [IP List] ([IP Address], [MAC Address]) values ('" + ip + "', '" + mac + "')", conn);
                        cmd.ExecuteNonQuery();
                    }
                }

                cmd = new OleDbCommand("CREATE TABLE [Switch Data] ([Switch] string, [IP] string, [Port] string, [Description] string, [Vlan] string, [MAC Address] string)", conn);
                cmd.ExecuteNonQuery();
                
                foreach (SwitchInfo sw in switches)
                {
                    foreach (SwitchPort prt in sw.Ports)
                    {
                        foreach (string mac in prt.getMacs())
                        {
                            string macString = mac.Substring(0, 4) + mac.Substring(5, 4) + mac.Substring(10, 4);
                            cmd = new OleDbCommand("INSERT INTO [Switch Data] ([Switch], [IP], [Port], [Description], [Vlan], [MAC Address]) values ('" + sw.Name + "', '" + sw.Address + "', '" + prt.Name + "', '" + prt.Description + "', '" + prt.Vlan + "', '" + macString + "')", conn);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            waitMode(false);
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsForm opFrm = new OptionsForm();
            opFrm.ShowDialog(this);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            bool found = false;

            foreach (Form frm in MdiChildren)
            {
                if (frm.Text.Equals(e.Node.Name))
                {
                    if (frm.Handle == IntPtr.Zero)
                        frm.Show();
                    frm.Focus();
                    found = true;
                }
                else
                {
                    if (!frm.Text.Equals("Search Results"))
                        frm.Close();
                }
            }

            if (found)
                return;
                
            // Form does not exist so create it
            Form form = null;

            if (e.Node.Name.Equals("Search Results"))
            {
                form = new SearchForm();
                searchForm = (SearchForm)form;
            }
            else
                form = new SwitchForm(getSwitch(e.Node.Name));

            form.MdiParent = this;

            if (form.Handle == IntPtr.Zero)
                form.Show();
            form.Focus();
            form.WindowState = FormWindowState.Maximized;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                TreeNode tn = treeView1.SelectedNode;
                foreach (Form frm in MdiChildren)
                    frm.Close();
                treeView1.SelectedNode = null;
                treeView1.SelectedNode = tn;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (switchUpdated)
            {
                DialogResult dr = MessageBox.Show("Do you wish to save all switch data?", "Shutdown", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    saveSwitchData();
                }
            }
        }

        private void editSwitchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditSwitchesForm frm = new EditSwitchesForm(this);
            frm.ShowDialog(this);
        }

    }
}
