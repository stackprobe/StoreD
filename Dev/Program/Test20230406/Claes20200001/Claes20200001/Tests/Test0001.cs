using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Charlotte.Commons;

namespace Charlotte.Tests
{
	public class Test0001
	{
		public void Test01()
		{
			int c = 10;
			Action a = () => Console.WriteLine(c);
			c++;
			a(); // 11
			c++;
			a(); // 12
			c++;
			a(); // 13
		}

		public void Test02()
		{
			Action a;

			{
				int c = 10;
				a = () => Console.WriteLine(c);
				c++;
			}

			a(); // 11
		}

		public void Test03()
		{
			Action a;

			{
				string s = "ABC";
				a = () => Console.WriteLine(s);
				s = "DEF";
			}

			a(); // DEF
		}

		public void Test04()
		{
			Action a;

			{
				int c = 10;
				a = () => Console.WriteLine(c);
				a(); // 10
				c++;
				a(); // 11
				c++;
			}

			a(); // 12

			{
				string s = "ABC";
				a = () => Console.WriteLine(s);
				a(); // ABC
				s = "DEF";
				a(); // DEF
				s = "GHI";
			}

			a(); // GHI
		}
	}
}
