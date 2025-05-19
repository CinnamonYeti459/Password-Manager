using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace Password_Manager.Models
{
    public static class GetLocalLocation
    {
        public static string GetIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString(); // This isn't the public IP
                }
            }
            return "127.0.0.1"; // Return localhost if it fails
        }

        private static readonly HttpClient httpClient = new HttpClient(); // Creates a HttpClient for sending web requests

        public static async Task<(string Location, string WiFiProvider)> GetLocationAndProviderAsync()
        {
            try
            {
                var ip = await httpClient.GetStringAsync("https://api.ipify.org"); // Gets public IP

                var response = await httpClient.GetStringAsync($"https://ipinfo.io/{ip}/json"); // Requests IP info in json format
                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                // If null, return blank
                var city = root.GetProperty("city").GetString() ?? "";
                var region = root.GetProperty("region").GetString() ?? "";
                var country = root.GetProperty("country").GetString() ?? "";
                var org = root.GetProperty("org").GetString() ?? "";

                // Combine the city, region and country in one string
                string location = $"{city}, {region}, {country}";
                return (location, org); // return the results
            }
            catch // if an error occurs then return this instead of a location or provider
            {
                return ("Unknown Location", "Unknown Provider");
            }
        }
    }
}
