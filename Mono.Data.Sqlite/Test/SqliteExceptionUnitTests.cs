// SqliteExceptionUnitTests.cs - NUnit Test Cases for Mono.Data.Sqlite.SqliteExceptions
//
// Author(s):	Thomas Zoechling <thomas.zoechling@gmx.at>


using System;
using System.Data;
using System.IO;
using System.Text;
using Mono.Data.Sqlite;

#if SILVERLIGHT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace MonoTests.Mono.Data.Sqlite
{
    [TestClass]
    public class SqliteExceptionUnitTests
    {
        readonly static string dbRootPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        readonly static string _uri = Path.Combine(dbRootPath, "test.db");
        readonly static string _connectionString = "URI=file://" + _uri + ", version=3";
        static SqliteConnection _conn = new SqliteConnection(_connectionString);

        public SqliteExceptionUnitTests()
        {
        }

        [TestMethod]
        public void WrongSyntax()
        {
            SqliteCommand insertCmd = new SqliteCommand("INSERT INTO t1 VALUES (,')", _conn);
            using (_conn)
            {
                _conn.Open();
                try
                {
                    int res = insertCmd.ExecuteNonQuery();
                    Assert.AreEqual(res, 1);
                    Assert.Fail();
                }
                catch (SqliteException)
                {
                }
                catch (Exception)
                {
                    Assert.Fail();
                }
            }
        }
    }
}