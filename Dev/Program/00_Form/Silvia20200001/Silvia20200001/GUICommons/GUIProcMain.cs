using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;
using Charlotte.Commons;

namespace Charlotte.GUICommons
{
	public static class GUIProcMain
	{
		public static DateTime BuiltDateTime;

		public static void GUIMain(Func<Form> getMainForm)
		{
			ProcMain.WriteLog = message => { };

			Application.ThreadException += new ThreadExceptionEventHandler((sender, e) => ErrorTermination(e.Exception));
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((sender, e) => ErrorTermination(e.ExceptionObject));
			SystemEvents.SessionEnding += new SessionEndingEventHandler((sender, e) => ProgramTermination());

			uint peTimeDateStamp = GetPETimeDateStamp(ProcMain.SelfFile);

			BuiltDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
				.AddSeconds(peTimeDateStamp)
				.ToLocalTime();

			KeepSingleInstance(peTimeDateStamp, () =>
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(getMainForm());
			});
		}

		private static void ErrorTermination(object e)
		{
			MessageBox.Show(
				"" + e,
				Path.GetFileNameWithoutExtension(ProcMain.SelfFile) + " / Error",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error
				);

			ProgramTermination();
		}

		private static void ProgramTermination()
		{
			Environment.Exit(1);
		}

		private static void KeepSingleInstance(uint peTimeDateStamp, Action routine)
		{
			// HACK: 別々のプログラムが偶然同じビルド時刻になってまうと、それらは同時に実行できなくなる。
			// -- レアケースなので看過する。

			// HACK: 同じアプリケーションでもビルドしなおすと(バージョンが違うと)排他制御が効かなくなる。

			Mutex procMutex = new Mutex(
				false,
				"Silvia20200001_PROC_MUTEX_{62f72aca-dc8c-432e-b00d-e589dc2bf9fa}_" + peTimeDateStamp
				);

			if (procMutex.WaitOne(0))
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
				Mutex globalProcMutex = new Mutex(
					false,
					@"Global\Silvia20200001_GLOBAL_PROC_MUTEX_{ffdbdfa1-6ba8-4ec5-899c-9b361bbb6a15}_" + peTimeDateStamp,
					out createdNew,
					security
					);

				bool globalLockFailed = false;

				if (globalProcMutex.WaitOne(0))
				{
					routine();

					globalProcMutex.ReleaseMutex();
				}
				else
				{
					globalLockFailed = true;
				}
				globalProcMutex.Close();
				globalProcMutex.Dispose();
				globalProcMutex = null;

				if (globalLockFailed)
				{
					// memo: ローカル側ロック(procMutex)解除前に表示すること。
					// -- プロセスを同時に複数起動したとき、このダイアログを複数表示させないため。
					//
					MessageBox.Show(
						"Already started on the other logon session !",
						Path.GetFileNameWithoutExtension(ProcMain.SelfFile) + " / Error",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error
						);
				}
				procMutex.ReleaseMutex();
			}
			procMutex.Close();
			procMutex.Dispose();
			procMutex = null;
		}

		private static uint GetPETimeDateStamp(string file)
		{
			using (FileStream reader = new FileStream(file, FileMode.Open, FileAccess.Read))
			{
				if (ReadByte(reader) != 'M') throw null;
				if (ReadByte(reader) != 'Z') throw null;

				reader.Seek(0x3c, SeekOrigin.Begin);

				uint peHedPos = (uint)ReadByte(reader);
				peHedPos |= (uint)ReadByte(reader) << 8;
				peHedPos |= (uint)ReadByte(reader) << 16;
				peHedPos |= (uint)ReadByte(reader) << 24;

				reader.Seek(peHedPos, SeekOrigin.Begin);

				if (ReadByte(reader) != 'P') throw null;
				if (ReadByte(reader) != 'E') throw null;
				if (ReadByte(reader) != 0x00) throw null;
				if (ReadByte(reader) != 0x00) throw null;

				reader.Seek(0x04, SeekOrigin.Current);

				uint timeDateStamp = (uint)ReadByte(reader);
				timeDateStamp |= (uint)ReadByte(reader) << 8;
				timeDateStamp |= (uint)ReadByte(reader) << 16;
				timeDateStamp |= (uint)ReadByte(reader) << 24;

				return timeDateStamp;
			}
		}

		private static int ReadByte(FileStream reader)
		{
			int chr = reader.ReadByte();

			if (chr == -1) // ? EOF
				throw new Exception("Read EOF");

			return chr;
		}
	}
}
