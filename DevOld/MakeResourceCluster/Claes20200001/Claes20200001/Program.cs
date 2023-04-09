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

			Main4(new ArgsReader(new string[] { @"C:\temp\Input", @"C:\temp\Output.dat" }));
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
			string clusterFile = SCommon.MakeFullPath(ar.NextArg());

			ar.End();

			Console.WriteLine("< " + resourceDir);
			Console.WriteLine("> " + clusterFile);

			if (!Directory.Exists(resourceDir))
				throw new Exception("no resourceDir");

			if (Directory.Exists(clusterFile))
				throw new Exception("Bad clusterFile");

			using (FileStream writer = new FileStream(clusterFile, FileMode.Create, FileAccess.Write))
			{
				string[] files = Directory.GetFiles(resourceDir, "*", SearchOption.AllDirectories);

				files = files.Where(v => !Path.GetFileName(v).StartsWith("_")).ToArray(); // 半角アンダースコア('_')で始まるファイルは除外する。

				Array.Sort(files, SCommon.CompIgnoreCase);

				foreach (string file in files)
				{
					string resPath = SCommon.ChangeRoot(file, resourceDir);
					byte[] data = File.ReadAllBytes(file);

					Console.WriteLine("+ " + resPath);
					Console.WriteLine("S " + data.Length);

					data = SCommon.Compress(data);
					LiteShuffleP29(data);

					SCommon.WritePartString(writer, resPath);
					SCommon.WritePartInt(writer, data.Length);
					SCommon.Write(writer, data);

					Console.WriteLine("done");
				}
			}
			Console.WriteLine("done!");
		}

		private static void LiteShuffleP29(byte[] data)
		{
			int l = 0;
			int r = data.Length - 2;
			int rr = Math.Max(3, data.Length / 109);

			while (l < r)
			{
				SCommon.Swap(data, l, r);

				l++;
				r -= rr;
			}
		}
	}
}
