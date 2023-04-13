using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.GameCommons;
using Charlotte.Drawings;

namespace Charlotte.Games
{
	public static class TitleMenu
	{
		public static void Run()
		{
			// TODO
			// TODO
			// TODO

			DD.SetCurtain(-1.0);
			DD.SetCurtainTarget(0.0);

			Musics.RemotestLibrary.Play();



			for (; ; )
			{
				DD.SetRotate(DD.ProcFrame / 10.0);
				DD.Draw(Pictures.Dummy, new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());

				if (DD.ProcFrame == 200)
					SoundEffects.Save.Play();

				if (DD.ProcFrame == 300)
					SoundEffects.Load.Play();

				if (DD.ProcFrame == 400)
					SoundEffects.Buy.Play();

				DD.EachFrame();
			}
		}
	}
}
