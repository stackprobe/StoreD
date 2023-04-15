using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using Charlotte.Commons;
using Charlotte.Drawings;
using Charlotte.GameCommons;

namespace Charlotte.Games
{
	public static class TitleMenu
	{
		private static Action DrawWall;

		public static void Run()
		{
			DD.SetCurtain(-1.0);
			DD.SetCurtainTarget(0.0);

			double z = 1.3;

			Musics.RemotestLibrary.Play();

			DrawWall = () =>
			{
				DD.Approach(ref z, 1.0, 0.999);

				DD.SetZoom(z);
				DD.Draw(Pictures.KAZUKIcghvbnkm, new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());
			};

			SimpleMenu menu = new SimpleMenu(40, 40, 40, 440, null, new string[]
			{
				"スタート",
				"コンテニュー",
				"設定",
				"終了",
			});

			for (; ; )
			{
				DD.FreezeInput();

				for (; ; )
				{
					DrawWall();

					if (menu.Draw())
						break;

					DD.EachFrame();
				}
				DD.FreezeInput();

				switch (menu.SelectedIndex)
				{
					case 0:
						StartTheGame();
						break;

					case 1:
						ContinueTheGame();
						break;

					case 2:
						Setting();
						break;

					case 3:
						goto endOfMenu;

					default:
						throw null; // never
				}
			}
		endOfMenu:

			DD.SetCurtainTarget(-1.0);
			Music.Fadeout(30);

			foreach (Scene scene in Scene.Create(40))
			{
				DrawWall();
				DD.EachFrame();
			}
		}

		private static void StartTheGame()
		{
			Game.Run();

			Musics.RemotestLibrary.Play();
		}

		private static void ContinueTheGame()
		{
			Game.Run();

			Musics.RemotestLibrary.Play();
		}

		private static void Setting()
		{
			SimpleMenu menu = new SimpleMenu(30, 40, 30, 540, "設定", new string[]
			{
				"ゲームパッドのボタン設定",
				"キーボードのキー設定",
				"ウィンドウサイズの変更",
				"ＢＭＧ音量",
				"ＳＥ音量",
				"マウスの使用／不使用",
				"戻る",
			});

			for (; ; )
			{
				DD.FreezeInput();

				for (; ; )
				{
					DrawWall();

					if (menu.Draw())
						break;

					DD.EachFrame();
				}
				DD.FreezeInput();

				switch (menu.SelectedIndex)
				{
					case 0:
						CustomizePad();
						break;

					case 1:
						CustomizeKeyboard();
						break;

					case 2:
						ChangeWindowSize();
						break;

					case 3:
						ChangeMusicVolume();
						break;

					case 4:
						ChangeSEVolume();
						break;

					case 5:
						ChangeMouseEnabled();
						break;

					case 6:
						goto endOfMenu;

					default:
						throw null; // never
				}
			}
		endOfMenu:

			DD.Save();
		}

		private static void CustomizePad()
		{
			throw new NotImplementedException();
		}

		private static void CustomizeKeyboard()
		{
			// TODO
			// TODO
			// TODO

			foreach (string name in DU.GetKeyboardKeyNames())
				ProcMain.WriteLog(name);
		}

		private static void ChangeWindowSize()
		{
			I2Size[] sizes = new I2Size[]
			{
				GameConfig.ScreenSize,
				(GameConfig.ScreenSize.ToD2Size() * 1.1).ToI2Size(),
				(GameConfig.ScreenSize.ToD2Size() * 1.2).ToI2Size(),
				(GameConfig.ScreenSize.ToD2Size() * 1.3).ToI2Size(),
				(GameConfig.ScreenSize.ToD2Size() * 1.4).ToI2Size(),
				(GameConfig.ScreenSize.ToD2Size() * 1.5).ToI2Size(),
				(GameConfig.ScreenSize.ToD2Size() * 1.6).ToI2Size(),
				(GameConfig.ScreenSize.ToD2Size() * 1.7).ToI2Size(),
				(GameConfig.ScreenSize.ToD2Size() * 1.8).ToI2Size(),
			};

			string[] items = sizes.Select(size => size.W + " x " + size.H)
				.Concat(new string[] { "フルスクリーン", "戻る" })
				.ToArray();

			items[0] += " (デフォルト)";

			SimpleMenu menu = new SimpleMenu(24, 40, 16, 420, "ウィンドウサイズの変更", items);

			for (; ; )
			{
				DD.FreezeInput();

				for (; ; )
				{
					DrawWall();

					if (menu.Draw())
						break;

					DD.EachFrame();
				}
				DD.FreezeInput();

				if (menu.SelectedIndex < sizes.Length)
				{
					I2Size size = sizes[menu.SelectedIndex];

					GameSetting.UserScreenSize.W = size.W;
					GameSetting.UserScreenSize.H = size.H;
					GameSetting.FullScreen = false;

					DD.SetRealScreenSize(size.W, size.H);
				}
				else if (menu.SelectedIndex == sizes.Length)
				{
					GameSetting.FullScreen = true;

					DD.SetRealScreenSize(DD.TargetMonitor.W, DD.TargetMonitor.H);
				}
				else if (menu.SelectedIndex == sizes.Length + 1)
				{
					break;
				}
				else
				{
					throw null; // never
				}
			}
		}

		private static void ChangeMusicVolume()
		{
			throw new NotImplementedException();
		}

		private static void ChangeSEVolume()
		{
			throw new NotImplementedException();
		}

		private static void ChangeMouseEnabled()
		{
			SimpleMenu menu = new SimpleMenu(30, 40, 30, 570, "マウスの使用／不使用の切り替え", new string[]
			{
				"マウスを使用する",
				"マウスを使用しない",
				"マウスカーソルを表示しない",
				"戻る",
			});

			DD.FreezeInput();

			for (; ; )
			{
				DrawWall();

				if (menu.Draw())
					break;

				DD.EachFrame();
			}
			DD.FreezeInput();

			switch (menu.SelectedIndex)
			{
				case 0:
					GameSetting.MouseCursorShow = true;
					GameSetting.MouseEnabled = true;
					break;

				case 1:
					GameSetting.MouseCursorShow = true;
					GameSetting.MouseEnabled = false;
					break;

				case 2:
					GameSetting.MouseCursorShow = false;
					GameSetting.MouseEnabled = false;
					break;

				case 3:
					break;

				default:
					throw null; // never
			}

			DX.SetMouseDispFlag(GameSetting.MouseCursorShow ? 1 : 0);
		}
	}
}
