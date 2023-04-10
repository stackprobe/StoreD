using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Drawings;

namespace Charlotte.GameConfigs
{
	/// <summary>
	/// リリース後に変更不可能な設定
	/// </summary>
	public static class GameConfig
	{
		public static I2Size ScreenSize = new I2Size(960, 540);

		public static double DefaultMusicVolume = 0.45;
		public static double DefaultSEVolume = 0.45;

		public static string[] FontFileResPaths = new string[]
		{
			@"General\Font\K Gothic\K Gothic.ttf",
			@"General\Font\木漏れ日ゴシック\komorebi-gothic.ttf",
		};
	}
}
