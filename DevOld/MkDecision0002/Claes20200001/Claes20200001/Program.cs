using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Charlotte.Commons;
using Charlotte.Tests;
using Charlotte.Utilities;

namespace Charlotte
{
	class Program
	{
		static void Main(string[] args)
		{
			ProcMain.CUIMain(new Program().Main2);
		}

		private void Main2(ArgsReader ar)
		{
			if (ProcMain.DEBUG)
			{
				Main3();
			}
			else
			{
				Main4(ar);
			}
			SCommon.OpenOutputDirIfCreated();
		}

		private void Main3()
		{
			// -- choose one --

			Main4(new ArgsReader(new string[] { @"C:\temp\Input.csv", @"C:\temp\Output.csv" }));
			//new Test0001().Test01();
			//new Test0002().Test01();
			//new Test0003().Test01();

			// --

			SCommon.Pause();
		}

		private void Main4(ArgsReader ar)
		{
			try
			{
				Main5(ar);
			}
			catch (Exception ex)
			{
				ProcMain.WriteLog(ex);

				//MessageBox.Show("" + ex, Path.GetFileNameWithoutExtension(ProcMain.SelfFile) + " / エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

				//Console.WriteLine("Press ENTER key. (エラーによりプログラムを終了します)");
				//Console.ReadLine();
			}
		}

		private class DecisionValueInfo
		{
			public string StrValue;
			public bool[] Row;
		}

		private class DecisionInfo
		{
			public string Name;
			public DecisionValueInfo[] Values;
		}

		private void Main5(ArgsReader ar)
		{
			string csvFile = SCommon.MakeFullPath(ar.NextArg());
			string destFile = SCommon.MakeFullPath(ar.NextArg());

			ar.End();

			if (!File.Exists(csvFile))
				throw new Exception("no csvFile");

			string[] csvRow;

			using (CsvFileReader reader = new CsvFileReader(csvFile))
			{
				csvRow = reader.ReadRow();
			}

			JsonNode[] cellJsons = csvRow
				.Select(x => JsonNode.Load(x))
				.ToArray();

			TreePath2Map = SCommon.CreateDictionary<List<JsonNode>>();

			foreach (JsonNode cellJson in cellJsons)
				JsonToKVList(cellJson);

			foreach (List<JsonNode> ms in TreePath2Map.Values)
			{
				List<string> names = new List<string>();

				foreach (JsonNode m in ms)
					foreach (JsonNode.Pair pair in m.Map)
						names.Add(pair.Name);

				names = names.DistinctOrderBy(SCommon.Comp).ToList();

				foreach (JsonNode m in ms)
				{
					foreach (string name in names)
					{
						if (!m.Map.Any(x => x.Name == name))
						{
							m.Map.Add(new JsonNode.Pair()
							{
								Name = name,
								Value = new JsonNode()
								{
									StringValue = Consts.VALUE_NO_KEY,
								},
							});
						}
					}
				}
			}

			TreePath2Map = null;

			List<string[][]> kvsList = new List<string[][]>();

			foreach (JsonNode cellJson in cellJsons)
				kvsList.Add(JsonToKVList(cellJson));

			DecisionInfo[] decisions = SCommon
				.Concat(kvsList)
				.Select(x => x[0])
				.DistinctOrderBy(SCommon.Comp)
				.Select(x => new DecisionInfo() { Name = x })
				.ToArray();

			foreach (DecisionInfo decision in decisions)
			{
				decision.Values = SCommon
					.Concat(kvsList)
					.Where(x => x[0] == decision.Name)
					.Select(x => x[1])
					.DistinctOrderBy(SCommon.Comp)
					.Select(x => new DecisionValueInfo() { StrValue = x, Row = new bool[kvsList.Count] })
					.ToArray();
			}
			for (int index = 0; index < kvsList.Count; index++)
			{
				string[][] kvs = kvsList[index];

				foreach (DecisionInfo decision in decisions)
				{
					foreach (DecisionValueInfo decisionValue in decision.Values)
					{
						decisionValue.Row[index] = kvs.Any(
							v => v[0] == decision.Name && v[1] == decisionValue.StrValue
							);
					}
				}
			}

			using (CsvFileWriter writer = new CsvFileWriter(destFile))
			{
				foreach (DecisionInfo decision in decisions)
				{
					writer.WriteCell(decision.Name);
					writer.EndRow();

					foreach (DecisionValueInfo decisionValue in decision.Values)
					{
						writer.WriteCell("");
						writer.WriteCell(decisionValue.StrValue);

						foreach (bool flag in decisionValue.Row)
							writer.WriteCell(flag ? "○" : "");

						writer.EndRow();
					}
				}
			}
		}

		private string[][] JsonToKVList(JsonNode root)
		{
			return JsonToKVList_Main(root, "").ToArray();
		}

		private Dictionary<string, List<JsonNode>> TreePath2Map = null;

		private IEnumerable<string[]> JsonToKVList_Main(JsonNode root, string treePath)
		{
			if (root.Map == null)
				throw new Exception("not Map (root element and array elements must be Map)");

			if (TreePath2Map != null) // add to TreePath2Map
			{
				if (!TreePath2Map.ContainsKey(treePath))
					TreePath2Map.Add(treePath, new List<JsonNode>());

				TreePath2Map[treePath].Add(root);
			}

			foreach (JsonNode.Pair pair in root.Map)
			{
				string name = treePath + pair.Name;
				string subTreePath = name + "/";
				JsonNode value = pair.Value;

				if (value.Array != null)
				{
					yield return new string[] { name, "List" };

					foreach (JsonNode element in value.Array)
						foreach (var relay in JsonToKVList_Main(element, subTreePath))
							yield return relay;
				}
				else if (value.Map != null)
				{
					yield return new string[] { name, "Map" };

					foreach (var relay in JsonToKVList_Main(value, subTreePath))
						yield return relay;
				}
				else if (value.StringValue != null)
				{
					yield return new string[] { name, value.StringValue };
				}
				else if (value.WordValue != null)
				{
					yield return new string[] { name, value.WordValue };
				}
				else
				{
					throw null; // never
				}
			}
		}
	}
}
