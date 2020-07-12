using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using OBD2Xam;
using Xamarin.Forms;

namespace OBD2Xam
{
    class BtSerialCommMgr
    {
        private Thread commThread;
        private static bool shutdownRequested = false;

        public delegate void LineRecievedHandler(BtSerialCommMgr drv, string line);
        public event LineRecievedHandler LineRecievedEvent = DefautLineRecievedHandler;

        private string deviceId = "";
        public delegate void ConnectdHandler(BtSerialCommMgr drv);
        public event ConnectdHandler OnConnect = DefautConnectedHandler;

        private const int retrySeconds = 60;

        private ISerialComm deviceCommDriver;
        private bool connected = false;

        private const string lineSeperator = "\r";

        private AutoResetEvent areLineRecieved = new AutoResetEvent(false);



        public BtSerialCommMgr()
        {
            deviceCommDriver = DependencyService.Get<ISerialComm>();
        }

        public async Task<List<BtDeviceNameID>> EnumerateDevices()
        {
           List<BtDeviceNameID> devices =  await deviceCommDriver.GetBTDevices();
           return devices;
        }

        public async Task<bool> BtConnect(string deviceId)
        {
            this.deviceId = deviceId;
            connected = await deviceCommDriver.BtConnect(deviceId);
            if(connected)
            {
                OnConnect(this);
            }
            return connected;
        }

        private void timeoutHandler()
        {
            MainPage.ShowText("Connection timeout.");


            System.Diagnostics.Debugger.Break();
            bool connected = false;
            // if the connection fails, log a message to the console and keep retrying for 1 minute
            if (!connected)
            {
                MainPage.ShowText("Connection failed.  Retrying...");
                deviceCommDriver.Close();

                DateTime dtStart = DateTime.Now;
                DateTime dtStopAt = dtStart.AddSeconds(retrySeconds);
                while (DateTime.Now < dtStopAt && !connected)
                {
                    System.Diagnostics.Debugger.Break();
                    Thread.Sleep(500);
                    deviceCommDriver.BtConnect(deviceId).ConfigureAwait(false);
                }
            }

            if (connected)
            {
                OnConnect.Invoke(this);
            }
            else
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        public void Start()
        {
            try
            {
                startRecieveThread(this);
                Thread.Sleep(100);

                // send a few commands just to be sure everything is working
                SendLine("ATZ");  // reset
                SendLine("AT SP 0"); // auto detect which protocol to use
                SendLine("AT DP"); // return the protocol being used
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
        }

        public void Stop()
        {
            try
            {
                shutdownRequested = true;
                Thread.Sleep(100);
                commThread.Join();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
                commThread.Abort();
            }
        }

        public void SendLine(string text)
        {
            try
            {
                deviceCommDriver.WriteLine(text + lineSeperator);
                System.Diagnostics.Debug.WriteLine("Line sent: " + text);

                // wait for a response
                if(!areLineRecieved.WaitOne(1000 * 30))
                {
                    System.Diagnostics.Debugger.Break();
                    timeoutHandler();
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
        }

        protected static void DefautLineRecievedHandler(BtSerialCommMgr mgr, string line)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Line Recieved: " + line);
                // signal that we recieved a response and it's ok to move on to the next command
                mgr.areLineRecieved.Set();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
        }

        static void DefautConnectedHandler(BtSerialCommMgr mgr)
        {
            if (mgr.connected)
            {
                MainPage.ShowText("Connected.");
            }
            else
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        private static void startRecieveThread(BtSerialCommMgr mgr)
        {

            // start the comm thread
            if (mgr.commThread == null)
            {
                ParameterizedThreadStart pts = new ParameterizedThreadStart(commThreadFunc);
                mgr.commThread = new Thread(pts);
                mgr.commThread.Start(mgr);
                Thread.Sleep(100);
            }
        }

        protected static void commThreadFunc(object obj)
        {
            BtSerialCommMgr mgr = (BtSerialCommMgr)obj;

            try
            {
                while (!shutdownRequested)
                {
                    string line = mgr.deviceCommDriver.ReadLine();
                    if (line.Length > 0)
                    {
                        mgr.LineRecievedEvent.Invoke(mgr, line);
                    }
                    else
                    {
                        continue;
                    }
                }
                mgr.deviceCommDriver.Close();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
        }


    }
}
