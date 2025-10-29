using Google.Protobuf.Reflection;
using MySql.Data.MySqlClient;
using System.Data;
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
        public enum CurrentScreen
        {
            WelcomeScreen,
            LoginScreen,
            CreateAccountScreen,
            QuizChoiceScreen,
            QuizScreen,
            QuizMakeScreen,
            QuizMakeScreen2,
            AdminScreen,
            leaderBoardScreen
        }

        private MediaPlayer player = new MediaPlayer();

        // Database connection string
        string connectionString = "Server=localhost;Database=quizmester;Uid=root;Pwd=;";
        private QuizQuestionLoader currentQuizLoader;
        private SpecialQuizQuestionLoader specialQuiz;
        long quizId;
        long questionId;
        private QuizQuestions _changer;
        string UserName;
        long userId;
        bool IsspecialQuiz = false;

        public MainWindow()
        {
            InitializeComponent();
            // Start with the welcome screen
            ShowScreen(CurrentScreen.WelcomeScreen);
            // test the database connection as soon as the application starts
            TestConnection();
            // Set DataContext for data binding
            DataContext = new Quiz();

            string musicPath = @"C:\Users\woutv\source\repos\Quizmester\Quizmester\images\MapleStory (2006 GMS) 2-Hour Music Compilation.mp3";

            player.Open(new Uri(musicPath, UriKind.Absolute));
            player.Volume = 0.1;
            player.MediaEnded -= (s, e) => player.Position = TimeSpan.Zero;
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
            DataContext = new Quiz();
        }
        private void CreateAccountButton(object sender, RoutedEventArgs e)
        {
            ShowScreen(CurrentScreen.CreateAccountScreen);
        }
        private void OnCreateQuiz(object sender, RoutedEventArgs e)
        {
            ShowScreen(CurrentScreen.QuizMakeScreen);
        }
        private void BackToMain_Click(object sender, RoutedEventArgs e)
        {
            ShowScreen(CurrentScreen.QuizChoiceScreen);
            DataContext = new Quiz();
        }


        private void ShowScreen(CurrentScreen screen)
        {
            // Hide all screens first
            leaderBoardScreen.Visibility = Visibility.Collapsed;
            WelcomeScreen.Visibility = Visibility.Collapsed;
            LoginScreen.Visibility = Visibility.Collapsed;
            CreateAccountScreen.Visibility = Visibility.Collapsed;
            QuizChoiceScreen.Visibility = Visibility.Collapsed;
            QuizScreen.Visibility = Visibility.Collapsed;
            QuizMakeScreen.Visibility = Visibility.Collapsed;
            QuizMakeScreen2.Visibility = Visibility.Collapsed;
            AdminScreen.Visibility = Visibility.Collapsed;

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
                case CurrentScreen.QuizMakeScreen:
                    QuizMakeScreen.Visibility = Visibility.Visible;
                    break;
                case CurrentScreen.QuizMakeScreen2:
                    QuizMakeScreen2.Visibility = Visibility.Visible;
                    break;
                case CurrentScreen.AdminScreen:
                    AdminScreen.Visibility = Visibility.Visible;
                    break;
                case CurrentScreen.leaderBoardScreen:
                    leaderBoardScreen.Visibility = Visibility.Visible;
                    break;
            }
        }

        public static void SwitchTo(CurrentScreen screen, int score)
        {
            if (Application.Current.MainWindow is MainWindow mw)
            {
                // Toon het scherm
                mw.ShowScreen(screen);

                // Alleen als we naar leaderboard gaan
                if (screen == CurrentScreen.leaderBoardScreen)
                {
                    // Insert the score using the instance
                    mw.insertScore(score);

                    using (MySqlConnection conn = new MySqlConnection(mw.connectionString))
                    {
                        conn.Open();
                        MySqlDataAdapter LeaderBoardAdapter = new MySqlDataAdapter("SELECT * FROM LeaderBoard", conn);
                        DataTable LeaderBoardTable = new DataTable();
                        LeaderBoardAdapter.Fill(LeaderBoardTable);
                        mw.LeaderBoardGrid.ItemsSource = LeaderBoardTable.DefaultView;
                    }
                }
            }
        }

        private void JokerButton(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("50/50 Joker used!");
            if (IsspecialQuiz)
            {
                specialQuiz.useJoker();
            }
            else
            {
                currentQuizLoader.useJoker();
            }
        }

        private void SkipButton(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Question skipped!");
            if (IsspecialQuiz)
            {
                specialQuiz.skipQuestion(5);

            }
            else
            {
                currentQuizLoader.skipQuestion(5);
            }
        }


        private void insertScore(int score)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Insert score
                    string sql = "INSERT INTO leaderboard (LeaderBoardUserName, LeaderBoardScore, UserId) VALUES (@userName, @score, @UserId)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userName", UserName);
                        cmd.Parameters.AddWithValue("@score", score);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Score saved successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
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

            string sql = "SELECT COUNT(*) FROM users WHERE UserName = @user AND UserPassword = @pass";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string sequal = "SELECT UserPermission FROM users WHERE UserName = @user AND UserPassword = @pass";

                    using (MySqlCommand cmd = new MySqlCommand(sequal, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", user);
                        cmd.Parameters.AddWithValue("@pass", pass);

                        userId = cmd.LastInsertedId;

                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            long UserPermission = Convert.ToInt64(result);
                            bool isAdmin = UserPermission == 1;

                            //MessageBox.Show("Login successful!");
                            UserName = UsernameBox.Text;
                            UsernameBox.Text = "";
                            PasswordBox.Password = "";

                            if (isAdmin)
                            {
                                LoadAdminData();
                                ShowScreen(CurrentScreen.AdminScreen);
                            }
                            else
                            {
                                ShowScreen(CurrentScreen.QuizChoiceScreen);
                            }
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
                if (IsspecialQuiz)
                {
                    // Start the Special Quiz
                    MessageBox.Show("Starting Special Quiz!");
                    specialQuiz = new SpecialQuizQuestionLoader(quizId);
                    DataContext = specialQuiz;
                }
                else
                {
                    // Start the Normal Quiz
                    currentQuizLoader = new QuizQuestionLoader(quizId);
                    DataContext = currentQuizLoader;
                }

                // Bind to DataContext


                // Show quiz screen
                ShowScreen(CurrentScreen.QuizScreen);
            }
            else
            {
                MessageBox.Show("Quiz ID not found or invalid button!");
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
            if (IsspecialQuiz)
            {
                specialQuiz.LoadNextQuestion(AnsweredQuestions);
            }
            else
            {
                currentQuizLoader.LoadNextQuestion(AnsweredQuestions);
            }
        }

        private void AnswerTwo(object sender, RoutedEventArgs e)
        {
            AnsweredQuestions = 2;
            if (IsspecialQuiz)
            {
                specialQuiz.LoadNextQuestion(AnsweredQuestions);
            }
            else
            {
                currentQuizLoader.LoadNextQuestion(AnsweredQuestions);
            }

        }

        private void AnswerThree(object sender, RoutedEventArgs e)
        {
            AnsweredQuestions = 3;
            if (IsspecialQuiz)
            {
                specialQuiz.LoadNextQuestion(AnsweredQuestions);
            }
            else
            {
                currentQuizLoader.LoadNextQuestion(AnsweredQuestions);
            }
        }

        private void AnswerFour(object sender, RoutedEventArgs e)
        {
            AnsweredQuestions = 4;
            if (IsspecialQuiz)
            {
                specialQuiz.LoadNextQuestion(AnsweredQuestions);
            }
            else
            {
                currentQuizLoader.LoadNextQuestion(AnsweredQuestions);
            }
        }

        public void changeClockText(int time)
        {
            //MyTextBlock.Text = $"Time left:{time}";
        }

        private void SaveQuiz_button(object sender, RoutedEventArgs e)
        {
            string title = QuizTitleInput.Text;
            string description = QuizDescriptionInput.Text;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Insert quiz
                    string sql = "INSERT INTO quizzes (QuizTitle, QuizDescription) VALUES (@title, @description)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@description", description);
                        cmd.ExecuteNonQuery();

                        // Get new QuizId
                        quizId = cmd.LastInsertedId;
                    }

                    MessageBox.Show($"Quiz created successfully! QuizId = {quizId}");

                    QuizTitleInput.Text = "";
                    QuizDescriptionInput.Text = "";
                    ShowScreen(CurrentScreen.QuizMakeScreen2);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void SaveQuestion_Click(object sender, RoutedEventArgs e)
        {
            string question = QuestionInput.Text;
            string a1 = Answer1Input.Text;
            string a2 = Answer2Input.Text;
            string a3 = Answer3Input.Text;
            string a4 = Answer4Input.Text;
            int correct = CorrectAnswerInput.SelectedIndex + 1;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Insert question
                    string sql = "INSERT INTO questions (QuizId, QuestionText) VALUES (@quizId, @question)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@quizId", quizId);
                        cmd.Parameters.AddWithValue("@question", question);
                        cmd.ExecuteNonQuery();

                        // Get the QuestionId AFTER executing
                        questionId = cmd.LastInsertedId;
                    }

                    // Insert answers (corrected column names)
                    string query = @"INSERT INTO answers 
                (QuizId, QuestionId, AnswerOne, AnswerTwo, AnswerThree, AnswerFour, CorrectAnswer) 
                VALUES (@quizId, @questionId, @a1, @a2, @a3, @a4, @correct)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@quizId", quizId);
                        cmd.Parameters.AddWithValue("@questionId", questionId);
                        cmd.Parameters.AddWithValue("@a1", a1);
                        cmd.Parameters.AddWithValue("@a2", a2);
                        cmd.Parameters.AddWithValue("@a3", a3);
                        cmd.Parameters.AddWithValue("@a4", a4);
                        cmd.Parameters.AddWithValue("@correct", correct);
                        cmd.ExecuteNonQuery(); //execute 
                    }

                    MessageBox.Show("Question and answers saved successfully!");

                    // Clear inputs
                    QuestionInput.Text = "";
                    Answer1Input.Text = "";
                    Answer2Input.Text = "";
                    Answer3Input.Text = "";
                    Answer4Input.Text = "";
                    CorrectAnswerInput.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void LoadAdminData()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Load users
                MySqlDataAdapter usersAdapter = new MySqlDataAdapter("SELECT * FROM users", conn);
                DataTable usersTable = new DataTable();
                usersAdapter.Fill(usersTable);
                UsersGrid.ItemsSource = usersTable.DefaultView;

                // Load quizzes
                MySqlDataAdapter quizzesAdapter = new MySqlDataAdapter("SELECT * FROM quizzes", conn);
                DataTable quizzesTable = new DataTable();
                quizzesAdapter.Fill(quizzesTable);
                QuizzesGrid.ItemsSource = quizzesTable.DefaultView;

                //load questions
                MySqlDataAdapter questionsAdapter = new MySqlDataAdapter("SELECT * FROM questions", conn);
                DataTable questionsTable = new DataTable();
                questionsAdapter.Fill(questionsTable);
                QuestionsGrid.ItemsSource = questionsTable.DefaultView;

                //load answeres
                MySqlDataAdapter answerAdaper = new MySqlDataAdapter("SELECT * FROM Answers", conn);
                DataTable AnswerTable = new DataTable();
                answerAdaper.Fill(AnswerTable);
                AnsweresGrid.ItemsSource = AnswerTable.DefaultView;
            }
        }

        private void SaveUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT * FROM users", conn);
                    MySqlCommandBuilder builder = new MySqlCommandBuilder(adapter);

                    // Important: ensures the DataAdapter knows the primary key
                    adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                    // Get the original bound DataTable, not a copy
                    DataView view = (DataView)UsersGrid.ItemsSource;
                    DataTable usersTable = view.Table;

                    adapter.Update(usersTable); // now it updates existing rows correctly

                    MessageBox.Show("User changes saved!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving users: " + ex.Message);
            }

        }
        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is DataRowView selectedRow)
            {
                selectedRow.Row.Delete();
            }
            else
            {
                MessageBox.Show("Please select a user to delete.");
            }
        }

        private void SaveQuizes_click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT * FROM quizzes", conn);
                    MySqlCommandBuilder builder = new MySqlCommandBuilder(adapter);

                    // Important: ensures the DataAdapter knows the primary key
                    adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                    // Get the original bound DataTable, not a copy
                    DataView view = (DataView)QuizzesGrid.ItemsSource;
                    DataTable quiztable = view.Table;

                    adapter.Update(quiztable); // now it updates existing rows correctly

                    MessageBox.Show("quiz changes saved!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving users: " + ex.Message);
            }
        }

        private void DeleteQuizes_click(object sender, RoutedEventArgs e)
        {
            if (QuizzesGrid.SelectedItem is DataRowView selectedRow)
            {
                selectedRow.Row.Delete();
            }
            else
            {
                MessageBox.Show("Please select a Quiz to delete.");
            }
        }

        private void HandleCheck(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb != null && rb.IsChecked == true)
            {
                IsspecialQuiz = true;
            }
            else
            {
                IsspecialQuiz = false;
            }
        }
        private void HandleCheckFalse(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb != null && rb.IsChecked == true)
            {
                IsspecialQuiz = false;
            }
            else
            {
                IsspecialQuiz = true;
            }
        }


    }
}