using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;
using Charlotte.Drawings;

namespace Charlotte.GameCommons
{
	public class Crash
	{
		private enum Kind_e
		{
			NONE = 1,
			POINT,
			CIRCLE,
			RECT,
			MULTI,
		}

		private Kind_e Kind;
		private D2Point Pt;
		private double R;
		private D4Rect Rect;
		private Crash[] Crashes;

		public static Crash CreateNone()
		{
			return new Crash()
			{
				Kind = Kind_e.NONE,
			};
		}

		public static Crash CreatePoint(D2Point pt)
		{
			return new Crash()
			{
				Kind = Kind_e.POINT,
				Pt = pt,
			};
		}

		public static Crash CreateCircle(D2Point pt, double r)
		{
			return new Crash()
			{
				Kind = Kind_e.CIRCLE,
				Pt = pt,
				R = r,
			};
		}

		public static Crash CreateRect(D4Rect rect)
		{
			return new Crash()
			{
				Kind = Kind_e.RECT,
				Rect = rect,
			};
		}

		public static Crash CreateMulti(params Crash[] crashes)
		{
			return new Crash()
			{
				Kind = Kind_e.MULTI,
				Crashes = crashes,
			};
		}

		private Crash()
		{ }

		public static bool IsCrashed(Crash a, Crash b)
		{
			if ((int)b.Kind < (int)a.Kind)
				SCommon.Swap(ref a, ref b);

			if (a.Kind == Kind_e.NONE)
				return false;

			if (b.Kind == Kind_e.MULTI)
				return IsCrashed_Any_Multi(a, b);

			if (a.Kind == Kind_e.POINT)
			{
				if (b.Kind == Kind_e.POINT)
					return false;

				if (b.Kind == Kind_e.CIRCLE)
					return IsCrashed_Circle_Point(b.Pt, b.R, a.Pt);

				if (b.Kind == Kind_e.RECT)
					return IsCrashed_Rect_Point(b.Rect, a.Pt);
			}
			else if (a.Kind == Kind_e.CIRCLE)
			{
				if (b.Kind == Kind_e.CIRCLE)
					return IsCrashed_Circle_Circle(a.Pt, a.R, b.Pt, b.R);

				if (b.Kind == Kind_e.RECT)
					return IsCrashed_Circle_Rect(a.Pt, a.R, b.Rect);
			}
			else if (a.Kind == Kind_e.RECT)
			{
				if (b.Kind == Kind_e.RECT)
					return IsCrashed_Rect_Rect(a.Rect, b.Rect);
			}
			throw new Exception("Bad Kind");
		}

		private static bool IsCrashed_Any_Multi(Crash a, Crash b)
		{
			if (a.Kind == Kind_e.MULTI)
				return IsCrashed_Multi_Multi(a, b);

			foreach (Crash crash in b.Crashes)
				if (IsCrashed(a, crash))
					return true;

			return false;
		}

		private static bool IsCrashed_Multi_Multi(Crash a, Crash b)
		{
			foreach (Crash ac in a.Crashes)
				foreach (Crash bc in b.Crashes)
					if (IsCrashed(ac, bc))
						return true;

			return false;
		}

		private static bool IsCrashed_Circle_Point(D2Point aPt, double aR, D2Point bPt)
		{
			return DD.GetDistance(aPt, bPt) < aR;
		}

		private static bool IsCrashed_Circle_Circle(D2Point aPt, double aR, D2Point bPt, double bR)
		{
			return DD.GetDistance(aPt, bPt) < aR + bR;
		}

		private static bool IsCrashed_Circle_Rect(D2Point aPt, double aR, D4Rect bRect)
		{
			if (aPt.X < bRect.L) // 左
			{
				if (aPt.Y < bRect.T) // 左上
				{
					return IsCrashed_Circle_Point(aPt, aR, new D2Point(bRect.L, bRect.T));
				}
				else if (bRect.B < aPt.Y) // 左下
				{
					return IsCrashed_Circle_Point(aPt, aR, new D2Point(bRect.L, bRect.B));
				}
				else // 左中段
				{
					return bRect.L < aPt.X + aR;
				}
			}
			else if (bRect.R < aPt.X) // 右
			{
				if (aPt.Y < bRect.T) // 右上
				{
					return IsCrashed_Circle_Point(aPt, aR, new D2Point(bRect.R, bRect.T));
				}
				else if (bRect.B < aPt.Y) // 右下
				{
					return IsCrashed_Circle_Point(aPt, aR, new D2Point(bRect.R, bRect.B));
				}
				else // 右中段
				{
					return aPt.X - aR < bRect.R;
				}
			}
			else // 真上・真ん中・真下
			{
				return bRect.T - aR < aPt.Y && aPt.Y < bRect.B + aR;
			}
		}

		private static bool IsCrashed_Rect_Point(D4Rect aRect, D2Point bPt)
		{
			return
				aRect.L < bPt.X && bPt.X < aRect.R &&
				aRect.T < bPt.Y && bPt.Y < aRect.B;
		}

		private static bool IsCrashed_Rect_Rect(D4Rect aRect, D4Rect bRect)
		{
			return
				aRect.L < bRect.R && bRect.L < aRect.R &&
				aRect.T < bRect.B && bRect.T < aRect.B;
		}
	}
}
