using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
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

			SimpleMenu menu = new SimpleMenu(40, 40, 40, 440, "", new string[]
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
						throw null; // TODO
						break;

					case 1:
						throw null; // TODO
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
			Music.Fadeout();

			foreach (Scene scene in Scene.Create(70))
			{
				DrawWall();
				DD.EachFrame();
			}
		}

		private static void Setting()
		{
			SimpleMenu menu = new SimpleMenu(30, 30, 30, 550, "設定", new string[]
			{
				"ゲームパッドのボタン設定",
				"キーボードのキー設定",
				"ウィンドウサイズ変更",
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
						throw null; // TODO
						break;

					case 1:
						throw null; // TODO
						break;

					case 2:
						throw null; // TODO
						break;

					case 3:
						throw null; // TODO
						break;

					case 4:
						throw null; // TODO
						break;

					case 5:
						ChangeMouseEnabled();
						break;

					case 6:
						return;

					default:
						throw null; // never
				}
			}
		}

		private static void ChangeMouseEnabled()
		{
			SimpleMenu menu = new SimpleMenu(30, 30, 30, 570, "マウスの使用／不使用の切り替え", new string[]
			{
				"マウスを使用する",
				"マウスを使用しない",
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
					GameSetting.MouseEnabled = true;
					DX.SetMouseDispFlag(1);
					break;

				case 1:
					GameSetting.MouseEnabled = false;
					DX.SetMouseDispFlag(0);
					break;

				case 2:
					break;

				default:
					throw null; // never
			}
		}
	}
}
