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

			Main4(new ArgsReader(new string[] { @"C:\temp\Store-main" }));
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
			string rDir = SCommon.MakeFullPath(ar.NextArg());

			if (!Directory.Exists(rDir))
				throw new Exception("no rDir");

			string wDir = SCommon.GetOutputDir();
			ProcMain.WriteLog("出力先へコピーしています...");
			SCommon.CopyDir(rDir, wDir);
			ProcMain.WriteLog("出力先へコピーしました。");
			FilterMain(wDir);
			ProcMain.WriteLog("完了しました。");
		}

		private void FilterMain(string rootDir)
		{
			string[] dirs = Directory.GetDirectories(rootDir, "*", SearchOption.AllDirectories);
			string[] files = Directory.GetFiles(rootDir, "*", SearchOption.AllDirectories);

			Array.Sort(dirs, (a, b) => SCommon.Comp(a, b) * -1); // 逆順
			Array.Sort(files, SCommon.Comp);

			for (int index = 0; index < files.Length; index++) // ファイルの削除
			{
				string file = files[index];

				if (SCommon.EqualsIgnoreCase(Path.GetFileName(file), "$$GHRF_Empty"))
				{
					SCommon.DeletePath(file);
					files[index] = null; // 削除マーク
				}
			}
			files = files.Where(file => file != null).ToArray(); // 削除マーク除去

			foreach (string file in files) // ファイルの編集
			{
				byte[] fileData = File.ReadAllBytes(file);

				if (IsEncodingUTF8WithBOM(fileData) || IsEncodingSJIS(fileData))
				{
					fileData = NewLineToCRLF(fileData).ToArray();

					ProcMain.WriteLog("* " + file);

					File.WriteAllBytes(file, fileData);
				}
			}
			foreach (string file in files) // ファイル名の変更
			{
				string localName = Path.GetFileName(file);
				string localNameNew = RestoreLocalName(localName);

				if (localName != localNameNew)
				{
					string fileNew = Path.Combine(Path.GetDirectoryName(file), localNameNew);

					ProcMain.WriteLog("< " + file);
					ProcMain.WriteLog("> " + fileNew);

					File.Move(file, fileNew);
				}
			}
			foreach (string dir in dirs) // ディレクトリ名の変更(配下のディレクトリから)
			{
				string localName = Path.GetFileName(dir);
				string localNameNew = RestoreLocalName(localName);

				if (localName != localNameNew)
				{
					string dirNew = Path.Combine(Path.GetDirectoryName(dir), localNameNew);

					ProcMain.WriteLog("< " + dir);
					ProcMain.WriteLog("> " + dirNew);

					Directory.Move(dir, dirNew);
				}
			}

			// ---- 以下 2022.10 以降に追加

			// ファイル名を変更したので再取得する。
			files = Directory.GetFiles(rootDir, "*", SearchOption.AllDirectories);
			Array.Sort(files, SCommon.Comp);

			DecompressMapDataFiles(files); // 注意：パス名を変更するかもしれない。
		}

		private bool IsEncodingUTF8WithBOM(byte[] fileData)
		{
			return
				3 <= fileData.Length &&
				fileData[0] == 0xef &&
				fileData[1] == 0xbb &&
				fileData[2] == 0xbf;
		}

		private bool IsEncodingSJIS(byte[] fileData)
		{
			for (int index = 0; index < fileData.Length; index++)
			{
				byte bChr = fileData[index];

				// ? 半角文字
				if (
					bChr == 0x09 || // 水平タブ
					bChr == 0x0a || // LF
					bChr == 0x0d || // CR
					(0x20 <= bChr && bChr <= 0x7e) || // US-ASCII
					(0xa1 <= bChr && bChr <= 0xdf) // 半角カナ
					)
				{
					// noop
				}
				// ? 全角文字
				else if (
					index + 1 < fileData.Length &&
					Common.IsJChar(fileData[index], fileData[index + 1])
					)
				{
					index++;
				}
				else // ? SJIS-テキストではない。
				{
					return false;
				}
			}
			return true;
		}

		private IEnumerable<byte> NewLineToCRLF(byte[] fileData)
		{
			foreach (byte chr in fileData)
			{
				if (chr == Consts.CR)
				{
					// noop
				}
				else if (chr == Consts.LF)
				{
					yield return Consts.CR;
					yield return Consts.LF;
				}
				else
				{
					yield return chr;
				}
			}
		}

		private string RestoreLocalName(string str)
		{
			StringBuilder buff = new StringBuilder();

			for (int index = 0; index < str.Length; index++)
			{
				char chr = str[index];

				if (
					index + 4 < str.Length &&
					chr == '$' &&
					SCommon.hexadecimal.Contains(char.ToLower(str[index + 1])) &&
					SCommon.hexadecimal.Contains(char.ToLower(str[index + 2])) &&
					SCommon.hexadecimal.Contains(char.ToLower(str[index + 3])) &&
					SCommon.hexadecimal.Contains(char.ToLower(str[index + 4]))
					)
				{
					chr = (char)Convert.ToUInt16(str.Substring(index + 1, 4), 16);

					if (!Common.IsUnicodeJChar(chr))
						throw new Exception("エスケープされた不正な文字コードを検出しました。ローカル名：" + str);

					index += 4;
				}
				buff.Append(chr);
			}
			return buff.ToString();
		}

		// ---- 以下 2022.10 以降に追加

		private void DecompressMapDataFiles(string[] files)
		{
			foreach (string file in files)
			{
				if (!SCommon.EndsWithIgnoreCase(file, ".txt_$$Compress.txt")) // ? 圧縮されたファイルではない。
					continue;

				string destFile = file.Substring(0, file.Length - 15); // "_$$Compress.txt" 除去

				Console.WriteLine("< " + file);
				Console.WriteLine("> " + destFile);

				string[] lines = File.ReadAllLines(file, SCommon.ENCODING_SJIS);
				string[] destLines = DecompressMapData(lines);

				SCommon.DeletePath(file);
				File.WriteAllLines(destFile, destLines, SCommon.ENCODING_SJIS);
			}
		}

		private string[] DecompressMapData(string[] lines)
		{
			List<string> destLines = new List<string>();

			for (int index = 0; index < lines.Length; )
			{
				if (lines[index] == ";;REPEAT;;")
				{
					destLines.AddRange(Enumerable.Range(1, int.Parse(lines[index + 1])).Select(dummy => lines[index + 2]));
					index += 3;
				}
				else
				{
					destLines.Add(lines[index]);
					index++;
				}
			}
			return destLines.ToArray();
		}
	}
}
