using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;
using Charlotte.Drawings;
using Charlotte.GameCommons;

namespace Charlotte
{
	public static class GameSetting
	{
		public static I2Size UserScreenSize;
		public static bool FullScreen = false;
		public static bool MouseCursorShow = true;
		public static bool MouseEnabled = true;
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

			// ---- このクラス内の項目ここから ----

			dest.Add(UserScreenSize.W);
			dest.Add(UserScreenSize.H);
			dest.Add(FullScreen);
			dest.Add(MouseCursorShow);
			dest.Add(MouseEnabled);
			dest.Add(DD.RateToPPB(MusicVolume));
			dest.Add(DD.RateToPPB(SEVolume));

			// ---- このクラス内の項目ここまで ----

			// ---- 他クラスの情報ここから ----

			foreach (Input input in Inputs.GetAllInput())
			{
				dest.Add(input.Key);
				dest.Add(input.Button);
			}

			// ---- 他クラスの情報ここまで ----

			dest.Insert(0, dest.Count + 1);

			return SCommon.Serializer.I.Join(dest.Select(v => v.ToString()).ToArray());
		}

		public static void Deserialize(string serializedString)
		{
			string[] src = SCommon.Serializer.I.Split(serializedString);
			int c = 0;

			if (int.Parse(src[c++]) != src.Length)
				throw new Exception("Bad Length");

			// ---- このクラス内の項目ここから ----

			UserScreenSize.W = SCommon.ToRange(int.Parse(src[c++]), 1, SCommon.IMAX);
			UserScreenSize.H = SCommon.ToRange(int.Parse(src[c++]), 1, SCommon.IMAX);
			FullScreen = bool.Parse(src[c++]);
			MouseCursorShow = bool.Parse(src[c++]);
			MouseEnabled = bool.Parse(src[c++]);
			MusicVolume = DD.PPBToRate(int.Parse(src[c++]));
			SEVolume = DD.PPBToRate(int.Parse(src[c++]));

			// ---- このクラス内の項目ここまで ----

			// ---- 他クラスの情報ここから ----

			foreach (Input input in Inputs.GetAllInput())
			{
				input.Key = SCommon.ToRange(int.Parse(src[c++]), 0, Keyboard.KEY_MAX - 1);
				input.Button = SCommon.ToRange(int.Parse(src[c++]), 0, Pad.BUTTON_MAX - 1);
			}

			// ---- 他クラスの情報ここまで ----

			if (c != src.Length)
				throw new Exception("Length error");
		}
	}
}
