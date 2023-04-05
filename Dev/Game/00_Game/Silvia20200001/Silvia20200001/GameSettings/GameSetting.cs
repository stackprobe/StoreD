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

		// ===============
		// ここからアプリ固有
		// ===============

		// ----

		public static void Initialize()
		{
			UserScreenSize = GameConfig.ScreenSize;
		}

		public static string Serialize()
		{
			List<object> dest = new List<object>();

			// ----

			dest.Add(UserScreenSize.W);
			dest.Add(UserScreenSize.H);
			dest.Add(FullScreen ? 1 : 0);

			// ===============
			// ここからアプリ固有
			// ===============

			// ----

			return SCommon.Serializer.I.Join(dest.Select(v => v.ToString()).ToArray());
		}

		public static void Deserialize(string serializedString)
		{
			string[] src = SCommon.Serializer.I.Split(serializedString);
			int c = 0;

			// ----

			UserScreenSize.W = int.Parse(src[c++]);
			UserScreenSize.H = int.Parse(src[c++]);
			FullScreen = int.Parse(src[c++]) == 1;

			// ===============
			// ここからアプリ固有
			// ===============

			// ----

			if (c != src.Length)
				throw new Exception("Bad serializedString");
		}
	}
}
