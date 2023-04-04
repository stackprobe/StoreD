using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;
using Charlotte.Utilities;

namespace Charlotte.Tests
{
	/// <summary>
	/// BitList.cs テスト
	/// </summary>
	public class Test0007
	{
		public void Test01()
		{
			BitList bits = new BitList();

			SCommon.ToThrowPrint(() =>
			{
				bits[-1L].ToString();
			});

			bits[long.MaxValue].ToString(); // 未定義領域は false を返す。

			SCommon.ToThrowPrint(() =>
			{
				bits[-1L] = true;
			});

			SCommon.ToThrowPrint(() =>
			{
				bits[long.MaxValue] = true;
			});
		}
	}
}
