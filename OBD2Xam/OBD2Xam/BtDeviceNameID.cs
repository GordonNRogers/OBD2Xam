using System;
using System.Collections.Generic;
using System.Text;

namespace OBD2Xam
{
    public class BtDeviceNameID
    {
        public string Name = "";
        public string ID = "";

        public BtDeviceNameID()
        {

        }

        public override string ToString()
        {
            return Name;
        }
    }
}
