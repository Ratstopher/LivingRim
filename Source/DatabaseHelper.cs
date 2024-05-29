using System;
using System.Data.SQLite;
using Verse;

namespace LivingRim
{
    public static class DatabaseHelper
    {
        private static string dbPath = "Data Source=chat_log.db;Version=3;";

        static DatabaseHelper()
        {
            InitializeDatabase();
        }

        private static void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                connection.Open();
                string tableCreationQuery = "CREATE TABLE IF NOT EXISTS chat_logs (id INTEGER PRIMARY KEY, character_id TEXT, prompt TEXT, response TEXT, timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)";
                using (var command = new SQLiteCommand(tableCreationQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void LogChat(string characterId, string prompt, string response)
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                connection.Open();
                string insertQuery = "INSERT INTO chat_logs (character_id, prompt, response) VALUES (@characterId, @prompt, @response)";
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@characterId", characterId);
                    command.Parameters.AddWithValue("@prompt", prompt);
                    command.Parameters.AddWithValue("@response", response);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void RetrieveChatLogs(Action<SQLiteDataReader> callback)
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                connection.Open();
                string selectQuery = "SELECT * FROM chat_logs";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        callback(reader);
                    }
                }
            }
        }
    }
}
