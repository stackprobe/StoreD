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

			Main4(new ArgsReader(new string[] { @"C:\temp\StoreD-main" }));
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

			ar.End();

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

				if (SCommon.EqualsIgnoreCase(Path.GetFileName(file), Consts.EMPRY_DIR_MARK_FILE_NAME))
				{
					SCommon.DeletePath(file);
					files[index] = null;
				}
			}
			files = files.Where(file => file != null).ToArray();

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
				string trueLocalName = RestoreLocalName(localName);

				if (localName != trueLocalName)
				{
					string trueFile = Path.Combine(Path.GetDirectoryName(file), trueLocalName);

					ProcMain.WriteLog("< " + file);
					ProcMain.WriteLog("> " + trueFile);

					File.Move(file, trueFile);
				}
			}
			foreach (string dir in dirs) // ディレクトリ名の変更(配下のディレクトリから)
			{
				string localName = Path.GetFileName(dir);
				string trueLocalName = RestoreLocalName(localName);

				if (localName != trueLocalName)
				{
					string trueDir = Path.Combine(Path.GetDirectoryName(dir), trueLocalName);

					ProcMain.WriteLog("< " + dir);
					ProcMain.WriteLog("> " + trueDir);

					Directory.Move(dir, trueDir);
				}
			}
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
					SCommon.HEXADECIMAL_LOWER.Contains(char.ToLower(str[index + 1])) &&
					SCommon.HEXADECIMAL_LOWER.Contains(char.ToLower(str[index + 2])) &&
					SCommon.HEXADECIMAL_LOWER.Contains(char.ToLower(str[index + 3])) &&
					SCommon.HEXADECIMAL_LOWER.Contains(char.ToLower(str[index + 4]))
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
	}
}
