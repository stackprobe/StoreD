using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;
using Charlotte.Drawings;
using Charlotte.GameCommons;

namespace Charlotte.Games
{
	public static class Game
	{
		public static void Run()
		{
			DD.FreezeInput();

			Musics.SunBeams.Play();

			D2Point plPos = new D2Point(GameConfig.ScreenSize.W / 4.0, GameConfig.ScreenSize.H / 2.0);
			List<Func<bool>> shots = new List<Func<bool>>();

			for (; ; )
			{
				{
					double SPEED = 4.0;

					if (1 <= Inputs.A.GetInput())
					{
						SPEED = 2.0;
					}

					double NANAME_SPEED = SPEED * 0.7;

					bool dir2 = 1 <= Inputs.DIR_2.GetInput();
					bool dir4 = 1 <= Inputs.DIR_4.GetInput();
					bool dir6 = 1 <= Inputs.DIR_6.GetInput();
					bool dir8 = 1 <= Inputs.DIR_8.GetInput();

					if (dir8 && dir4)
					{
						plPos.X -= NANAME_SPEED;
						plPos.Y -= NANAME_SPEED;
					}
					else if (dir8 && dir6)
					{
						plPos.X += NANAME_SPEED;
						plPos.Y -= NANAME_SPEED;
					}
					else if (dir2 && dir4)
					{
						plPos.X -= NANAME_SPEED;
						plPos.Y += NANAME_SPEED;
					}
					else if (dir2 && dir6)
					{
						plPos.X += NANAME_SPEED;
						plPos.Y += NANAME_SPEED;
					}
					else if (dir8)
					{
						plPos.Y -= SPEED;
					}
					else if (dir2)
					{
						plPos.Y += SPEED;
					}
					else if (dir4)
					{
						plPos.X -= SPEED;
					}
					else if (dir6)
					{
						plPos.X += SPEED;
					}
					else
					{
						plPos.X = (double)SCommon.ToInt(plPos.X);
						plPos.Y = (double)SCommon.ToInt(plPos.Y);
					}

					plPos.X = SCommon.ToRange(plPos.X, 0.0, (double)GameConfig.ScreenSize.W);
					plPos.Y = SCommon.ToRange(plPos.Y, 0.0, (double)GameConfig.ScreenSize.H);
				}

				if (1 <= Inputs.B.GetInput() && Inputs.B.GetInput() % 10 == 1)
				{
					double x = plPos.X;
					double y = plPos.Y;

					shots.Add(() =>
					{
						x += 7.0;

						DD.SetZoom(0.2);
						DD.Draw(Pictures.Dummy, new D2Point(x, y));

						return Crash.IsCrashed_Circle_Rect(
							new D2Point(x, y),
							10.0,
							new I4Rect(0, 0, GameConfig.ScreenSize.W, GameConfig.ScreenSize.H).ToD4Rect()
							);
					});
				}

				if (Inputs.PAUSE.GetInput() == 1)
				{
					Pause();
				}
				if (Inputs.START.GetInput() == 1)
				{
					break;
				}

				DD.SetBright(new I3Color(0, 128, 0).ToD3Color());
				DD.Draw(Pictures.WhiteBox, new I4Rect(0, 0, GameConfig.ScreenSize.W, GameConfig.ScreenSize.H).ToD4Rect());

				DD.ExecuteTasks(shots);

				DD.SetRotate(DD.ProcFrame / 10.0);
				DD.Draw(Pictures.Dummy, plPos);

				DD.EachFrame();
			}
			DD.FreezeInput();
		}

		private static SubScreen PauseWall = new SubScreen(GameConfig.ScreenSize.W, GameConfig.ScreenSize.H);

		private static void Pause()
		{
			PauseWall.ChangeDrawScreenToThis();
			DD.Draw(DD.LastMainScreen.GetPicture(), new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());
			DD.MainScreen.ChangeDrawScreenToThis();

			SimpleMenu menu = new SimpleMenu(24, 30, 16, 200, "PAUSE", new string[]
 			{
				"Save",
				"Load",
				"Buy",
				"RETURN",
			});

			menu.NoPound = true;
			menu.CancelByPause = true;

			foreach (Input input in Inputs.GetAllInput())
				input.FreezeInputUntilRelease();

			for (; ; )
			{
				DD.FreezeInput();

				double frostRate = 0.0;

				for (; ; )
				{
					DD.Approach(ref frostRate, 0.5, 0.98);

					DD.Draw(PauseWall.GetPicture(), new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());
					DD.Frosting(frostRate);

					if (menu.Draw())
						break;

					DD.EachFrame();
				}
				DD.FreezeInput();

				switch (menu.SelectedIndex)
				{
					case 0:
						SoundEffects.Save.Play();
						break;

					case 1:
						SoundEffects.Load.Play();
						break;

					case 2:
						SoundEffects.Buy.Play();
						break;

					case 3:
						goto endOfMenu;

					default:
						throw null; // never
				}
			}
		endOfMenu:

			foreach (Input input in Inputs.GetAllInput())
				input.FreezeInputUntilRelease();

			Inputs.DIR_2.UnfreezeInputUntilRelease();
			Inputs.DIR_4.UnfreezeInputUntilRelease();
			Inputs.DIR_6.UnfreezeInputUntilRelease();
			Inputs.DIR_8.UnfreezeInputUntilRelease();

			PauseWall.Unload();
		}
	}
}
