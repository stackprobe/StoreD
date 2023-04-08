using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using Charlotte.Drawings;
using Charlotte.GameCommons;
using Charlotte.GameSettings;

namespace Charlotte
{
	public static class UserGameMain
	{
		public static void GameMain()
		{
			Musics.RemotestLibrary.Play();

			for (; ; )
			{
				DD.SetRotate(DD.ProcFrame / 10.0);
				DD.Draw(Pictures.Dummy, GameConfig.ScreenSize.W / 2.0, GameConfig.ScreenSize.H / 2.0);

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
