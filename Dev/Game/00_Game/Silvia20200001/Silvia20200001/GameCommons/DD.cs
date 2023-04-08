using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using DxLibDLL;
using Charlotte.Commons;
using Charlotte.Drawings;
using Charlotte.GameSettings;

namespace Charlotte.GameCommons
{
	/// <summary>
	/// ゲームに関する共通機能・便利機能はできるだけこのクラスに集約する。
	/// </summary>
	public static class DD
	{
		public static List<Action> Finalizers = new List<Action>();
		public static string MainWindowTitle;
		public static I4Rect TargetMonitor;
		public static I2Size RealScreenSize;
		public static SubScreen MainScreen;
		public static SubScreen LastMainScreen;
		public static SubScreen KeptMainScreen;
		public static int ProcFrame;
		public static int FreezeInputFrame;
		public static bool WindowIsActive;

		private static Func<string, byte[]> ResFileDataGetter = null;

		public static byte[] GetResFileData(string resPath)
		{
			if (ResFileDataGetter == null)
				ResFileDataGetter = GetResFileDataGetter();

			return ResFileDataGetter(resPath);
		}

		private static Func<string, byte[]> GetResFileDataGetter()
		{
			string clusterFile = Path.Combine(ProcMain.SelfDir, "Resource.dat");
			Func<string, byte[]> getter;

			if (File.Exists(clusterFile))
			{
				ResourceCluster rc = new ResourceCluster(clusterFile);
				getter = resPath => rc.GetData(resPath);
			}
			else
			{
				getter = resPath => File.ReadAllBytes(Path.Combine(@"..\..\..\..\Resource", resPath));
			}
			return getter;
		}

		#region Draw

		/// <summary>
		/// 描画設定クラス
		/// 全てのフィールドはデフォルト値で初期化すること。
		/// </summary>
		private class DrawSettingInfo
		{
			public bool IgnoreDrawErrorFlag = false;
			public bool MosaicFlag = false;
			public int R = 255;
			public int G = 255;
			public int B = 255;
			public int A = 255;
			public double Rot = 0.0;
			public double XZoom = 1.0;
			public double YZoom = 1.0;
		}

		/// <summary>
		/// 描画設定
		/// </summary>
		private static DrawSettingInfo DrawSetting = new DrawSettingInfo();

		/// <summary>
		/// 描画設定：
		/// -- 描画関数がエラーを返しても例外を投げない。
		/// </summary>
		public static void SetIgnoreDrawError()
		{
			DrawSetting.IgnoreDrawErrorFlag = true;
		}

		/// <summary>
		/// 描画設定：
		/// -- アンチエイリアシングを行わない。
		/// </summary>
		public static void SetMosaic()
		{
			DrawSetting.MosaicFlag = true;
		}

		/// <summary>
		/// 描画設定：
		/// -- 明度をセットする。
		/// </summary>
		/// <param name="color">明度</param>
		public static void SetBright(I3Color color)
		{
			SetBright(color.ToD3Color());
		}

		/// <summary>
		/// 描画設定：
		/// -- 明度をセットする。
		/// </summary>
		/// <param name="color">明度</param>
		public static void SetBright(D3Color color)
		{
			DrawSetting.R = DU.RateToByte(color.R);
			DrawSetting.G = DU.RateToByte(color.G);
			DrawSetting.B = DU.RateToByte(color.B);
		}

		/// <summary>
		/// 描画設定：
		/// -- 不透明度をセットする。
		/// </summary>
		/// <param name="a">不透明度</param>
		public static void SetAlpha(double a)
		{
			DrawSetting.A = DU.RateToByte(a);
		}

		/// <summary>
		/// 描画設定：
		/// -- 回転する角度(ラジアン角)をセットする。
		/// </summary>
		/// <param name="rot">ラジアン角</param>
		public static void SetRotate(double rot)
		{
			DrawSetting.Rot = rot;
		}

		/// <summary>
		/// 描画設定：
		/// -- 拡大率をセットする。
		/// </summary>
		/// <param name="zoom">拡大率</param>
		public static void SetZoom(double zoom)
		{
			SetZoom(zoom, zoom);
		}

		/// <summary>
		/// 描画設定：
		/// -- 拡大率をセットする。
		/// </summary>
		/// <param name="xZoom">横方向の拡大率</param>
		/// <param name="yZoom">縦方向の拡大率</param>
		public static void SetZoom(double xZoom, double yZoom)
		{
			DrawSetting.XZoom = xZoom;
			DrawSetting.YZoom = yZoom;
		}

		/// <summary>
		/// 描画する。
		/// </summary>
		/// <param name="picture">画像</param>
		/// <param name="x">描画する位置の中心座標(X位置)</param>
		/// <param name="y">描画する位置の中心座標(Y位置)</param>
		public static void Draw(Picture picture, double x, double y)
		{
			Draw(picture, new D2Point(x, y));
		}

		/// <summary>
		/// 描画する。
		/// </summary>
		/// <param name="picture">画像</param>
		/// <param name="pt">描画する位置の中心座標</param>
		public static void Draw(Picture picture, D2Point pt)
		{
			P4Poly poly = D4Rect.XYWH(pt.X, pt.Y, picture.W * DrawSetting.XZoom, picture.H * DrawSetting.YZoom).Poly;

			DD.Rotate(ref poly.LT, pt, DrawSetting.Rot);
			DD.Rotate(ref poly.RT, pt, DrawSetting.Rot);
			DD.Rotate(ref poly.RB, pt, DrawSetting.Rot);
			DD.Rotate(ref poly.LB, pt, DrawSetting.Rot);

			Draw(picture, poly);
		}

		/// <summary>
		/// 描画する。
		/// 描画設定の回転・拡大率は適用されない。
		/// </summary>
		/// <param name="picture">画像</param>
		/// <param name="pt">描画する位置の中心座標</param>
		public static void DrawSimple(Picture picture, D2Point pt)
		{
			Draw(picture, D4Rect.XYWH(pt.X, pt.Y, picture.W, picture.H));
		}

		/// <summary>
		/// 描画する。
		/// 描画設定の回転・拡大率は適用されない。
		/// </summary>
		/// <param name="picture">画像</param>
		/// <param name="rect">描画する領域</param>
		public static void Draw(Picture picture, I4Rect rect)
		{
			Draw(picture, rect.ToD4Rect());
		}

		/// <summary>
		/// 描画する。
		/// 描画設定の回転・拡大率は適用されない。
		/// </summary>
		/// <param name="picture">画像</param>
		/// <param name="rect">描画する領域</param>
		public static void Draw(Picture picture, D4Rect rect)
		{
			Draw(picture, rect.Poly);
		}

		/// <summary>
		/// 描画する。
		/// 描画設定の回転・拡大率は適用されない。
		/// </summary>
		/// <param name="picture">画像</param>
		/// <param name="poly">描画する領域</param>
		public static void Draw(Picture picture, P4Poly poly)
		{
			// 描画設定の適用ここから

			if (DrawSetting.MosaicFlag)
			{
				DX.SetDrawMode(DX.DX_DRAWMODE_NEAREST);
			}
			if (DrawSetting.R != 255 || DrawSetting.G != 255 || DrawSetting.B != 255)
			{
				DX.SetDrawBright(DrawSetting.R, DrawSetting.G, DrawSetting.B);
			}
			if (DrawSetting.A != 255)
			{
				DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, DrawSetting.A);
			}

			// 描画設定の適用ここまで

			// ? 失敗
			if (DX.DrawModiGraphF(
				(float)poly.LT.X,
				(float)poly.LT.Y,
				(float)poly.RT.X,
				(float)poly.RT.Y,
				(float)poly.RB.X,
				(float)poly.RB.Y,
				(float)poly.LB.X,
				(float)poly.LB.Y,
				picture.GetHandle(),
				1
				)
				!= 0
				)
			{
				if (!DrawSetting.IgnoreDrawErrorFlag)
					throw new Exception("DrawModiGraphF failed");
			}

			// 描画設定の解除ここから

			if (DrawSetting.MosaicFlag)
			{
				DX.SetDrawMode(DX.DX_DRAWMODE_ANISOTROPIC);
			}
			if (DrawSetting.R != 255 || DrawSetting.G != 255 || DrawSetting.B != 255)
			{
				DX.SetDrawBright(255, 255, 255);
			}
			if (DrawSetting.A != 255)
			{
				DX.SetDrawBlendMode(DX.DX_BLENDMODE_NOBLEND, 0);
			}

			// 描画設定の解除ここまで

			DrawSetting = new DrawSettingInfo(); // 描画設定をリセットする。
		}

		#endregion

		#region Print

		/// <summary>
		/// 文字列の描画を初期化する。
		/// </summary>
		/// <param name="l">左座標</param>
		/// <param name="t">上座標</param>
		/// <param name="yStep">行間ステップ</param>
		/// <param name="fontName">フォント名</param>
		/// <param name="fontSize">フォントサイズ</param>
		public static void SetPrint(int l, int t, int yStep, string fontName = null, int fontSize = -1)
		{
			if (
				l < 0 || SCommon.IMAX < l ||
				t < 0 || SCommon.IMAX < t ||
				yStep < 0 || SCommon.IMAX < yStep
				)
				throw new Exception("Bad params");

			Prints.L = l;
			Prints.T = t;
			Prints.YStep = yStep;
			Prints.X = 0;
			Prints.Y = 0;

			if (fontName == null && fontSize == -1) // ? デフォルトのフォントを使用する。
			{
				// none
			}
			else if (!string.IsNullOrEmpty(fontName) && SCommon.IsRange(fontSize, 1, SCommon.IMAX)) // ? フォント指定
			{
				// none
			}
			else
			{
				throw new Exception("Bad font params");
			}
			Prints.FontName = fontName;
			Prints.FontSize = fontSize;

			Prints.P_Color = Prints.DEFAULT_COLOR;
			Prints.BorderColor = Prints.DEFAULT_BORDER_COLOR;
			Prints.BorderSize = 0;
		}

		/// <summary>
		/// 文字列の描画の設定：
		/// 文字の色を設定する。
		/// 次の文字列の描画の初期化(SetPrint)でリセットされる。
		/// </summary>
		/// <param name="color"></param>
		public static void SetPrintColor(I3Color color)
		{
			Prints.P_Color = color;
		}

		/// <summary>
		/// 文字列の描画の設定：
		/// 文字の輪郭を設定する。
		/// 次の文字列の描画の初期化(SetPrint)でリセットされる。
		/// </summary>
		/// <param name="color">輪郭の色</param>
		/// <param name="size">輪郭の幅</param>
		public static void SetPrintBorder(I3Color color, int size)
		{
			if (size < 1 || SCommon.IMAX < size)
				throw new Exception("Bad size");

			Prints.BorderColor = color;
			Prints.BorderSize = size;
		}

		public static void Print(string line)
		{
			if (line == null)
				throw new Exception("Bad line");

			Prints.Print(line);
		}

		public static void PrintRet()
		{
			Prints.X = 0;
			Prints.Y += Prints.YStep;
		}

		public static void PrintLine(string line)
		{
			Print(line);
			PrintRet();
		}

		private static class Prints
		{
			public static readonly I3Color DEFAULT_COLOR = new I3Color(255, 255, 255);
			public static readonly I3Color DEFAULT_BORDER_COLOR = new I3Color(64, 64, 64);

			public static int L = 0;
			public static int T = 0;
			public static int YStep = 20;
			public static int X = 0;
			public static int Y = 0;
			public static string FontName = null;
			public static int FontSize = -1; // -1 == デフォルトのフォントを使用する。
			public static I3Color P_Color = DEFAULT_COLOR;
			public static I3Color BorderColor = DEFAULT_BORDER_COLOR;
			public static int BorderSize = 0; // 0 == 文字の輪郭を描画しない。

			public static void Print(string line)
			{
				int x = L + X;
				int y = T + Y;

				DrawString(line, x, y);

				x += GetWidth(line);
			}

			private static void DrawString(string line, int x, int y)
			{
				if (BorderSize != 0)
					for (int xc = -1; xc <= 1; xc++)
						for (int yc = -1; yc <= 1; yc++)
							if (xc != 0 || yc != 0)
								DrawString_Main(line, x + xc * BorderSize, y + yc * BorderSize, BorderColor);

				DrawString_Main(line, x, y, P_Color);
			}

			private static void DrawString_Main(string line, int x, int y, I3Color color)
			{
				if (FontSize == -1)
					DX.DrawString(x, y, line, DU.ToDXColor(color));
				else
					DX.DrawStringToHandle(x, y, line, DU.ToDXColor(color), DU.GetFontHandle(FontName, FontSize), 0u, 0);
			}

			private static int GetWidth(string line)
			{
				int w;

				if (FontSize == -1)
					w = DX.GetDrawStringWidth(line, SCommon.ENCODING_SJIS.GetByteCount(line));
				else
					w = DX.GetDrawStringWidthToHandle(line, SCommon.ENCODING_SJIS.GetByteCount(line), DU.GetFontHandle(FontName, FontSize), 0);

				if (w < 0 || SCommon.IMAX < w)
					throw new Exception("GetDrawStringWidth or GetDrawStringWidthToHandle failed");

				return w;
			}
		}

		#endregion

		public static void EachFrame()
		{
			DD.Curtain.EachFrame();
			Music.EachFrame();
			SoundEffect.EachFrame();

			SubScreen.ChangeDrawScreenToBack();

			DD.SetBright(new I3Color(0, 0, 0));
			DD.Draw(Pictures.WhiteBox, new D4Rect(0.0, 0.0, DD.RealScreenSize.W, DD.RealScreenSize.H));

			I4Rect mainScreenDrawRect = DD.EnlargeFullInterior(
				GameConfig.ScreenSize.ToD2Size(),
				new D4Rect(0.0, 0.0, DD.RealScreenSize.W, DD.RealScreenSize.H)
				)
				.ToI4Rect();

			int mag = mainScreenDrawRect.W / GameConfig.ScreenSize.W;

			if (
				1 <= mag &&
				mainScreenDrawRect.W == GameConfig.ScreenSize.W * mag &&
				mainScreenDrawRect.H == GameConfig.ScreenSize.H * mag
				)
				DD.SetMosaic();

			DD.Draw(DD.MainScreen.GetPicture(), mainScreenDrawRect.ToD4Rect());

			GC.Collect();

			Keep60Hz();

			DX.ScreenFlip();

			if (DX.CheckHitKey(DX.KEY_INPUT_ESCAPE) == 1 || DX.ProcessMessage() == -1)
			{
				throw new Exception("ゲーム中断");
			}

			// ALT + ENTER -> フルスクリーンの切り替え
			//
			if ((1 <= Keyboard.GetInput(DX.KEY_INPUT_LALT) || 1 <= Keyboard.GetInput(DX.KEY_INPUT_RALT)) && Keyboard.GetInput(DX.KEY_INPUT_RETURN) == 1)
			{
				// ? 現在フルスクリーン -> フルスクリーン解除
				if (
					DD.RealScreenSize.W == DD.TargetMonitor.W &&
					DD.RealScreenSize.H == DD.TargetMonitor.H
					)
				{
					GameProcMain.SetRealScreenSize(GameSetting.UserScreenSize.W, GameSetting.UserScreenSize.H);
					GameSetting.FullScreen = false;
				}
				else // ? 現在フルスクリーンではない -> フルスクリーンにする
				{
					GameProcMain.SetRealScreenSize(DD.TargetMonitor.W, DD.TargetMonitor.H);
					GameSetting.FullScreen = true;
				}
				DD.FreezeInput(30); // エンターキー押下がゲームに影響しないように
			}

			SCommon.Swap(ref DD.MainScreen, ref DD.LastMainScreen);
			DD.MainScreen.ChangeDrawScreenToThis();

			ProcFrame++;
			DU.Countdown(ref FreezeInputFrame);
			WindowIsActive = DX.GetActiveFlag() != 0;

			if (SCommon.IMAX < ProcFrame) // 192.9 days limit
				throw new Exception("ProcFrame counter has exceeded the limit");

			Keyboard.EachFrame();

			DX.ClearDrawScreen();
		}

		public static void KeepLastMainScreen()
		{
			SCommon.Swap(ref DD.LastMainScreen, ref DD.KeptMainScreen);
		}

		private static long HzChaserTime;

		private static void Keep60Hz()
		{
			long currentTime = DU.GetCurrentTime();

			HzChaserTime += 16L;
			HzChaserTime = SCommon.ToRange(HzChaserTime, currentTime - 100L, currentTime + 100L);

			while (currentTime < HzChaserTime)
			{
				Thread.Sleep(1);
				currentTime = DU.GetCurrentTime();
			}
		}

		public static void FreezeInput(int frame = 1)
		{
			if (frame < 1 || SCommon.IMAX < frame)
				throw new Exception("Bad frame");

			FreezeInputFrame = frame;
		}

		public static void DrawCurtain(double whiteLevel)
		{
			if (whiteLevel == 0.0)
				return;

			whiteLevel = SCommon.ToRange(whiteLevel, -1.0, 1.0);

			if (whiteLevel < 0.0)
			{
				DD.SetAlpha(-whiteLevel);
				DD.SetBright(new I3Color(0, 0, 0));
			}
			else
			{
				DD.SetAlpha(whiteLevel);
			}
			DD.Draw(Pictures.WhiteBox, new I4Rect(0, 0, GameConfig.ScreenSize.W, GameConfig.ScreenSize.H));
		}

		public static void SetCurtain(double destWhiteLevel)
		{
			Curtain.NextWhiteLevels.Clear();

			foreach (DD.Scene scene in DD.CreateScene(30))
			{
				Curtain.NextWhiteLevels.Enqueue(DD.AToBRate(Curtain.CurrWhiteLevel, destWhiteLevel, scene.Rate));
			}
		}

		private static class Curtain
		{
			public static double CurrWhiteLevel = 0.0;
			public static Queue<double> NextWhiteLevels = new Queue<double>();

			public static void EachFrame()
			{
				if (1 <= NextWhiteLevels.Count)
				{
					CurrWhiteLevel = NextWhiteLevels.Dequeue();
				}
				DD.DrawCurtain(CurrWhiteLevel);
			}
		}

		public class Scene
		{
			public int Numer;
			public int Denom;

			public double Rate
			{
				get
				{
					return (double)this.Numer / this.Denom;
				}
			}
		}

		public static IEnumerable<Scene> CreateScene(int denom)
		{
			for (int numer = 0; numer <= denom; numer++)
			{
				yield return new Scene()
				{
					Numer = numer,
					Denom = denom,
				};
			}
		}

		public static void Rotate(ref double x, ref double y, double rot)
		{
			double w;

			w = x * Math.Cos(rot) - y * Math.Sin(rot);
			y = x * Math.Sin(rot) + y * Math.Cos(rot);
			x = w;
		}

		public static void Rotate(ref D2Point pt, D2Point origin, double rot)
		{
			pt -= origin;

			Rotate(ref pt.X, ref pt.Y, rot);

			pt += origin;
		}

		public static double GetDistance(double x, double y)
		{
			return Math.Sqrt(x * x + y * y);
		}

		public static double GetDistance(D2Point pt, D2Point origin)
		{
			pt -= origin;

			return GetDistance(pt.X, pt.Y);
		}

		public static double GetAngle(double x, double y)
		{
			if (y < 0.0) return Math.PI * 2.0 - GetAngle(x, -y);
			if (x < 0.0) return Math.PI - GetAngle(-x, y);

			if (x < y) return Math.PI / 2.0 - GetAngle(y, x);
			if (x < SCommon.MICRO) return 0.0; // 極端に原点に近い座標の場合、常に右真横を返す。

			if (y == 0.0) return 0.0;
			if (y == x) return Math.PI / 4.0;

			double r1 = 0.0;
			double r2 = Math.PI / 4.0;
			double t = y / x;
			double rm;

			for (int c = 1; ; c++)
			{
				rm = (r1 + r2) / 2.0;

				if (10 <= c)
					break;

				double rmt = Math.Tan(rm);

				if (t < rmt)
					r2 = rm;
				else
					r1 = rm;
			}
			return rm;
		}

		public static double GetAngle(D2Point pt, D2Point origin)
		{
			pt -= origin;

			return GetAngle(pt.X, pt.Y);
		}

		public static D2Point AngleToPoint(double angle, double distance)
		{
			return new D2Point(
				distance * Math.Cos(angle),
				distance * Math.Sin(angle)
				);
		}

		public static void MakeXYSpeed(double originX, double originY, double targetX, double targetY, double speed, out double xSpeed, out double ySpeed)
		{
			D2Point pt = AngleToPoint(GetAngle(targetX - originX, targetY - originY), speed);

			xSpeed = pt.X;
			ySpeed = pt.Y;
		}

		/// <summary>
		/// (0, 0), (0.5, 1), (1, 0) を通る放物線
		/// </summary>
		/// <param name="x">X軸の値</param>
		/// <returns>Y軸の値</returns>
		public static double Parabola(double x)
		{
			return (x - x * x) * 4.0;
		}

		/// <summary>
		/// S字曲線
		/// (0, 0), (0.5, 0.5), (1, 1) を通る曲線
		/// x &lt;= 0.5 の区間は加速(等加速)する。
		/// 0.5 &lt;= x の区間は減速(等加速)する。
		/// </summary>
		/// <param name="x">X軸の値</param>
		/// <returns>Y軸の値</returns>
		public static double SCurve(double x)
		{
			if (x < 0.5)
				return (1.0 - Parabola(x + 0.5)) * 0.5;
			else
				return (1.0 + Parabola(x - 0.5)) * 0.5;
		}

		/// <summary>
		/// 始点から終点までの間の指定レートの位置の値を返す。
		/// </summary>
		/// <param name="a">始点</param>
		/// <param name="b">終点</param>
		/// <param name="rate">レート</param>
		/// <returns>レートの値</returns>
		public static double AToBRate(double a, double b, double rate)
		{
			return a + (b - a) * rate;
		}

		/// <summary>
		/// 始点から終点までの間の指定レートの位置を返す。
		/// </summary>
		/// <param name="a">始点</param>
		/// <param name="b">終点</param>
		/// <param name="rate">レート</param>
		/// <returns>レートの位置</returns>
		public static D2Point AToBRate(D2Point a, D2Point b, double rate)
		{
			return a + (b - a) * rate;
		}

		/// <summary>
		/// 始点から終点までの間の位置をレートに変換する。
		/// </summary>
		/// <param name="a">始点</param>
		/// <param name="b">終点</param>
		/// <param name="value">位置</param>
		/// <returns>レート</returns>
		public static double RateAToB(double a, double b, double value)
		{
			return (value - a) / (b - a);
		}

		/// <summary>
		/// アスペクト比を維持して指定サイズを指定領域いっぱいに広げる。
		/// 戻り値：
		/// -- new D4Rect[] { interior, exterior }
		/// ---- interior == 指定領域の内側に張り付く拡大領域
		/// ---- exterior == 指定領域の外側に張り付く拡大領域
		/// </summary>
		/// <param name="size">指定サイズ</param>
		/// <param name="rect">指定領域</param>
		/// <returns>拡大領域の配列</returns>
		public static D4Rect[] EnlargeFull(D2Size size, D4Rect rect)
		{
			double w_h = (rect.H * size.W) / size.H; // 高さを基準にした幅
			double h_w = (rect.W * size.H) / size.W; // 幅を基準にした高さ

			D4Rect rect1;
			D4Rect rect2;

			rect1.L = rect.L + (rect.W - w_h) / 2.0;
			rect1.T = rect.T;
			rect1.W = w_h;
			rect1.H = rect.H;

			rect2.L = rect.L;
			rect2.T = rect.T + (rect.H - h_w) / 2.0;
			rect2.W = rect.W;
			rect2.H = h_w;

			D4Rect interior;
			D4Rect exterior;

			if (w_h < rect.W)
			{
				interior = rect1;
				exterior = rect2;
			}
			else
			{
				interior = rect2;
				exterior = rect1;
			}
			return new D4Rect[] { interior, exterior };
		}

		/// <summary>
		/// アスペクト比を維持して指定サイズを指定領域の内側いっぱいに広げる。
		/// スライド率：
		/// -- 0.0 ～ 1.0
		/// -- 0.0 == 拡大領域を最も左上に寄せる。指定領域と拡大領域の上辺と左側面が重なる。
		/// -- 0.5 == 中央
		/// -- 1.0 == 拡大領域を最も右下に寄せる。指定領域と拡大領域の底辺と右側面が重なる。
		/// </summary>
		/// <param name="size">指定サイズ</param>
		/// <param name="rect">指定領域</param>
		/// <param name="slideRate">スライド率</param>
		/// <returns>拡大領域</returns>
		public static D4Rect EnlargeFullInterior(D2Size size, D4Rect rect, double slideRate = 0.5)
		{
			D4Rect interior = EnlargeFull(size, rect)[0];

			interior.L = rect.L + (rect.W - interior.W) * slideRate;
			interior.T = rect.T + (rect.H - interior.H) * slideRate;

			return interior;
		}

		/// <summary>
		/// アスペクト比を維持して指定サイズを指定領域の外側いっぱいに広げる。
		/// スライド率：
		/// -- 0.0 ～ 1.0
		/// -- 0.0 == 拡大領域を最も左上に寄せる。指定領域と拡大領域の底辺と右側面が重なる。
		/// -- 0.5 == 中央
		/// -- 1.0 == 拡大領域を最も右下に寄せる。指定領域と拡大領域の上辺と左側面が重なる。
		/// </summary>
		/// <param name="size">指定サイズ</param>
		/// <param name="rect">指定領域</param>
		/// <param name="slideRate">スライド率</param>
		/// <returns>拡大領域</returns>
		public static D4Rect EnlargeFullExterior(D2Size size, D4Rect rect, double slideRate = 0.5)
		{
			D4Rect exterior = EnlargeFull(size, rect)[1];

			exterior.L = rect.L - (exterior.W - rect.W) * (1.0 - slideRate);
			exterior.T = rect.T - (exterior.H - rect.H) * (1.0 - slideRate);

			return exterior;
		}
	}
}
