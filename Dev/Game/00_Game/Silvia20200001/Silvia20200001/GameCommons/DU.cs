using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DxLibDLL;
using Charlotte.Commons;
using Charlotte.Drawings;

namespace Charlotte.GameCommons
{
	/// <summary>
	/// この名前空間の配下から呼び出される機能をこのクラスに集約する。
	/// </summary>
	public class DU
	{
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
				W = w,
				H = h,
				Handle = handle,
			};
		}

		#region Font

		public static void AddFontFile(string resPath)
		{
			string file = new WorkingDir().GetPath(Path.GetFileName(resPath));
			byte[] fileData = DD.GetResFileData(resPath);

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

		// TODO: 以下DDへ移動するかもしれない。

		public static IEnumerable<T> Reverse<T>(IList<T> list)
		{
			for (int index = list.Count - 1; 0 <= index; index--)
			{
				yield return list[index];
			}
		}

		/// <summary>
		/// レートを十億分率に変換する。
		/// </summary>
		/// <param name="rate">レート</param>
		/// <returns>十億分率</returns>
		public static int RateToPPB(double rate)
		{
			return SCommon.ToRange(SCommon.ToInt(rate * SCommon.IMAX), 0, SCommon.IMAX);
		}

		/// <summary>
		/// 十億分率をレートに変換する。
		/// </summary>
		/// <param name="ppb">十億分率</param>
		/// <returns>レート</returns>
		public static double PPBToRate(int ppb)
		{
			return SCommon.ToRange((double)ppb / SCommon.IMAX, 0.0, 1.0);
		}

		/// <summary>
		/// レートをバイト値(0～255)に変換する。
		/// </summary>
		/// <param name="rate">レート</param>
		/// <returns>バイト値</returns>
		public static int RateToByte(double rate)
		{
			return SCommon.ToRange(SCommon.ToInt(rate * 255.0), 0, 255);
		}

		/// <summary>
		/// バイト値(0～255)をレートに変換する。
		/// </summary>
		/// <param name="value">バイト値</param>
		/// <returns>レート</returns>
		public static double ByteToRate(int value)
		{
			return SCommon.ToRange((double)value / 255.0, 0.0, 1.0);
		}

		public static uint ToDXColor(I3Color color)
		{
			return DX.GetColor(color.R, color.G, color.B);
		}

		public static void Countdown(ref int counter)
		{
			if (0 < counter)
				counter--;
			else if (counter < 0)
				counter++;
		}

		/// <summary>
		/// タスクリストを実行する。
		/// </summary>
		/// <param name="tasks">タスクリスト</param>
		public static void ExecuteTasks(List<Func<bool>> tasks)
		{
			for (int index = 0; index < tasks.Count; )
			{
				if (!tasks[index]())
				{
					tasks[index] = null;
				}
			}
			tasks.RemoveAll(v => v == null);
		}

		/// <summary>
		/// タスクシーケンスを実行する。
		/// </summary>
		/// <param name="tasks">タスクシーケンス</param>
		/// <returns>ビジー状態か(タスクを実行したか)</returns>
		public static bool ExecuteTaskSequence(List<Func<bool>> tasks)
		{
			if (1 <= tasks.Count)
			{
				if (!tasks[0]())
				{
					tasks.RemoveAt(0); // HACK: ボトルネックになるかも。
				}
				return true;
			}
			return false;
		}
	}
}
