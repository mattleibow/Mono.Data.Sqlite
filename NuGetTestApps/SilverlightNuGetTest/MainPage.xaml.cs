using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using PortableNuGetTest;

namespace SilverlightNuGetTest
{
	public partial class MainPage : UserControl
	{
		public MainPage()
		{
			InitializeComponent();

			NuGetTestClass.RunTests("test.db");

		}
	}
}
