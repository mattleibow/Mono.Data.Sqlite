using System;

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

using PortableDataAccess;

namespace WindowsPhoneApp
{
    [TestClass]
    public class TaskItemRepositoryUnitTest
    {
        private const string connectionString = "Data Source=tasks.db;FailIfMissing=false;";

        [TestMethod]
        public void EnsureRepositoryCanBeCreated()
        {
            var repo = new TaskItemRepository(connectionString);
        }

        [TestMethod]
        public void EnsureRepositoryCanBeLoaded()
        {
            var repo = new TaskItemRepository(connectionString);
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
