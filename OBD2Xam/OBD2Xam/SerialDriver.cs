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
    class SerialDriver
    {
        protected Thread commThread;
        protected static bool shutdownRequested = false;

        public delegate void LineRecievedHandler(SerialDriver drv, string line);
        public event LineRecievedHandler LineRecievedEvent = DefautLineRecievedHandler;


        public delegate void ConnectdHandler(SerialDriver drv);
        public event ConnectdHandler OnConnect = DefautConnectedHandler;

        private const int retrySeconds = 60;

        private ISerialComm commPort;
        private bool connected = false;

        private const string lineSeperator = "\r";

        public SerialDriver()
        {
            commPort = DependencyService.Get<ISerialComm>();
        }

        public async Task<List<BtDeviceNameID>> EnumerateDevices()
        {
           List<BtDeviceNameID> devices =  await commPort.GetBTDevices();
           return devices;
        }

        public async Task<bool> BtConnect(string deviceId)
        {
            connected = await commPort.BtConnect(deviceId);
            if(connected)
            {
                OnConnect(this);
            }
            return connected;
        }

        private void timeoutHandler()
        {
            MainPage.ShowText("Connection timeout.");


            //System.Threading.Thread.Sleep(1000);
            System.Diagnostics.Debugger.Break();
            bool connected = false;
            //bool connected = commPort.IsOpen();
            // if the connection fails, log a message to the console and keep retrying for 1 minute
            if (!connected)
            {
                MainPage.ShowText("Connection failed.  Retrying...");

                DateTime dtStart = DateTime.Now;
                DateTime dtStopAt = dtStart.AddSeconds(retrySeconds);
                while (DateTime.Now < dtStopAt && !connected)
                {
                    Thread.Sleep(500);
                    //commPort.Open();
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
            shutdownRequested = true;
            Thread.Sleep(100);
            commThread.Join();
        }

        public void SendLine(string text)
        {
            try
            {
                commPort.WriteLine(text + lineSeperator);
                System.Diagnostics.Debug.WriteLine("Line sent: " + text);
                Thread.Sleep(100);
            }
            catch(Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
        }
        static void DefautConnectedHandler(SerialDriver drv)
        {
            MainPage.ShowText("Connected.");

            if (drv.connected)
            {
                
            }
            else
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        private static void startRecieveThread(SerialDriver drv)
        {

            // start the comm thread
            if (drv.commThread == null)
            {
                ParameterizedThreadStart pts = new ParameterizedThreadStart(commThreadFunc);
                drv.commThread = new Thread(pts);
                drv.commThread.Start(drv);
                Thread.Sleep(100);
            }
        }

        protected static void DefautLineRecievedHandler(SerialDriver drv, string line)
        {
            // do nothing
            System.Diagnostics.Debug.WriteLine("Line Recieved: " + line);
        }

        private static List<string> chunks = null;
        private static int ichunk = -1;
        private static void buildChunks()
        {
            chunks = new List<string>();
            chunks.Add("OK1\r");
            chunks.Add("OK2\n");
            chunks.Add("OK3\r\n");
            chunks.Add("OK4");
            chunks.Add("\r");
            chunks.Add("OK5");
            chunks.Add("\n");
            chunks.Add("\n");
            chunks.Add("\r");
            chunks.Add("TEST\rDUMMY\n");
            chunks.Add("this is ");
            chunks.Add("a test\rof really");
            chunks.Add("dumb stuff\n\rjust for no ");
            chunks.Add("point at all\r");
        }

        private static string getNextChunk()
        {
            if (chunks == null)
                buildChunks();

            try
            {
                ichunk = (ichunk + 1) % chunks.Count;
                return chunks[ichunk];
            }
            catch(Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
            return "";
        }

        protected static void commThreadFunc(object obj)
        {
            SerialDriver driver = (SerialDriver)obj;
            char[] eolChars = "\r\n".ToCharArray();
            string buffer = "";
            int eolPos = -1;

            try
            {
                while (!shutdownRequested)
                {
                    string nextChunk = driver.commPort.ReadAvailableData();
                    if (nextChunk.Length == 0)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    //System.Diagnostics.Debug.WriteLine("nextChunk: " + nextChunk);

                    //string nextChunk = getNextChunk();
                    buffer += nextChunk;  // store chunks in a buffer until one or more complete lines are formed.

                    // pick out complete lines and send them to the line handler

                    // find the first \r or \n, remove the line from the buffer
                    // process the line up to that point
                    eolPos = buffer.IndexOfAny(eolChars);
                    while (eolPos >= 0)
                    {
                        string line = buffer.Substring(0, eolPos);
                        buffer = buffer.Substring(eolPos + 1);

                        if (line.Length > 0)
                        {
                            driver.LineRecievedEvent.Invoke(driver, line);
                        }

                        eolPos = buffer.IndexOfAny(eolChars);
                    }

                    Thread.Yield();
                }
                driver.commPort.Close();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
        }


    }
}
