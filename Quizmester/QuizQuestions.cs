using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;
using System.Windows;

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

    public class QuizQuestionLoader
    {
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

        public QuizQuestionLoader(string quizId)
        {
            _Quiz = new ObservableCollection<QuizQuestions>();
            _quizId = quizId;
            GetQuestionId();
        }

        public QuizQuestionLoader(int answeredQuestions)
        {
            answeredQuestion = answeredQuestions;
            LoadNextQuestion();
        }

        public void LoadNextQuestion()
        {
            currentQuestionIndex++;
            questionId++;
            GetQuestion();
        }


        // Get the question ID from the database where the quizid is equal to the quizid passed in the constructor
        public void GetQuestionId()
        {
            string sql = $"SELECT QuestionText, QuestionId FROM Questions WHERE QuizId = {_quizId} LIMIT 1 ";

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
                            MessageBox.Show($"Question ID: {questionId}");
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

        // Get the question text from the database where the quizid is equal to the quizid passed in the constructor and the questionid is equal to the questionid retrieved from GetQuestionId
        public void GetQuestion()
        {
            MessageBox.Show(questionId.ToString());
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

        public void GetAnswers()
        {
            string sql = $"SELECT AnswerOne, AnswerTwo, AnswerThree, AnswerFour, CorrectAnswer FROM Answers WHERE QuizId = {_quizId} AND QuestionId = {questionId} LIMIT 1";

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
            _Quiz.Add(question);      // add the new question
            return question;
        }
    }
}
