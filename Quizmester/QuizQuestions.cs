using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel; // For INotifyPropertyChanged
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
    }

    public class TimerThings
    {
        public int TimerText { get; set; }
    }

    public class QuizQuestionLoader : INotifyPropertyChanged // NEW: Implement INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged; // NEW: Required for notifications

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
        int currentQuestionIndex;

        // Timer fields
        private DispatcherTimer _timer;
        private int _timeLeft;

        // NEW: Public property to expose the timer value for binding. This will notify the UI when changed.
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
            TimerText = _timeLeft; // Use the property to notify UI (replaces ChangetimerText call)
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

        public void LoadNextQuestion(int AnsweredQuestions)
        {
            _timer.Stop(); // stop timer when user answers

            answeredQuestion = AnsweredQuestions;
            currentQuestionIndex++;
            questionId++;

            if (answeredQuestion == CorrectAnswer)
            {
                MessageBox.Show("Correct!");
            }
            else if (answeredQuestion != -1)
            {
                MessageBox.Show("Incorrect!");
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
                CorrectAnswer = CorrectAnswer
            };

            _Quiz.Clear();
            _Quiz.Add(question);

            StartTimer(30); // give 30 seconds per question
            return question;
        }
    }
}
