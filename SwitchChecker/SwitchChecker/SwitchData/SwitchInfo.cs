using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SwitchChecker
{
    public class SwitchInfo
    {
        private string _switchName;
        private string _hostAddress;
        private string _userName;
        private string _encryptedPassword;
        private Collection<SwitchPort> ports = new Collection<SwitchPort>();

        /// <summary>
        /// Contains information about a switch
        /// </summary>
        /// <param name="switchName">Host Name of the switch</param>
        /// <param name="address">IP Address of the switch</param>
        public SwitchInfo(string switchName, string address, string userName, string encryptedPassword)
        {
            _switchName = switchName;
            _hostAddress = address;
            _userName = userName;
            _encryptedPassword = encryptedPassword;
        }

        public string Name
        {
            get
            {
                return _switchName;
            }
            set
            {
                _switchName = value;
            }
        }

        public string Address
        {
            get
            {
                return _hostAddress;
            }
            set
            {
                _hostAddress = value;
            }
        }

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
            }
        }

        public string Password
        {
            get
            {
                return Functions.DecryptPassword(_encryptedPassword);
            }
            set
            {
                _encryptedPassword = Functions.EncryptPassword(value);
            }
        }

        public string EncryptedPassword
        {
            get
            {
                return _encryptedPassword;
            }
        }

        public Collection<SwitchPort> Ports
        {
            get
            {
                return ports;
            }
            set
            {
                ports = value;
            }
        }

        public void addPort(SwitchPort port)
        {
            if (port != null)
            {
                SwitchPort p = getPort(port.Name);
                if (p != null)
                    ports.Remove(p);
                ports.Add(port);
            }
        }

        public SwitchPort getPort(string portName)
        {
            foreach (SwitchPort prt in ports)
            {
                if (prt.Name == portName)
                    return prt;
            }
            return null;
        }
    }
}
