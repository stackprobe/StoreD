using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;
using Charlotte.Drawings;
using Charlotte.GameCommons;

namespace Charlotte.Games
{
	public class SimpleMenu
	{
		private static string FONT_NAME = "木漏れ日ゴシック";
		private static I4Color BACK_COLOR = new I4Color(0, 0, 0, 128);
		private static I3Color TEXT_COLOR = new I3Color(255, 255, 255);

		private static int LastDrawProcFrame = -1;
		private static double Shadow_W = 0.0;

		private int ThisLastDrawProcFrame = -1;
		private bool FirstTimeDraw = true;

		/// <summary>
		/// 選択中の項目のインデックス
		/// 設定・参照用
		/// </summary>
		public int SelectedIndex = 0;

		/// <summary>
		/// ポーズ画面向け
		/// 設定用
		/// </summary>
		public bool NoPound = false;

		/// <summary>
		/// ポーズ画面向け
		/// 設定用
		/// </summary>
		public bool CancelByPause = false;

		private int FontSize;
		private int Item_L;
		private int FirstItem_T;
		private int Item_H;
		private int Item_YStep;
		private int Menu_W;
		private int MousePointMargin;
		private string Title; // null == タイトル無し
		private string[] Items;

		/// <summary>
		/// メニューを作成する。
		/// </summary>
		/// <param name="fontSize">フォントサイズ</param>
		/// <param name="item_l">項目の表示位置(左座標)</param>
		/// <param name="itemYSpan">項目の縦間隔</param>
		/// <param name="menu_w">メニューの幅</param>
		/// <param name="title">タイトル(null:タイトル無し)</param>
		/// <param name="items">項目の配列</param>
		public SimpleMenu(int fontSize, int item_l, int itemYSpan, int menu_w, string title, string[] items)
		{
			if (fontSize < 20 || SCommon.IMAX < fontSize)
				throw new Exception("Bad fontSize");

			if (item_l < 0 || GameConfig.ScreenSize.W < item_l)
				throw new Exception("Bad item_l");

			if (itemYSpan < 0 || SCommon.IMAX < itemYSpan)
				throw new Exception("Bad itemYSpan");

			if (menu_w < 20 || GameConfig.ScreenSize.W < menu_w)
				throw new Exception("Bad menu_w");

			if (title == "")
				throw new Exception("Bad title");

			if (items == null || items.Length < 1 || items.Any(v => string.IsNullOrEmpty(v)))
				throw new Exception("Bad items");

			int item_h = fontSize;
			int item_yStep = item_h + itemYSpan;

			this.FontSize = fontSize;
			this.Item_L = item_l;
			this.FirstItem_T = (GameConfig.ScreenSize.H - (item_yStep * items.Length - itemYSpan)) / 2;
			this.Item_H = item_h;
			this.Item_YStep = item_yStep;
			this.Menu_W = menu_w;
			this.MousePointMargin = itemYSpan / 3;
			this.Title = title;
			this.Items = items;

			if (title != null) // ? タイトル有り -> タイトルの分アイテムを下へずらす。
				this.FirstItem_T += item_yStep / 2;
		}

		/// <summary>
		/// メニューの処理と描画をする。
		/// </summary>
		/// <returns>決定したか</returns>
		public bool Draw()
		{
			// メニュー離脱チェック
			{
				if (LastDrawProcFrame + 30 < DD.ProcFrame)
					Shadow_W = 0.0;

				LastDrawProcFrame = DD.ProcFrame;
			}

			// このメニューからの離脱チェック
			{
				if (this.ThisLastDrawProcFrame + 30 < DD.ProcFrame)
					this.FirstTimeDraw = true;

				this.ThisLastDrawProcFrame = DD.ProcFrame;
			}

			DD.Approach(ref Shadow_W, this.Menu_W, 0.9);

			bool firstTime = this.FirstTimeDraw;
			this.FirstTimeDraw = false;

			bool inputtedFlag = false;
			bool mouseHoveringFlag = false;

			if (this.NoPound ? Inputs.DIR_8.GetInput() == 1 : Inputs.DIR_8.IsPound())
			{
				this.SelectedIndex = (this.SelectedIndex + this.Items.Length - 1) % this.Items.Length;
				inputtedFlag = true;
			}
			if (this.NoPound ? Inputs.DIR_2.GetInput() == 1 : Inputs.DIR_2.IsPound())
			{
				this.SelectedIndex = (this.SelectedIndex + 1) % this.Items.Length;
				inputtedFlag = true;
			}
			if (Inputs.A.GetInput() == 1)
			{
				return true;
			}
			if (Inputs.B.GetInput() == 1)
			{
				if (this.SelectedIndex == this.Items.Length - 1)
					return true;

				this.SelectedIndex = this.Items.Length - 1;
				inputtedFlag = true;
			}
			if (Inputs.PAUSE.GetInput() == 1 && this.CancelByPause)
			{
				this.SelectedIndex = this.Items.Length - 1;
				return true;
			}

			if (GameSetting.MouseEnabled)
			{
				bool mouseIdling = 30 < Mouse.LastClickFrame;

				if (inputtedFlag || (firstTime && mouseIdling))
				{
					const int BTN_RB_X = -10;
					const int BTN_RB_Y = -10;

					Mouse.Position = new I2Point(
						this.Menu_W + BTN_RB_X,
						this.FirstItem_T + this.SelectedIndex * this.Item_YStep + this.Item_H + BTN_RB_Y
						);
				}

				for (int index = 0; index < this.Items.Length; index++)
				{
					if (Crash.IsCrashed_Circle_Rect(
						Mouse.Position.ToD2Point(),
						(double)this.MousePointMargin,
						new I4Rect(0, this.FirstItem_T + index * this.Item_YStep, this.Menu_W, this.Item_H).ToD4Rect()
						))
					{
						this.SelectedIndex = index;
						mouseHoveringFlag = true;

						if (Mouse.L.GetInput() == -1)
						{
							return true;
						}
						break;
					}
				}
			}

			// ここから描画

			DD.SetAlpha(BACK_COLOR.A / 255.0);
			DD.SetBright(BACK_COLOR.WithoutAlpha().ToD3Color());
			DD.Draw(Pictures.WhiteBox, new D4Rect(0.0, 0.0, Shadow_W, (double)GameConfig.ScreenSize.H));

			if (this.Title != null) // ? タイトル有り
			{
				DD.SetPrint(this.Item_L, this.FirstItem_T - this.Item_YStep, 0, FONT_NAME, this.FontSize);
				DD.SetPrintColor(TEXT_COLOR);
				DD.Print(this.Title);
			}
			DD.SetPrint(this.Item_L, this.FirstItem_T, this.Item_YStep, FONT_NAME, this.FontSize);
			DD.SetPrintColor(TEXT_COLOR);

			for (int index = 0; index < this.Items.Length; index++)
			{
				if (index != this.SelectedIndex)
					DD.Print("[ ] ");
				else if (GameSetting.MouseEnabled && !mouseHoveringFlag)
					DD.Print("[-] ");
				else
					DD.Print("[>] ");

				DD.PrintLine(this.Items[index]);
			}
			return false;
		}
	}
}
