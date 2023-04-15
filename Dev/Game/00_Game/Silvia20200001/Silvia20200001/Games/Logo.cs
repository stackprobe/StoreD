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
			double z1 = 0.3;
			double z2 = 2.0;
			double z3 = 3.7;

			foreach (Scene scene in Scene.Create(60))
			{
				DD.DrawCurtain(-1.0);

				DD.SetAlpha(scene.Rate);
				DD.SetZoom(z1);
				DD.Draw(Pictures.Copyright, new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());

				DD.SetAlpha((1.0 - scene.Rate) * 0.7);
				DD.SetZoom(0.8 + 0.5 * scene.Rate);
				DD.Draw(Pictures.Copyright, new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());

				DD.SetAlpha((1.0 - scene.Rate) * 0.5);
				DD.SetZoom(z2);
				DD.Draw(Pictures.Copyright, new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());

				DD.SetAlpha((1.0 - scene.Rate) * 0.3);
				DD.SetZoom(z3);
				DD.Draw(Pictures.Copyright, new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());

				DD.Approach(ref z1, 1.0, 0.9);
				DD.Approach(ref z2, 1.0, 0.98);
				DD.Approach(ref z3, 1.0, 0.95);

				DD.EachFrame();
			}
			DateTime logoShowedTime = DateTime.Now;
			Touch();

			while ((DateTime.Now - logoShowedTime).TotalSeconds < 1.5)
			{
				DD.DrawCurtain(-1.0);
				DD.Draw(Pictures.Copyright, new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());

				DD.EachFrame();
			}
			foreach (Scene scene in Scene.Create(60))
			{
				DD.DrawCurtain(-1.0);

				DD.SetAlpha((1.0 - scene.Rate) * 0.5);
				DD.SetRotate(scene.Rate * -0.1);
				DD.SetZoom(1.0 - 0.3 * scene.Rate);
				DD.Draw(Pictures.Copyright, new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());

				DD.SetAlpha((1.0 - scene.Rate) * 0.5);
				DD.SetRotate(scene.Rate * 0.1);
				DD.SetZoom(1.0 + 0.8 * scene.Rate);
				DD.Draw(Pictures.Copyright, new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());

				DD.SetAlpha((1.0 - scene.Rate) * 0.3);
				DD.Draw(Pictures.Copyright, new D2Point(GameConfig.ScreenSize.W / 2.0 + scene.Rate * 100.0, GameConfig.ScreenSize.H / 2.0));

				DD.SetAlpha((1.0 - scene.Rate) * 0.3);
				DD.Draw(Pictures.Copyright, new D2Point(GameConfig.ScreenSize.W / 2.0, GameConfig.ScreenSize.H / 2.0 + scene.Rate * 50.0));

				DD.EachFrame();
			}
			TitleMenu.Run();
		}

		private static void Touch()
		{
			// none
		}
	}
}
