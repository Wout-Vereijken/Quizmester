using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;

namespace Quizmester
{
    // Holds one question with answers
    public class QuizQuestions
    {
        public string QuestionText { get; set; }
        public string QuizAnswerOne { get; set; }
        public string QuizAnswerTwo { get; set; }
        public string QuizAnswerThree { get; set; }
        public string QuizAnswerFour { get; set; }
        public int CorrectAnswer { get; set; }
    }

    // This class loads questions only once
    public class QuizQuestionLoader
    {
        private readonly ObservableCollection<QuizQuestions> _QuizQuestions;
        public ObservableCollection<QuizQuestions> QuizQuestions => _QuizQuestions;
        private bool _loaded = false;
        private string connectionString = "Server=localhost;Database=quizmester;Uid=root;Pwd=;";
        private string _quizId;
        private List<QuizQuestions> _allQuestions;

        public QuizQuestionLoader(string quizId)
        {
            _QuizQuestions = new ObservableCollection<QuizQuestions>();
            _quizId = quizId;
            if (_loaded) return;
            LoadQuestions();
            _loaded = true;
        }

        private void LoadQuestions()
        {
            _allQuestions = new List<QuizQuestions>();
            string sqlQuestions = "SELECT QuestionText, QuestionId FROM questions WHERE QuizId = @quizId ORDER BY QuestionId ASC LIMIT 1";
            string sqlAnswers = "SELECT AnswerOne, AnswerTwo, AnswerThree, AnswerFour, CorrectAnswer FROM answers WHERE QuestionId = @QuestionId LIMIT 1";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    var questions = new List<(int QuestionId, string QuestionText)>();

                    using (MySqlCommand cmd = new MySqlCommand(sqlQuestions, conn))
                    {
                        cmd.Parameters.AddWithValue("@quizId", _quizId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                questions.Add((reader.GetInt32("QuestionId"), reader.GetString("QuestionText")));
                            }
                        }
                    }

                    foreach (var q in questions)
                    {
                        using (MySqlCommand cmd2 = new MySqlCommand(sqlAnswers, conn))
                        {
                            cmd2.Parameters.AddWithValue("@QuestionId", q.QuestionId);
                            using (MySqlDataReader answerReader = cmd2.ExecuteReader())
                            {
                                if (answerReader.Read())
                                {
                                    var quizQuestion = new QuizQuestions
                                    {
                                        QuestionText = q.QuestionText,
                                        QuizAnswerOne = answerReader.GetString("AnswerOne"),
                                        QuizAnswerTwo = answerReader.GetString("AnswerTwo"),
                                        QuizAnswerThree = answerReader.GetString("AnswerThree"),
                                        QuizAnswerFour = answerReader.GetString("AnswerFour"),
                                        CorrectAnswer = answerReader.GetInt32("CorrectAnswer")
                                    };

                                    _QuizQuestions.Add(quizQuestion); // <-- Add to ObservableCollection so UI updates
                                }
                            }
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
}
