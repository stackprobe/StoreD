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

			Main4(new ArgsReader(new string[] { }));
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

		private void Main5(ArgsReader ar)
		{
			string targetDir = SCommon.MakeFullPath(ar.NextArg());

			ar.End();

			if (!Directory.Exists(targetDir))
				throw new Exception("no targetDir");

			foreach (string dir in Directory.GetDirectories(targetDir))
			{
				string tableOrderingFile = Path.Combine(dir, "table-ordering.txt");

				Console.WriteLine("< " + tableOrderingFile);

				if (!File.Exists(tableOrderingFile))
					throw new Exception("no tableOrderingFile");

				string[] tableOrdering = File.ReadAllLines(tableOrderingFile, Encoding.ASCII);

				List<string> dest = new List<string>();

				foreach (string name in tableOrdering)
				{
					string csvFile = Path.Combine(dir, name + ".csv");

					Console.WriteLine("< " + csvFile);

					if (!File.Exists(csvFile))
						throw new Exception("no csvFile");

					string[] csvLines = File.ReadAllLines(csvFile, Encoding.ASCII);

					dest.Add("");
					dest.Add(name);
					dest.AddRange(csvLines);
				}
				string destFile = dir + ".csv";

				Console.WriteLine("> " + destFile);

				File.WriteAllLines(destFile, dest, Encoding.ASCII);

				Console.WriteLine("done");
			}
		}
	}
}
