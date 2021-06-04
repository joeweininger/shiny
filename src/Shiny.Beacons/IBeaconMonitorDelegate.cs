using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Shiny.Beacons
{
    public interface IBeaconMonitorDelegate
    {
        Task OnStatusChanged(BeaconRegionState newStatus, BeaconRegion region);

        List<object> GetNearbyDevices();
    }
}
