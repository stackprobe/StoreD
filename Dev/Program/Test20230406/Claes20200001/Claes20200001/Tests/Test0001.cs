using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Charlotte.Commons;
using System.Threading;

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

		public void Test05()
		{
			for (DateTime dt = DateTime.Now; dt.Second == DateTime.Now.Second; )
				Thread.Sleep(1);

			DateTime stDt = DateTime.Now;

			for (long count = 0L; ; count++)
			{
				Console.WriteLine(string.Join(", ", count, (DateTime.Now - stDt).TotalSeconds));
				Thread.Sleep(1000);
			}
		}

		public void Test06()
		{
			for (DateTime dt = DateTime.Now; dt.Second == DateTime.Now.Second; )
				Thread.Sleep(1);

			DateTime stDt = DateTime.Now;
			int millis = 1000;

			for (long count = 0L; ; count++)
			{
				Console.WriteLine(string.Join(", ", count, (DateTime.Now - stDt).TotalSeconds.ToString("F9"), millis));

				if (count < (DateTime.Now - stDt).TotalSeconds)
					millis--;
				else
					millis++;

				Thread.Sleep(millis);
			}
		}

		public void Test07()
		{
			for (DateTime dt = DateTime.Now; dt.Second == DateTime.Now.Second; )
				Thread.Sleep(1);

			DateTime stDt = DateTime.Now;
			int millis = 1000;

			for (long count = 0L; ; count++)
			{
				Console.WriteLine(string.Join(", ", count, (DateTime.Now - stDt).TotalSeconds.ToString("F9"), millis));

				if (count < (DateTime.Now - stDt).TotalSeconds)
				{
					if (millis < 1000)
						millis--;
					else
						millis -= 2;
				}
				else
				{
					if (1000 < millis)
						millis++;
					else
						millis += 2;
				}

				Thread.Sleep(millis);
			}
		}

		public void Test08()
		{
			for (DateTime dt = DateTime.Now; dt.Second == DateTime.Now.Second; )
				Thread.Sleep(1);

			DateTime stDt = DateTime.Now;
			int millis = 1000;

			for (long count = 0L; ; count++)
			{
				Console.WriteLine(string.Join(", ", count, (DateTime.Now - stDt).TotalSeconds.ToString("F9"), millis));

				if (count < (DateTime.Now - stDt).TotalSeconds)
				{
					if (millis <= 1000)
						millis--;
					else
						millis = 1000;
				}
				else
				{
					if (1000 <= millis)
						millis++;
					else
						millis = 1000;
				}

				Thread.Sleep(millis);
			}
		}

		public void Test09()
		{
			for (DateTime dt = DateTime.Now; dt.Second == DateTime.Now.Second; )
				Thread.Sleep(1);

			DateTime stDt = DateTime.Now;
			int millis = -1;

			for (long count = 0L; ; count++)
			{
				Console.WriteLine(string.Join(", ", count, (DateTime.Now - stDt).TotalSeconds.ToString("F9"), millis));

				millis = (int)(((count + 1L) - (DateTime.Now - stDt).TotalSeconds) * 1000.0);
				millis = Math.Max(1, millis);

				Thread.Sleep(millis);
			}
		}
	}
}
