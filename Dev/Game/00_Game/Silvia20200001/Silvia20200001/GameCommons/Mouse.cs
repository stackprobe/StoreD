using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using Charlotte.Commons;
using Charlotte.GameSettings;

namespace Charlotte.GameCommons
{
	public static class Mouse
	{
		private static int P_Rot = 0;

		public static int Rot
		{
			get
			{
				return 1 <= DD.FreezeInputFrame ? 0 : P_Rot;
			}
		}

		public class Button
		{
			public int Status = 0;

			// MEMO: ボタン・キー押下は 1 マウス押下は -1 で判定する。

			public int GetInput()
			{
				return 1 <= DD.FreezeInputFrame ? 0 : this.Status;
			}
		}

		public static Button L = new Button();
		public static Button R = new Button();
		public static Button M = new Button();

		public static int X = 0;
		public static int Y = 0;

		public static void EachFrame()
		{
			int status;

			if (DD.WindowIsActive)
			{
				P_Rot = DX.GetMouseWheelRotVol();
				status = DX.GetMouseInput();
			}
			else
			{
				P_Rot = 0;
				status = 0;
			}

			P_Rot = SCommon.ToRange(P_Rot, -SCommon.IMAX, SCommon.IMAX);

			DU.UpdateButtonCounter(ref L.Status, (status & DX.MOUSE_INPUT_LEFT) != 0);
			DU.UpdateButtonCounter(ref R.Status, (status & DX.MOUSE_INPUT_RIGHT) != 0);
			DU.UpdateButtonCounter(ref M.Status, (status & DX.MOUSE_INPUT_MIDDLE) != 0);

			if (DX.GetMousePoint(out X, out Y) != 0) // ? 失敗
				throw new Exception("GetMousePoint failed");

			X -= DD.MainScreenDrawRect.L;
			X *= GameConfig.ScreenSize.W;
			X /= DD.MainScreenDrawRect.W;
			Y -= DD.MainScreenDrawRect.T;
			Y *= GameConfig.ScreenSize.H;
			Y /= DD.MainScreenDrawRect.H;
		}

		/// <summary>
		/// マウスカーソルの位置を強制的に移動する。
		/// </summary>
		/// <param name="x">X座標</param>
		/// <param name="y">Y座標</param>
		public static void SetMousePosition(int x, int y)
		{
			x = SCommon.ToRange(x, 0, GameConfig.ScreenSize.W - 1);
			y = SCommon.ToRange(y, 0, GameConfig.ScreenSize.H - 1);

			x *= DD.MainScreenDrawRect.W;
			x /= GameConfig.ScreenSize.W;
			x += DD.MainScreenDrawRect.L;
			y *= DD.MainScreenDrawRect.H;
			y /= GameConfig.ScreenSize.H;
			y += DD.MainScreenDrawRect.T;

			if (DX.SetMousePoint(x, y) != 0) // ? 失敗
				throw new Exception("SetMousePoint failed");
		}
	}
}
