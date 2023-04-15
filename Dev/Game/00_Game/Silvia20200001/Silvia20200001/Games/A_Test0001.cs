using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;
using Charlotte.Drawings;
using Charlotte.GameCommons;

namespace Charlotte.Games
{
	public static class A_Test0001 // ★要削除
	{
		private static void Pause()
		{
			foreach (Input input in Inputs.GetAllInput())
				input.FreezeInputUntilRelease();

			for (; ; )
			{
				if (Inputs.A.GetInput() == 1)
					break;

				DD.EachFrame();
			}

			foreach (Input input in Inputs.GetAllInput())
				input.FreezeInputUntilRelease();

			Inputs.DIR_2.UnfreezeInputUntilRelease();
			Inputs.DIR_4.UnfreezeInputUntilRelease();
			Inputs.DIR_6.UnfreezeInputUntilRelease();
			Inputs.DIR_8.UnfreezeInputUntilRelease();
		}

		private static SubScreen PauseWall = new SubScreen(GameConfig.ScreenSize.W, GameConfig.ScreenSize.H);

		private static void Pause2()
		{

#if true
			PauseWall.ChangeDrawScreenToThis();
			DD.Draw(DD.LastMainScreen.GetPicture(), new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());
			DD.MainScreen.ChangeDrawScreenToThis();
#elif true
			SCommon.Swap(ref PauseWall, ref DD.LastMainScreen);
#else
			DD.KeepLastMainScreen();
			SCommon.Swap(ref PauseWall, ref DD.KeptMainScreen);
#endif

			DD.FreezeInput();

			for (; ; )
			{
				DD.Draw(PauseWall.GetPicture(), new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());

				if (Inputs.A.GetInput() == 1)
					break;

				DD.EachFrame();
			}
			DD.FreezeInput();

			PauseWall.Unload(); // ★★★ 使い終わったらアンロードすること。
		}
	}
}
