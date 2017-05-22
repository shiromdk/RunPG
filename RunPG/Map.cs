using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps;
using Android.Locations;
using Android.Gms.Maps.Model;
using System.Json;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Android.Util;
using Plugin.Settings;

namespace RunPG
{
    [Activity(Label = "Map")]
    public class Map : Activity, IOnMapReadyCallback, ILocationListener
    {
        int score;
        Location location;
        LocationManager locationManager;
        string locationProvider;
        private GoogleMap _map;
        private MapFragment _mapFragment;
        double currentLat;
        double currentLong;
        TextView status, scoreText;
        JsonValue spawnList;
        

        private async void getSpawns(double lat, double lon)
        {
            string url = "http://inft2050assign.herokuapp.com/api/spawnlocations?lat="+
                lat.ToString()+
                "&lon="+
                lon.ToString();
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/json";
            request.Method = "GET";
            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    spawnList = await Task.Run(()=>JsonObject.Load(stream));
                }
            }
        }

        private async void interactSpawn(double spawnLat, double spawnLon)
        {
            JsonValue res;
            string url = "http://inft2050assign.herokuapp.com/api/interactspawn?currentlat=" +
                currentLat.ToString() +
                "&currentlon=" +
                currentLong.ToString() +
                "&spawnlat=" +
                spawnLat.ToString() +
                "&spawnlon=" +
                spawnLon.ToString();
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/json";
            request.Method = "GET";
            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    status.Text = "Starting Request";
                    res = await Task.Run(() => JsonObject.Load(stream));
                }
            }
            string webResponse = res["response"];
            status.Text = res.ToString();
            if (webResponse == "1")
            {
                status.Text = "GOT IT";
                score++;
                var settings = CrossSettings.Current;
                settings.AddOrUpdateValue<int>("score", score);
                scoreText.Text = "Score: " + score.ToString();
            }
            if (webResponse == "2")
            {
                status.Text = "Spawn not close enough";
            }
            if (webResponse == "3")
            {
                status.Text = "Too Slow. The Spawn got Away";
            }
        }

        public void setMarkers() {
            _map.Clear();
            if (spawnList!=null)
            {
                for(int i = 0; i < spawnList.Count; i++)
                {
                    LatLng tempPosition = new LatLng(spawnList[i]["spawnLatitude"], spawnList[i]["spawnLongitude"]);
                    MarkerOptions options = new MarkerOptions().
                        SetPosition(tempPosition);
                    _map.AddMarker(options);
                }
            }
            BitmapDescriptor icon = BitmapDescriptorFactory.FromAsset("15minuteman.bmp");
         
            LatLng currentLatLng = new LatLng(currentLat, currentLong);
            MarkerOptions playerOption = new MarkerOptions()
                .SetPosition(currentLatLng)
                .SetIcon(icon)
                .SetTitle("Player");
            _map.AddMarker(playerOption);

        }

        public void _map_MarkerClick(object sender, GoogleMap.MarkerClickEventArgs e)
        {
            TextView coordinatesTextView = FindViewById<TextView>(Resource.Id.locationText);
            LatLng pos = e.Marker.Position;
            if (e.Marker.Title == "Player")
            {
                status.Text = "Status: Checking yourself out";
            }
            else {
                status = FindViewById<TextView>(Resource.Id.status);
                status.Text = "Status: Checking Clicked Spawn";
                interactSpawn(pos.Latitude, pos.Longitude);
            }
        }
        public void OnMapReady(GoogleMap googleMap)
        {
            _map = googleMap;
            setMarkers();
            _map.MarkerClick += _map_MarkerClick;


        }
        private void InitMapFragment()
        {
            _mapFragment = FragmentManager.FindFragmentByTag("map") as MapFragment;
            if (_mapFragment == null)
            {
                GoogleMapOptions mapOptions = new GoogleMapOptions()
                    .InvokeMapType(GoogleMap.MapTypeSatellite)
                    .InvokeZoomControlsEnabled(false)
                    .InvokeCompassEnabled(true);

                FragmentTransaction fragTx = FragmentManager.BeginTransaction();
                _mapFragment = MapFragment.NewInstance(mapOptions);
                fragTx.Add(Resource.Id.map, _mapFragment, "map");
                fragTx.Commit();
            }
            _mapFragment.GetMapAsync(this);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            currentLat = 0;
            currentLong = 0;
            RequestWindowFeature(Android.Views.WindowFeatures.NoTitle);
            var settings = CrossSettings.Current;
            score = settings.GetValueOrDefault<int>("score", 0);
            settings.AddOrUpdateValue<int>("score", score);






            spawnList = null;
            base.OnCreate(savedInstanceState);
            InitializeLocationManager();
            SetContentView(Resource.Layout.Map);
            status = FindViewById<TextView>(Resource.Id.status);
            scoreText = FindViewById<TextView>(Resource.Id.score);
            scoreText.Text = "Score: "+score.ToString();
            // Create your application here
            InitMapFragment();
            Button locationbtn = FindViewById<Button>(Resource.Id.MyLocationBtn);


            locationbtn.Click += (sender, e) =>
            {
                LatLng mapLocation = new LatLng(currentLat, currentLong);
                CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
                builder.Target(mapLocation);
                builder.Zoom(18);
                builder.Bearing(155);
                builder.Tilt(65);
                CameraPosition cameraPosition = builder.Build();
                CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

                if (_map != null)
                {
                    _map.AnimateCamera(cameraUpdate);
                }

            };


        }




        protected override void OnResume()
        {
            base.OnResume();
            locationManager.RequestLocationUpdates(locationProvider, 0, 0, this);

        }

        protected override void OnPause()
        {
            base.OnPause();

        }


        public void OnLocationChanged(Location location)
        {
            this.location = location;
            TextView coordinatesTextView = FindViewById<TextView>(Resource.Id.locationText);
            coordinatesTextView.Text = $"{location.Latitude},{location.Longitude}";
            getSpawns(location.Latitude,location.Longitude);
            setMarkers();
            currentLat = location.Latitude;
            currentLong = location.Longitude;
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
   
        private void InitializeLocationManager()
        {
            //Getting Location Service into the Location Manager
            locationManager = (LocationManager)GetSystemService(LocationService);
            //Setting how accurate the location manager will be
            Criteria criteria = new Criteria()
            {
                Accuracy = Accuracy.Medium
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