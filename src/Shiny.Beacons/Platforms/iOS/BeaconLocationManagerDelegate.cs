using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using CoreLocation;
using Foundation;

namespace Shiny.Beacons
{
    public class BeaconLocationManagerDelegate : ShinyLocationDelegate
    {
        readonly IServiceProvider services;
        readonly Subject<Beacon> rangeSubject;


        public BeaconLocationManagerDelegate(IServiceProvider services)
        {
            this.services = services;
            this.rangeSubject = new Subject<Beacon>();
        }


        public IObservable<Beacon> WhenBeaconRanged() => this.rangeSubject;
        public override void DidRangeBeacons(CLLocationManager manager, CLBeacon[] beacons, CLBeaconRegion region)
        {
            foreach (var native in beacons)
            {
                this.rangeSubject.OnNext(new Beacon
                (
                    native.ProximityUuid.ToGuid(),
                    native.Major.UInt16Value,
                    native.Minor.UInt16Value,
                    native.Proximity.FromNative(),
                    (int)native.Rssi,
                    native.Accuracy
                ));
            }
        }

        public override void DidDetermineState(CLLocationManager manager, CLRegionState state, CLRegion region)
        {
            if (state == CLRegionState.Inside)
            {
                Debug.WriteLine(">>>> LocationManager DidDetermineState: Inside");
                this.Invoke(region, BeaconRegionState.Entered);
            }
            else if (state == CLRegionState.Outside)
            {
                Debug.WriteLine(">>>> LocationManager DidDetermineState: Outside");
                this.Invoke(region, BeaconRegionState.Exited);
            }
            else
            {
                Debug.WriteLine(">>>> LocationManager DidDetermineState: Unknown");
                this.Invoke(region, BeaconRegionState.Exited);
            }
        }


        public override void DidStartMonitoringForRegion(CLLocationManager manager, CLRegion region)
        {
            Debug.WriteLine($">>>> Started monitoring region - {region.Identifier}");
        }

        public override void Failed(CLLocationManager manager, NSError error)
        {
            Debug.WriteLine($">>>> Failed monitoring region - {error.LocalizedDescription}");
        }

        public override void MonitoringFailed(CLLocationManager manager, CLRegion? region, NSError error)
        {
            Debug.WriteLine($">>>> LocationManager MonitoringFailed - {error.LocalizedDescription}");
        }

        async void Invoke(CLRegion region, BeaconRegionState status)
        {
            if (region is CLBeaconRegion native)
            {
                var beaconRegion = new BeaconRegion(
                    native.Identifier,
                    native.ProximityUuid.ToGuid(),
                    native.Major?.UInt16Value,
                    native.Minor?.UInt16Value
                );
                await this.services.RunDelegates<IBeaconMonitorDelegate>(
                    x => x.OnStatusChanged(status, beaconRegion)
                );
            }
        }
    }
}
