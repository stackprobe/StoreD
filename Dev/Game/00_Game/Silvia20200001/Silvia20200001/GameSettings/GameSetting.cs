using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;
using Charlotte.Drawings;
using Charlotte.GameCommons;

namespace Charlotte.GameSettings
{
	/// <summary>
	/// 変更可能な設定群
	/// </summary>
	public static class GameSetting
	{
		public static I2Size UserScreenSize;
		public static bool FullScreen = false;
		public static double MusicVolume;
		public static double SEVolume;

		public static void Initialize()
		{
			UserScreenSize = GameConfig.ScreenSize;
			MusicVolume = GameConfig.DefaultMusicVolume;
			SEVolume = GameConfig.DefaultSEVolume;
		}

		public static string Serialize()
		{
			List<object> dest = new List<object>();

			// ----

			dest.Add(UserScreenSize.W);
			dest.Add(UserScreenSize.H);
			dest.Add(FullScreen);
			dest.Add(DU.RateToPPB(MusicVolume));
			dest.Add(DU.RateToPPB(SEVolume));

			// ----

			return SCommon.Serializer.I.Join(dest.Select(v => v.ToString()).ToArray());
		}

		public static void Deserialize(string serializedString)
		{
			string[] src = SCommon.Serializer.I.Split(serializedString);
			int c = 0;

			// ----

			UserScreenSize.W = SCommon.ToRange(int.Parse(src[c++]), 1, SCommon.IMAX);
			UserScreenSize.H = SCommon.ToRange(int.Parse(src[c++]), 1, SCommon.IMAX);
			FullScreen = bool.Parse(src[c++]);
			MusicVolume = DU.PPBToRate(int.Parse(src[c++]));
			SEVolume = DU.PPBToRate(int.Parse(src[c++]));

			// ----

			if (c != src.Length)
				throw new Exception("Bad Length");
		}
	}
}
