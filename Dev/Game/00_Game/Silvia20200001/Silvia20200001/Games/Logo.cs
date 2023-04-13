using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using Charlotte.Drawings;
using Charlotte.GameCommons;

namespace Charlotte.Games
{
	public static class Logo
	{
		public static void Run()
		{
			DD.SetCurtain(-1.0);
			DD.SetCurtainTarget(0.0);

			foreach (DD.Scene scene in DD.CreateScene(40))
			{
				DrawLogo();
				DD.EachFrame();
			}
			DateTime logoShowedTime = DateTime.Now;
			Touch();

			for (; ; )//while ((DateTime.Now - logoShowedTime).TotalSeconds < 1.0)
			{
				DrawLogo();
				DD.EachFrame();
			}
			DD.SetCurtainTarget(-1.0);

			foreach (DD.Scene scene in DD.CreateScene(40))
			{
				DrawLogo();
				DD.EachFrame();
			}
			TitleMenu.Run();
		}

		private static void DrawLogo()
		{
			DD.DrawCurtain(-1.0);
			DD.Draw(Pictures.Copyright, new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());



			/*
			DX.DrawGraph(
				100,
				100,
				Pictures.Copyright.GetHandle(),
				1
				);
			DX.DrawGraph(
				100,
				100,
				Pictures.Copyright.GetHandle(),
				1
				);
			 * */
		}

		private static void Touch()
		{
			// none
		}
	}
}
