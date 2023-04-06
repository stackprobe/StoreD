using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;
using Charlotte.Drawings;

namespace Charlotte.GameSettings
{
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

		private const string SIGNATURE_FOOTER = "Gattonero-2023-04-05_SaveData_End";

		public static string Serialize()
		{
			List<object> dest = new List<object>();

			// ----

			dest.Add(UserScreenSize.W);
			dest.Add(UserScreenSize.H);
			dest.Add(FullScreen);
			dest.Add(MusicVolume.ToString("R"));
			dest.Add(SEVolume.ToString("R"));

			// ----

			dest.Add(SIGNATURE_FOOTER);

			return SCommon.Serializer.I.Join(dest.Select(v => v.ToString()).ToArray());
		}

		public static void Deserialize(string serializedString)
		{
			string[] src = SCommon.Serializer.I.Split(serializedString);
			int c = 0;

			// ----

			UserScreenSize.W = int.Parse(src[c++]);
			UserScreenSize.H = int.Parse(src[c++]);
			FullScreen = bool.Parse(src[c++]);
			MusicVolume = double.Parse(src[c++]);
			SEVolume = double.Parse(src[c++]);

			// ----

			if (SIGNATURE_FOOTER != src[c++])
				throw new Exception("Bad SIGNATURE_FOOTER");

			if (c != src.Length)
				throw new Exception("Bad Length");
		}
	}
}
