using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using PortableNuGetTest;

namespace ConsoleNuGetTest
{
	class Program
	{
		static void Main(string[] args)
		{
			var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "test.db");
			NuGetTestClass.RunTests(dbPath);
		}
	}
}
