using System;
using System.Transactions;

#if WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#elif __ANDROID__
using TestClassAttribute = NUnit.Framework.TestFixtureAttribute;
using TestMethodAttribute = NUnit.Framework.TestAttribute;
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

using PortableDataAccess;

namespace WindowsPhoneApp
{
    [TestClass]
    public class TaskItemRepositoryUnitTest
    {
        private static string connectionString
        {
            get
            {
#if __ANDROID__
                var dbPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "tasks.db");
#else
                var dbPath = "tasks.db";
#endif
                return "Data Source=" + dbPath + ";FailIfMissing=false;";
            }
        }

        [TestMethod]
        public void EnsureRepositoryCanBeCreated()
        {
            var repo = new TaskItemRepository(connectionString);
        }

        [TestMethod]
        public void EnsureRepositoryCanBeLoaded()
        {
            var repo = new TaskItemRepository(connectionString);
            var transactionOptions = new TransactionOptions {IsolationLevel = IsolationLevel.Serializable};
            var tasks = repo.GetAllTasks();
        }

        [TestMethod]
        public void EnsureTaskCanBeAdded()
        {
            var name = new Guid().ToString("N");
            var task = new TaskItem(name);
            var repo = new TaskItemRepository(connectionString);
            var newId = repo.AddTask(task);
            Assert.IsTrue(newId > 0);
        }

        [TestMethod]
        public void EnsureTaskIsCorrectlyAdded()
        {
            var name = new Guid().ToString("N");
            var task = new TaskItem(name);

            var repo = new TaskItemRepository(connectionString);
            var newId = repo.AddTask(task);

            var loadedTask = repo.GetTask(newId);

            Assert.IsNotNull(loadedTask);
            Assert.AreEqual(newId, loadedTask.Id);
            Assert.AreEqual(name, loadedTask.Name);
            Assert.AreEqual(false, loadedTask.IsComplete);
        }
    }
}
