using System;
using System.Collections.Generic;
using System.Text;

namespace OBD2Xam
{
    public static class Constants
    {
        // per https://www.bluetooth.com/specifications/assigned-numbers/service-discovery/,
        // the service type 1101 is the serial port
        public const string BT_SERIAL_PORT_INTERFACE = @"{00001101-0000-1000-8000-00805F9B34FB}";
    }                                                
}
