using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Media;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static Quizmester.MainWindow;

namespace Quizmester
{
    public class QuizQuestions
    {
        public string QuestionText { get; set; }
        public string QuestionId { get; set; }
        public string QuizAnswerOne { get; set; }
        public string QuizAnswerTwo { get; set; }
        public string QuizAnswerThree { get; set; }
        public string QuizAnswerFour { get; set; }
        public int CorrectAnswer { get; set; }
        public string CurrentQuestion { get; set; }
        public bool IsSpecial { get; set; } // Added for special question flag
    }

    public class QuizQuestionLoader : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ObservableCollection<QuizQuestions> _Quiz;
        public ObservableCollection<QuizQuestions> Quiz => _Quiz;

        private string connectionString = "Server=localhost;Database=quizmester;Uid=root;Pwd=;";
        private string _quizId;
        private int questionId;
        private int currentQuestionIndex = 1;
        private int Score;
        private int CorrectAnswer;
        private string questionText, answerOne, answerTwo, answerThree, answerFour;
        private MediaPlayer player = new MediaPlayer();

        // Special question system
        private int specialQuestionIndex;
        private bool isSpecialQuestion;

        // Timer fields
        private DispatcherTimer _timer;
        private int _timeLeft;

        public int TimerText
        {
            get => _timeLeft;
            set
            {
                _timeLeft = value;
                OnPropertyChanged(nameof(TimerText));
            }
        }

        private MainWindow _mainWindow;

        public QuizQuestionLoader(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public QuizQuestionLoader(string quizId)
        {
            _Quiz = new ObservableCollection<QuizQuestions>();
            _quizId = quizId;
            InitTimer();

            // Randomly select a special question within the first 20
            Random rnd = new Random();
            specialQuestionIndex = rnd.Next(1, 21); // Between 1 and 20 inclusive

            GetQuestionId();
        }

        private void InitTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        private void StartTimer(int seconds)
        {
            _timeLeft = seconds;
            TimerText = _timeLeft;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timeLeft--;
            TimerText = _timeLeft;

            if (_timeLeft <= 0)
            {
                _timer.Stop();
                MessageBox.Show("Time is up!");
                LoadNextQuestion(0);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void skipQuestion(int answeredQuestion)
        {
            LoadNextQuestion(answeredQuestion);
        }

        public void LoadNextQuestion(int answeredQuestion)
        {
            _timer.Stop();
            var mainWindow = Application.Current.MainWindow as MainWindow;

            if (answeredQuestion == CorrectAnswer)
            {
                if (isSpecialQuestion)
                {
                    Score += 3;
                    mainWindow.ShowOverlay(Colors.Green, 1.5);
                }
                else
                {
                    Score++;
                    mainWindow.ShowOverlay(Colors.Green, 1);
                }
            }
            else if (answeredQuestion == 5)
            {
                mainWindow.ShowOverlay(Colors.Yellow, 1);
            }
            else
            {
                Score--;
                mainWindow.ShowOverlay(Colors.Red, 1);
            }

            currentQuestionIndex++;

            // Dynamically fetch the next question from DB
            string nextQuestionSql = $"SELECT QuestionId FROM Questions WHERE QuizId = {_quizId} AND QuestionId > {questionId} ORDER BY QuestionId ASC LIMIT 1";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(nextQuestionSql, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        questionId = reader.GetInt32("QuestionId");
                        GetQuestion();
                    }
                    else
                    {
                        // No more questions left 
                        MessageBox.Show($"Quiz Over!\nFinal Score: {Score}");
                        MainWindow.SwitchTo(CurrentScreen.leaderBoardScreen, Score);
                    }
                }
            }
        }


        public void GetQuestionId()
        {
            string sql = $"SELECT QuestionText, QuestionId FROM Questions WHERE QuizId = {_quizId} LIMIT 1";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            questionId = reader.GetInt32("QuestionId");
                        }
                    }
                    GetQuestion();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

        public void GetQuestion()
        {
            string sql = $"SELECT QuestionText FROM Questions WHERE QuizId = {_quizId} AND QuestionId = {questionId} LIMIT 1";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            questionText = reader.GetString("QuestionText");
                        }
                        else
                        {
                            // ❌ No more questions found — end the quiz
                            MessageBox.Show($"No more questions available!\nFinal Score: {Score}");
                            MainWindow.SwitchTo(CurrentScreen.leaderBoardScreen, Score);
                            return;
                        }
                    }
                    GetAnswers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }


        public void GetAnswers()
        {
            string sql = $"SELECT AnswerOne, AnswerTwo, AnswerThree, AnswerFour, CorrectAnswer " +
                         $"FROM Answers WHERE QuizId = {_quizId} AND QuestionId = {questionId} LIMIT 1";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            answerOne = reader.GetString("AnswerOne");
                            answerTwo = reader.GetString("AnswerTwo");
                            answerThree = reader.GetString("AnswerThree");
                            answerFour = reader.GetString("AnswerFour");
                            CorrectAnswer = reader.GetInt32("CorrectAnswer");
                        }
                    }
                    ShowQuestion();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

        public void useJoker()
        {
            if (CorrectAnswer == 1)
            {
                answerTwo = "";
                answerThree = "";
            }
            else if (CorrectAnswer == 2)
            {
                answerOne = "";
                answerFour = "";
            }
            else if (CorrectAnswer == 3)
            {
                answerOne = "";
                answerFour = "";
            }
            else if (CorrectAnswer == 4)
            {
                answerTwo = "";
                answerThree = "";
            }

            ShowQuestion();
        }

        public QuizQuestions ShowQuestion()
        {
            // Check if this question is special
            isSpecialQuestion = (currentQuestionIndex == specialQuestionIndex);

            if (isSpecialQuestion)
            {
                string musicPath = @"C:\Users\woutv\source\repos\Quizmester\Quizmester\images\maplestory-lvl-up.mp3";

                player.MediaEnded -= (s, e) => player.Position = TimeSpan.Zero;
                player.Open(new Uri(musicPath, UriKind.Absolute));
                player.Play();

                // Change design 
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowOverlay(Colors.Gold, 0.8);
            }

            var question = new QuizQuestions
            {
                QuestionText = isSpecialQuestion ? $"⭐ SPECIAL QUESTION! ⭐\n\n{questionText}" : questionText,
                QuestionId = questionId.ToString(),
                QuizAnswerOne = answerOne,
                QuizAnswerTwo = answerTwo,
                QuizAnswerThree = answerThree,
                QuizAnswerFour = answerFour,
                CorrectAnswer = CorrectAnswer,
                CurrentQuestion = currentQuestionIndex.ToString(),
                IsSpecial = isSpecialQuestion
            };

            _Quiz.Clear();
            _Quiz.Add(question);

            StartTimer(30);
            return question;
        }
    }
}
