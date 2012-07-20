using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestRunner
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                IsolatedStorageFile.GetUserStoreForApplication().Remove();
                IsolatedStorageFile.GetUserStoreForApplication().IncreaseQuotaTo(100 * 1024 * 1024);
            }
            catch
            {
            }

            ThreadPool.QueueUserWorkItem(RunTests);
        }

        private void RunTests(object state)
        {
            Type testAssembly = typeof(TestAssembly);

            Type[] types = testAssembly.Assembly.GetTypes();

            IEnumerable<Type> testFixtures = types.Where(x => x.GetCustomAttributes(typeof(TestClassAttribute), true).Any());
            foreach (Type testFixture in testFixtures)
            {
                object theTestFixture = Activator.CreateInstance(testFixture);

                RunTestInit(testFixture, theTestFixture, typeof(ClassInitializeAttribute),  (TestContext)null );

                IEnumerable<MethodInfo> tests = testFixture.GetMethods().Where(x => x.GetCustomAttributes(typeof(TestMethodAttribute), true).Any());

                foreach (MethodInfo test in tests)
                {
                    RunTestInit(testFixture, theTestFixture, typeof(TestInitializeAttribute));

                    RunTest(testFixture, test, theTestFixture);
                }
            }

            Dispatcher.BeginInvoke(() => label1.Text = "Complete!");
        }

        private void RunTestInit(Type testFixture, object theTestFixture, Type attributeType, params object[] p)
        {
            var setup = testFixture.GetMethods().FirstOrDefault(x => x.GetCustomAttributes(attributeType, true).Any());
            if (setup != null)
            {
                Dispatcher.BeginInvoke(() => label1.Text = string.Format("Setup: {0}.{1}", testFixture.Name, setup.Name));
                string testName = testFixture.Name + "." + setup.Name;
                try
                {
                    DateTime past = DateTime.Now;
                    setup.Invoke(theTestFixture, p);

                    var message = string.Format("SETUP: {0} - pass: {1}", testName, (DateTime.Now - past).TotalMilliseconds);
                    Dispatcher.BeginInvoke(() => listBox1.Items.Add(testFixture.Name + "." + setup.Name + message));
                }
                catch (Exception ex)
                {
                    var message = string.Format("SETUP: {0} - fail: {1}", testName, ex.InnerException.Message);
                    Dispatcher.BeginInvoke(() => listBox2.Items.Add(testFixture.Name + "." + setup.Name + message));
                }
            }
        }

        private void RunTest(Type fixture, MethodInfo test1, object theTestFixture)
        {
            Dispatcher.BeginInvoke(() => label1.Text = string.Format("Testing: {0}.{1}", fixture.Name, test1.Name));

            string testName = fixture.Name + "." + test1.Name;
            try
            {
                DateTime past = DateTime.Now;
                test1.Invoke(theTestFixture, null);

                var message = string.Format("{0} - pass: {1}", testName, (DateTime.Now - past).TotalMilliseconds);
                Dispatcher.BeginInvoke(() => listBox1.Items.Add(fixture.Name + "." + test1.Name + message));
            }
            catch (Exception ex)
            {
                var message = string.Format("{0} - fail: {1}", testName, ex.InnerException.Message);
                Dispatcher.BeginInvoke(() => listBox2.Items.Add(fixture.Name + "." + test1.Name + message));
            }
        }
    }
}
