using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Password_Manager.Models
{
    public static class BreachChecker
    {
        public static async Task<bool> IsPasswordBreached(string password)
        {
            // Hashes the password
            using var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Gets the hash byte array to a hex string
            string fullHash = BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();

            string prefix = fullHash.Substring(0, 5); // First 5 characters
            string suffix = fullHash.Substring(5); // Remainder of the characters

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "PasswordManager/1.0");

            // Queries the API
            var response = await client.GetStringAsync($"https://api.pwnedpasswords.com/range/{prefix}");
            var lines = response.Split('\n'); // Splits every line returned

            return lines.Any(line => line.StartsWith(suffix)); // Searches to see if the hash is the same and returns true or false
        }
    }
}
