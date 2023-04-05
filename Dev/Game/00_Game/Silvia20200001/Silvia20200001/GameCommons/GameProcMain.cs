using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using DxLibDLL;
using Charlotte.Commons;
using Charlotte.GUICommons;

namespace Charlotte.GameCommons
{
	public static class GameProcMain
	{
		public static List<Action> Finalizers = new List<Action>();

		private static Action GameStarted;

		public static void GameMain(Form mainForm, Action userGameMain)
		{
			Thread th = new Thread(() =>
			{
				bool aliving = true;

				GameStarted = () =>
				{
					mainForm.BeginInvoke((MethodInvoker)delegate
					{
						if (aliving)
							mainForm.Visible = false;
					});

					userGameMain();
				};

				Main2();

				mainForm.BeginInvoke((MethodInvoker)delegate
				{
					aliving = false;
					mainForm.Close();
				});
			});

			th.Start();
		}

		private static void Main2()
		{
			try
			{
				Main3();
			}
			catch (Exception e)
			{
				ProcMain.WriteLog(e);
			}
			finally
			{
				while (1 <= Finalizers.Count)
				{
					try
					{
						SCommon.UnaddElement(Finalizers)();
					}
					catch (Exception ex)
					{
						ProcMain.WriteLog(ex);
					}
				}
			}
		}

		private static void Main3()
		{
			string logSaveDir;
			string logFile;

			if (ProcMain.DEBUG)
			{
				logSaveDir = @"C:\temp";
				logFile = @"C:\temp\Game.log";
			}
			else
			{
				logSaveDir = new WorkingDir().GetPath(".");
				logFile = Path.Combine(ProcMain.SelfDir, "Game.log");
			}

			File.WriteAllBytes(logFile, SCommon.EMPTY_BYTES);

			ProcMain.WriteLog = message =>
			{
				File.AppendAllText(logFile, "[" + DateTime.Now + "] " + message + "\r\n", Encoding.UTF8);
			};

			string title =
				Path.GetFileNameWithoutExtension(ProcMain.SelfFile)
				+ " / "
				+ GUIProcMain.BuiltDateTime.ToString("yyyy-MM-dd-HH-mm-ss");

			Icon icon;

			using (MemoryStream mem = new MemoryStream(DD.GetResFileData(@"General\app.ico")))
			{
				icon = new Icon(mem);
			}

			DX.SetApplicationLogSaveDirectory(logSaveDir);
			DX.SetOutApplicationLogValidFlag(1); // ログを出力/1:する/0:しない
			DX.SetAlwaysRunFlag(1); // 非アクティブ時に/1:動く/0:止まる
			DX.SetMainWindowText(title);
			DX.SetGraphMode(600, 480, 32); // 幅, 高さ, ビット数(16 or 32)
			DX.ChangeWindowMode(1); // 1:ウィンドウ/0:フルスクリーン
			DX.SetWindowIconHandle(icon.Handle);

			if (DX.DxLib_Init() != 0) // ? 失敗
				throw new Exception("DxLib_Init failed");

			Finalizers.Add(() =>
			{
				if (DX.DxLib_End() != 0) // ? 失敗
					throw new Exception("DxLib_End failed");
			});

			DX.SetWindowSizeChangeEnableFlag(0); // ウィンドウの右下をドラッグでサイズ変更/1:する/0:しない
			DX.SetMouseDispFlag(1); // マウスカーソルを表示/1:する/0:しない
			DX.SetDrawMode(DX.DX_DRAWMODE_ANISOTROPIC);

			GameStarted();
		}
	}
}
