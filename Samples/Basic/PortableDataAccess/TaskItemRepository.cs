using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Data.Sqlite;

namespace PortableDataAccess
{
    using System.Transactions;

    public class TaskItemRepository
    {
        private readonly string connectionString = null;

        public TaskItemRepository(string connectionString)
        {
            this.connectionString = connectionString;

            InitializeRepository();
        }

        private void InitializeRepository()
        {
            using (var conn = new SqliteConnection(connectionString))
            using (var cmd = new SqliteCommand(@"
CREATE TABLE IF NOT EXISTS [Tasks] (
    [Id] INTEGER PRIMARY KEY,
    [Name] TEXT NOT NULL,
    [IsComplete] BOOL NOT NULL
)", conn))
            {
                conn.Open();
                using (var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    cmd.ExecuteNonQuery();
                
                    trans.Complete();
                }
            }
        }

        public List<TaskItem> GetAllTasks()
        {
            List<TaskItem> tasks = new List<TaskItem>();
            using (var conn = new SqliteConnection(connectionString))
            using (var cmd = new SqliteCommand("SELECT * FROM [Tasks]", conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    foreach (var row in reader)
                    {
                        tasks.Add(ReadTask(reader));
                    }
                }
            }
            return tasks;
        }

        public TaskItem GetTask(int taskId)
        {
            TaskItem task = null;
            using (var conn = new SqliteConnection(connectionString))
            using (var cmd = new SqliteCommand("SELECT * FROM [Tasks] WHERE [Id] = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", taskId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        task = ReadTask(reader);
                    }
                }
            }

            return task;
        }

        private static TaskItem ReadTask(SqliteDataReader reader)
        {
            TaskItem task = new TaskItem
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = Convert.ToString(reader["Name"]),
                IsComplete = Convert.ToBoolean(reader["IsComplete"]),
            };
            return task;
        }
        
        public int AddTask(TaskItem task)
        {
            using (var conn = new SqliteConnection(connectionString))
            using (var cmd = new SqliteCommand(@"
INSERT INTO [Tasks] (
    [Name], 
    [IsComplete]
) VALUES (
    @name,
    @complete
);
SELECT last_insert_rowid();", conn))
            {
                cmd.Parameters.AddWithValue("@name", task.Name);
                cmd.Parameters.AddWithValue("@complete", task.IsComplete);
                conn.Open();

                using (var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions {IsolationLevel = IsolationLevel.Serializable}))
                {
                    var result = cmd.ExecuteScalar();
                    trans.Complete();
                    return Convert.ToInt32(result);
                }
            }
        }
        
        public void DeleteTask(int taskId)
        {

        }

        public void UpdateTask(TaskItem task)
        {

        }
    }
}
