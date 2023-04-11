using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;
using Charlotte.Games;

namespace Charlotte
{
	public class TarutoGameMain
	{
		public static void GameMain()
		{
			if (ProcMain.DEBUG)
			{
				// -- choose one --

				Logo.Run();
				//TitleMenu.Run();
				//new Test0001().Test01();
				//new Test0001().Test02();
				//new Test0001().Test03();

				// --
			}
			else
			{
				Logo.Run();
			}
		}
	}
}
