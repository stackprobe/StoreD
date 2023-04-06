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

			Main4(new ArgsReader(new string[] { "D" }));
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

		private string WRootDir;

		private void Main5(ArgsReader ar)
		{
			string alpha = ar.NextArg();

			ar.End();

			if (!Regex.IsMatch(alpha, "^[A-Z]$"))
				throw new Exception("Bad alpha (not A-Z)");

			WRootDir = string.Format(Consts.W_ROOT_DIR_FORMAT, alpha);

			ProcMain.WriteLog("< " + Consts.R_ROOT_DIR);
			ProcMain.WriteLog("> " + WRootDir);

			if (!Directory.Exists(Consts.R_ROOT_DIR))
				throw new Exception("no R_ROOT_DIR");

			if (!Directory.Exists(WRootDir))
				throw new Exception("no WRootDir");

			ProcMain.WriteLog("start!");

			foreach (string dir in Directory.GetDirectories(Consts.R_ROOT_DIR))
			{
				CopyProject(dir, Path.Combine(WRootDir, Path.GetFileName(dir)));
			}
			ProcMain.WriteLog("done!");
		}

		private void CopyProject(string projectDir, string destDir)
		{
			string projectDirLocalName = Path.GetFileName(projectDir);

			if (projectDirLocalName[0] == '.') // ? ".metadata" はプロジェクトじゃないので除外
				return;

			if (SCommon.EqualsIgnoreCase(projectDirLocalName, "Servers")) // 除外
				return;

			ProcMain.WriteLog("< " + projectDir);
			ProcMain.WriteLog("> " + destDir);

			// 出力先クリア
			SCommon.DeletePath(destDir);
			SCommon.CreateDir(destDir);

			CopySourceDir(projectDir, destDir, "src");
		}

		private void CopySourceDir(string projectDir, string destDir, string subDirLocalName)
		{
			string rDir = Path.Combine(projectDir, subDirLocalName);
			string wDir = Path.Combine(destDir, subDirLocalName);

			ProcMain.WriteLog("< " + rDir);
			ProcMain.WriteLog("> " + wDir);

			SCommon.CopyDir(rDir, wDir);
		}
	}
}
