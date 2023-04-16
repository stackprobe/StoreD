using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DxLibDLL;
using Charlotte.Commons;
using Charlotte.Drawings;

namespace Charlotte.GameCommons
{
	/// <summary>
	/// この名前空間内で使用される共通機能・便利機能をこのクラスに集約する。
	/// </summary>
	public static class DU
	{
		public class CoffeeBreak : Exception
		{ }

		private static WorkingDir _wd = null;

		/// <summary>
		/// 各機能自由に使ってよい作業フォルダ
		/// </summary>
		public static WorkingDir WD
		{
			get
			{
				if (_wd == null)
					_wd = new WorkingDir();

				return _wd;
			}
		}

		/// <summary>
		/// 各機能自由に使ってよいスクリーン
		/// </summary>
		public static SubScreen FreeScreen = new SubScreen(GameConfig.ScreenSize.W, GameConfig.ScreenSize.H);

		public static void Pin<T>(T data)
		{
			GCHandle h = GCHandle.Alloc(data, GCHandleType.Pinned);

			DD.Finalizers.Add(() =>
			{
				h.Free();
			});
		}

		public static void PinOn<T>(T data, Action<IntPtr> routine)
		{
			GCHandle pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
			try
			{
				routine(pinnedData.AddrOfPinnedObject());
			}
			finally
			{
				pinnedData.Free();
			}
		}

		private static I2Point GetMousePosition()
		{
			return new I2Point(Cursor.Position.X, Cursor.Position.Y);
		}

		private static I4Rect[] Monitors = null;

		private static I4Rect[] GetAllMonitor()
		{
			if (Monitors == null)
			{
				Monitors = Screen.AllScreens.Select(screen => new I4Rect(
					screen.Bounds.Left,
					screen.Bounds.Top,
					screen.Bounds.Width,
					screen.Bounds.Height
					))
					.ToArray();
			}
			return Monitors;
		}

		private static I2Point GetMainWindowPosition()
		{
			Win32APIWrapper.POINT p;

			p.X = 0;
			p.Y = 0;

			Win32APIWrapper.W_ClientToScreen(Win32APIWrapper.GetMainWindowHandle(), out p);

			return new I2Point(p.X, p.Y);
		}

		private static I2Point GetMainWindowCenterPosition()
		{
			I2Point p = GetMainWindowPosition();

			p.X += DD.RealScreenSize.W / 2;
			p.Y += DD.RealScreenSize.H / 2;

			return p;
		}

		/// <summary>
		/// 起動時におけるターゲット画面を取得する。
		/// </summary>
		/// <returns>画面の領域</returns>
		public static I4Rect GetTargetMonitor_Boot()
		{
			I2Point mousePos = GetMousePosition();

			foreach (I4Rect monitor in GetAllMonitor())
			{
				if (
					monitor.L <= mousePos.X && mousePos.X < monitor.R &&
					monitor.T <= mousePos.Y && mousePos.Y < monitor.B
					)
					return monitor;
			}
			return GetAllMonitor()[0]; // 何故か見つからない -> 適当な画面を返す。
		}

		/// <summary>
		/// 現在のターゲット画面を取得する。
		/// </summary>
		/// <returns>画面の領域</returns>
		public static I4Rect GetTargetMonitor()
		{
			I2Point mainWinCenterPt = GetMainWindowCenterPosition();

			foreach (I4Rect monitor in GetAllMonitor())
			{
				if (
					monitor.L <= mainWinCenterPt.X && mainWinCenterPt.X < monitor.R &&
					monitor.T <= mainWinCenterPt.Y && mainWinCenterPt.Y < monitor.B
					)
					return monitor;
			}
			return GetAllMonitor()[0]; // 何故か見つからない -> 適当な画面を返す。
		}

		public static void SetMainWindowPosition(int l, int t)
		{
			DX.SetWindowPosition(l, t);

			I2Point p = DU.GetMainWindowPosition();

			l += l - p.X;
			t += t - p.Y;

			DX.SetWindowPosition(l, t);
		}

		/// <summary>
		/// コンピュータを起動してから経過した時間を返す。
		/// 単位：ミリ秒
		/// </summary>
		/// <returns>時間(ミリ秒)</returns>
		public static long GetCurrentTime()
		{
			return DX.GetNowHiPerformanceCount() / 1000L;
		}

		public static Picture.PictureDataInfo GetPictureData(byte[] fileData)
		{
			int softImage = -1;

			DU.PinOn(fileData, p => softImage = DX.LoadSoftImageToMem(p, fileData.Length));

			if (softImage == -1)
				throw new Exception("LoadSoftImageToMem failed");

			int w;
			int h;

			if (DX.GetSoftImageSize(softImage, out w, out h) != 0) // ? 失敗
				throw new Exception("GetSoftImageSize failed");

			if (w < 1 || SCommon.IMAX < w)
				throw new Exception("Bad w");

			if (h < 1 || SCommon.IMAX < h)
				throw new Exception("Bad h");

			// RGB -> RGBA
			{
				int newSoftImage = DX.MakeARGB8ColorSoftImage(w, h);

				if (newSoftImage == -1) // ? 失敗
					throw new Exception("MakeARGB8ColorSoftImage failed");

				if (DX.BltSoftImage(0, 0, w, h, softImage, 0, 0, newSoftImage) != 0) // ? 失敗
					throw new Exception("BltSoftImage failed");

				if (DX.DeleteSoftImage(softImage) != 0) // ? 失敗
					throw new Exception("DeleteSoftImage failed");

				softImage = newSoftImage;
			}

			int handle = DX.CreateGraphFromSoftImage(softImage);

			if (handle == -1) // ? 失敗
				throw new Exception("CreateGraphFromSoftImage failed");

			if (DX.DeleteSoftImage(softImage) != 0) // ? 失敗
				throw new Exception("DeleteSoftImage failed");

			return new Picture.PictureDataInfo()
			{
				Handle = handle,
				W = w,
				H = h,
			};
		}

		#region Font

		public static void AddFontFile(string resPath)
		{
			string dir = DU.WD.MakePath();
			string file = Path.Combine(dir, Path.GetFileName(resPath));
			byte[] fileData = DD.GetResFileData(resPath);

			SCommon.CreateDir(dir);
			File.WriteAllBytes(file, fileData);

			P_AddFontFile(file);

			DD.Finalizers.Add(() => P_RemoveFontFile(file));
		}

		private static void P_AddFontFile(string file)
		{
			if (Win32APIWrapper.W_AddFontResourceEx(file, Win32APIWrapper.FR_PRIVATE, IntPtr.Zero) == 0) // ? 失敗
				throw new Exception("W_AddFontResourceEx failed");
		}

		private static void P_RemoveFontFile(string file)
		{
			if (Win32APIWrapper.W_RemoveFontResourceEx(file, Win32APIWrapper.FR_PRIVATE, IntPtr.Zero) == 0) // ? 失敗
				throw new Exception("W_RemoveFontResourceEx failed");
		}

		public static int GetFontHandle(string fontName, int fontSize)
		{
			if (string.IsNullOrEmpty(fontName))
				throw new Exception("Bad fontName");

			if (fontSize < 1 || SCommon.IMAX < fontSize)
				throw new Exception("Bad fontSize");

			return Fonts.GetHandle(fontName, fontSize);
		}

		public static void UnloadAllFontHandle()
		{
			Fonts.UnloadAll();
		}

		private static class Fonts
		{
			private static Dictionary<string, int> Handles = SCommon.CreateDictionary<int>();

			private static string GetKey(string fontName, int fontSize)
			{
				return string.Join("_", fontName, fontSize);
			}

			public static int GetHandle(string fontName, int fontSize)
			{
				string key = GetKey(fontName, fontSize);

				if (!Handles.ContainsKey(key))
					Handles.Add(key, CreateHandle(fontName, fontSize));

				return Handles[key];
			}

			public static void UnloadAll()
			{
				foreach (int handle in Handles.Values)
					ReleaseHandle(handle);

				Handles.Clear();
			}

			private static int CreateHandle(string fontName, int fontSize)
			{
				int handle = DX.CreateFontToHandle(
					fontName,
					fontSize,
					6,
					DX.DX_FONTTYPE_ANTIALIASING_8X8,
					-1,
					0
					);

				if (handle == -1) // ? 失敗
					throw new Exception("CreateFontToHandle failed");

				return handle;
			}

			private static void ReleaseHandle(int handle)
			{
				if (DX.DeleteFontToHandle(handle) != 0) // ? 失敗
					throw new Exception("DeleteFontToHandle failed");
			}
		}

		#endregion

		public static void UpdateButtonCounter(ref int counter, bool status)
		{
			if (1 <= counter) // ? 前回は押していた。
			{
				if (status) // ? 今回も押している。
				{
					counter++; // 押している。
				}
				else // ? 今回は離している。
				{
					counter = -1; // 離し始めた。
				}
			}
			else // ? 前回は離していた。
			{
				if (status) // ? 今回は押している。
				{
					counter = 1; // 押し始めた。
				}
				else // ? 今回も離している。
				{
					counter = 0; // 離している。
				}
			}
		}

		private const int POUND_FIRST_DELAY = 17;
		private const int POUND_DELAY = 4;

		public static bool IsPound(int count)
		{
			return count == 1 || POUND_FIRST_DELAY < count && (count - POUND_FIRST_DELAY) % POUND_DELAY == 1;
		}

		public static class Hasher
		{
			private static byte[] COUNTER_SHUFFLE = Encoding.ASCII.GetBytes("Gattonero-2023-04-05_COUNTER_SHUFFLE_{e43e01aa-ca4f-43d3-8be7-49cd60e9415e}_");
			private const int HASH_SIZE = 20;

			public static byte[] AddHash(byte[] data)
			{
				if (data == null)
					throw new Exception("Bad data");

				return SCommon.Join(new byte[][] { data, GetHash(data) });
			}

			public static byte[] UnaddHash(byte[] data)
			{
				try
				{
					return UnaddHash_Main(data);
				}
				catch (Exception e)
				{
					throw new Exception("読み込まれたデータは破損しているかバージョンが異なります。", e);
				}
			}

			private static byte[] UnaddHash_Main(byte[] data)
			{
				if (data == null)
					throw new Exception("Bad data");

				if (data.Length < HASH_SIZE)
					throw new Exception("Bad Length");

				byte[] rDat = data.Take(data.Length - HASH_SIZE).ToArray();
				byte[] hash = data.Skip(data.Length - HASH_SIZE).ToArray();
				byte[] recalcedHash = GetHash(rDat);

				if (SCommon.Comp(hash, recalcedHash) != 0)
					throw new Exception("Bad hash");

				return rDat;
			}

			private static byte[] GetHash(byte[] data)
			{
				return Encoding.ASCII.GetBytes(SCommon.Base64.I.Encode(SCommon.GetSHA512(new byte[][] { COUNTER_SHUFFLE, data }).Take(15).ToArray()));
			}
		}

		public static void StoreAllSubScreen()
		{
			foreach (SubScreen screen in SubScreen.GetAllSubScreen())
			{
				if (screen.IsLoaded())
				{
					string bmpFile = WD.MakePath();

					DX.SetDrawScreen(screen.GetHandle());
					DX.SaveDrawScreenToBMP(0, 0, screen.W, screen.H, bmpFile);

					screen.StoredObject = bmpFile;
				}
			}
			DX.SetDrawScreen(DX.DX_SCREEN_BACK);
		}

		public static void RestoreAllSubScreen()
		{
			foreach (SubScreen screen in SubScreen.GetAllSubScreen())
			{
				if (screen.StoredObject != null)
				{
					string bmpFile = (string)screen.StoredObject;

					screen.StoredObject = null;

					int handle = DU.GetPictureData(File.ReadAllBytes(bmpFile)).Handle;
					DX.SetDrawScreen(screen.GetHandle());
					DX.DrawExtendGraph(0, 0, screen.W, screen.H, handle, 0);
					DX.DeleteGraph(handle);

					SCommon.DeletePath(bmpFile);
				}
			}
			DX.SetDrawScreen(DX.DX_SCREEN_BACK);
		}

		public static string[] GetKeyboardKeyNames()
		{
			return KeyboardKeys.GetNames();
		}

		private static class KeyboardKeys
		{
			private static string[] P_Names = null;

			public static string[] GetNames()
			{
				if (P_Names == null)
					P_Names = P_GetNames();

				return P_Names;
			}

			private static string[] P_GetNames()
			{
				string[] names = new string[Keyboard.KEY_MAX];

				for (int index = 0; index < Keyboard.KEY_MAX; index++)
					names[index] = "(" + index + ")";

				foreach (KeyInfo info in GetKeys())
					names[info.Value] = info.Name;

				return names;
			}

			private class KeyInfo
			{
				public string Name;
				public int Value;
			}

			private static KeyInfo[] GetKeys()
			{
				return typeof(DX).GetFields(BindingFlags.Public | BindingFlags.Static)
					.Where(field => field.IsLiteral && !field.IsInitOnly && field.Name.StartsWith("KEY_INPUT_"))
					.Select(field => new KeyInfo() { Name = field.Name.Substring(10), Value = (int)field.GetValue(null) })
					.ToArray();
			}
		}
	}
}
