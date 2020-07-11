using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace OBD2Xam.Droid
{
    public class BluetoothComm
    {

        private Android.Bluetooth.BluetoothAdapter adapter;

        public BluetoothComm()
        {
        }


        // https://brianpeek.com/connect-to-a-bluetooth-device-with-xamarinandroid/
        // https://github.com/acaliaro/TestBth
        private async void testCode()
        {

            // First, grab an instance of the default BluetoothAdapter on the Android device and determine if it is enabled:
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
            BluetoothSocket _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
            await _socket.ConnectAsync();


            // Now that the device is connected, communication occurs via the InputStream and OutputStream properties 
            // which live on the BluetoothSocket object These properties are standard .NET Stream objects and can be used exactly as you’d expect:
            // Read data from the device
            byte[] buffer = new byte[1024*1024];
            await _socket.InputStream.ReadAsync(buffer, 0, buffer.Length);

            // Write data to the device
            await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}