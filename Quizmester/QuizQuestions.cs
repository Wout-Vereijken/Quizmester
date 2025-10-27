using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel; // For INotifyPropertyChanged
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
    }

    public class TimerThings
    {
        public int TimerText { get; set; }
    }

    public class QuizQuestionLoader : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged; 

        private readonly ObservableCollection<QuizQuestions> _Quiz;
        public ObservableCollection<QuizQuestions> Quiz => _Quiz;

        private string connectionString = "Server=localhost;Database=quizmester;Uid=root;Pwd=;";
        private int answeredQuestion = 0;
        private string _quizId;
        string questionText;
        int questionId;
        string answerOne;
        string answerTwo;
        string answerThree;
        string answerFour;
        int CorrectAnswer;
        int currentQuestionIndex = 1;
        int Score;

        // Timer fields
        private DispatcherTimer _timer;
        private int _timeLeft;

        // Public property to expose the timer value for binding. This will notify the UI when changed.
        public int TimerText
        {
            get => _timeLeft;
            set
            {
                _timeLeft = value;
                OnPropertyChanged(nameof(TimerText)); // Notify UI to update binding
            }
        }

        // TextBlock reference (unused now with binding)
        private TextBlock _timerTextBlock;
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
            TimerText = _timeLeft; // Update the bound property (notifies UI)
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timeLeft--;
            TimerText = _timeLeft; // Use the property to notify UI
            Console.WriteLine($"Time left: {_timeLeft}");

            if (_timeLeft <= 0)
            {
                _timer.Stop();
                MessageBox.Show("Time is up!");
                LoadNextQuestion(0); // 0 means no answer given
            }
        }

        // Helper to raise PropertyChanged events
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
            _timer.Stop(); // stop timer when user answers

            var mainWindow = Application.Current.MainWindow as MainWindow;

            if (answeredQuestion == CorrectAnswer)
            {
                Score++;
                //MessageBox.Show($"Correct!\nQuestion: {currentQuestionIndex}\nScore: {Score}");
                mainWindow.ShowOverlay(Colors.Green, 1);
            }
            else if (answeredQuestion == 5)
            {
                //MessageBox.Show($"Time's up!\nQuestion: {currentQuestionIndex}\nScore: {Score}");
                mainWindow.ShowOverlay(Colors.Yellow, 1);
            }
            else
            {
                Score--;
                //MessageBox.Show($"Wrong!\nQuestion: {currentQuestionIndex}\nScore: {Score}");
                mainWindow.ShowOverlay(Colors.Red, 1);
            } 

            currentQuestionIndex++;
            questionId++;

            if (currentQuestionIndex > 20)
            {
                MessageBox.Show($"Quiz Over!\nFinal Score: {Score}/20");
                MainWindow.SwitchTo(CurrentScreen.leaderBoardScreen, Score);
                return;
            }

            GetQuestion();
        }



        // Get the first questionId for this quiz
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


        // Get question text
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
                    }
                    GetAnswers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

        // Get answers
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
            var question = new QuizQuestions
            {
                QuestionText = questionText,
                QuestionId = questionId.ToString(),
                QuizAnswerOne = answerOne,
                QuizAnswerTwo = answerTwo,
                QuizAnswerThree = answerThree,
                QuizAnswerFour = answerFour,
                CorrectAnswer = CorrectAnswer,
                CurrentQuestion = currentQuestionIndex.ToString()

            };

            _Quiz.Clear();
            _Quiz.Add(question);

            StartTimer(30); // give 30 seconds per question
            return question;
        }


    }
}
