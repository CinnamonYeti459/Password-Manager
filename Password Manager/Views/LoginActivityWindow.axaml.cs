using Avalonia.Controls;
using Avalonia.Interactivity;
using Password_Manager.ViewModels;

namespace Password_Manager.Views
{
    public partial class LoginActivityWindow : Window
    {
        public LoginActivityWindow()
        {
            InitializeComponent();
            DataContext = new LoginActivityViewModel();
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show(); // Opens a new window

            this.Close(); // Closes this window
        }
    }
}