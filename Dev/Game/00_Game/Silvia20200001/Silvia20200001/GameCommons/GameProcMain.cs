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
using Charlotte.Drawings;
using Charlotte.GUICommons;
using Charlotte.GameSettings;

namespace Charlotte.GameCommons
{
	public static class GameProcMain
	{
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
				while (1 <= DD.Finalizers.Count)
				{
					try
					{
						SCommon.UnaddElement(DD.Finalizers)();
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

			Keyboard.Initialize();

			string title =
				Path.GetFileNameWithoutExtension(ProcMain.SelfFile)
				+ " / "
				+ GUIProcMain.BuiltDateTime.ToString("yyyy-MM-dd-HH-mm-ss");

			Icon icon;

			using (MemoryStream mem = new MemoryStream(DD.GetResFileData(@"General\app.ico")))
			{
				icon = new Icon(mem);
			}

			// DXLib 初期化 ここから

			DX.SetApplicationLogSaveDirectory(logSaveDir);
			DX.SetOutApplicationLogValidFlag(1); // ログを出力/1:する/0:しない
			DX.SetAlwaysRunFlag(1); // 非アクティブ時に/1:動く/0:止まる
			DX.SetMainWindowText(title);
			DX.SetGraphMode(GameConfig.ScreenSize.W, GameConfig.ScreenSize.H, 32); // 幅, 高さ, ビット数(16 or 32)
			DX.ChangeWindowMode(1); // 1:ウィンドウ/0:フルスクリーン
			DX.SetWindowIconHandle(icon.Handle);

			if (DX.DxLib_Init() != 0) // ? 失敗
				throw new Exception("DxLib_Init failed");

			DD.Finalizers.Add(() =>
			{
				if (DX.DxLib_End() != 0) // ? 失敗
					throw new Exception("DxLib_End failed");
			});

			DX.SetDrawScreen(DX.DX_SCREEN_BACK);
			DX.SetDrawMode(DX.DX_DRAWMODE_ANISOTROPIC);
			DX.SetWindowSizeChangeEnableFlag(0); // ウィンドウの右下をドラッグでサイズ変更/1:する/0:しない
			DX.SetMouseDispFlag(1); // マウスカーソルを表示/1:する/0:しない

			// DXLib 初期化 ここまで

			Pad.Initialize();

			string saveDataFile = Path.Combine(ProcMain.SelfDir, "SaveData.dat");

			if (File.Exists(saveDataFile))
				GameSetting.Deserialize(File.ReadAllText(saveDataFile, Encoding.ASCII));
			else
				GameSetting.Initialize();

			DD.Finalizers.Add(() =>
			{
				File.WriteAllText(saveDataFile, GameSetting.Serialize(), Encoding.ASCII);
			});

			DD.MainWindowTitle = title;
			DD.TargetMonitor = DU.GetTargetMonitor_Boot();
			DD.RealScreenSize = GameSetting.FullScreen ?
				new I2Size(DD.TargetMonitor.W, DD.TargetMonitor.H) :
				GameSetting.UserScreenSize;
			DD.MainScreenDrawRect = new I4Rect(0, 0, DD.RealScreenSize.W, DD.RealScreenSize.H); // 不適切な領域 -- 後でちゃんとした領域をセットする。
			DD.MainScreen = new SubScreen(GameConfig.ScreenSize.W, GameConfig.ScreenSize.H);
			DD.LastMainScreen = new SubScreen(GameConfig.ScreenSize.W, GameConfig.ScreenSize.H);
			DD.KeptMainScreen = new SubScreen(GameConfig.ScreenSize.W, GameConfig.ScreenSize.H);

			foreach (string resPath in GameConfig.FontFileResPaths)
				DU.AddFontFile(resPath);

			SetRealScreenSize(DD.RealScreenSize.W, DD.RealScreenSize.H);

			GameStarted();
		}

		public static void SetRealScreenSize(int w, int h)
		{
			DD.TargetMonitor = DU.GetTargetMonitor();

			DX.SetDrawScreen(DX.DX_SCREEN_BACK);

			Picture.UnloadAll();
			SubScreen.UnloadAll();
			DU.UnloadAllFontHandle();
			//Music.UnloadAll(); // アンロード不要
			//SoundEffect.UnloadAll(); // アンロード不要

			DX.SetGraphMode(w, h, 32);
			DX.SetDrawScreen(DX.DX_SCREEN_BACK);
			DX.SetDrawMode(DX.DX_DRAWMODE_ANISOTROPIC);
			DX.SetWindowSizeChangeEnableFlag(0);
			DX.SetMouseDispFlag(1);

			int l = DD.TargetMonitor.L + (DD.TargetMonitor.W - w) / 2;
			int t = DD.TargetMonitor.T + (DD.TargetMonitor.H - h) / 2;

			DU.SetMainWindowPosition(l, t);

			DD.RealScreenSize.W = w;
			DD.RealScreenSize.H = h;

			DD.MainScreenDrawRect = DD.EnlargeFullInterior(
				GameConfig.ScreenSize.ToD2Size(),
				new I4Rect(0, 0, DD.RealScreenSize.W, DD.RealScreenSize.H).ToD4Rect()
				)
				.ToI4Rect();
		}
	}
}
