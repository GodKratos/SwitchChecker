using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SwitchChecker
{
    public class SwitchPort
    {
        private string _name;
        private string _description;
        public string Vlan { get; set; }
        public string Speed { get; set; }
        public string Duplex { get; set; }
        public string Type { get; set; }
        private Collection<string> _macAddresses = new Collection<string>();

        public SwitchPort(string name, string description)
        {
            _name = name;
            _description = description;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
        }

        public bool isTrunk()
        {
            return (Vlan.Equals("trunk"));
        }

        public void addMac(string mac)
        {
            _macAddresses.Add(mac);
        }

        public Collection<string> getMacs()
        {
            return _macAddresses;
        }
    }
}
