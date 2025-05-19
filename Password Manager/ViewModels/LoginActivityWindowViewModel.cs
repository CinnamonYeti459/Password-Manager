using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Password_Manager.Models;

namespace Password_Manager.ViewModels
{
    public class LoginActivityViewModel : ViewModelBase
    {
        public ObservableCollection<LoginActivityEntry> LoginActivityEntries { get; } = new(); // Used for storing login activity

        public LoginActivityViewModel()
        {
            LoadLoginActivity();
        }

        private void LoadLoginActivity()
        {
            try
            {
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PasswordManager");
                string currentUser = App.CurrentUser;
                if (string.IsNullOrEmpty(currentUser))
                    return;

                string loginActivityFile = Path.Combine(folderPath, $"{currentUser} Login Activity.json");

                if (!File.Exists(loginActivityFile))
                    return;

                string encryptedData = File.ReadAllText(loginActivityFile);
                string decryptedData = CryptoHelper.Decrypt(encryptedData);

                var entries = JsonSerializer.Deserialize<LoginActivityEntry[]>(decryptedData);

                if (entries != null)
                {
                    LoginActivityEntries.Clear();
                    foreach (var entry in entries)
                    {
                        LoginActivityEntries.Add(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading login activity: {ex.Message}");
            }
        }
    }
}
