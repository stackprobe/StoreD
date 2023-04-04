using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Charlotte.Commons;
using Charlotte.Utilities;

namespace Charlotte.Tests
{
	/// <summary>
	/// JapaneseDate.cs テスト
	/// </summary>
	public class Test0006
	{
		public void Test01()
		{
			string file = SCommon.NextOutputPath() + ".txt";

			using (StreamWriter writer = new StreamWriter(file, false, Encoding.UTF8))
			{
				for (int y = 1; y <= 9999; y++)
				{
					if (y % 100 == 0) Console.WriteLine(y); // cout

					for (int m = 1; m <= 12; m++)
					{
						for (int d = 1; d <= 31; d++)
						{
							JapaneseDate date = new JapaneseDate(y * 10000 + m * 100 + d);

							writer.WriteLine(string.Format("{0:D4}/{1:D2}/{2:D2} ⇒ {3}"
								, y
								, m
								, d
								, date.ToString()));
						}
					}
				}
			}
		}

		public void Test02()
		{
			Test02_a(int.MaxValue);
			Test02_a(int.MaxValue - 1);
			Test02_a(int.MaxValue - 2);
			Test02_a(int.MaxValue - 3);
			Test02_a(0);
			Test02_a(1);
			Test02_a(2);
			Test02_a(3);
		}

		private void Test02_a(int ymd)
		{
			Console.WriteLine(new JapaneseDate(ymd));

			// ----

			{
				JapaneseDate date = new JapaneseDate(ymd);
				string str = date.ToString();
				JapaneseDate date2 = JapaneseDate.Create(str);

				if (date2.GetYMD() != ymd)
					throw null; // bug !!!
			}
		}

		public void Test03()
		{
			for (int y = 1; y <= 9999; y++)
			{
				if (y % 100 == 0) Console.WriteLine(y); // cout

				for (int m = 1; m <= 12; m++)
				{
					for (int d = 1; d <= 31; d++)
					{
						JapaneseDate date = new JapaneseDate(y * 10000 + m * 100 + d);
						string str = date.ToString();
						JapaneseDate date2 = JapaneseDate.Create(str);

						if (
							date2.Y != y ||
							date2.M != m ||
							date2.D != d
							)
							throw null; // bug !!!
					}
				}
			}
		}
	}
}
