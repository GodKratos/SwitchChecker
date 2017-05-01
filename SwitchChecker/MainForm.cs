using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace SwitchChecker
{
    public partial class MainForm : Form
    {
        private static string DATA_PATH = "SwitchData\\";
        public static Collection<SwitchInfo> switches = new Collection<SwitchInfo>();
        private NameValueCollection ipList = new NameValueCollection();
        private StringCollection failedSwitches = new StringCollection();
        private SearchForm searchForm;
        internal BackgroundWorker worker = new BackgroundWorker();
        public bool switchUpdated = false;
        private bool updating = false;

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
                if (s.Name.ToLower() == name.ToLower())
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
                    if (Properties.Settings.Default.ExcludeTrunksSearch && prt.isTrunk())
                        continue;

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
                    port.SetAttribute("Status", prt.Status);
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


        public static void deleteSwitch(SwitchInfo swtch)
        {
            string filename = DATA_PATH + swtch.Name + ".xml";

            MainForm.switches.Remove(swtch);

            if (File.Exists(filename))
                File.Delete(filename);
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
                    prt.Status = port.Attributes["Status"].Value;
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
            updating = true;
            while (treeView1.Nodes.Count > 1)
                treeView1.Nodes.RemoveAt(1);
            //treeView1.Nodes.Clear();
            //treeView1.Nodes.Add(treeNode1);

            foreach (SwitchInfo sw in switches)
            {
                System.Windows.Forms.TreeNode tn = new System.Windows.Forms.TreeNode(sw.Name);
                tn.Name = sw.Name;
                tn.ToolTipText = sw.Address;
                
                this.treeView1.Nodes.Add(tn);
            }
            updating = false;
            //treeView1.SelectedNode = null;
            //treeView1.SelectedNode = treeView1.Nodes[0];
        }

        private string[] getResultData(string inputData, string firstMatchPattern)
        {
            List<string> lines = inputData.Split("\r".ToCharArray()).ToList();
            var indx = lines.FindIndex(r => r.ToLower().Contains(firstMatchPattern.ToLower()));

            if (indx == 0)
                return lines.ToArray();
            if (indx > -1)
            {
                lines.RemoveRange(0, indx);
                return lines.ToArray();
            }
            else
                return null;
        }

        private void updateSwitch(SwitchInfo sw)
        {
            //If no username specified use defaults for username and password
            string username = (string.IsNullOrEmpty(sw.UserName) ? Properties.Settings.Default.DefaultUserName : sw.UserName);
            string password = (string.IsNullOrEmpty(sw.UserName) ? Functions.DecryptPassword(Properties.Settings.Default.DefaultEncryptedPassword) : sw.Password);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                if (InvokeRequired)
                    Invoke(new UpdateStatus(updateStatus), "Credentials invalid for " + sw.Name + ".");
                if (failedSwitches != null)
                {
                    failedSwitches.Add(sw.Name);
                }
                else
                {
                    if (InvokeRequired)
                        Invoke(new ShowWarning(showWarning), "Credentials are not set correctly to access " + sw.Name + ".", "Error", false);
                }
                return;
            }

            if (InvokeRequired)
                Invoke(new UpdateStatus(updateStatus), "Connecting to " + sw.Name + "...");
            using (var sshclient = new SshClient(sw.Address, username, password))
            {
                try
                {
                    if (InvokeRequired)
                        Invoke(new UpdateStatus(updateStatus), "Gathering data for " + sw.Name + "...");

                    sw.Ports = new Collection<SwitchPort>();
                    sshclient.Connect();
                    var reply = sshclient.RunCommand("show interface description").Result;
                    sshclient.Disconnect();
                    string[] lines = getResultData(reply, "Interface");
                    if (lines != null && lines.Length > 1)
                    {
                        for (int i = 1; i < lines.Length; i++)
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
                    }

                    if (sw.Ports.Count < 1)
                    {
                        if (failedSwitches != null)
                        {
                            failedSwitches.Add(sw.Name);
                        }
                        else
                        {
                            if (InvokeRequired)
                                Invoke(new ShowWarning(showWarning), "Failed to collect data for switch " + sw.Name, "Error", false);
                        }

                        return;
                    }

                    sshclient.Connect();
                    reply = sshclient.RunCommand("show interface status").Result;
                    sshclient.Disconnect();
                    lines = getResultData(reply, "Port");
                    if (lines != null && lines.Length > 1)
                    {
                        for (int i = 1; i < lines.Length; i++)
                        {
                            char[] lineChars = lines[i].ToCharArray();
                            string strInt = "";
                            string strStatus = "";
                            string strVlan = "";
                            string strDuplex = "";
                            string strSpeed = "";
                            string strType = "";
                            for (int j = 0; j < lineChars.Length; j++)
                            {
                                if (j < 10)
                                    strInt += lineChars[j];
                                if (j >= 30 && j < 43)
                                    strStatus += lineChars[j];
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
                            strStatus = strStatus.Trim();
                            strVlan = strVlan.Trim();
                            strDuplex = strDuplex.Trim();
                            strSpeed = strSpeed.Trim();
                            strType = strType.Trim();
                            if (strInt.Length > 0)
                            {
                                SwitchPort prt = sw.getPort(strInt);
                                if (prt != null)
                                {
                                    prt.Status = strStatus;
                                    prt.Vlan = strVlan;
                                    prt.Duplex = strDuplex;
                                    prt.Speed = strSpeed;
                                    prt.Type = strType;
                                }
                            }
                        }
                    }

                    sshclient.Connect();
                    reply = sshclient.RunCommand("show ip arp").Result;
                    sshclient.Disconnect();
                    lines = getResultData(reply, "Protocol");
                    if (lines != null && lines.Length > 1)
                    {
                        for (int i = 1; i < lines.Length; i++)
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
                    }

                    sshclient.Connect();
                    reply = sshclient.RunCommand("show mac address").Result;
                    sshclient.Disconnect();
                    lines = getResultData(reply, "mac address");
                    if (lines != null && lines.Length > 1)
                    {
                        List<string> lineList = lines.ToList();
                        // Remove irrelevant multicast entries
                        if (lineList.FindIndex(r => r.Contains("Multicast Entries")) >= 0)
                        {
                            lineList.RemoveRange(lineList.FindIndex(r => r.Contains("Multicast Entries")), lineList.Count - lineList.FindIndex(r => r.Contains("Multicast Entries")));
                        }

                        foreach (SwitchPort prt in sw.Ports)
                        {
                            foreach (string line in lineList)
                            {
                                var portname = Regex.Match(line.TrimEnd(), @" [^ ]+$");
                                if (portname.Success && prt.Name.Equals(Regex.Replace(portname.Value, @" ([a-zA-Z][a-zA-Z])[a-zA-Z]*([0-9].*)", "$1$2"), StringComparison.OrdinalIgnoreCase))
                                {
                                    var macAdd = Regex.Match(line, @"([0-9a-f]{4}\.){2}[0-9a-f]{4}");
                                    if (macAdd.Success && !prt.getMacs().Contains(macAdd.Value))
                                        prt.addMac(macAdd.Value);
                                }
                            }
                            if (InvokeRequired)
                                Invoke(new UpdateProgress(updateProgress), Math.Max(100 / sw.Ports.Count, 1), true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (failedSwitches != null)
                    {
                        failedSwitches.Add(sw.Name);
                    }
                    else
                    {
                        if (InvokeRequired)
                            Invoke(new ShowWarning(showWarning), "Failed to update switch " + sw.Name + "\r\n\r\n" + ex.Message, "Error", false);
                    }
                }
                finally
                {
                    sshclient.Disconnect();
                    sshclient.Dispose();
                }
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
        internal void waitMode(bool start, string message, bool showProgress)
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
            failedSwitches = new StringCollection();
            if (e.Argument == null)
                switchList = switches;
            else if (e.Argument is Collection<SwitchInfo>)
                switchList = (Collection<SwitchInfo>)e.Argument;
            else
            {
                failedSwitches = null;
                switchList.Add((SwitchInfo)e.Argument);
            }
            
            int i = 0;
            foreach (SwitchInfo sw in switchList)
            {
                updateSwitch(sw);
                if (InvokeRequired)
                    Invoke(new UpdateProgress(updateProgress), ++i * 100, false);
            }

            if (failedSwitches != null && failedSwitches.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Failed to connect to the following switches:");
                foreach (string sw in failedSwitches)
                    sb.AppendLine(sw);
                if (InvokeRequired)
                    Invoke(new ShowWarning(showWarning), sb.ToString(), "Error", true);
            }

            if (e.Argument is SwitchInfo)
                e.Result = e.Argument;
            else
                e.Result = null;
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

        private delegate void ShowWarning(string title, string caption, bool editableText);
        private void showWarning(string text, string caption, bool editableText)
        {
            if (editableText)
            {
                Form warningForm = new TextForm(caption, text);
                warningForm.ShowDialog(this);
            }
            else
                MessageBox.Show(this, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private delegate void UpdateProgress(int value, bool increment);
        private void updateProgress(int value, bool increment)
        {
            if (increment)
                value += progressBar1.Value;
            progressBar1.Value = Math.Min(value, progressBar1.Maximum);
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
            {
                try
                {
                    File.Delete(saveFileDialog1.FileName);
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                    waitMode(false);
                    return;
                }

            }

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

                cmd = new OleDbCommand("CREATE TABLE [Switch Data] ([Switch] string, [IP] string, [Port] string, [Description] string, [Status] string, [Vlan] string, [Speed] string, [Duplex] string, [MAC Address] string)", conn);
                cmd.ExecuteNonQuery();
                
                foreach (SwitchInfo sw in switches)
                {
                    foreach (SwitchPort prt in sw.Ports)
                    {
                        string desc = prt.Description.Replace("'", "");
                        if (prt.getMacs().Count > 0)
                        {
                            foreach (string mac in prt.getMacs())
                            {
                                string macString = mac.Substring(0, 4) + mac.Substring(5, 4) + mac.Substring(10, 4);
                                cmd = new OleDbCommand("INSERT INTO [Switch Data] ([Switch], [IP], [Port], [Description], [Status], [Vlan], [Speed], [Duplex], [MAC Address]) values ('" + sw.Name + "', '" + sw.Address + "', '" + prt.Name + "', '" + desc + "', '" + prt.Status + "', '" + prt.Vlan + "', '" + prt.Speed + "', '" + prt.Duplex + "', '" + mac + "')", conn);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            cmd = new OleDbCommand("INSERT INTO [Switch Data] ([Switch], [IP], [Port], [Description], [Status], [Vlan], [Speed], [Duplex], [MAC Address]) values ('" + sw.Name + "', '" + sw.Address + "', '" + prt.Name + "', '" + desc + "', '" + prt.Status + "', '" + prt.Vlan + "', '" + prt.Speed + "', '" + prt.Duplex + "', 'None')", conn);
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
            if (updating)
                return;

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

        private void treeView1_DoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || treeView1.SelectedNode == null || treeView1.SelectedNode == treeView1.Nodes[0])
                return;

            editSwitchNode();
        }

        private void editSwitchNode()
        {
            SwitchInfo sw = getSwitch(treeView1.SelectedNode.Name);

            Form frm = new EditSwitchForm(sw);
            DialogResult dr = frm.ShowDialog(this);

            if (dr == DialogResult.OK)
            {
                populateTabData();
                switchUpdated = true;
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.Node == treeView1.Nodes[0])
                return;
            
            treeView1.SelectedNode = e.Node;
            switchMenuStrip.Show(treeView1, e.Location);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editSwitchNode();
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SwitchInfo sw = getSwitch(treeView1.SelectedNode.Name);

            Process telnetProcess = new Process();
            string cmdPath = System.IO.Path.Combine(Environment.GetEnvironmentVariable("windir"), @"system32\telnet.exe");
            if (!File.Exists(cmdPath))
                cmdPath = System.IO.Path.Combine(Environment.GetEnvironmentVariable("windir"), @"sysnative\telnet.exe");
            if (!File.Exists(cmdPath))
            {
                MessageBox.Show("Unable to find Telnet exectuable.", "Error", MessageBoxButtons.OK);
                return;
            }
            
            telnetProcess.StartInfo.FileName = cmdPath;
            telnetProcess.StartInfo.Arguments = sw.Address;
            telnetProcess.Start();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("This will delete all data associated with the selected switch and cannot be undone.\r\n\r\nAre you sure you wish to continue?",
                "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dr != DialogResult.Yes)
                return;

            deleteSwitch(getSwitch(treeView1.SelectedNode.Text));
            populateTabData();
        }
    }
}
