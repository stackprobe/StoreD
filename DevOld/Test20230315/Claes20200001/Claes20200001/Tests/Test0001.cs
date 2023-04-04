using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using Charlotte.Commons;

namespace Charlotte.Tests
{
	public class Test0001
	{
		public void Test01()
		{
			using (WorkingDir wd = new WorkingDir())
			{
				Console.WriteLine(wd.MakePath());
				Console.WriteLine(wd.MakePath());
				Console.WriteLine(wd.MakePath());
			}

			using (WorkingDir wd = new WorkingDir())
			{
				Console.WriteLine(wd.MakePath());
				Console.WriteLine(wd.MakePath());
				Console.WriteLine(wd.MakePath());
			}

			using (WorkingDir wd = new WorkingDir())
			{
				Console.WriteLine(wd.MakePath());
				Console.WriteLine(wd.MakePath());
				Console.WriteLine(wd.MakePath());
			}
		}

		public void Test02()
		{
			using (WorkingDir wd = new WorkingDir())
			{
				for (int c = 0; c < 1001; c++)
				{
					string file = wd.GetPath("テキストファイル.txt");

					file = SCommon.ToCreatablePath(file);
					File.WriteAllText(file, "テキスト_テキスト_テキスト", Encoding.UTF8);

					Console.WriteLine(file);
				}
			}
		}

		public void Test03()
		{
			Test03_a("AAABBBCCC", 'B', 2, 6);
			Test03_a("BBBBBBBB", 'B', -1, 8);
			Test03_a("ABBBBBC", 'B', 0, 6);
			Test03_a("AAACCC", 'B', 2, 3);
			Test03_a("AABCC", 'B', 1, 3);
			Test03_a("AAAA", 'B', 3, 4);
			Test03_a("CCC", 'B', -1, 0);
			Test03_a("AB", 'B', 0, 2);
			Test03_a("B", 'B', -1, 1);
			Test03_a("", 'B', -1, 0);

			Console.WriteLine("OK!");
		}

		private void Test03_a(string str, char target, int expectRange_L, int expectRange_R)
		{
			int[] range = SCommon.GetRange(str.ToCharArray(), target, (a, b) => (int)a - (int)b);

			Console.WriteLine(string.Join(", ", range[0], range[1], expectRange_L, expectRange_R)); // cout

			if (
				range[0] != expectRange_L ||
				range[1] != expectRange_R
				)
				throw null;

			Console.WriteLine("OK");
		}

		public void Test04()
		{
			Test04_a("AAAABCCCC", 'B', 4);
			Test04_a("AAAAAAAB", 'B', 7);
			Test04_a("BCCCCCC", 'B', 0);
			Test04_a("AAACCC", 'B', -1);
			Test04_a("AABCC", 'B', 2);
			Test04_a("CCCC", 'B', -1);
			Test04_a("AAA", 'B', -1);
			Test04_a("AB", 'B', 1);
			Test04_a("B", 'B', 0);
			Test04_a("", 'B', -1);
		}

		private void Test04_a(string str, char target, int expect)
		{
			int ret = SCommon.GetIndex(str.ToCharArray(), target, (a, b) => (int)a - (int)b);

			Console.WriteLine(string.Join(", ", ret, expect)); // cout

			if (ret != expect)
				throw null;

			Console.WriteLine("OK");
		}

		public void Test05()
		{
			Console.WriteLine(SCommon.ASCII);
			Console.WriteLine(SCommon.KANA);
			Console.WriteLine(SCommon.HALF);

			// ----

			ShowRange(SCommon.ENCODING_SJIS.GetBytes(SCommon.ASCII));
			ShowRange(SCommon.ENCODING_SJIS.GetBytes(SCommon.KANA));
			ShowRange(SCommon.ENCODING_SJIS.GetBytes(SCommon.HALF));
		}

		private void ShowRange(byte[] data)
		{
			int[][] ranges = data.Select(v => new int[] { (int)v, (int)v }).ToArray();

			for (int c = 0; c < 100; c++) // rough limit
			{
				for (int index = 0; index + 1 < ranges.Length; index += 2)
				{
					if (ranges[index][1] + 1 == ranges[index + 1][0])
					{
						ranges[index][1] = ranges[index + 1][1];
						ranges[index + 1] = null;
					}
				}
				ranges = ranges.Where(v => v != null).ToArray();
			}
			Console.WriteLine(string.Join(" and ", ranges.Select(v => string.Format("from 0x{0:x2} to 0x{1:x2}", v[0], v[1]))));
		}

		public void Test06()
		{
			//SCommon.Batch(new string[] { "TIMEOUT 15" });
			SCommon.Batch(new string[] { "START *Error_123_" });
		}

		public void Test07()
		{
			const string EXPECT_MBC_DECIMAL = "０１２３４５６７８９";
			const string EXPECT_MBC_ALPHA_UPPER = "ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ";
			const string EXPECT_MBC_ALPHA_LOWER = "ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ";

			if (EXPECT_MBC_DECIMAL != SCommon.MBC_DECIMAL) throw null;
			if (EXPECT_MBC_ALPHA_UPPER != SCommon.MBC_ALPHA_UPPER) throw null;
			if (EXPECT_MBC_ALPHA_LOWER != SCommon.MBC_ALPHA_LOWER) throw null;

			// ----

			if (SCommon.DECIMAL != SCommon.ToAsciiHalf(EXPECT_MBC_DECIMAL)) throw null;
			if (SCommon.ALPHA_UPPER != SCommon.ToAsciiHalf(EXPECT_MBC_ALPHA_UPPER)) throw null;
			if (SCommon.ALPHA_LOWER != SCommon.ToAsciiHalf(EXPECT_MBC_ALPHA_LOWER)) throw null;

			// - - -

			if (SCommon.HEXADECIMAL_UPPER != SCommon.ToAsciiHalf(SCommon.MBC_HEXADECIMAL_UPPER)) throw null;
			if (SCommon.HEXADECIMAL_LOWER != SCommon.ToAsciiHalf(SCommon.MBC_HEXADECIMAL_LOWER)) throw null;
			if (SCommon.ASCII != SCommon.ToAsciiHalf(SCommon.MBC_ASCII)) throw null;

			// ----

			Console.WriteLine("OK!");
		}

		public void Test08()
		{
			List<int[]> ranges = new List<int[]>();

			foreach (char chr in SCommon.GetJChars())
			{
				ranges.Add(new int[] { (int)chr, (int)chr });
			}
			ranges = ranges.DistinctOrderBy((a, b) => a[0] - b[0]).ToList();

			for (int index = ranges.Count - 2; 0 <= index; index--)
			{
				if (ranges[index][1] + 1 == ranges[index + 1][0])
				{
					ranges[index][1] = ranges[index + 1][1];
					ranges[index + 1] = null;
				}
			}
			ranges.RemoveAll(v => v == null);

			Console.WriteLine(string.Join(" and ", ranges.Select(v => string.Format("from 0x{0:x4} to 0x{1:x4}", v[0], v[1]))));

			// ----

			List<string> lines = new List<string>();

			for (int a = 0; a < 256; a++)
			{
				StringBuilder buff = new StringBuilder();

				for (int b = 0; b < 256; b++)
				{
					int chr = a * 256 + b;

#if true
					bool flag = SCommon.GetIndex(ranges, range =>
					{
						//if (range[0] <= chr && chr <= range[1])
						//return 0;

						if (chr < range[0])
							return 1;

						if (range[1] < chr)
							return -1;

						return 0;
					})
					!= -1;
#elif true
					bool flag = ranges.Any(v => v[0] <= chr && chr <= v[1]);
#else
					bool flag = SCommon.GetJChars().Contains((char)chr);
#endif

					buff.Append(flag ? '#' : '-');
				}
				lines.Add(buff.ToString());
			}
			File.WriteAllLines(SCommon.NextOutputPath() + ".txt", lines, Encoding.ASCII);
		}
	}
}
