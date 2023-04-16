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

			Main4(new ArgsReader(new string[] { @"C:\Dev\Game\00_Game\Resource", @"C:\Dev\Game\00_Game\doc\Readme.txt", @"C:\temp\Readme.txt" }));
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

				MessageBox.Show("" + ex, Path.GetFileNameWithoutExtension(ProcMain.SelfFile) + " / エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

				//Console.WriteLine("Press ENTER key. (エラーによりプログラムを終了します)");
				//Console.ReadLine();
			}
		}

		private void Main5(ArgsReader ar)
		{
			string resourceDir = SCommon.MakeFullPath(ar.NextArg());
			string manualFile = SCommon.MakeFullPath(ar.NextArg());
			string destFile = ar.NextArg();

			ar.End();

			if (destFile == "*")
				destFile = manualFile;
			else
				destFile = SCommon.MakeFullPath(destFile);

			Console.WriteLine("R " + resourceDir);
			Console.WriteLine("< " + manualFile);
			Console.WriteLine("> " + destFile);

			if (!Directory.Exists(resourceDir))
				throw new Exception("no resourceDir");

			if (!File.Exists(manualFile))
				throw new Exception("no manualFile");

			if (Directory.Exists(destFile))
				throw new Exception("Bad destFile");

			string[] lines = File.ReadAllLines(manualFile, SCommon.ENCODING_SJIS);
			string[] srcOfResLines = GetSourceOfResourceLines(resourceDir);

			lines = ReplaceLineToLines(lines, "${SOURCE-OF-RESOURCE}", srcOfResLines);

			File.WriteAllLines(destFile, lines, SCommon.ENCODING_SJIS);

			Console.WriteLine("done!");
		}

		private string[] GetSourceOfResourceLines(string resourceDir)
		{
			List<string> dest = new List<string>();
			string[] files = Directory.GetFiles(resourceDir, "_source.txt", SearchOption.AllDirectories);

			if (files.Length == 0)
				throw new Exception("出どころを記したファイルが見つかりません。");

			Array.Sort(files, SCommon.CompIgnoreCase);

			for (int fileIndex = 0; fileIndex < files.Length; fileIndex++)
			{
				string file = files[fileIndex];
				string[] lines = File.ReadAllLines(file, SCommon.ENCODING_SJIS);

				for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
				{
					string indent = string.Concat(Enumerable.Repeat("　", lineIndex + 1));
					string line = lines[lineIndex];

					dest.Add(indent + line);
				}
				dest.Add(""); // 空行
			}
			dest.RemoveAt(dest.Count - 1); // 最後の空行は余計なので除去する。

			return dest.ToArray();
		}

		private string[] ReplaceLineToLines(string[] lines, string repSrcLine, string[] repDestLines)
		{
			List<string> dest = new List<string>();

			foreach (string line in lines)
			{
				if (line == repSrcLine)
					dest.AddRange(repDestLines);
				else
					dest.Add(line);
			}
			return dest.ToArray();
		}
	}
}
