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
using System.ComponentModel;

[assembly: Xamarin.Forms.Dependency(typeof(BTSerialComm))]

namespace OBD2Xam.Droid
{
    class BTSerialComm : IBTSerialComm
    {
        // https://brianpeek.com/connect-to-a-bluetooth-device-with-xamarinandroid/

        private Android.Bluetooth.BluetoothAdapter adapter=null;
        private BluetoothSocket _socket = null;

        public event EventHandler<BtDeviceAddedParams>   BtDeviceAdded;
        public event EventHandler<BtDeviceRemovedParams> BtDeviceRemoved;
        public event EventHandler<BtDeviceUpdatedParams> BtDeviceUpdated;
        public event EventHandler BtDeviceEnumerationStarted;
        public event EventHandler BtDeviceEnumerationComplete;

        public BTSerialComm()
        {
            BtDeviceEnumerationComplete += SerialComm_BtDeviceEnumerationComplete;
            BtDeviceEnumerationStarted += SerialComm_BtDeviceEnumerationStarted;
            BtDeviceAdded += SerialComm_BtDeviceAdded;
            BtDeviceRemoved += SerialComm_BtDeviceRemoved;
            BtDeviceUpdated += SerialComm_BtDeviceUpdated;
        }

        private void SerialComm_BtDeviceUpdated(object sender, BtDeviceUpdatedParams e)
        {
        }

        private void SerialComm_BtDeviceRemoved(object sender, BtDeviceRemovedParams e)
        {
        }

        private void SerialComm_BtDeviceAdded(object sender, BtDeviceAddedParams e)
        {
        }

        private void SerialComm_BtDeviceEnumerationStarted(object sender, EventArgs e)
        {
        }

        private void SerialComm_BtDeviceEnumerationComplete(object sender, EventArgs e)
        {
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {

            try
            {
                _socket.Close();
                adapter.Dispose();

                _socket = null;
                adapter = null;
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
                // read a single byte at a time until encountering a newline or CF, then return whatever we have (even if empy)
                List<byte> valuesRead = new List<byte>();
                byte nextCharVal = 0;
                do
                {
                    int nextIntVal = _socket.InputStream.ReadByte();
                    //System.Diagnostics.Debug.WriteLine(string.Format("nextIntVal: 0x{0:X4}", nextIntVal));
                    nextCharVal = (byte)nextIntVal;
                    if (nextCharVal == '\n' || nextCharVal == '\r')
                    {
                        break;
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine(string.Format("nextCharVal: 0x{0:X4}", nextCharVal));
                        valuesRead.Add((byte)nextCharVal);
                    }
                } while (true);

                result = Encoding.UTF8.GetString(valuesRead.ToArray());

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
                byte[] buffer = Encoding.UTF8.GetBytes(text);
                _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
        }

        public async Task<bool> BtConnect(string deviceAddress)
        {
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
                                          //where bd.Name == "NameOfTheDevice"
                                          where bd.Address == deviceAddress
                                          select bd).FirstOrDefault();

                if (device == null)
                    throw new Exception("Specified device not found.");

                // Finally, use the device’s CreateRfCommSocketToServiceRecord method, which will return a BluetoothSocket 
                // that can be used for connection and communication. Note that the UUID specified below is the standard UUID for SPP:
                _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString(Constants.BT_SERIAL_PORT_INTERFACE));
                await _socket.ConnectAsync().ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }

            return IsOpen();
        }

        public bool IsOpen()
        {
            bool open = false;
            open = (_socket==null)? false : _socket.IsConnected;
            return open;
        }

        public void StartBtDeviceEnumeration()
        {
            try
            {
                if (BluetoothAdapter.DefaultAdapter != null && BluetoothAdapter.DefaultAdapter.IsEnabled)
                {
                    BtDeviceEnumerationStarted(this, new EventArgs());
                    foreach (var pairedDevice in BluetoothAdapter.DefaultAdapter.BondedDevices)
                    {
                        BtDeviceAdded(this, new BtDeviceAddedParams()
                        {
                            name = pairedDevice.Name,
                            id = pairedDevice.Address
                        });
                    }
                    BtDeviceEnumerationComplete(this, new EventArgs());
                }
                else
                {
                    throw new Exception("No Bluetooth adapter found.");
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
        }

    }
}
