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
		public const string APP_IDENT = "{c9e92c41-52cf-44fe-8c46-b5139531e666}";
		public const string APP_TITLE = "APP-20200001";

		public static string SelfFile;
		public static string SelfDir;

		public static ArgsReader ArgsReader;

		public static void CUIMain(Action<ArgsReader> mainFunc)
		{
			try
			{
				WriteLog = message => Console.WriteLine("[" + DateTime.Now + "] " + message);

				SelfFile = Assembly.GetEntryAssembly().Location;
				SelfDir = Path.GetDirectoryName(SelfFile);

				WorkingDir.Root = WorkingDir.CreateProcessRoot();

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

				MessageBox.Show("" + e, APP_TITLE + " / Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				//Console.WriteLine("Press ENTER key. (Error termination)");
				//Console.ReadLine();
			}
		}

		public static void GUIMain(Func<Form> getMainForm)
		{
			Application.ThreadException += new ThreadExceptionEventHandler(ApplicationThreadException);
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomainUnhandledException);
			SystemEvents.SessionEnding += new SessionEndingEventHandler(SessionEnding);

			//WriteLog = message => { };

			SelfFile = Assembly.GetEntryAssembly().Location;
			SelfDir = Path.GetDirectoryName(SelfFile);

			string procMutexName;

			using (SHA512 sha512 = SHA512.Create())
			{
				string s = APP_IDENT + GetPETimeDateStamp().ToString();
				byte[] b = Encoding.UTF8.GetBytes(s);
				byte[] bh = sha512.ComputeHash(b);
				string h = string.Join("", bh.Select(bChr => bChr.ToString("x02")));

				procMutexName = h;
			}

			Mutex procMutex = new Mutex(false, procMutexName);

			if (procMutex.WaitOne(0))
			{
				if (GlobalProcMtx.Create(procMutexName, APP_TITLE))
				{
					CheckSelfFile();
					Directory.SetCurrentDirectory(SelfDir);
					CheckLogonUserAndTmp();

					WorkingDir.Root = WorkingDir.CreateProcessRoot();

					ArgsReader = GetArgsReader();

					// core >

					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);
					Application.Run(getMainForm());

					// < core

					WorkingDir.Root.Delete();
					WorkingDir.Root = null;

					GlobalProcMtx.Release();
				}
				procMutex.ReleaseMutex();
			}
			procMutex.Close();
		}

		public static Action<object> WriteLog = message => { };

		private static ArgsReader GetArgsReader()
		{
			return new ArgsReader(Environment.GetCommandLineArgs(), 1);
		}

		private static void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
		{
			try
			{
				MessageBox.Show(
					"[Application_ThreadException]\n" + e.Exception,
					APP_TITLE + " / Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
					);
			}
			catch
			{ }

			Environment.Exit(1);
		}

		private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			try
			{
				MessageBox.Show(
					"[CurrentDomain_UnhandledException]\n" + e.ExceptionObject,
					APP_TITLE + " / Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
					);
			}
			catch
			{ }

			Environment.Exit(2);
		}

		private static void SessionEnding(object sender, SessionEndingEventArgs e)
		{
			Environment.Exit(3);
		}

		private static void CheckSelfFile()
		{
			string file = SelfFile;
			Encoding SJIS = Encoding.GetEncoding(932);

			if (file != SJIS.GetString(SJIS.GetBytes(file)))
			{
				MessageBox.Show(
					"Shift_JIS に変換できない文字を含むパスからは実行できません。",
					APP_TITLE + " / エラー",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
					);

				Environment.Exit(4);
			}
			if (file.Substring(1, 2) != ":\\")
			{
				MessageBox.Show(
					"ネットワークパスからは実行できません。",
					APP_TITLE + " / エラー",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
					);

				Environment.Exit(5);
			}
		}

		private static void CheckLogonUserAndTmp()
		{
			string userName = Environment.GetEnvironmentVariable("UserName");
			Encoding SJIS = Encoding.GetEncoding(932);

			if (
				userName == null ||
				userName == "" ||
				userName != SJIS.GetString(SJIS.GetBytes(userName)) ||
				userName.StartsWith(" ") ||
				userName.EndsWith(" ")
				)
			{
				MessageBox.Show(
					"Windows ログオン・ユーザー名に問題があります。",
					APP_TITLE + " / エラー",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
					);

				Environment.Exit(6);
			}

			string tmp = Environment.GetEnvironmentVariable("TMP");

			if (
				tmp == null ||
				tmp == "" ||
				tmp != SJIS.GetString(SJIS.GetBytes(tmp)) ||
				tmp.Length < 4 || // ルートDIR禁止
				tmp[1] != ':' ||
				tmp[2] != '\\' ||
				!Directory.Exists(tmp) ||
				tmp.Contains(' ')
				)
			{
				MessageBox.Show(
					"環境変数 TMP に問題があります。",
					APP_TITLE + " / エラー",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
					);

				Environment.Exit(7);
			}
		}

		private static class GlobalProcMtx
		{
			private static Mutex ProcMtx;

			public static bool Create(string procMtxName, string title)
			{
				try
				{
					MutexSecurity security = new MutexSecurity();

					security.AddAccessRule(
						new MutexAccessRule(
							new SecurityIdentifier(
								WellKnownSidType.WorldSid,
								null
								),
							MutexRights.FullControl,
							AccessControlType.Allow
							)
						);

					bool createdNew;
					ProcMtx = new Mutex(false, @"Global\Global_" + procMtxName, out createdNew, security);

					if (ProcMtx.WaitOne(0))
						return true;

					ProcMtx.Close();
					ProcMtx = null;
				}
				catch (Exception e)
				{
					MessageBox.Show("" + e, title + " / Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}

				CloseProcMtx();

				MessageBox.Show(
					"Already started on the other logon session !",
					title + " / Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
					);

				return false;
			}

			public static void Release()
			{
				CloseProcMtx();
			}

			private static void CloseProcMtx()
			{
				try { ProcMtx.ReleaseMutex(); }
				catch { }

				try { ProcMtx.Close(); }
				catch { }

				ProcMtx = null;
			}
		}

		private static uint? PETimeDateStamp = null;

		public static uint GetPETimeDateStamp()
		{
			if (PETimeDateStamp == null)
				PETimeDateStamp = GetPETimeDateStamp_Main();

			return PETimeDateStamp.Value;
		}

		private static uint GetPETimeDateStamp_Main()
		{
			using (FileStream reader = new FileStream(SelfFile, FileMode.Open, FileAccess.Read))
			{
				if (F_ReadByte(reader) != 'M') throw null;
				if (F_ReadByte(reader) != 'Z') throw null;

				reader.Seek(0x3c, SeekOrigin.Begin);

				uint peHedPos = (uint)F_ReadByte(reader);
				peHedPos |= (uint)F_ReadByte(reader) << 8;
				peHedPos |= (uint)F_ReadByte(reader) << 16;
				peHedPos |= (uint)F_ReadByte(reader) << 24;

				reader.Seek(peHedPos, SeekOrigin.Begin);

				if (F_ReadByte(reader) != 'P') throw null;
				if (F_ReadByte(reader) != 'E') throw null;
				if (F_ReadByte(reader) != 0x00) throw null;
				if (F_ReadByte(reader) != 0x00) throw null;

				reader.Seek(0x04, SeekOrigin.Current);

				uint timeDateStamp = (uint)F_ReadByte(reader);
				timeDateStamp |= (uint)F_ReadByte(reader) << 8;
				timeDateStamp |= (uint)F_ReadByte(reader) << 16;
				timeDateStamp |= (uint)F_ReadByte(reader) << 24;

				return timeDateStamp;
			}
		}

		private static int F_ReadByte(FileStream reader)
		{
			int bChr = reader.ReadByte();

			if (bChr == -1) // ? EOF
				throw new Exception("Read EOF");

			return bChr;
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
