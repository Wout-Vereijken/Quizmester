using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Quizmester
{
    // Simple model: just holds one question's text + quiz ID
    public class QuizQuestions
    {
        public string QuestionText { get; set; }
        public string QuizId { get; set; }
    }

    // This class actually loads questions from the database
    public class quizQuestion
    {
        // Backing field for our collection of questions
        private readonly ObservableCollection<QuizChoice> _QuizQuestions;

        // Public property so WPF can bind to it
        public ObservableCollection<QuizChoice> QuizQuestions => _QuizQuestions;

        // MySQL connection string – update DB/table/credentials if needed
        string connectionString = "Server=localhost;Database=quizmester;Uid=root;Pwd=;";

        // Constructor: we pass in quizId and it fetches questions for that quiz
        public quizQuestion(string quizId)
        {
            // Debug popup: see which quiz ID was passed
            MessageBox.Show("Loading questions for quiz ID: " + quizId);

            _QuizQuestions = new ObservableCollection<QuizChoice>();

            // SQL: IMPORTANT – make sure `questions` is the right table name
            // and that you have columns `QuestionText` and `QuizId`
            string sql = "SELECT QuestionText FROM questions WHERE QuizId = @quizId";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open(); // Try to connect to DB

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        // Add parameter
                        cmd.Parameters.AddWithValue("@quizId", quizId);

                        // Execute query
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Debug: check if query was executed
                            MessageBox.Show("Executing query to load questions...");

                            // Loop through results
                            while (reader.Read())
                            {
                                // Get the QuestionText column from the current row
                                string questionText = reader.GetString("QuestionText");

                                // Debug: show what we loaded
                                MessageBox.Show("Loaded question: " + questionText);

                                // Add it to collection → WPF can now display it
                                _QuizQuestions.Add(new QuizChoice
                                {
                                    QuestionText = questionText
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If anything fails (SQL error, connection, etc.) we see it here
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
    }
}
