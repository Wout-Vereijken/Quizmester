using MySql.Data.MySqlClient; // MySQL client library for connecting to MySQL
using System;
using System.Collections.ObjectModel; // Needed for ObservableCollection
using System.Windows;
using System.Linq;
using Org.BouncyCastle.Bcpg; // Needed for .Select() when joining strings

namespace Quizmester
{
    // Class representing one quiz choice (title + description)
    public class QuizChoice
    {
        public string QuizTitle { get; set; }       // Title of the quiz
        public string QuizDescription { get; set; } // Description of the quiz
        public string QuizId { get; set; } // ID of the quiz
    }

    // Class to handle retrieving quizzes from the database
    public class Quiz
    {
        // Holds the collection of quizzes (updates UI automatically if bound in WPF)
        private readonly ObservableCollection<QuizChoice> _QuizChoice;
        public ObservableCollection<QuizChoice> QuizChoices => _QuizChoice;

        // Connection string for MySQL database
        string connectionString = "Server=localhost;Database=quizmester;Uid=root;Pwd=;";

        // Constructor: loads all quizzes when this class is created
        public Quiz()
        {
            _QuizChoice = new ObservableCollection<QuizChoice>();

            // SQL query to get all quiz titles and descriptions
            string sql = "SELECT QuizTitle, QuizDescription, QuizId FROM quizzes";

            // Open a connection to the database
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open(); // Try to connect to MySQL

                    // Create and execute SQL command
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Loop through the results
                        while (reader.Read())
                        {
                            // Read each column value by column name
                            string title = reader.GetString("QuizTitle");
                            string description = reader.GetString("QuizDescription");
                            string QuizId = reader.GetInt32("QuizId").ToString();


                            // Add the quiz to the collection
                            _QuizChoice.Add(new QuizChoice
                            {
                                QuizTitle = title,
                                QuizDescription = description,
                                QuizId = QuizId
                            });
                        }
                    }

                    //show all quizzes in a MessageBox
                    string allCategories = string.Join("\n",
                        _QuizChoice.Select(q => $"{q.QuizTitle}: {q.QuizDescription}"));

                    MessageBox.Show("Available Quizzes:\n" + allCategories);
                }
                catch (Exception ex)
                {
                    // Show error if something goes wrong (e.g. connection failed)
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
    }
}