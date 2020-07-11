using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OBD2Xam
{
    public class SerialDefs
    {
        public delegate void ConnectionEventType();
    }


    public interface ISerialComm
    {

        // methods/props for dealing with bluetooth device
        Task< List<BtDeviceNameID> > GetBTDevices();  // TODO: this will need to return a list of devices...not sure what that's going to look like yet, string/class
        Task<bool> BtConnect(string deviceID);


        //bool InitializeSerialPort(string port, int baud);
        //void Open();
        string ReadAvailableData();
        void WriteLine(string text);
        void Close();
        bool IsOpen();


        //event SerialDefs.ConnectionEventType OnConnect;
        //event SerialDefs.ConnectionEventType OnTimeout;
    }
}
