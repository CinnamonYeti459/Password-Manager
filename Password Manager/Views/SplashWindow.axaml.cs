using Avalonia.Controls;
using System;
using System.Threading.Tasks;

namespace Password_Manager.Views
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            StartSplashScreenTimer();
        }

        private async void StartSplashScreenTimer()
        {
            Random random = new Random();
            await Task.Delay(random.Next(500, 2500));

            // After delay, open MainWindow
            var loginWindow = new LoginWindow();
            loginWindow.Show();

            // Close the splash window
            this.Close();
        }
    }
}
