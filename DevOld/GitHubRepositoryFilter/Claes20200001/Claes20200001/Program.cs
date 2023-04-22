using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

			Main4(new ArgsReader(new string[] { @"C:\home\GitHub" }));
			//new Test0001().Test01();
			//new Test0002().Test01();
			//new Test0003().Test01();

			// --

			//SCommon.Pause();
		}

		private void Main4(ArgsReader ar)
		{
			try
			{
				Main5(ar);
			}
			catch (Exception e)
			{
				ProcMain.WriteLog(e);
			}
		}

		private void Main5(ArgsReader ar)
		{
			string rootDir = SCommon.MakeFullPath(ar.NextArg());

			ar.End();

			if (!Directory.Exists(rootDir))
				throw new Exception("no rootDir");

			string[] repositoryDirs = GetRepositoryDirs(rootDir).ToArray();
			string[] paths = SCommon.Concat(repositoryDirs.Select(repositoryDir => GetCommitmentPaths(repositoryDir))).ToArray();

			// ソート
			// 1. ファイル -> ディレクトリ
			// 2. 深いパス -> 浅いパス
			// 3. 辞書順
			{
				Func<string, int> order_01 = path =>
				{
					if (File.Exists(path))
						return 1;

					if (Directory.Exists(path))
						return 2;

					throw null; // never
				};

				Func<string, int> order_02 = path => path.Count(chr => chr == '\\') * -1;

				Array.Sort(paths, (a, b) =>
				{
					int ret = order_01(a) - order_01(b);

					if (ret != 0)
						return ret;

					ret = order_02(a) - order_02(b);

					if (ret != 0)
						return ret;

					ret = SCommon.CompIgnoreCase(a, b);
					return ret;
				});
			}

			foreach (string path in paths)
			{
				string markFile = Path.Combine(path, Consts.EMPRY_DIR_MARK_FILE_NAME);

				if (File.Exists(markFile) && !IsOneFileDir(path))
				{
					SCommon.DeletePath(markFile);
				}
				else if (IsEmptyDir(path))
				{
					File.WriteAllBytes(markFile, SCommon.EMPTY_BYTES);
				}
			}

			foreach (string path in paths)
			{
				string dir = Path.GetDirectoryName(path);
				string localName = Path.GetFileName(path);
				string escLocalName = ChangeLocalName(localName);

				if (!SCommon.EqualsIgnoreCase(localName, escLocalName))
				{
					string escPath = Path.Combine(dir, escLocalName);

					Console.WriteLine("< " + path);
					Console.WriteLine("> " + escPath);

					if (Directory.Exists(path))
						Directory.Move(path, escPath);
					else
						File.Move(path, escPath);
				}
			}
		}

		private bool IsEmptyDir(string path)
		{
			return
				Directory.Exists(path) &&
				Directory.GetDirectories(path).Length == 0 &&
				Directory.GetFiles(path).Length == 0;
		}

		private bool IsOneFileDir(string path)
		{
			return
				Directory.Exists(path) &&
				Directory.GetDirectories(path).Length == 0 &&
				Directory.GetFiles(path).Length == 1;
		}

		private string ChangeLocalName(string localName)
		{
			StringBuilder buff = new StringBuilder();

			foreach (char chr in localName)
			{
				if (SCommon.HALF.Contains(chr) || chr == ' ')
				{
					buff.Append(chr);
				}
				else
				{
					buff.Append('$');
					buff.Append(((int)chr).ToString("x4"));
				}
			}
			return buff.ToString();
		}

		private IEnumerable<string> GetRepositoryDirs(string currDir)
		{
			if (Directory.Exists(Path.Combine(currDir, ".git")))
			{
				yield return currDir;
			}
			else
			{
				foreach (string dir in Directory.GetDirectories(currDir))
					foreach (var relay in GetRepositoryDirs(dir))
						yield return relay;
			}
		}

		private IEnumerable<string> GetCommitmentPaths(string currDir)
		{
			foreach (string dir in Directory.GetDirectories(currDir))
			{
				if (SCommon.EqualsIgnoreCase(Path.GetFileName(dir), ".git"))
					continue; // 除外

				foreach (var relay in GetCommitmentPaths(dir))
					yield return relay;

				yield return dir;
			}
			foreach (string file in Directory.GetFiles(currDir))
			{
				if (SCommon.EqualsIgnoreCase(Path.GetFileName(file), ".gitattributes"))
					continue; // 除外

				yield return file;
			}
		}
	}
}
