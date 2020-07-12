using OBD2Xam.Droid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;

[assembly: Xamarin.Forms.Dependency(typeof(SerialComm))]


// https://brianpeek.com/connect-to-a-bluetooth-device-with-xamarinandroid/
namespace OBD2Xam.Droid
{
    class SerialComm : ISerialComm
    {

        private Android.Bluetooth.BluetoothAdapter adapter=null;
        private BluetoothSocket _socket = null;

        public event EventHandler<BtDeviceAddedParams> BtDeviceAdded;
        public event EventHandler<BtDeviceRemovedParams> BtDeviceRemoved;
        public event EventHandler<BtDeviceUpdatedParams> BtDeviceUpdated;
        public event EventHandler BtDeviceEnumerationComplete;
        public event EventHandler BtDeviceEnumerationStarted;

        //public event SerialDefs.ConnectionEventType OnConnect = new SerialDefs.ConnectionEventType(defaultConnectionEventHandler);
        //public event SerialDefs.ConnectionEventType OnTimeout = new SerialDefs.ConnectionEventType(defaultConnectionEventHandler);


        private static void defaultConnectionEventHandler()
        {
            // do nothing, just something to call
        }

        public bool Open()
        {
            bool result = false;

            try
            {
                adapter = BluetoothAdapter.DefaultAdapter;
                if (adapter == null)
                    throw new Exception("No Bluetooth adapter found.");

                if (!adapter.IsEnabled)
                    throw new Exception("Bluetooth adapter is not enabled.");


                // Next, get an instance of the BluetoothDevice representing the physical device you’re connecting to. 
                // You can get a list of currently paired devices using the adapter’s BondedDevices collection. 
                // I use some simple LINQ to find the device I’m looking for:
                BluetoothDevice device = (from bd in adapter.BondedDevices
                                          where bd.Name == "NameOfTheDevice"
                                          select bd).FirstOrDefault();

                if (device == null)
                    throw new Exception("Named device not found.");

                // Finally, use the device’s CreateRfCommSocketToServiceRecord method, which will return a BluetoothSocket 
                // that can be used for connection and communication. Note that the UUID specified below is the standard UUID for SPP:
                _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                //await 
                _socket.ConnectAsync().ConfigureAwait(true);

                result = true;

            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }

            return result;
        }

        public void Close()
        {

            try
            {
                adapter.Dispose();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
        }

        public string ReadLine()
        {
            string result = "";
            try
            {
                // Now that the device is connected, communication occurs via the InputStream and OutputStream properties 
                // which live on the BluetoothSocket object These properties are standard .NET Stream objects and can be used exactly as you’d expect:
                // Read data from the device
                byte[] buffer = new byte[1024 * 1024];
                //await 
                //_socket.InputStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                int  numBytesRead = _socket.InputStream.Read(buffer, 0, buffer.Length);
                buffer[numBytesRead] = 0;

                // TODO: convert bytes to string
                Encoding.UTF8.GetString(buffer);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
            return result;
        }

        public void WriteLine(string text)
        {
            try
            {
                // convert text to bytes
                byte[] buffer = Encoding.UTF8.GetBytes(text);
                // Write data to the device
                //await 
                _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
        }

        /*
        public async Task<List<BtDeviceNameID>> GetBTDevices()
        {
            List<BtDeviceNameID> devices = new List<BtDeviceNameID>();

            if (BluetoothAdapter.DefaultAdapter != null && BluetoothAdapter.DefaultAdapter.IsEnabled)
            {
                foreach (var pairedDevice in BluetoothAdapter.DefaultAdapter.BondedDevices)
                {
                    //devices.Add(pairedDevice.Address + "::" + pairedDevice.Name);
                    devices.Add(new BtDeviceNameID()
                    {
                        Name = pairedDevice.Name,
                        ID = pairedDevice.Address
                    });
                }
            }

            return devices;
        }
        */


        public async Task<bool> BtConnect(string deviceID)
        {
            throw new NotImplementedException();
        }

        public bool IsOpen()
        {
            throw new NotImplementedException();
        }

        public void StartBtDeviceEnumeration()
        {
            throw new NotImplementedException();
        }
    }
}
