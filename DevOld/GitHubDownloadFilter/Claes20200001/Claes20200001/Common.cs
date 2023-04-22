using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Charlotte.Commons;

namespace Charlotte
{
	public static class Common
	{
		private static bool[,] JChars = null;

		/// <summary>
		/// SJISの2バイト文字か判定する。
		/// </summary>
		/// <param name="lead">第1バイト</param>
		/// <param name="trail">第2バイト</param>
		/// <returns>SJISの2バイト文字か</returns>
		public static bool IsJChar(byte lead, byte trail)
		{
			if (JChars == null)
			{
				JChars = new bool[256, 256];

				foreach (UInt16 chr in SCommon.GetJCharCodes())
				{
					JChars[chr >> 8, chr & 0xff] = true;
				}
			}
			return JChars[lead, trail];
		}

		private static bool[] UnicodeJChars = null;

		/// <summary>
		/// Unicodeの全角文字(SJISの2バイト文字)か判定する。
		/// </summary>
		/// <param name="value">文字コード</param>
		/// <returns>Unicodeの全角文字(SJISの2バイト文字)か</returns>
		public static bool IsUnicodeJChar(UInt16 value)
		{
			if (UnicodeJChars == null)
			{
				UnicodeJChars = new bool[65536];

				foreach (char chr in SCommon.GetJChars())
				{
					UnicodeJChars[(int)chr] = true;
				}
			}
			return UnicodeJChars[(int)value];
		}
	}
}
