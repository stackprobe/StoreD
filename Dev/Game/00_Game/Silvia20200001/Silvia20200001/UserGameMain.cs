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
			for (; ; )
			{
				DX.ClearDrawScreen();

				DD.SetRotate(DD.ProcFrame / 10.0);
				DD.Draw(Pictures.Dummy, GameConfig.ScreenSize.W / 2.0, GameConfig.ScreenSize.H / 2.0);

				DD.EachFrame();
			}
		}
	}
}
