using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

			SimpleMenu menu = new SimpleMenu(40, 40, 40, 440, new string[]
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
						Run_Setting();
						break;

					case 3:
						return;

					default:
						throw new Exception("Bad SelectedIndex");
				}
			}
		}

		private static void Run_Setting()
		{
			SimpleMenu menu = new SimpleMenu(30, 30, 30, 550, new string[]
			{
				"ゲームパッドのボタン設定",
				"キーボードのキー設定",
				"ウィンドウサイズ変更",
				"ＢＭＧ音量",
				"ＳＥ音量",
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
					break;

				default:
					throw new Exception("Bad SelectedIndex");
			}
		}
	}
}
