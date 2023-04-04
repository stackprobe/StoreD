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

			Main4(new ArgsReader(new string[] { @"C:\temp\Input.txt", @"C:\temp\Output.csv" }));
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
			string file = SCommon.MakeFullPath(ar.NextArg());
			string destFile = SCommon.MakeFullPath(ar.NextArg());

			ar.End();

			List<string> row1 = new List<string>();
			List<string> row2 = new List<string>();
			List<string> entity = null;

			string[] lines = File.ReadAllLines(file, Encoding.ASCII);

			Action endEntity = () =>
			{
				if (entity != null)
				{
					row2.Add(SCommon.LinesToText(entity).Trim());
				}
			};

			foreach (string line in lines)
			{
				if (Regex.IsMatch(line, "^[0-9]{3}$"))
				{
					endEntity();
					row1.Add("'" + line);
					entity = new List<string>();
				}
				else if (entity != null)
				{
					entity.Add(line);
				}
			}
			endEntity();

			if (row1.Count != row2.Count)
				throw null;

			using (CsvFileWriter writer = new CsvFileWriter(destFile))
			{
				writer.WriteRow(row1);
				writer.WriteRow(row2);
			}
		}
	}
}
