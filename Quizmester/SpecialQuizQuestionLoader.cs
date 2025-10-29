using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Media;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using static Quizmester.MainWindow;

namespace Quizmester
{
    public class SpecialQuizQuestionLoader : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ObservableCollection<QuizQuestions> _Quiz;
        public ObservableCollection<QuizQuestions> Quiz => _Quiz;

        private string connectionString = "Server=localhost;Database=quizmester;Uid=root;Pwd=;";
        private string _quizId;
        private int questionId;
        private int correctCount = 0;
        private int totalTimeSeconds = 0;
        private Stopwatch stopwatch;
        private string questionText, answerOne, answerTwo, answerThree, answerFour;
        private int CorrectAnswer;
        private Random rnd = new Random();

        // UI timer text property (bound to quiz timer label)
        private int _elapsedSeconds;
        public int TimerText
        {
            get => _elapsedSeconds;
            set
            {
                _elapsedSeconds = value;
                OnPropertyChanged(nameof(TimerText));
            }
        }

        // DispatcherTimer for live updates
        private DispatcherTimer _uiTimer;

        public SpecialQuizQuestionLoader(string quizId)
        {
            _Quiz = new ObservableCollection<QuizQuestions>();
            _quizId = quizId;

            stopwatch = new Stopwatch();
            stopwatch.Start();

            InitUITimer();
            LoadRandomQuestion();
        }

        private void InitUITimer()
        {
            _uiTimer = new DispatcherTimer();
            _uiTimer.Interval = TimeSpan.FromSeconds(1);
            _uiTimer.Tick += (s, e) =>
            {
                TimerText = (int)(stopwatch.Elapsed.TotalSeconds) + totalTimeSeconds;
            };
            _uiTimer.Start();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        public void skipQuestion(int answeredQuestion)
        {
            LoadNextQuestion(answeredQuestion);
        }

        public void LoadNextQuestion(int answeredQuestion)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;

            if (answeredQuestion == CorrectAnswer)
            {
                correctCount++;
                mainWindow.ShowOverlay(Colors.Green, 1.5);
            }
            else
            {
                // Add penalty of 5 seconds for wrong answers
                totalTimeSeconds += 5;
                mainWindow.ShowOverlay(Colors.Red, 1);
            }

            if (correctCount >= 10)
            {
                stopwatch.Stop();
                _uiTimer.Stop();

                totalTimeSeconds += (int)stopwatch.Elapsed.TotalSeconds;
                MessageBox.Show($"🏁 Special Quiz Finished!\nTotal Time: {totalTimeSeconds} seconds");
                MainWindow.SwitchTo(CurrentScreen.leaderBoardScreen, totalTimeSeconds);
                return;
            }

            LoadRandomQuestion();
        }

        public void LoadRandomQuestion()
        {
            string sql = $"SELECT QuestionId FROM Questions WHERE QuizId = {_quizId} ORDER BY RAND() LIMIT 1";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
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
            }

            GetQuestion();
        }

        private void GetQuestion()
        {
            string sql = $"SELECT QuestionText FROM Questions WHERE QuizId = {_quizId} AND QuestionId = {questionId} LIMIT 1";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
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
            }

            GetAnswers();
        }

        private void GetAnswers()
        {
            string sql = $"SELECT AnswerOne, AnswerTwo, AnswerThree, AnswerFour, CorrectAnswer " +
                         $"FROM Answers WHERE QuizId = {_quizId} AND QuestionId = {questionId} LIMIT 1";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
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
            }

            ShowQuestion();
        }

        private void ShowQuestion()
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
                CurrentQuestion = correctCount.ToString(),
                IsSpecial = false
            };

            _Quiz.Clear();
            _Quiz.Add(question);
        }
    }
}
