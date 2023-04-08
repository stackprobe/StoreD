using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Charlotte.Commons;

namespace Charlotte.GameCommons
{
	public class FontFile
	{
		private static List<FontFile> Instances = new List<FontFile>();
		private static WorkingDir WD = null;

		private string FontFilePath;

		public FontFile(string resPath)
		{
			if (WD == null)
				WD = new WorkingDir();

			string dir = WD.MakePath();
			string file = Path.Combine(dir, Path.GetFileName(resPath));
			byte[] fileData = DD.GetResFileData(resPath);

			SCommon.CreateDir(dir);
			File.WriteAllBytes(file, fileData);

			this.FontFilePath = file;

			if (Win32APIWrapper.W_AddFontResourceEx(this.FontFilePath, Win32APIWrapper.FR_PRIVATE, IntPtr.Zero) == 0) // ? 失敗
				throw new Exception("W_AddFontResourceEx failed");

			Instances.Add(this);

			if (Instances.Count == 1) // ? 初回
			{
				DD.Finalizers.Add(() =>
				{
					foreach (FontFile instance in Instances)
						instance.Release();

					WD.Dispose();
					WD = null;
				});
			}
		}

		private void Release()
		{
			if (Win32APIWrapper.W_RemoveFontResourceEx(this.FontFilePath, Win32APIWrapper.FR_PRIVATE, IntPtr.Zero) == 0) // ? 失敗
				throw new Exception("W_RemoveFontResourceEx failed");
		}
	}
}
