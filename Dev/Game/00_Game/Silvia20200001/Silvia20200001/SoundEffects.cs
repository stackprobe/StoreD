using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.GameCommons;

namespace Charlotte
{
	public static class SoundEffects
	{
		public static object TOUCH_HANDLE = new object();

		public static SoundEffect Save = new SoundEffect(@"General\SoundEffect\ユーフルカ\save-01.wav");
		public static SoundEffect Load = new SoundEffect(@"General\SoundEffect\ユーフルカ\load.wav");
		public static SoundEffect Buy = new SoundEffect(@"General\SoundEffect\ユーフルカ\shop.wav");
	}
}
