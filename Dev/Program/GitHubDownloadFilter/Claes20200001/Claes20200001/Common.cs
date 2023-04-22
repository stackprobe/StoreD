using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;
using System.IO;

namespace Charlotte
{
	public static class Common
	{
		public static bool ExtIs(string file, string ext)
		{
			return SCommon.EqualsIgnoreCase(Path.GetExtension(file), ext);
		}

		private static bool[,] S_JChars = null;

		/// <summary>
		/// SJISの2バイト文字か判定する。
		/// </summary>
		/// <param name="lead">第1バイト</param>
		/// <param name="trail">第2バイト</param>
		/// <returns>SJISの2バイト文字か</returns>
		public static bool IsJChar(byte lead, byte trail)
		{
			if (S_JChars == null)
			{
				S_JChars = new bool[256, 256];

				foreach (UInt16 chr in SCommon.GetJCharCodes())
				{
					S_JChars[chr >> 8, chr & 0xff] = true;
				}
			}
			return S_JChars[lead, trail];
		}

		private static bool[] S_UnicodeJChars = null;

		/// <summary>
		/// Unicodeの全角文字(SJISの2バイト文字)か判定する。
		/// </summary>
		/// <param name="value">文字コード</param>
		/// <returns>Unicodeの全角文字(SJISの2バイト文字)か</returns>
		public static bool IsUnicodeJChar(UInt16 value)
		{
			if (S_UnicodeJChars == null)
			{
				S_UnicodeJChars = new bool[65536];

				foreach (char chr in SCommon.GetJChars())
				{
					S_UnicodeJChars[(int)chr] = true;
				}
			}
			return S_UnicodeJChars[(int)value];
		}
	}
}
