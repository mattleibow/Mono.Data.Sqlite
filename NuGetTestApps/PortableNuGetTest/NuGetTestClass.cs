using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Mono.Data.Sqlite;

namespace PortableNuGetTest
{
    public class NuGetTestClass
    {
		public static void RunTests(string dbPath)
	    {
			using (var cnn = new SqliteConnection("Data Source=" + dbPath))
			{
				cnn.Open();

				// commit
				using (var trn = cnn.BeginTransaction(IsolationLevel.Serializable))
				using (var cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS nugettest (id INTEGER PRIMARY KEY AUTOINCREMENT, data TEXT);", cnn))
				{
					cmd.ExecuteNonQuery();

					cmd.CommandText = "DELETE FROM nugettest;";
					cmd.ExecuteNonQuery();

					cmd.CommandText = "INSERT INTO nugettest (data) VALUES (@someData);";
					cmd.Parameters.AddWithValue("@someData", "data here");

					cmd.ExecuteNonQuery();

					trn.Commit();
				}

				// rollback
				using (var trn = cnn.BeginTransaction(IsolationLevel.Serializable))
				using (var cmd = new SqliteCommand("INSERT INTO nugettest (data) VALUES (@someData);", cnn))
				{
					cmd.Parameters.AddWithValue("@someData", "data here");
					cmd.ExecuteNonQuery();

					trn.Rollback();
				}

				// check
				using (var cmd = new SqliteCommand("SELECT COUNT(*) nugettest;", cnn))
				{
					if (Convert.ToInt32(cmd.ExecuteScalar()) != 1)
					{
						throw new Exception("Something bad happened!");
					}
				}
			}
	    }
    }
}
