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
	/// HTTPClient.cs テスト
	/// </summary>
	public class Test0004
	{
		public void Test01()
		{
			Test01a("https://www.google.com");
			Test01a("https://www.youtube.com");
			Test01a("https://www.amazon.co.jp");
		}

		/// <summary>
		/// ★クライアント実行テンプレート
		/// </summary>
		/// <param name="url"></param>
		private void Test01a(string url)
		{
			using (WorkingDir wd = new WorkingDir())
			{
				string resFile = wd.MakePath();

				HTTPClient hc = new HTTPClient(url)
				{
					ConnectTimeoutMillis = 60000, // 1 min
					TimeoutMillis = 86400000, // 1 day
					IdleTimeoutMillis = 180000, // 3 min
					ResBodySizeMax = 100000000000000, // 100 TB
					ResFile = resFile,
				};

				hc.Get();

				File.Copy(resFile, SCommon.NextOutputPath() + ".txt"); // ★サンプル -- 要削除
			}
		}
	}
}
