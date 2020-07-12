﻿using OBD2Xam.UWP;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using System.IO.Ports;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;
using Windows.Foundation;
using System.Threading;
using System.IO;
using Windows.Storage;
using System.Text;
using System.Runtime.CompilerServices;

[assembly: Xamarin.Forms.Dependency(typeof(SerialComm))]
#pragma warning disable 1998  // disable 'no await' warnings

namespace OBD2Xam.UWP
{
    class SerialComm : ISerialComm
    {
        // per https://www.bluetooth.com/specifications/assigned-numbers/service-discovery/,
        // the service type 1101 is the serial port
        private const string SERIAL_PORT_INTERFACE = @"{00001101-0000-1000-8000-00805F9B34FB}";

        StreamSocket streamSocket = null;
        DataWriter dw = null;
        StreamReader sr = null;

        public SerialComm()
        {
            BtDeviceEnumerationStarted += SerialComm_BtDeviceEnumerationStarted;
            BtDeviceEnumerationComplete += SerialComm_BtDeviceEnumerationComplete;
        }

        private void SerialComm_BtDeviceEnumerationStarted(object sender, EventArgs e)
        {
        }

        private void SerialComm_BtDeviceEnumerationComplete(object sender, EventArgs e)
        {
        }

        public void Close()
        {
            try
            {
                sr?.Dispose();
                dw?.Dispose();
                streamSocket?.Dispose();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
        }

        // https://docs.microsoft.com/en-us/windows/uwp/packaging/app-capability-declarations
        // https://docs.microsoft.com/en-us/uwp/api/windows.devices.serialcommunication
        // https://docs.microsoft.com/en-us/uwp/api/windows.devices.serialcommunication.serialdevice

        private static void defaultConnectionEventHandler()
        {
            // do nothing, just something to call
        }

        public string ReadLine()
        {
            string result = "";
            try
            {
                if (sr == null)
                {
                    System.Diagnostics.Debugger.Break();
                    return result;
                }

                lock (sr)
                {
                    // the stream only supports blocking reads, so the thread will be blocked while trying to read
                    // the thread will block until the next byte is read, but that's ok as long as that's the sole purpose of the thread
                    List<byte> valuesRead = new List<byte>();
                    byte nextCharVal = 0;
                    do
                    {
                        int nextIntVal = sr.Read();
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
                    //System.Diagnostics.Debug.WriteLine("result: " + result);
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
            return result;
        }

        public async void WriteLine(string text)
        {
            try
            {
                dw.WriteString(text);
                await dw.StoreAsync();
                await dw.FlushAsync();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
        }

        public bool IsOpen()
        {
            bool open = false;

            if (streamSocket != null && dw != null && sr != null)
                open = true;

            return open;
        }

        // https://docs.microsoft.com/en-us/uwp/api/windows.devices.enumeration.devicewatcher?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev15.query%3FappId%3DDev15IDEF1%26l%3DEN-US%26k%3Dk(Windows.Devices.Enumeration.DeviceWatcher)%3Bk(TargetFrameworkMoniker-.NETCore%2CVersion%3Dv5.0)%3Bk(DevLang-csharp)%26rd%3Dtrue
        private DeviceWatcher deviceWatcher = null;

        public event EventHandler<BtDeviceAddedParams> BtDeviceAdded;
        public event EventHandler<BtDeviceRemovedParams> BtDeviceRemoved;
        public event EventHandler<BtDeviceUpdatedParams> BtDeviceUpdated;
        public event EventHandler BtDeviceEnumerationComplete;
        public event EventHandler BtDeviceEnumerationStarted ;

        private void onBtDeviceAdded (DeviceWatcher dw, DeviceInformation di)
        {
            BtDeviceAdded(this, new BtDeviceAddedParams { name = di.Name, id = di.Id });
        }

        private void onBtDeviceRemoved(DeviceWatcher dw, DeviceInformationUpdate diu)
        {
            BtDeviceRemoved(this, new BtDeviceRemovedParams { id = diu.Id });
        }

        private void onBtDeviceUpdated(DeviceWatcher dw, DeviceInformationUpdate diu)
        {
            BtDeviceUpdated(this, new BtDeviceUpdatedParams { id = diu.Id });
        }

        private void OnBtEnumerationComplete(DeviceWatcher dw, object obj)
        {
            BtDeviceEnumerationComplete(this, new EventArgs());
        }

        public void StartBtDeviceEnumeration()
        {
            string[] requestedProperties = new string[] {
                "System.Devices.Aep.DeviceAddress",
                "System.Devices.Aep.IsConnected"
            };

            deviceWatcher?.Stop();  // if deviceWatcher already exists, stop it
            deviceWatcher = DeviceInformation.CreateWatcher("(System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\")",
                                                            requestedProperties,
                                                            DeviceInformationKind.AssociationEndpoint);

            // start the enumeration, but run it in background, let the events manage the list
            deviceWatcher.Added += new Windows.Foundation.TypedEventHandler<DeviceWatcher, DeviceInformation>(onBtDeviceAdded);
            deviceWatcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(onBtDeviceRemoved);
            deviceWatcher.Updated += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(onBtDeviceUpdated);
            deviceWatcher.EnumerationCompleted += new TypedEventHandler<DeviceWatcher, object>(OnBtEnumerationComplete);
            deviceWatcher.Stopped += new TypedEventHandler<DeviceWatcher, object>(OnBtEnumerationComplete);
            deviceWatcher.Start();
        }

        public async Task<bool> BtConnect(string deviceID)
        {

            try
            {
                //System.Diagnostics.Debug.Assert(Thread.CurrentThread.IsBackground, "SerialComm:BtConnect() cannot be called from the UI thread.");

                DeviceAccessStatus accessStatus = DeviceAccessInformation.CreateFromId(deviceID).CurrentStatus;
                if (accessStatus == DeviceAccessStatus.DeniedByUser)
                {
                    //await OBD2Xam.MainPage.Instance.DisplayAlert("Error",
                    //    "This app does not have access to connect to the remote device.  Please grant access in Settings > Privacy > Other Devices.",
                    //    "Cancel");
                    await Xamarin.Forms.Device.InvokeOnMainThreadAsync(
                        () => { OBD2Xam.MainPage.Instance.DisplayAlert("Error",
                            "This app does not have access to connect to the remote device.  Please grant access in Settings > Privacy > Other Devices.", 
                            "Cancel"); }
                        );
                    return false;
                }

                BluetoothDevice device = await BluetoothDevice.FromIdAsync(deviceID);
                if (device == null)
                {
                    //rootPage.NotifyUser("Bluetooth Device returned null. Access Status = " + accessStatus.ToString(), NotifyType.ErrorMessage);
                    System.Diagnostics.Debug.WriteLine("Bluetooth Device returned null. Access Status = " + accessStatus.ToString());
                    System.Diagnostics.Debugger.Break();
                    return false;
                }
                //System.Diagnostics.Debug.WriteLine(device.ConnectionStatus);

                DeviceAccessStatus das;
                das = await device.RequestAccessAsync();  // might not always work...
                //System.Diagnostics.Debugger.Break();

                /*
                // RequestAccessAsync() needs to executed on the UI thread, which means the UI thread cannot be blocked
                // while waiting for all this other crap to run.  So this code needs to be executed in backround,
                // WITHOUT an await, because that would cause the UI thread to block.
                bool invokeComplete = false;
                Xamarin.Forms.Device.BeginInvokeOnMainThread( async () => {
                    das = await device.RequestAccessAsync();
                    invokeComplete = true;
                });

                // now we wait for the UI thread to finish it's task, without await
                // because BeginInvokeOnMainThread() isn't awaitable.
                while (!invokeComplete)
                {
                    System.Diagnostics.Debug.WriteLine("waiting...");
                    System.Threading.Thread.Sleep(100);
                }
                */

                if (das == DeviceAccessStatus.Allowed)
                {
                    RfcommDeviceServicesResult rfResultList = await device.GetRfcommServicesAsync().AsTask().ConfigureAwait(false);
                    logRfTypes(rfResultList);

                    // https://blog.j2i.net/2018/07/29/connecting-to-bluetooth-rfcomm-with-uwp/
                    if (rfResultList.Services.Count > 0)
                    {
                        foreach (var service in rfResultList.Services)
                        {
                            if (service.ServiceId.AsString() == SERIAL_PORT_INTERFACE)
                            {
                                streamSocket = new StreamSocket(); 
                                await streamSocket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName);
                                dw = new DataWriter(streamSocket.OutputStream);
                                sr = new StreamReader(streamSocket.InputStream.AsStreamForRead(256));
                                break;
                            }
                        }
                        if(!IsOpen())
                        {
                            throw new Exception("Service not found)");
                        }
                    }
                }

            }
            catch (Exception exc)
            {
                if (exc.Message == "Element not found. (Exception from HRESULT: 0x80070490)")
                {
                    //System.Diagnostics.Debug.WriteLine("Device not listening.");
                    await Xamarin.Forms.Device.InvokeOnMainThreadAsync(
                        () => { OBD2Xam.MainPage.Instance.DisplayAlert("Error", "Device not listening.", "Cancel"); } 
                        );
                    this.Close();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(exc.Message);
                    System.Diagnostics.Debugger.Break();
                }
            }


            return IsOpen();
        }

        private async void logRfTypes(RfcommDeviceServicesResult rfResultList)
        {
            try
            {
                string fileName = "BtServices.log";

                // can't use Syste.IO in UWP, use Windows.Storage instead: 
                // https://docs.microsoft.com/en-us/windows/uwp/files/quickstart-reading-and-writing-files
                // path is determined when project is deployed, so it could change
                // currently C:\Users\Gordon.000\AppData\Local\Packages\aaa810d2-9117-4b86-9d5d-a47aaebb1c5c_3rdrqcfnf2zwp\LocalState
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile logFile = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                List<string> lines = new List<string>();

                BluetoothDevice device = rfResultList.Services[0].Device;
                lines.Add("Device.Name: " + device.Name);
                lines.Add("Device.ConnectionStatus: " + device.ConnectionStatus);
                lines.Add("Device.BluetoothAddress: " + device.BluetoothAddress);
                lines.Add("Device.BluetoothDeviceId: " + device.BluetoothDeviceId.Id);
                lines.Add("Device.ClassOfDevice: " + device.ClassOfDevice.ServiceCapabilities);
                lines.Add("");

                foreach (RfcommDeviceService rf in rfResultList.Services)
                {
                    lines.Add("ServiceId: " + rf.ServiceId.AsString());
                    lines.Add("ConnectionHostName: " + rf.ConnectionHostName);
                    lines.Add("ConnectionServiceName: " + rf.ConnectionServiceName);
                    //lines.Add("selector: " + RfcommDeviceService.GetDeviceSelectorForBluetoothDeviceAndServiceId(rf.Device, rf.ServiceId));

                    DeviceAccessStatus serviceAccessStatus = await rf.RequestAccessAsync();
                    lines.Add("serviceAccessStatus: " + serviceAccessStatus);

                    lines.Add("----------------------------");

                }
                
                await FileIO.WriteLinesAsync(logFile, lines);
                lines.Clear();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }

        }
    }
}
