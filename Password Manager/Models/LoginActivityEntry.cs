using System;

namespace Password_Manager.Models
{
    public class LoginActivityEntry
    {
        public string Location { get; set; } = "";
        public string WiFiProvider { get; set; } = "";
        public string IPAddress { get; set; } = "";
        public DateTime Date { get; set; }
        public bool IsSuccessful { get; set; }
    }
}