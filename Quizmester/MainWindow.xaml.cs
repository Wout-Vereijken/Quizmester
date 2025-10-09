using Google.Protobuf.Reflection;
using MySql.Data.MySqlClient;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
namespace Quizmester
{
    public partial class MainWindow : Window
    {
        // Enum to track current screen
        private enum CurrentScreen
        {
            WelcomeScreen,
            LoginScreen,
            CreateAccountScreen,
            QuizChoiceScreen,
            QuizScreen
        }

        private MediaPlayer player = new MediaPlayer();

        // Database connection string
        string connectionString = "Server=localhost;Database=quizmester;Uid=root;Pwd=;";
        private QuizQuestionLoader currentQuizLoader;

        private QuizQuestions _changer;

        public MainWindow()
        {
            InitializeComponent();
            // Start with the welcome screen
            ShowScreen(CurrentScreen.WelcomeScreen);
            // test the database connection as soon as the application starts
            TestConnection();
            // Set DataContext for data binding
            DataContext = new Quiz();

            string musicPath = @"C:\Users\woutv\source\repos\Quizmester\Quizmester\music\MapleStory (2006 GMS) 2-Hour Music Compilation.mp3";

            player.Open(new Uri(musicPath, UriKind.Absolute));
            player.MediaEnded += (s, e) => player.Position = TimeSpan.Zero; // optional: loop
            player.Play();
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
            QuizChoiceScreen.Visibility = Visibility.Collapsed;
            QuizScreen.Visibility = Visibility.Collapsed;

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
                case CurrentScreen.QuizChoiceScreen:
                    QuizChoiceScreen.Visibility = Visibility.Visible;
                    break;
                case CurrentScreen.QuizScreen:
                    QuizScreen.Visibility = Visibility.Visible;
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

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Check if username exists
                    string checkSql = "SELECT COUNT(*) FROM users WHERE UserName = @user";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@user", user);

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (count > 0)
                        {
                            MessageBox.Show("This username is already taken. Please choose another one.");
                            return;
                        }
                    }

                    // Insert if username doesn't exist
                    string sql = "INSERT INTO users (UserName, UserPassword) VALUES (@user, @pass)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", user);
                        cmd.Parameters.AddWithValue("@pass", pass);

                        int rows = cmd.ExecuteNonQuery();
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


            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }
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
                                // Load quizzes for regular user
                                //LoadQuizList();
                                // Navigate to user quiz screen
                                ShowScreen(CurrentScreen.QuizChoiceScreen);
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
        #endregion

        #region start quiz

        public void OnStartQuiz(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string quizId)
            {

                // Create one instance of the loader
                currentQuizLoader = new QuizQuestionLoader(quizId);


                // Bind it to DataContext
                DataContext = currentQuizLoader;

                ShowScreen(CurrentScreen.QuizScreen);
            }
            else
            {
                MessageBox.Show("Not working");
            }
        }

        public async void ShowOverlay(Color color, double seconds)
        {
            // Set overlay color and make it visible
            Overlay.Background = new SolidColorBrush(color);
            Overlay.Background.Opacity = 0.5;
            Overlay.Visibility = Visibility.Visible;

            // Animate opacity to 1 (fully visible)
            var fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(100));
            Overlay.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            // Wait for the given duration
            await Task.Delay(TimeSpan.FromSeconds(seconds));

            // Fade out and hide
            var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s, e) => Overlay.Visibility = Visibility.Collapsed;
            Overlay.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        #endregion
        // answer quiz questions
        int AnsweredQuestions = 0;
        private void AnswerOne(object sender, RoutedEventArgs e)
        {
            AnsweredQuestions = 1;
            currentQuizLoader.LoadNextQuestion(AnsweredQuestions);
            ShowOverlay(Colors.Green, 1.0);
        }

        private void AnswerTwo(object sender, RoutedEventArgs e)
        {
            AnsweredQuestions = 2;
            currentQuizLoader.LoadNextQuestion(AnsweredQuestions);
            ShowOverlay(Colors.Red, 1.0);
        }

        private void AnswerThree(object sender, RoutedEventArgs e)
        {
            AnsweredQuestions = 3;
            currentQuizLoader.LoadNextQuestion(AnsweredQuestions);
        }

        private void AnswerFour(object sender, RoutedEventArgs e)
        {
            AnsweredQuestions = 4;
            currentQuizLoader.LoadNextQuestion(AnsweredQuestions);
        }

        public void changeClockText(int time)
        {
            //MyTextBlock.Text = $"Time left:{time}";
        }

    }
}