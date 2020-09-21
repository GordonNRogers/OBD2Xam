using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OBD2Xam
{
    public interface IBTSerialComm : IBtEnum, IDisposable
    {

        // methods/props for dealing with bluetooth device
        Task<bool> BtConnect(string deviceID);


        string ReadLine();
        void WriteLine(string text);
        void Close();
        bool IsOpen();

    }
}
