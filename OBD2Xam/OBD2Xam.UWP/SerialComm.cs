#define USE_STREAMREADER
using OBD2Xam.UWP;
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

[assembly: Xamarin.Forms.Dependency(typeof(SerialComm))]
#pragma warning disable 1998  // disable 'no await' warnings

namespace OBD2Xam.UWP
{
    class SerialComm : ISerialComm
    {
        //protected string comPort = "";
        //protected SerialPort sp;
        //protected SerialDevice sd = null;
        private bool isOpen = false;

        //public event SerialDefs.ConnectionEventType OnConnect = new SerialDefs.ConnectionEventType(defaultConnectionEventHandler);
        //public event SerialDefs.ConnectionEventType OnTimeout = new SerialDefs.ConnectionEventType(defaultConnectionEventHandler);

        //protected SerialDevice serialDevice = null;

        //private BluetoothAdapter adapter = null;
        //  private BluetoothSocket _socket = null;

        // per https://www.bluetooth.com/specifications/assigned-numbers/service-discovery/,
        // the service type 1101 is the serial port
        private const string SERIAL_PORT_INTERFACE = @"{00001101-0000-1000-8000-00805F9B34FB}";

        StreamSocket streamSocket = null;
        DataWriter dw = null;

#if USE_STREAMREADER
        StreamReader sr = null;
#else
        DataReader dr = null;
#endif

        public void Close()
        {
            try
            {

#if USE_STREAMREADER
                sr.Dispose();
#else
                dr.Dispose();
#endif

                dw.Dispose();
                streamSocket.Dispose();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
            finally
            {
                isOpen = false;
            }
        }

        // https://docs.microsoft.com/en-us/windows/uwp/packaging/app-capability-declarations
        // https://docs.microsoft.com/en-us/uwp/api/windows.devices.serialcommunication
        // https://docs.microsoft.com/en-us/uwp/api/windows.devices.serialcommunication.serialdevice

        private static void defaultConnectionEventHandler()
        {
            // do nothing, just something to call
        }

        public async void Open()
        {
            //bool isOpen = false;
            try
            {
                // https://stackoverflow.com/questions/44308078/getting-the-com-port-name-for-a-known-bluetooth-device-in-uwp


                //string selector = BluetoothAdapter.GetDeviceSelector();
                //adapter = await BluetoothAdapter.FromIdAsync(selector);


                //adapter = await BluetoothAdapter.GetDefaultAsync();
                //if (adapter == null)
                //    throw new Exception("No Bluetooth adapter found.");

                //Windows.Devices.Radios.Radio radio = await adapter.GetRadioAsync();
                //if(radio.State != Windows.Devices.Radios.RadioState.On)
                //    throw new Exception("Bluetooth adapter is not enabled.");

                //string deviceId = adapter.DeviceId;
                //SerialDevice sd = await SerialDevice.FromIdAsync(deviceId);

                //RfcommDeviceService service = RfcommDeviceService.GetDeviceSelectorForBluetoothDevice()

                //string deviceSelector = BluetoothDevice.GetDeviceSelector();

                // System.Devices.DevObjectType:=5 AND System.Devices.Aep.ProtocolId:="{E0CBF06C-CD8B-4647-BB8A-263B43F0F974}" 
                // AND (System.Devices.Aep.IsPaired:=System.StructuredQueryType.Boolean#True 
                // OR System.Devices.Aep.Bluetooth.IssueInquiry:=System.StructuredQueryType.Boolean#False)
                // BluetoothDevice bd = await BluetoothDevice.FromIdAsync(deviceSelector);



                /*
                // Next, get an instance of the BluetoothDevice representing the physical device you’re connecting to. 
                // You can get a list of currently paired devices using the adapter’s BondedDevices collection. 
                // I use some simple LINQ to find the device I’m looking for:
                BluetoothDevice device = (from bd in adapter. .BondedDevices
                                          where bd.Name == "NameOfTheDevice"
                                          select bd).FirstOrDefault();

                if (device == null)
                    throw new Exception("Named device not found.");

                // Finally, use the device’s CreateRfCommSocketToServiceRecord method, which will return a BluetoothSocket 
                // that can be used for connection and communication. Note that the UUID specified below is the standard UUID for SPP:
                _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                //await 
                _socket.ConnectAsync().ConfigureAwait(true);
                */

                //isOpen = true;
            }
            catch (System.IO.IOException ioe)
            {
                string msg = ioe.Message.Trim();
                if (msg == "Element not found.")
                {
                    // COM port or device not available
                    System.Diagnostics.Debugger.Break();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(ioe.ToString());
                    System.Diagnostics.Debugger.Break();
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }

        }

        /*
        public bool InitializeSerialPort(string port, int baud)
        {
            bool initialized = false;
            try
            {
                comPort = port;
                //sp = new SerialPort(comPort, baud);
                //sp.ReadBufferSize = Constants.BUFFER_SIZE;
                initialized = true;
            }
            catch(Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }

            return initialized;
        }
        */

        public string ReadAvailableData()
        {
            string result = "";
            try
            {

#if USE_STREAMREADER
                lock (sr)
                {
                    // the stream only supports blocking reads, so the thread will be blocked while trying to read
                    // the thread will block until the next byte is read, but that's ok as long as that's the sole purpose of the thread
                    List<Byte> valuesRead = new List<Byte>();
                    Byte nextValue = 0;
                    while (nextValue != '\r' && nextValue != '\n')
                    {
                        int nextIntVal = sr.Read();
                        //System.Diagnostics.Debug.WriteLine(string.Format("nextIntVal: 0x{0:X4}", nextIntVal));
                        nextValue = (Byte)nextIntVal;
                        valuesRead.Add((byte)nextValue);
                    }

                    result = Encoding.UTF8.GetString(valuesRead.ToArray());
                    //System.Diagnostics.Debug.WriteLine("result: " + result);
                }

#else
                //long buflen = dr.ReadBuffer(1).Length;

                System.Diagnostics.Debug.WriteLine("UnconsumedBufferLength: " + dr.UnconsumedBufferLength);
                if (dr.UnconsumedBufferLength > 0)
                {
                    result = dr.ReadString(dr.UnconsumedBufferLength);
                    System.Diagnostics.Debug.WriteLine("result: " + result);
                }

                //result = dr.ReadString(1);
                //System.Diagnostics.Debug.WriteLine(result);
#endif


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
            return isOpen;
        }


        private void OnBtDeviceAdded (DeviceWatcher dw, DeviceInformation di)
        {
            if (!btDevices.ContainsKey(di.Id))
                btDevices.Add(di.Id, di);
        }

        private void OnBtDeviceRemoved(DeviceWatcher dw, DeviceInformationUpdate diu)
        {
            if (!btDevices.ContainsKey(diu.Id))
                btDevices.Remove(diu.Id);
        }

        private void OnBtDeviceUpdated(DeviceWatcher dw, DeviceInformationUpdate diu)
        {
            return;

            //System.Diagnostics.Debugger.Break();

            // TODO: update each property listed in diu.Properties
            if (btDevices.ContainsKey(diu.Id))
            {
                //btDevices[diu.Id] = diu.Properties
                foreach(string skey in diu.Properties.Keys)
                {
                    System.Diagnostics.Debug.WriteLine(skey);
                    System.Diagnostics.Debugger.Break();
                    // System.Devices.Aep.IsPaired
                    //btDevices[diu.Id].
                }
            }
        }

        private void OnBtEnumerationComplete(DeviceWatcher dw, object obj)
        {
            btEnumerationFinished = true;
        }

        // https://docs.microsoft.com/en-us/uwp/api/windows.devices.enumeration.devicewatcher?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev15.query%3FappId%3DDev15IDEF1%26l%3DEN-US%26k%3Dk(Windows.Devices.Enumeration.DeviceWatcher)%3Bk(TargetFrameworkMoniker-.NETCore%2CVersion%3Dv5.0)%3Bk(DevLang-csharp)%26rd%3Dtrue
        private DeviceWatcher deviceWatcher = null;
        // TODO:  this needs to be exposed as a bindable property so it can be bound to a control
        private Dictionary<string, DeviceInformation> btDevices = new Dictionary<string, DeviceInformation>();
        private bool btEnumerationFinished = false;

        public async Task<List<BtDeviceNameID>> GetBTDevices()
        {
            string[] requestedProperties = new string[] { 
                "System.Devices.Aep.DeviceAddress", 
                "System.Devices.Aep.IsConnected" 
            };

            btDevices.Clear();
            btEnumerationFinished = false;
            deviceWatcher?.Stop();  // if deviceWatcher already exists, stop it
            deviceWatcher = DeviceInformation.CreateWatcher("(System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\")",
                                                            requestedProperties,
                                                            DeviceInformationKind.AssociationEndpoint);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // TODO:  start the enumeration, but run it in background, let the events manage the list
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            deviceWatcher.Added += new Windows.Foundation.TypedEventHandler<DeviceWatcher, DeviceInformation>(OnBtDeviceAdded);
            deviceWatcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(OnBtDeviceRemoved);
            deviceWatcher.Updated += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(OnBtDeviceUpdated);
            deviceWatcher.EnumerationCompleted += new TypedEventHandler<DeviceWatcher, object>(OnBtEnumerationComplete);
            deviceWatcher.Stopped += new TypedEventHandler<DeviceWatcher, object>(OnBtEnumerationComplete);
            deviceWatcher.Start();

            // wait for the enumeration to finish
            string lastStatus = "";
            string newStatus = "";
            while (!btEnumerationFinished)
            {
                System.Threading.Thread.Sleep(100);
                newStatus = deviceWatcher.Status.ToString();
                if (newStatus != lastStatus)
                {
                    System.Diagnostics.Debug.WriteLine("BT Enumeration: " + newStatus);
                    lastStatus = newStatus;
                }
            }


            // return a list of atrings to display...needs to contain the device id
            //System.Diagnostics.Debugger.Break();
            List<BtDeviceNameID> devices = new List<BtDeviceNameID>();
            foreach(string key in btDevices.Keys)
            {
                //devices.Add(key + "::" + btDevices[key].Name);
                devices.Add(new BtDeviceNameID() {
                    Name = btDevices[key].Name,
                    ID =key
                });
            }
            return devices;  // TODO: do away with this and let OnDeviceAdded/OnDeviceRemoved handlers update the list
        }

        public async Task<bool> BtConnect(string deviceID)
        {

            try
            {
                //System.Diagnostics.Debug.Assert(Thread.CurrentThread.IsBackground, "SerialComm:BtConnect() cannot be called from the UI thread.");

                //System.Diagnostics.Debugger.Break();
                DeviceAccessStatus accessStatus = DeviceAccessInformation.CreateFromId(deviceID).CurrentStatus;
                if (accessStatus == DeviceAccessStatus.DeniedByUser)
                {
                    await OBD2Xam.MainPage.Instance.DisplayAlert("BT Error",
                        "This app does not have access to connect to the remote device.  Please grant access in Settings > Privacy > Other Devices.",
                        "");
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
                        // SERIAL_PORT_INTERFACE
                        foreach (var service in rfResultList.Services)
                        {
                            if (service.ServiceId.AsString() == SERIAL_PORT_INTERFACE)
                            {
                                streamSocket = new StreamSocket(); 
                                await streamSocket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName);
                                dw = new DataWriter(streamSocket.OutputStream);
#if USE_STREAMREADER
                                sr = new StreamReader(streamSocket.InputStream.AsStreamForRead(256));
#else
                                dr = new DataReader(streamSocket.InputStream);
#endif
                                isOpen = true;
                                // read/write still not working...connecting to the right service???
                                // test with a bluetooth serial terminal and see if that will connect
                                break;
                            }
                        }
                        if(!isOpen)
                        {
                            throw new Exception("Service not found)");
                        }
                    }
                }

            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debugger.Break();
            }


            return isOpen;
        }

        private async void logRfTypes(RfcommDeviceServicesResult rfResultList)
        {
            try
            {
                //DateTime dtNow = DateTime.Now;
                //string sTimeStamp = dtNow.ToString("yyyyMMdd_hhmmss");
                //string fileName = sTimeStamp + ".log";
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
                //var sdpRecs = device.SdpRecords;
                lines.Add("");

                foreach (RfcommDeviceService rf in rfResultList.Services)
                {
                    lines.Add("ServiceId: " + rf.ServiceId.AsString());
                    lines.Add("ConnectionHostName: " + rf.ConnectionHostName);
                    lines.Add("ConnectionServiceName: " + rf.ConnectionServiceName);
                    //lines.Add("selector: " + RfcommDeviceService.GetDeviceSelectorForBluetoothDeviceAndServiceId(rf.Device, rf.ServiceId));

                    //RfcommServiceProvider rsp = await RfcommServiceProvider.CreateAsync(rf.ServiceId);

                    DeviceAccessStatus serviceAccessStatus = await rf.RequestAccessAsync();
                    lines.Add("serviceAccessStatus: " + serviceAccessStatus);

                    /*
                    SerialDevice sd = await SerialDevice.FromIdAsync(rf.Device.DeviceId);
                    if(sd!=null)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    */
                    lines.Add("----------------------------");

                }
                
                await FileIO.WriteLinesAsync(logFile, lines);
                lines.Clear();

                //System.Diagnostics.Debugger.Break();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }

        }
    }
}
