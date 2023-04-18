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

			Main4(new ArgsReader(new string[] { @"C:\Berry", @"C:\temp" }));
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
			string rRootDir = SCommon.MakeFullPath(ar.NextArg());
			string wRootDir = SCommon.MakeFullPath(ar.NextArg());

			Console.WriteLine("< " + rRootDir);
			Console.WriteLine("> " + wRootDir);

			if (!Directory.Exists(rRootDir))
				throw new Exception("no rRootDir");

			if (!Directory.Exists(wRootDir))
				throw new Exception("no wRootDir");

			foreach (string rProjectDir in Directory.GetDirectories(rRootDir))
			{
				string wProjectDir = Path.Combine(wRootDir, Path.GetFileName(rProjectDir));

				Console.WriteLine("P " + rProjectDir);
				Console.WriteLine("> " + wProjectDir);

				SCommon.DeletePath(wProjectDir);
				SCommon.CreateDir(wProjectDir);

				foreach (string rDir in Directory.GetDirectories(rProjectDir))
				{
					string wDir = Path.Combine(wProjectDir, Path.GetFileName(rDir));

					Console.WriteLine("D " + rDir);
					Console.WriteLine("> " + wDir);

					if (rDir.EndsWith("20200001"))
					{
						SCommon.CopyDir(rDir, wDir);
					}
					else
					{
						string treeFile = Path.Combine(wDir, "_Tree.txt");
						string[] treeFileData = MakeTreeFileData(rDir);

						SCommon.CreateDir(wDir);

						File.WriteAllLines(treeFile, treeFileData, Encoding.UTF8);
					}
				}
				foreach (string rFile in Directory.GetFiles(rProjectDir))
				{
					string wFile = Path.Combine(wProjectDir, Path.GetFileName(rFile));

					Console.WriteLine("F " + rFile);
					Console.WriteLine("> " + wFile);

					File.Copy(rFile, wFile);
				}
				Console.WriteLine("done");
			}
			Console.WriteLine("done!");
		}

		private string[] MakeTreeFileData(string targDir)
		{
			string[] paths = Directory.GetDirectories(targDir, "*", SearchOption.AllDirectories)
				.Concat(Directory.GetFiles(targDir, "*", SearchOption.AllDirectories))
				.OrderBy(SCommon.Comp)
				.ToArray();

			List<string> dest = new List<string>();

			foreach (string path in paths)
			{
				dest.Add(SCommon.ChangeRoot(path, targDir));

				if (Directory.Exists(path))
				{
					dest.Add("\t-> Directory");
				}
				else
				{
					FileInfo info = new FileInfo(path);

					dest.Add(string.Format(
						"\t-> File {0} / {1} / {2:#,0}"
						, new SCommon.SimpleDateTime(info.CreationTime)
						, new SCommon.SimpleDateTime(info.LastWriteTime)
						, info.Length
						));
				}
			}

			if (dest.Count == 0)
				dest.Add("Nothing");

			return dest.ToArray();
		}
	}
}
