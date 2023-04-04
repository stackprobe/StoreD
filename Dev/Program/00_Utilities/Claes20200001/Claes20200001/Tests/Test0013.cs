using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;
using Charlotte.Utilities;

namespace Charlotte.Tests
{
	/// <summary>
	/// CtrCipher.cs テスト
	/// </summary>
	public class Test0013
	{
		public void Test01()
		{
			Test01_a(30000, 100);
			Test01_a(10000, 300);
			Test01_a(3000, 1000);
			Test01_a(1000, 3000);
			Test01_a(300, 10000);
			Test01_a(100, 30000);

			Console.WriteLine("OK!");
		}

		private void Test01_a(int testCount, int scale)
		{
			for (int testcnt = 0; testcnt < testCount; testcnt++)
			{
				byte[] testData = SCommon.CRandom.GetBytes(SCommon.CRandom.GetInt(scale));
				int rawKeySize = SCommon.CRandom.ChooseOne(new int[] { 16, 24, 32 });
				byte[] rawKey = SCommon.CRandom.GetBytes(rawKeySize);
				byte[] iv = SCommon.CRandom.GetBytes(16);

				using (CtrCipher cc = new CtrCipher(rawKey, iv))
				{
					byte[] workData = testData.ToArray(); // 複製

					if (SCommon.Comp(testData, workData) != 0) // 2bs
						throw null;

					cc.Mask(workData);

					if (SCommon.Comp(testData, workData) == 0
						&& 20 < testData.Length) // 極端に短ければ同じこともある。
						throw null;

					cc.Reset();
					cc.Mask(workData);

					if (SCommon.Comp(testData, workData) != 0)
						throw null;
				}

				{
					byte[] workData = testData.ToArray(); // 複製

					if (SCommon.Comp(testData, workData) != 0) // 2bs
						throw null;

					using (CtrCipher cc = new CtrCipher(rawKey, iv))
					{
						cc.Mask(workData);
					}

					if (SCommon.Comp(testData, workData) == 0
						&& 20 < testData.Length) // 極端に短ければ同じこともある。
						throw null;

					using (CtrCipher cc = new CtrCipher(rawKey, iv))
					{
						cc.Mask(workData);
					}

					if (SCommon.Comp(testData, workData) != 0)
						throw null;
				}
			}
			Console.WriteLine("OK");
		}
	}
}
