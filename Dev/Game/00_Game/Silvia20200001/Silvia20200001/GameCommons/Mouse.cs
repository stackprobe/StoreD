using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using Charlotte.Commons;
using Charlotte.Drawings;

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

			X = SCommon.ToRange(X, 0, GameConfig.ScreenSize.W - 1);
			Y = SCommon.ToRange(Y, 0, GameConfig.ScreenSize.H - 1);
		}

		/// <summary>
		/// マウスカーソルの位置を強制的に移動する。
		/// </summary>
		/// <param name="pt">新しい座標</param>
		public static void SetMousePosition(I2Point pt)
		{
			X = pt.X;
			Y = pt.Y;

			X *= DD.MainScreenDrawRect.W;
			X /= GameConfig.ScreenSize.W;
			X += DD.MainScreenDrawRect.L;
			Y *= DD.MainScreenDrawRect.H;
			Y /= GameConfig.ScreenSize.H;
			Y += DD.MainScreenDrawRect.T;

			X = SCommon.ToRange(X, 0, DD.RealScreenSize.W - 1);
			Y = SCommon.ToRange(Y, 0, DD.RealScreenSize.H - 1);

			if (DX.SetMousePoint(X, Y) != 0) // ? 失敗
				throw new Exception("SetMousePoint failed");
		}
	}
}
