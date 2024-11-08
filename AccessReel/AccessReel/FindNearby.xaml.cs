using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AccessReel;

//TODO:
//  Colletion View
//  Include Images for each theater?
//  Opening Times ?
public partial class FindNearby : ContentPage
{
    HttpClient client = new HttpClient();
    CancellationTokenSource cts;
    bool isCheckingLocation;
    string url = "https://maps.googleapis.com/maps/api/place/nearbysearch/json";
    string apiKey = "AIzaSyDPn64T6-3v7u1z9HUcjti-urRewiU5LcM";
    double latitude = -31.743185;
    double longitude = 115.782235;
    int radius = 10000;

    public FindNearby()
	{
		InitializeComponent();
	}

    private void BtnFindTheaters_Clicked(object sender, EventArgs e)
    {
        GetCurrentLocation();
    }

    public async Task GetCurrentLocation()
    {
        try
        {
            isCheckingLocation = true;
            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            cts = new CancellationTokenSource();
            Location location = await Geolocation.Default.GetLocationAsync(request, cts.Token);
            if (location != null)
            {
                Debug.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                SearchAPI(location.Latitude, location.Longitude);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        finally
        {
            isCheckingLocation = false;
        }
    }

    public async void SearchAPI(double latitude, double longitude)
    {
        var completeUrl = $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={latitude},{longitude}&radius={radius}&keyword=theater&type=movie_theater&key={apiKey}";
        Debug.WriteLine(completeUrl);
        //var completeUrl = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=-31.743185,115.782235&radius=5000&keyword=theater&type=movie_theater&key=AIzaSyDPn64T6-3v7u1z9HUcjti-urRewiU5LcM";

        if (!(Connectivity.NetworkAccess == NetworkAccess.Internet))
        {
            Debug.WriteLine("No Internet Connection!");
            return;
        }

        try
        {
            HttpResponseMessage response = await client.GetAsync(completeUrl);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(response.Content);
            JObject json = JObject.Parse(responseBody);
            Debug.WriteLine(responseBody);
            if (json["results"] != null)
            {
                foreach(var theater in json["results"])
                {
                    string name = theater["name"]?.ToString();
                    string address = theater["vicinity"]?.ToString();
                    LblTheaters.Text += $"\n {name} - {address}";
                }
            }
            else
            {
                LblTheaters.Text = "No Theaters Found";
            }

        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    private void BtnFindWithPreset_Clicked(object sender, EventArgs e)
    {
        SearchAPI(latitude, longitude);
    }
}