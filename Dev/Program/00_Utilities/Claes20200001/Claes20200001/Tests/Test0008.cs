using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;
using Charlotte.Utilities;

namespace Charlotte.Tests
{
	/// <summary>
	/// StringSpliceSequencer.cs テスト
	/// </summary>
	public class Test0008
	{
		public void Test01()
		{
			Test01_a(10, 1000);
			Test01_a(100, 100);
			Test01_a(1000, 10);

			Console.WriteLine("OK!");
		}

		public void Test01_a(int testDataScale, int testCount)
		{
			for (int testcnt = 0; testcnt < testCount; testcnt++)
			{
				Test01_b(testDataScale);
			}
			Console.WriteLine("OK");
		}

		private void Test01_b(int testDataScale)
		{
			string[] words = new string[]
			{
				"Sio",
				"Miso",
				"Syoyu",
				"Hakata",
				"Sapporo",
				"Kitakata",
			};

			string src = string.Join("", Enumerable.Range(0, SCommon.CRandom.GetInt(testDataScale)).Select(dummy => SCommon.CRandom.ChooseOne(words)));
			string word1 = SCommon.CRandom.ChooseOne(words);
			string word2 = SCommon.CRandom.ChooseOne(words);

			string ans1 = src.Replace(word1, word2);
			string ans2;

			{
				StringSpliceSequencer sss = new StringSpliceSequencer(src);
				int index = 0;

				for (; ; )
				{
					index = src.IndexOf(word1, index);

					if (index == -1)
						break;

					sss.Splice(index, word1.Length, word2);
					index += word1.Length;
				}
				ans2 = sss.GetString();
			}

			if (ans1 != ans2) // ? 不一致
				throw null;
		}

		public void Test02()
		{
			Test02_a(100, 10000);
			Test02_a(300, 3000);
			Test02_a(1000, 1000);
			Test02_a(3000, 300);
			Test02_a(10000, 100);

			Console.WriteLine("OK!");
		}

		public void Test02_a(int testDataScale, int testCount)
		{
			for (int testcnt = 0; testcnt < testCount; testcnt++)
			{
				Test02_b(testDataScale, testDataScale);
				Test02_b(testDataScale, testDataScale / 3);
				Test02_b(testDataScale, testDataScale / 10);
			}
			Console.WriteLine("OK");
		}

		private void Test02_b(int testDataScale, int subTestDataScale)
		{
			if (subTestDataScale < 2) throw null; // 2bs

			char[] TEST_CHARS = SCommon.HALF.ToArray();
			string src = new string(Enumerable
				.Range(0, SCommon.CRandom.GetInt(testDataScale))
				.Select(dummy => SCommon.CRandom.ChooseOne(TEST_CHARS))
				.ToArray());

			string ans1 = src;
			string ans2;

			int index1 = 0;
			int index2 = 0;

			StringSpliceSequencer sss = new StringSpliceSequencer(src);

			for (; ; )
			{
				int span = SCommon.CRandom.GetInt(subTestDataScale);

				index1 += span;
				index2 += span;

				int removeLength = SCommon.CRandom.GetInt(subTestDataScale);

				if (ans1.Length < index1 + removeLength)
					break;

				string newPart = new string(Enumerable
					.Range(0, SCommon.CRandom.GetInt(subTestDataScale))
					.Select(dummy => SCommon.CRandom.ChooseOne(TEST_CHARS))
					.ToArray());

				ans1 = ans1.Substring(0, index1) + newPart + ans1.Substring(index1 + removeLength);
				sss.Splice(index2, removeLength, newPart);

				index1 += newPart.Length;
				index2 += removeLength;
			}
			ans2 = sss.GetString();

			if (ans1 != ans2) // ? 不一致
				throw null;
		}
	}
}
