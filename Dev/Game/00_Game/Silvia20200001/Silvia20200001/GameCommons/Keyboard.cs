using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;

namespace Charlotte.GameCommons
{
	public class Keyboard
	{
		private const int KEY_MAX = 256;

		private static int[] Counters = new int[KEY_MAX];
		private static byte[] Statuses = new byte[KEY_MAX];

		public static void Initialize()
		{
			DU.Pin(Statuses);
		}

		public static void EachFrame()
		{
			if (DD.WindowIsActive)
			{
				if (DX.GetHitKeyStateAll(Statuses) != 0) // ? 失敗
					throw new Exception("GetHitKeyStateAll failed");

				for (int index = 0; index < KEY_MAX; index++)
					DU.UpdateButtonCounter(ref Counters[index], Statuses[index] != 0);
			}
			else
			{
				for (int index = 0; index < KEY_MAX; index++)
					DU.UpdateButtonCounter(ref Counters[index], false);
			}
		}

		private const int _DUMMY_キーのフィールド名を探すためにF12で飛んで行く用 = DX.KEY_INPUT_0;

		public static int GetInput(int key)
		{
			// key == DX.KEY_INPUT_RETURN etc.

			return 1 <= DD.FreezeInputFrame ? 0 : Counters[key];
		}

		public static bool IsPound(int key)
		{
			return DU.IsPound(GetInput(key));
		}
	}
}
