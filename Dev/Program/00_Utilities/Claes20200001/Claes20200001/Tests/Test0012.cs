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
	/// XMLNode.cs テスト
	/// </summary>
	public class Test0012
	{
		public void Test01()
		{
			using (WorkingDir wd = new WorkingDir())
			{
				string file = wd.MakePath();

				File.WriteAllText(file, "<Root><Parent AttributeTest=\"AttributeValueTest\">TEST</Parent></Root>");

				XMLNode root = XMLNode.LoadFromFile(file); // 読み込み

				if (root.Name != "Root") throw null;
				if (root.Value != "") throw null;
				if (root.Children.Count != 1) throw null;
				if (root.Children[0].Name != "Parent") throw null;
				if (root.Children[0].Value != "TEST") throw null;
				if (root.Children[0].Children.Count != 1) throw null;
				if (root.Children[0].Children[0].Name != "AttributeTest") throw null;
				if (root.Children[0].Children[0].Value != "AttributeValueTest") throw null;

				root.WriteToFile(file); // 出力
				Console.WriteLine("出力した内容：" + File.ReadAllText(file, Encoding.UTF8));
				root = XMLNode.LoadFromFile(file); // 再読み込み

				if (root.Name != "Root") throw null;
				if (root.Value != "") throw null;
				if (root.Children.Count != 1) throw null;
				if (root.Children[0].Name != "Parent") throw null;
				if (root.Children[0].Value != "TEST") throw null;
				if (root.Children[0].Children.Count != 1) throw null;
				if (root.Children[0].Children[0].Name != "AttributeTest") throw null;
				if (root.Children[0].Children[0].Value != "AttributeValueTest") throw null;

				List<string> xmlPaths = new List<string>();

				root.Search((node, xmlPath) =>
				{
					xmlPaths.Add(xmlPath);
					return true;
				});

				if (xmlPaths.Count != 3) throw null;
				if (xmlPaths[0] != "Root") throw null;
				if (xmlPaths[1] != "Root/Parent") throw null;
				if (xmlPaths[2] != "Root/Parent/AttributeTest") throw null;
			}
			Console.WriteLine("OK!");
		}
	}
}
