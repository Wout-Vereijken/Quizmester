using System.Windows;
using MySql.Data.MySqlClient;
namespace Quizmester
{
    public partial class MainWindow : Window
    {
        // Enum to track current screen
        private enum CurrentScreen
        {
            WelcomeScreen,
            LoginScreen,
            CreateAccountScreen
        }

        // Database connection string
        string connectionString = "Server=localhost;Database=quizmester;Uid=root;Pwd=;";

        public MainWindow()
        {
            InitializeComponent();
            // Start with the welcome screen
            ShowScreen(CurrentScreen.WelcomeScreen);
            // test the database connection as soon as the application starts
            TestConnection();
        }
        //testing database connection
        #region Debugging
        private void TestConnection()
        {
            string connectionString = "Server=localhost;Database=quizmester;Uid=root;Pwd=;";


            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MessageBox.Show("Connected to MySQL!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        #endregion
        // page navigation
        #region navigation

        private void ExitApplicationButton(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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

        #endregion
        //enter login credentials into database
        #region Login and Account Creation

        private void InsertCreateAccountButton(object sender, RoutedEventArgs e)
        {
            string user = CreateUsernameBox.Text;
            string pass = CreatePasswordBox.Password;

            string sql = "INSERT INTO users (UserName, UserPassword) VALUES (@user, @pass)";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", user);
                        cmd.Parameters.AddWithValue("@pass", pass);

                        int rows = cmd.ExecuteNonQuery(); // Execute
                        if (rows > 0)
                        {
                            MessageBox.Show("Account created successfully!");
                            CreateUsernameBox.Text = "";
                            CreatePasswordBox.Password = "";
                            ShowScreen(CurrentScreen.WelcomeScreen);
                        }
                        else
                        {
                            MessageBox.Show("Something went wrong.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void InsertLoginButton(object sender, RoutedEventArgs e)
        {
            // Use the correct TextBoxes for login
            string user = UsernameBox.Text;
            string pass = PasswordBox.Password;

            // Placeholder for admin check logic
            bool isAdmin = false; 

            string sql = "SELECT COUNT(*) FROM users WHERE UserName = @user AND UserPassword = @pass";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", user);
                        cmd.Parameters.AddWithValue("@pass", pass);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show("Login successful!");
                            UsernameBox.Text = "";
                            PasswordBox.Password = "";
                            
                            // TODO: Navigate to quiz screen or admin screen
                            if (isAdmin)
                            {
                                // Navigate to admin screen
                                ShowScreen(CurrentScreen.WelcomeScreen);
                            }
                            else
                            {
                                // Navigate to user quiz screen
                                ShowScreen(CurrentScreen.WelcomeScreen);
                            }
                            // Navigate to quiz screen
                            
                        }
                        else
                        {
                            MessageBox.Show("Invalid username or password.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

    }

    #endregion
}

