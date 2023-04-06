using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charlotte.Tests
{
	public class Test0002
	{
		public void Test01()
		{
			Test01_a(0.0);
			Test01_a(0.1);
			Test01_a(0.5);
			Test01_a(0.25);
			Test01_a(0.125);
			Test01_a(1.0 / 3.0);
			Test01_a(1.0 / 6.0);
			Test01_a(1.0 / 7.0);
			Test01_a(1.0 / 9.0);
			Test01_a(Math.PI);
			Test01_a(Math.Sqrt(2.0));
		}

		private void Test01_a(double value)
		{
			Console.WriteLine(value);
			Console.WriteLine(value.ToString("R"));
		}
	}
}
