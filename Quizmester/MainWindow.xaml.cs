using System.Windows;

namespace Quizmester
{
    public partial class MainWindow : Window
    {
        private enum CurrentScreen
        {
            WelcomeScreen,
            LoginScreen,
            CreateAccountScreen
        }

        private void ShowScreen(CurrentScreen screen)
        {
            // Hide all screens first
            WelcomeScreen.Visibility = Visibility.Collapsed;
            LoginScreen.Visibility = Visibility.Collapsed;
            CreateAccountScreen.Visibility = Visibility.Collapsed;

            // Show selected screen
            switch (screen)
            {
                case CurrentScreen.WelcomeScreen:
                    WelcomeScreen.Visibility = Visibility.Visible;
                    break;
                case CurrentScreen.LoginScreen:
                    LoginScreen.Visibility = Visibility.Visible;
                    break;
                case CurrentScreen.CreateAccountScreen:
                    CreateAccountScreen.Visibility = Visibility.Visible;
                    break;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            ShowScreen(CurrentScreen.WelcomeScreen);
        }

        private void LoginButton(object sender, RoutedEventArgs e)
        {
            ShowScreen(CurrentScreen.LoginScreen);
        }
        private void BackButton(object sender, RoutedEventArgs e)
        {
            ShowScreen(CurrentScreen.WelcomeScreen);
        }
        private void CreateAccountButton(object sender, RoutedEventArgs e)
        {
            ShowScreen(CurrentScreen.CreateAccountScreen);
        }
    }
}
