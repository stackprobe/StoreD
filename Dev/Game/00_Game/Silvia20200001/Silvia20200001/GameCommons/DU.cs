using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using DxLibDLL;
using Charlotte.Commons;
using Charlotte.Drawings;
using System.Windows.Forms;

namespace Charlotte.GameCommons
{
	/// <summary>
	/// この名前空間内から呼び出される機能群
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
			int siHandle = -1;

			DU.PinOn(fileData, p => siHandle = DX.LoadSoftImageToMem(p, fileData.Length));

			if (siHandle == -1)
				throw new Exception("LoadSoftImageToMem failed");

			int w;
			int h;

			if (DX.GetSoftImageSize(siHandle, out w, out h) != 0) // ? 失敗
				throw new Exception("GetSoftImageSize failed");

			if (w < 1 || SCommon.IMAX < w)
				throw new Exception("Bad w");

			if (h < 1 || SCommon.IMAX < h)
				throw new Exception("Bad h");

			// RGB -> RGBA
			{
				int newSIHandle = DX.MakeARGB8ColorSoftImage(w, h);

				if (newSIHandle == -1) // ? 失敗
					throw new Exception("MakeARGB8ColorSoftImage failed");

				if (DX.BltSoftImage(0, 0, w, h, siHandle, 0, 0, newSIHandle) != 0) // ? 失敗
					throw new Exception("BltSoftImage failed");

				if (DX.DeleteSoftImage(siHandle) != 0) // ? 失敗
					throw new Exception("DeleteSoftImage failed");

				siHandle = newSIHandle;
			}

			int handle = DX.CreateGraphFromSoftImage(siHandle);

			if (handle == -1) // ? 失敗
				throw new Exception("CreateGraphFromSoftImage failed");

			if (DX.DeleteSoftImage(siHandle) != 0) // ? 失敗
				throw new Exception("DeleteSoftImage failed");

			return new Picture.PictureDataInfo()
			{
				W = w,
				H = h,
				Handle = handle,
			};
		}
	}
}
