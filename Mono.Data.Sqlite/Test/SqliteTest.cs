//
// SqliteTest.cs - Test for the Sqlite ADO.NET Provider in Mono.Data.Sqlite
//                 This provider works on Linux and Windows and uses the native
//                 sqlite.dll or sqlite.so library.
//
// Modify or add to this test as needed...
//
// SQL Lite can be downloaded from
// http://www.hwaci.com/sw/sqlite/download.html
//
// There are binaries for Windows and Linux.
//
// To compile:
//  mcs SqliteTest.cs -r System.Data.dll -r Mono.Data.Sqlite.dll
//
// Author:
//     Daniel Morgan <danmorg@sc.rr.com>
//

using System;
using System.Data;
using Mono.Data.Sqlite;

#if SILVERLIGHT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Test.Mono.Data.Sqlite
{
    using System.IO;

    [TestClass]
    public class SqliteTest
    {
        [TestMethod]
        public void TestFalseNull()
        {
            Test(false, null);
        }

        [TestMethod]
        public void TestFalseIso()
        {
            Test(false, "ISO-8859-1");
        }

        [TestMethod]
        public void TestTrueNull()
        {
            Test(true, null);
        }

        public void Test(bool v3, string encoding)
        {
            if (!v3)
                System.Diagnostics.Debug.WriteLine("Testing Version 2" + (encoding != null ? " with " + encoding + " encoding" : ""));
            else
                System.Diagnostics.Debug.WriteLine("Testing Version 3");

            SqliteConnection dbcon = new SqliteConnection();

            // the connection string is a URL that points
            // to a file.  If the file does not exist, a 
            // file is created.

            // "URI=file:some/path"
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path,
                                    "SqliteTest" + Environment.TickCount + ".db");
            string connectionString = "URI=file://" + path;
            if (v3)
                connectionString += ",Version=3";
            if (encoding != null)
                connectionString += ",encoding=" + encoding;
            dbcon.ConnectionString = connectionString;

            dbcon.Open();

            SqliteCommand dbcmd = new SqliteCommand();
            dbcmd.Connection = dbcon;

            dbcmd.CommandText =
                "CREATE TABLE MONO_TEST ( " +
                "NID INT, " +
                "NDESC TEXT, " +
                "NTIME DATETIME); " +
                "INSERT INTO MONO_TEST  " +
                "(NID, NDESC, NTIME) " +
                "VALUES(1,'One (unicode test: \u05D0)', '2006-01-01')";
            System.Diagnostics.Debug.WriteLine("Create & insert modified rows = 1: " + dbcmd.ExecuteNonQuery());

            dbcmd.CommandText =
                "INSERT INTO MONO_TEST  " +
                "(NID, NDESC, NTIME) " +
                "VALUES(:NID,:NDESC,:NTIME)";
            dbcmd.Parameters.Add(new SqliteParameter("NID", 2));
            dbcmd.Parameters.Add(new SqliteParameter(":NDESC", "Two (unicode test: \u05D1)"));
            dbcmd.Parameters.Add(new SqliteParameter(":NTIME", DateTime.Now));
            System.Diagnostics.Debug.WriteLine("Insert modified rows with parameters = 1, 2: " + dbcmd.ExecuteNonQuery() + " , " + dbcmd.LastInsertRowID());

            dbcmd.CommandText =
                "INSERT INTO MONO_TEST  " +
                "(NID, NDESC, NTIME) " +
                "VALUES(3,'Three, quoted parameter test, and next is null; :NTIME', NULL)";
            System.Diagnostics.Debug.WriteLine("Insert with null modified rows and ID = 1, 3: " + dbcmd.ExecuteNonQuery() + " , " + dbcmd.LastInsertRowID());

            dbcmd.CommandText =
                "INSERT INTO MONO_TEST  " +
                "(NID, NDESC, NTIME) " +
                "VALUES(4,'Four with ANSI char: ü', NULL)";
            System.Diagnostics.Debug.WriteLine("Insert with ANSI char ü = 1, 4: " + dbcmd.ExecuteNonQuery() + " , " + dbcmd.LastInsertRowID());

            dbcmd.CommandText =
                "INSERT INTO MONO_TEST  " +
                "(NID, NDESC, NTIME) " +
                "VALUES(?,?,?)";
            dbcmd.Parameters.Clear();
            IDbDataParameter param1 = dbcmd.CreateParameter();
            param1.DbType = DbType.Int32;
            param1.Value = 5;
            dbcmd.Parameters.Add(param1);
            IDbDataParameter param2 = dbcmd.CreateParameter();
            param2.Value = "Using unnamed parameters";
            dbcmd.Parameters.Add(param2);
            IDbDataParameter param3 = dbcmd.CreateParameter();
            param3.DbType = DbType.DateTime;
            param3.Value = DateTime.Parse("2006-05-11 11:45:00");
            dbcmd.Parameters.Add(param3);
            System.Diagnostics.Debug.WriteLine("Insert with unnamed parameters = 1, 5: " + dbcmd.ExecuteNonQuery() + " , " + dbcmd.LastInsertRowID());

            dbcmd.CommandText =
                "SELECT * FROM MONO_TEST";
            using (var reader = dbcmd.ExecuteReader())
            {
                System.Diagnostics.Debug.WriteLine("read and display data...");
                while (reader.Read())
                    for (int i = 0; i < reader.FieldCount; i++)
                        System.Diagnostics.Debug.WriteLine(
                            " Col {0}: {1} (type: {2}, data type: {3})",
                            i,
                            reader[i] == null ? "(null)" : reader[i].ToString(),
                            reader[i] == null ? "(null)" : reader[i].GetType().FullName,
                            reader.GetDataTypeName(i));
            }
            dbcmd.CommandText = "SELECT NDESC FROM MONO_TEST WHERE NID=2";
            System.Diagnostics.Debug.WriteLine("read and display a scalar = 'Two': " + dbcmd.ExecuteScalar());

            dbcmd.CommandText = "SELECT count(*) FROM MONO_TEST";
            System.Diagnostics.Debug.WriteLine("read and display a non-column scalar = 3: " + dbcmd.ExecuteScalar());

            try
            {
                dbcmd.CommandText = "SELECT NDESC INVALID SYNTAX FROM MONO_TEST WHERE NID=2";
                dbcmd.ExecuteNonQuery();
                Assert.Fail("Should not reach here.");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Testing a syntax error: " + e.GetType().Name + ": " + e.Message);
            }

            dbcmd.Dispose();
            dbcon.Close();
        }
    }

    public static class SqliteCommandExtensions
    {
        public static string LastInsertRowID(this SqliteCommand command)
        {
            using (var lastInsertRowId = new SqliteCommand("SELECT last_insert_rowid();", command.Connection, command.Transaction))
                return lastInsertRowId.ExecuteScalar().ToString();
        }
    }
}