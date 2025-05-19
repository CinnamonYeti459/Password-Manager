using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Password_Manager.Models;
using Password_Manager.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Password_Manager.Views;

public partial class MainView : UserControl
{
    public MainViewModel ViewModel => (MainViewModel)DataContext;
    private UserIdleDetector? _idleDetector;

    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();

        #region Idle Detection
        _idleDetector = new UserIdleDetector(this, TimeSpan.FromSeconds(60));
        _idleDetector.UserBecameIdle += (s, e) => // s = event sender | e = extra information about the event
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();

            var window = this.VisualRoot as Window;
            if (window != null)
            {
                window.Close();
            }
        };
        #endregion

        #region Password File Reader For The Data Grid
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string folderPath = Path.Combine(appDataPath, "PasswordManager");
        string currentUser = App.CurrentUser ?? "default";
        string filePath = Path.Combine(folderPath, $"{currentUser}_passwords.json");

        if (File.Exists(filePath))
        {
            string encryptedJson = File.ReadAllText(filePath);
            string decryptedJson = CryptoHelper.Decrypt(encryptedJson);

            var entries = JsonSerializer.Deserialize<ObservableCollection<PasswordEntry>>(decryptedJson);
            if (entries != null)
            {
                ViewModel.PasswordEntries.Clear();
                foreach (var entry in entries)
                    ViewModel.PasswordEntries.Add(entry);
            }
        }
        else
        {
            Directory.CreateDirectory(folderPath);
            File.WriteAllText(filePath, "[]"); // Keep empty JSON array for valid deserialization
        }
        #endregion
    }

    private async void PasswordBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        string password = Password.Text;
        if (password == "")
        {
            PasswordBreachWarning.Text = string.Empty;
        }
        else
        {
            bool breached = await BreachChecker.IsPasswordBreached(password); // Checks if the password's been breached

            PasswordBreachWarning.Text = breached
                ? "Warning! Password detected in a breach." // Is breached
                : "Password not found in any known breaches."; // Not breached

            PasswordBreachWarning.Foreground = breached
                ? Brushes.Red        // colour for breached warning
                : Brushes.Green;     // colour for no breach
        }
    }

    public void Button_Click(object sender, RoutedEventArgs args)
    {
        ViewModel.PasswordEntries.Add(new PasswordEntry // Adds an entry to the password data grid
        {
            Service = Service.Text,
            Username = Username.Text,
            Password = Password.Text,
            IsPasswordVisible = false
        });

        SaveEntriesToFile();
    }

    public void Generate_Password_Button_Click(object sender, RoutedEventArgs args)
    {
        PasswordGenerated.Text = PasswordGenerator.Generate();
    }

    public void SaveEntriesToFile()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string folderPath = Path.Combine(appDataPath, "PasswordManager");
        Directory.CreateDirectory(folderPath);

        string currentUser = App.CurrentUser ?? "default";
        string filePath = Path.Combine(folderPath, $"{currentUser}_passwords.json");

        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(ViewModel.PasswordEntries, options);

        string encryptedJson = CryptoHelper.Encrypt(json); // Encrypts the JSON

        File.SetAttributes(filePath, FileAttributes.Normal); // Converts to a non-hidden file, so it can be wrote to
        File.WriteAllText(filePath, encryptedJson);
        File.SetAttributes(filePath, FileAttributes.Hidden);// Converts to a hidden file, so it's hidden
    }

    private PasswordEntry selectedEntry;
    public void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Cast the selected entry as a PasswordEntry
        selectedEntry = PasswordsDataGrid.SelectedItem as PasswordEntry;

        // Enable the button unless the entry's null
        RemoveEntryButton.IsEnabled = selectedEntry != null;
    }

    public void RemoveEntryButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedEntry != null) // Check if an entry's selected
        {
            ViewModel.PasswordEntries.Remove(selectedEntry); // Remove the entry
            SaveEntriesToFile(); // Save the current entries to the file

            PasswordsDataGrid.SelectedItem = null; // Deselect the entry
            RemoveEntryButton.IsEnabled = false; // Disable the button as no entry is selected
            selectedEntry = null; // Make selectedEntry null as no entry is selected nos
        }
    }

    private void GitHubLink_Click(object sender, PointerPressedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/CinnamonYeti459",
            UseShellExecute = true // Asks the OS to open the link above
        });
    }

    // Login activity button
    private void ActivityText_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var loginActivityWindow = new LoginActivityWindow();
        loginActivityWindow.Show();

        var window = this.VisualRoot as Window;
        if (window != null)
        {
            window.Close();
        }
    }

    // Logout button
    private void LogoutText_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var loginWindow = new LoginWindow();
        loginWindow.Show();

        var window = this.VisualRoot as Window;
        if (window != null)
        {
            window.Close();
        }
    }
}