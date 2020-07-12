using System;
using System.Collections.Generic;
using System.Text;

namespace OBD2Xam
{
    public class BtDeviceAddedParams
    {
        public string name;
        public string id;
    }

    public class BtDeviceRemovedParams
    {
        public string id;
    }
    public class BtDeviceUpdatedParams
    {
        public string id;
    }

    public interface IBtEnum
    {
        event EventHandler<BtDeviceAddedParams> BtDeviceAdded;
        event EventHandler<BtDeviceRemovedParams> BtDeviceRemoved;
        event EventHandler<BtDeviceUpdatedParams> BtDeviceUpdated;
        event EventHandler BtDeviceEnumerationStarted;
        event EventHandler BtDeviceEnumerationComplete;
        void StartBtDeviceEnumeration();
    }
}
