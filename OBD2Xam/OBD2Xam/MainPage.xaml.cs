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
        private static MainPage instance = null;
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
           // editor.IsEnabled = false;

            driver.LineRecievedEvent += Driver_LineRecievedEvent;
            driver.OnConnect += Driver_OnConnect;


            populateDevices();
        }


        // break this out of the constructor so we can decorate with async
        private async void populateDevices()
        {
            // get a list of devices to throw into a list box
            List<BtDeviceNameID> devices = await driver.EnumerateDevices();

            // extract the names and add them to the listbox
            // use a map with the name as the key to lookup the device ID in response to the connect buttons
            List<string> names = new List<string>();
            foreach(BtDeviceNameID d in devices)
            {
                names.Add(d.Name);

                // build the device map while we're at it
                deviceMap.Add(d.Name, d);
            }
            deviceList.ItemsSource = names;
        }

        private async void StartButton_Clicked(object sender, EventArgs e)
        {

            if (deviceList.SelectedItem!=null)
            {
                string name = deviceList.SelectedItem.ToString();

                if (!started)
                {
                    instance.showText(string.Format("Connecting to \"{0}\"...", name));
                }

                started = await driver.BtConnect(deviceMap[name].ID).ConfigureAwait(false);
            }

            if (started)
            {
                driver.Start();
                instance.showText("done");
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
             instance.showText("Connected.");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            //driver.Stop();
        }

        public static void ShowText(string text)
        {
            Device.BeginInvokeOnMainThread(() => {
                instance.showText(text);
            });
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

    }
}
