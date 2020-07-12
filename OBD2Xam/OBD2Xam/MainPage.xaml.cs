using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Windows.Devices.Enumeration;
using Xamarin.Forms;

namespace OBD2Xam
{
    public partial class MainPage : ContentPage
    {
        private BtSerialCommMgr driver = new BtSerialCommMgr();
        public static MainPage instance = null;
        private bool started = false;
        private Dictionary<string, BtDeviceNameID> deviceMap = new Dictionary<string, BtDeviceNameID>();  // to map a device name back to an ID so we can connect to it

        public static MainPage Instance
        {
            get { return instance; }
        }

        public  MainPage()
        {
            instance = this;

            InitializeComponent();

            driver.LineRecievedEvent += Driver_LineRecievedEvent;
            driver.OnConnect += Driver_OnConnect;

            driver.BtDeviceAdded += Driver_BtDeviceAdded;
            driver.BtDeviceEnumerationComplete += Driver_BtDeviceEnumerationComplete;
            driver.BtDeviceEnumerationStarted += Driver_BtDeviceEnumerationStarted;
            driver.BtDeviceRemoved += Driver_BtDeviceRemoved;
            driver.BtDeviceUpdated += Driver_BtDeviceUpdated;
            deviceList.ItemsSource = deviceMap.Values;
            driver.StartBtDeviceEnumeration();
        }

        private void Driver_BtDeviceEnumerationStarted(object sender, EventArgs e)
        {
            deviceMap.Clear();
        }

        private void Driver_BtDeviceUpdated(object sender, BtDeviceUpdatedParams e)
        {
            //throw new NotImplementedException();
        }

        private void Driver_BtDeviceAdded(object sender, BtDeviceAddedParams e)
        {
            deviceMap.Add(e.name, new BtDeviceNameID() { ID = e.id, Name = e.name });
        }

        private void Driver_BtDeviceRemoved(object sender, BtDeviceRemovedParams e)
        {
            deviceMap.Remove(e.id);
        }

        private void Driver_BtDeviceEnumerationComplete(object sender, EventArgs e)
        {
        }



        private async void StartButton_Clicked(object sender, EventArgs e)
        {

            try
            {
                if (deviceList.SelectedItem!=null)
                {
                    string name = deviceList.SelectedItem.ToString();

                    if (!started)
                    {
                        Instance.showText(string.Format("Connecting to \"{0}\"...", name));
                    }

                    started = await driver.BtConnect(deviceMap[name].ID).ConfigureAwait(false);
                }

                if (started)
                {
                    driver.Start();
                    Instance.showText("done");
                }
            }
            catch(Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debugger.Break();
            }
        }

        private void Device_Selected(object sender, EventArgs e)
        {
            // TODO: connect to the selected device
            System.Diagnostics.Debugger.Break();
        }

        private void Driver_OnConnect(BtSerialCommMgr drv)
        {
            //System.Diagnostics.Debugger.Break();
             Instance.showText("Connected.");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            //driver.Stop();
        }

        public static void ShowText(string text)
        {
            Device.BeginInvokeOnMainThread((Action)(() => {
                MainPage.Instance.showText(text);
            }));
            System.Threading.Thread.Yield();
        }

        private void showText(string text)
        {
            //editor.Text += (text + System.Environment.NewLine);
        }

        private void Driver_LineRecievedEvent(BtSerialCommMgr drv, string line)
        {
            MainPage.ShowText(line);
        }

        public void StartBtDeviceEnumeration()
        {
            throw new NotImplementedException();
        }
    }
}
