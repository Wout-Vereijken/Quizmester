using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
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
        private readonly ObservableCollection<QuizQuestions> _QuizQuestions;
        public ObservableCollection<QuizQuestions> QuizQuestions => _QuizQuestions;

        private string connectionString = "Server=localhost;Database=quizmester;Uid=root;Pwd=;";
        private string _quizId;
        private int currentQuestionIndex = 0;

        public QuizQuestionLoader(string quizId)
        {
            _QuizQuestions = new ObservableCollection<QuizQuestions>();
            _quizId = quizId;
            LoadNextQuestion();
        }

        // This constructor is unclear and unused — you may want to remove it or fix usage.
        public QuizQuestionLoader(int answeredQuestions)
        {
            _QuizQuestions = new ObservableCollection<QuizQuestions>();
            currentQuestionIndex = answeredQuestions;
            LoadNextQuestion();
        }

        public void LoadNextQuestion()
        {
            var question = GetQuestionFromDatabase(_quizId, currentQuestionIndex);
            if (question != null)
            {
                _QuizQuestions.Add(question);
                currentQuestionIndex++; // increment for next call
            }
            else
            {
                MessageBox.Show("No more questions available or an error occurred.");
            }
        }

        private QuizQuestions GetQuestionFromDatabase(string quizId, int offset)
        {
            string sqlQuestion = @"
                SELECT q.QuestionText, q.QuestionId, a.AnswerOne, a.AnswerTwo, a.AnswerThree, a.AnswerFour, a.CorrectAnswer
                FROM questions q
                INNER JOIN answers a ON q.QuestionId = a.QuestionId
                WHERE q.QuizId = @quizId
                ORDER BY q.QuestionId ASC
                LIMIT 1 OFFSET @offset";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    using (MySqlCommand cmd = new MySqlCommand(sqlQuestion, conn))
                    {
                        cmd.Parameters.AddWithValue("@quizId", quizId);
                        cmd.Parameters.AddWithValue("@offset", offset);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new QuizQuestions
                                {
                                    QuestionId = reader.GetInt32("QuestionId").ToString(),
                                    QuestionText = reader.GetString("QuestionText"),
                                    QuizAnswerOne = reader.GetString("AnswerOne"),
                                    QuizAnswerTwo = reader.GetString("AnswerTwo"),
                                    QuizAnswerThree = reader.GetString("AnswerThree"),
                                    QuizAnswerFour = reader.GetString("AnswerFour"),
                                    CorrectAnswer = reader.GetInt32("CorrectAnswer")
                                };
                            }
                            else
                            {
                                // No rows found for this offset - no more questions
                                return null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading question: " + ex.Message);
                return null;
            }
        }
    }
}
