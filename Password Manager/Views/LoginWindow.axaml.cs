using Avalonia.Controls;
using Avalonia.Interactivity;
using Password_Manager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Password_Manager.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void OnLoginClicked(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text;
            var password = PasswordBox.Text;

            // Check for empty inputs
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorText.Text = "Username and password cannot be empty.";
                ErrorText.IsVisible = true;
                return;
            }

            // Get the user's file path
            var userFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PasswordManager", $"{username}.json");

            // Check if user file doesn't
            if (!File.Exists(userFile))
            {
                // Log failed login attempt (user not found)
                LogLoginActivityAsync(username, false);

                ErrorText.Text = "User does not exist.";
                ErrorText.IsVisible = true;
                return;
            }

            try
            {
                // Read and decrypt file
                string encryptedData = File.ReadAllText(userFile);
                string decryptedData = CryptoHelper.Decrypt(encryptedData);

                // Deserialize to UserCredentials
                var credentials = JsonSerializer.Deserialize<UserData>(decryptedData);

                
                if (credentials != null && credentials.Password == password) // Credientals are valid
                {
                    // Log successful login
                    LogLoginActivityAsync(username, true);

                    App.CurrentUser = username; // Set the user to the username

                    var mainWindow = new MainWindow();
                    mainWindow.Show(); // Show the main window

                    this.Close(); // Close the current login window
                }
                else // Log failed login attempt (wrong password)
                {
                    LogLoginActivityAsync(username, false);

                    ErrorText.Text = "Invalid password.";
                    ErrorText.IsVisible = true;
                }
            }
            catch (Exception ex) // Error when trying to login
            {
                ErrorText.Text = "Error reading user data.";
                ErrorText.IsVisible = true;

                // Define the error log path
                var logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PasswordManager");
                var errorLogPath = Path.Combine(logDirectory, "Error Log.txt");

                try
                {
                    // Ensure directory exists
                    Directory.CreateDirectory(logDirectory);

                    // Write error details to the log file
                    File.AppendAllText(errorLogPath, $"[{DateTime.Now}] Error for user '{username}': {ex}\n\n");
                }
                catch
                {
                    // Fallback to console logging
                    Console.WriteLine($"Failed to write to error log.\n{ex}");
                }
            }
        }

        private void OnCreateAccountClicked(object sender, RoutedEventArgs e)
        {
            var newUsername = NewUsernameBox.Text.Trim(); // Removes any type of whitespace
            var newPassword = NewPasswordBox.Text.Trim(); // Removes any type of whitespace

            if (string.IsNullOrWhiteSpace(newUsername) || string.IsNullOrWhiteSpace(newPassword)) // Checks if the username/password is empty
            {
                CreateAccountStatusText.Text = "Username and password cannot be empty.";
                CreateAccountStatusText.Foreground = Avalonia.Media.Brushes.Red;
                CreateAccountStatusText.IsVisible = true;
                return; // Stop the function
            }

            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string folderPath = Path.Combine(appDataPath, "PasswordManager");
                Directory.CreateDirectory(folderPath); // Creates the filepath unless it exists

                string userFilePath = Path.Combine(folderPath, $"{newUsername}.json");

                if (File.Exists(userFilePath)) // Checks if the user already exists
                {
                    CreateAccountStatusText.Text = "Username already exists. Please choose another.";
                    CreateAccountStatusText.Foreground = Avalonia.Media.Brushes.Red;
                    CreateAccountStatusText.IsVisible = true;
                    return;
                }

                // Create the user data object
                var initialUserData = new
                {
                    Username = newUsername,
                    Password = newPassword,
                    Entries = new PasswordEntry[0]
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(initialUserData, options); // Turns the data to JSON with indents

                string encryptedJson = CryptoHelper.Encrypt(json); // Encrypts JSON

                File.WriteAllText(userFilePath, encryptedJson); // Write the encrypted file

                CreateAccountStatusText.Text = $"Account '{newUsername}' created successfully!";
                CreateAccountStatusText.Foreground = Avalonia.Media.Brushes.LightGreen;
                CreateAccountStatusText.IsVisible = true;

                // Clear inputs
                NewUsernameBox.Text = "";
                NewPasswordBox.Text = "";
            }
            catch (Exception ex) // Display account creation error
            {
                CreateAccountStatusText.Text = $"Error creating account: {ex.Message}";
                CreateAccountStatusText.Foreground = Avalonia.Media.Brushes.Red;
                CreateAccountStatusText.IsVisible = true;
            }
        }

        private async Task LogLoginActivityAsync(string username, bool isSuccess)
        {
            try
            {
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PasswordManager");
                Directory.CreateDirectory(folderPath); // Create the file path if it doesn't exist

                string loginActivityFile = Path.Combine(folderPath, $"{username} Login Activity.json");

                var ip = GetLocalLocation.GetIP(); // Get IP
                var (location, provider) = await GetLocalLocation.GetLocationAndProviderAsync(); // Get location information

                var entry = new LoginActivityEntry // Add an entry into the login activity
                {
                    Location = location,
                    WiFiProvider = provider,
                    IPAddress = ip,
                    Date = DateTime.UtcNow,
                    IsSuccessful = isSuccess
                };

                // Read existing entries
                LoginActivityEntry[] existingEntries = Array.Empty<LoginActivityEntry>();
                if (File.Exists(loginActivityFile))
                {
                    string encryptedData = File.ReadAllText(loginActivityFile);
                    string decryptedData = CryptoHelper.Decrypt(encryptedData);

                    // Converts the decrypted JSOn to an array for login entries
                    existingEntries = JsonSerializer.Deserialize<LoginActivityEntry[]>(decryptedData) ?? Array.Empty<LoginActivityEntry>();
                }

                // Add new entry
                var updatedEntries = new List<LoginActivityEntry>(existingEntries) { entry };

                // Serialize and encrypt
                string json = JsonSerializer.Serialize(updatedEntries, new JsonSerializerOptions { WriteIndented = true });
                string encryptedJson = CryptoHelper.Encrypt(json);

                // Save file
                File.WriteAllText(loginActivityFile, encryptedJson);
            }
            catch (Exception ex)
            {
                // Define the error log path
                var logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PasswordManager");
                var errorLogPath = Path.Combine(logDirectory, "Error Log.txt");

                try
                {
                    // Ensure directory exists
                    Directory.CreateDirectory(logDirectory);

                    // Write error details to the log file
                    File.AppendAllText(errorLogPath, $"Failed to log login activity for {username}: {ex.Message}");
                }
                catch
                {
                    // Fallback to console logging
                    Console.WriteLine($"Failed to write to error log for {username}.\n{ex}");
                }
            }
        }


    }
}
