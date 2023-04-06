using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Charlotte
{
	public static class Consts
	{
		/// <summary>
		/// 入力ルートDIR
		/// </summary>
		public static string R_ROOT_DIR
		{
			get
			{
				if (_rRootDir == null)
				{
					string dir1 = @"C:\pleiades";
					string dir2fmt = "^[0-9]{4}-[0-9]{2}$";
					string dir3 = "workspace";

					if (!Directory.Exists(dir1))
						throw new Exception("no dir1");

					string[] eDir2 = Directory.GetDirectories(dir1).Where(v => Regex.IsMatch(Path.GetFileName(v), dir2fmt)).ToArray();
					string[] eDir3 = eDir2.Select(v => Path.Combine(v, dir3)).Where(v => Directory.Exists(v)).ToArray();

					if (eDir3.Length != 1)
						throw new Exception("no dir2 or no dir3");

					_rRootDir = eDir3[0];
				}
				return _rRootDir;
			}
		}

		private static string _rRootDir = null;

		/// <summary>
		/// 出力ルートDIR の フォーマット
		/// </summary>
		public const string W_ROOT_DIR_FORMAT = @"C:\home\GitHub\Store{0}\workspace";
	}
}
