using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Locations;
using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RunPG
{
    [Activity(Label = "RunPG", MainLauncher =false, Icon = "@drawable/icon")]
    public class MainActivity : Activity, ILocationListener
    {
        Location location;
        LocationManager locationManager;
        string locationProvider;
  

        protected override void OnResume()
        {
            base.OnResume();
            locationManager.RequestLocationUpdates(locationProvider, 0, 0, this);


        }
        public void OnLocationChanged(Location location)
        {
            this.location = location;
    

        }

        public void OnProviderDisabled(string provider)
        {

        }

        public void OnProviderEnabled(string provider)
        {

        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {

        }

        protected override void OnCreate(Bundle bundle)
        {


            base.OnCreate(bundle);
            RequestWindowFeature(Android.Views.WindowFeatures.NoTitle);
           



            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            ImageView header;
            InitializeLocationManager();
            Button playButton = FindViewById<Button>(Resource.Id.playButton);
            Button quitButton = FindViewById<Button>(Resource.Id.quitButton);
            playButton.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(Map));
                StartActivity(intent);
            };
            quitButton.Click += (sender, e) =>
            {
                Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
            };
        }
        private void InitializeLocationManager()
        {
            //Getting Location Service into the Location Manager
            locationManager = (LocationManager)GetSystemService(LocationService);
            //Setting how accurate the location manager will be
            Criteria criteria = new Criteria()
            {
                Accuracy = Accuracy.Coarse,
                PowerRequirement = Power.High
            };
            //Creating a list of all providers that can provide the above accuracy
            IList<string> acceptableLocationProviders = locationManager.GetProviders(criteria, true);

            //Checks to see if there are any acceptable location providers
            if (acceptableLocationProviders.Count > 0)
            {
                //sets to first in the list
                locationProvider = acceptableLocationProviders.First();
            }
            else
            {
                locationProvider = string.Empty;
            }
        }

    }
}

