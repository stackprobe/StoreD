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

			D2Point plPos = new D2Point(GameConfig.ScreenSize.W / 2.0, GameConfig.ScreenSize.H / 2.0);

			for (; ; )
			{
				{
					const double SPEED = 2.0;
					const double NANAME_SPEED = 1.4;

					bool dir2 = 1 <= Inputs.DIR_2.GetInput();
					bool dir4 = 1 <= Inputs.DIR_4.GetInput();
					bool dir6 = 1 <= Inputs.DIR_6.GetInput();
					bool dir8 = 1 <= Inputs.DIR_8.GetInput();

					if (dir2 && dir4)
					{
						plPos.X -= NANAME_SPEED;
						plPos.Y += NANAME_SPEED;
					}
					else if (dir2 && dir6)
					{
						plPos.X += NANAME_SPEED;
						plPos.Y += NANAME_SPEED;
					}
					else if (dir8 && dir4)
					{
						plPos.X -= NANAME_SPEED;
						plPos.Y -= NANAME_SPEED;
					}
					else if (dir8 && dir6)
					{
						plPos.X += NANAME_SPEED;
						plPos.Y -= NANAME_SPEED;
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
					else if (dir8)
					{
						plPos.Y -= SPEED;
					}
				}

				if (1 <= Inputs.A.GetInput() && Inputs.A.GetInput() % 10 == 1)
				{
					const double SPEED = 3.5;

					foreach (int xa in new int[] { -1, 1 })
					{
						foreach (int ya in new int[] { -1, 1 })
						{
							double x = plPos.X;
							double y = plPos.Y;

							double xSpeed = xa * SPEED;
							double ySpeed = ya * SPEED;

							DD.EL.Add(() =>
							{
								x += xSpeed;
								y += ySpeed;

								DD.SetZoom(0.2);
								DD.Draw(Pictures.Dummy, new D2Point(x, y));

								return Crash.IsCrashed_Circle_Rect(
									new D2Point(x, y),
									10.0,
									new I4Rect(0, 0, GameConfig.ScreenSize.W, GameConfig.ScreenSize.H).ToD4Rect()
									);
							});
						}
					}
				}
				if (1 <= Inputs.B.GetInput() && Inputs.B.GetInput() % 10 == 1)
				{
					const double SPEED = 5.0;

					Action<int, int> r = (xa, ya) =>
					{
						double x = plPos.X;
						double y = plPos.Y;

						double xSpeed = xa * SPEED;
						double ySpeed = ya * SPEED;

						DD.EL.Add(() =>
						{
							x += xSpeed;
							y += ySpeed;

							DD.SetZoom(0.2);
							DD.Draw(Pictures.Dummy, new D2Point(x, y));

							return Crash.IsCrashed_Circle_Rect(
								new D2Point(x, y),
								10.0,
								new I4Rect(0, 0, GameConfig.ScreenSize.W, GameConfig.ScreenSize.H).ToD4Rect()
								);
						});
					};

					r(-1, 0);
					r(0, -1);
					r(1, 0);
					r(0, 1);
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

			SimpleMenu menu = new SimpleMenu(24, 30, 16, 300, "PAUSE", new string[]
 			{
				"ITEM-01",
				"ITEM-02",
				"ITEM-03",
				"RETURN",
			});

			menu.NoPound = true;
			menu.CancelByPause = true;

			foreach (Input input in Inputs.GetAllInput())
				input.FreezeInputUntilRelease();

			for (; ; )
			{
				DD.FreezeInput();

				for (; ; )
				{
					DD.Draw(PauseWall.GetPicture(), new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());

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
