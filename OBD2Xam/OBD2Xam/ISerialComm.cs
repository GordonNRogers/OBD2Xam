using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OBD2Xam
{
    /*
    public class SerialDefs
    {
        public delegate void ConnectionEventType();
    }
    */


    public interface ISerialComm : IBtEnum
    {

        // methods/props for dealing with bluetooth device
        //Task< List<BtDeviceNameID> > GetBTDevices();
        Task<bool> BtConnect(string deviceID);


        string ReadLine();
        void WriteLine(string text);
        void Close();
        bool IsOpen();

    }
}
