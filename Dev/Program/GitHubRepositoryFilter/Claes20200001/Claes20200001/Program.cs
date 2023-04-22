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

			Main4(new ArgsReader(new string[] { "GIT-HUB-REPO-UNSAFE-MOD", @"C:\home\GitHub" }));
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

			// 処理が一瞬で終わってもコンソールが見えるように
			Thread.Sleep(500);
		}

		private void Main5(ArgsReader ar)
		{
			if (!ar.ArgIs("GIT-HUB-REPO-UNSAFE-MOD"))
				throw new Exception("Need GIT-HUB-REPO-UNSAFE-MOD command-option");

			string rootDir = SCommon.MakeFullPath(ar.NextArg());

			if (!Directory.Exists(rootDir))
				throw new Exception("no rootDir");

			string[] repositoryDirs = GetRepositoryDirs(rootDir).ToArray();
			string[] paths = SCommon.Concat(repositoryDirs.Select(repositoryDir => GetCommitingPaths(repositoryDir))).ToArray();

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

					if (ret == 0)
					{
						ret = order_02(a) - order_02(b);

						if (ret == 0)
							ret = SCommon.CompIgnoreCase(a, b);
					}
					return ret;
				});
			}

			foreach (string path in paths)
			{
				if (IsEmptyDir(path))
				{
					// HACK: 元からこのファイルがあったのか判別できない。
					// -- そもそも無いと思う。-- そんな名前のファイルを置かない。
					// -- 本プログラムをコミット前に複数回実行しても良いようにしたいので、エスケープするなどができない。
					// --> というわけで看過する。

					File.WriteAllBytes(Path.Combine(path, "$$GHRF_Empty"), SCommon.EMPTY_BYTES);
				}
			}

			// 注意：以下パス名変更を行うので、パスに対する処理(ファイル更新など)はここまでに行っておくこと。

			foreach (string path in paths)
			{
				string dir = Path.GetDirectoryName(path);
				string localName = Path.GetFileName(path);
				string localNameNew = ChangeLocalName(localName);
				string pathNew = Path.Combine(dir, localNameNew);

				if (!SCommon.EqualsIgnoreCase(path, pathNew)) // ? パス名変更有り
				{
					Console.WriteLine("< " + path);
					Console.WriteLine("> " + pathNew);

					if (Directory.Exists(path))
						Directory.Move(path, pathNew);
					else
						File.Move(path, pathNew);
				}
			}

			// ---- 以下 2022.10 以降に追加

			// パス名を変更したので再取得する。
			paths = SCommon.Concat(repositoryDirs.Select(repositoryDir => GetCommitingPaths(repositoryDir))).ToArray();

			CompressMapDataFiles(paths); // 注意：パス名を変更するかもしれない。
		}

		private bool IsEmptyDir(string path)
		{
			return
				Directory.Exists(path) &&
				Directory.GetDirectories(path).Length == 0 &&
				Directory.GetFiles(path).Length == 0;
		}

		private string ChangeLocalName(string localName)
		{
			StringBuilder buff = new StringBuilder();

			foreach (char chr in localName)
			{
				// HACK: 元の名前に $xxxx を含む場合を想定していない。
				// -- そもそも無いと思う。-- そんな名前付けない。
				// -- 本プログラムをコミット前に複数回実行しても良いようにしたいので、エスケープするなどができない。
				// --> というわけで看過する。

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
					foreach (string relay in GetRepositoryDirs(dir))
						yield return relay;
			}
		}

		private IEnumerable<string> GetCommitingPaths(string currDir)
		{
			foreach (string dir in Directory.GetDirectories(currDir))
			{
				if (SCommon.EqualsIgnoreCase(Path.GetFileName(dir), ".git"))
					continue; // 除外

				foreach (string relay in GetCommitingPaths(dir))
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

		// ---- 以下 2022.10 以降に追加

		private void CompressMapDataFiles(string[] paths)
		{
			string[] files = paths
				.Where(v => SCommon.ContainsIgnoreCase(v, @"\dat\res\World\Map\"))
				.Where(v => SCommon.EndsWithIgnoreCase(v, ".txt"))
				.Where(v => !SCommon.EndsWithIgnoreCase(v, ".txt_$$Compress.txt"))
				.Where(v => File.Exists(v))
				.Where(v => IsMapDataFile(v))
				.ToArray();

			foreach (string file in files)
			{
				string destFile = file + "_$$Compress.txt";

				Console.WriteLine("< " + file);
				Console.WriteLine("> " + destFile);

				string[] lines = File.ReadAllLines(file, SCommon.ENCODING_SJIS);
				string[] destLines = CompressMapData(lines);

				SCommon.DeletePath(file);
				File.WriteAllLines(destFile, destLines, SCommon.ENCODING_SJIS);
			}
		}

		private bool IsMapDataFile(string file)
		{
			using (StreamReader reader = new StreamReader(file, SCommon.ENCODING_SJIS))
			{
				return
					IsNextLineSimpleUInt(reader) &&
					IsNextLineSimpleUInt(reader);
			}
		}

		private bool IsNextLineSimpleUInt(StreamReader reader)
		{
			string line = reader.ReadLine();

			return
				line != null &&
				Regex.IsMatch(line, "^[0-9]+$");
		}

		private string[] CompressMapData(string[] lines)
		{
			List<string> destLines = new List<string>();

			for (int index = 0; index < lines.Length; )
			{
				int c;

				for (c = 1; index + c < lines.Length; c++)
					if (lines[index] != lines[index + c])
						break;

				if (c == 1)
				{
					destLines.Add(lines[index]);
				}
				else
				{
					destLines.Add(";;REPEAT;;");
					destLines.Add("" + c);
					destLines.Add(lines[index]);
				}
				index += c;
			}
			return destLines.ToArray();
		}
	}
}
