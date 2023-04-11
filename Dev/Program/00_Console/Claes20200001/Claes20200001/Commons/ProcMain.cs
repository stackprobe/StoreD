using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using Microsoft.Win32;

namespace Charlotte.Commons
{
	public static class ProcMain
	{
		public static string SelfFile;
		public static string SelfDir;

		public static ArgsReader ArgsReader;

		public static Action<object> WriteLog = message => { };

		public static void CUIMain(Action<ArgsReader> mainFunc)
		{
			try
			{
				WriteLog = message => Console.WriteLine("[" + DateTime.Now + "] " + message);

				SelfFile = Assembly.GetEntryAssembly().Location;
				SelfDir = Path.GetDirectoryName(SelfFile);

				WorkingDir.Root = new WorkingDir.RootInfo();

				ArgsReader = GetArgsReader();

				mainFunc(ArgsReader);

				WorkingDir.Root.Delete();
				WorkingDir.Root = null;
			}
			catch (Exception e)
			{
				WriteLog(e);

				// ここに到達する場合は想定外の致命的なエラーである。-> 何か出すべき。
				// ウィンドウ非表示で実行されているかもしれないのでメッセージダイアログを出す。

				MessageBox.Show("" + e, "Claes20200001 / Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				//Console.WriteLine("Press ENTER key. (Error termination)");
				//Console.ReadLine();
			}
		}

		private static ArgsReader GetArgsReader()
		{
			return new ArgsReader(Environment.GetCommandLineArgs(), 1);
		}

		public static bool DEBUG
		{
			get
			{
#if DEBUG
				return true;
#else
				return false;
#endif
			}
		}
	}
}
