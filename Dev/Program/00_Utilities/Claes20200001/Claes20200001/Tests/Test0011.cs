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
	/// JsonNode.cs テスト
	/// </summary>
	public class Test0011
	{
		public void Test01()
		{
			Test01_a(Encoding.UTF8);
			Test01_a(Encoding.Unicode);
			Test01_a(Encoding.UTF32);

			Console.WriteLine("OK!");
		}

		private void Test01_a(Encoding encoding)
		{
			using (WorkingDir wd = new WorkingDir())
			{
				string file = wd.MakePath();

				File.WriteAllText(file, "[[[ 'TEST' ]]]", encoding);

				JsonNode root = JsonNode.LoadFromFile(file);

				if (root.Array[0].Array[0].Array[0].StringValue != "TEST")
					throw null;
			}
		}
	}
}
