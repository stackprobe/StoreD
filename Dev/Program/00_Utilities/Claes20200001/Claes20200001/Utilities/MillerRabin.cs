using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charlotte.Utilities
{
	public static class MillerRabin
	{
		/// <summary>
		/// ミラーラビン素数判定法によって 0 以上 2^64 未満の整数が素数であるか判定する。
		/// 入力値の大きさに対して「確実に判定するために必要なテスト値」を全てテストしているので、
		/// このメソッドによる判定結果は常に正しい。
		/// </summary>
		/// <param name="n">判定する整数</param>
		/// <returns>判定結果</returns>
		public static bool IsPrime(ulong n)
		{
			if (n <= 61)
				return new uint[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61 }.Contains((uint)n);

			if (n % 2 == 0)
				return false;

			ulong d = n;
			int r;
			for (r = 0; ((d >>= 1) & 1) == 0; r++) ;

			if (n <= uint.MaxValue)
				return !new uint[] { 2, 7, 61 }
					.Any(x => !MillerRabinTest32(x, (uint)d, r, (uint)n));
			else
				return !new uint[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37 }
					.Any(x => !MillerRabinTest64((ulong)x, d, r, n));
		}

		private static bool MillerRabinTest32(uint x, uint d, int r, uint n)
		{
			x = ModPow32(x, d, n);

			if (x != 1 && x != n - 1)
			{
				for (int s = r; ; s--)
				{
					if (s <= 0)
						return false;

					x = (uint)(((ulong)x * x) % n);

					if (x == n - 1)
						break;
				}
			}
			return true;
		}

		private static uint ModPow32(uint b, uint e, uint m)
		{
			uint a = 1;

			for (; 1 <= e; e >>= 1)
			{
				if ((e & 1) != 0)
					a = (uint)(((ulong)a * b) % m);

				b = (uint)(((ulong)b * b) % m);
			}
			return a;
		}

		private static bool MillerRabinTest64(ulong x, ulong d, int r, ulong n)
		{
			x = ModPow64(x, d, n);

			if (x != 1 && x != n - 1)
			{
				for (int c = r; ; c--)
				{
					if (c <= 0)
						return false;

					x = ModPow64(x, 2, n);

					if (x == n - 1)
						break;
				}
			}
			return true;
		}

		private static ulong ModPow64(ulong b, ulong e, ulong m)
		{
			ulong a = 1;

			for (; 1 <= e; e >>= 1)
			{
				if ((e & 1) != 0)
					a = ModMul64(a, b, m);

				b = ModMul64(b, b, m);
			}
			return a;
		}

		private static ulong ModMul64(ulong b, ulong e, ulong m)
		{
			ulong a = 0;

			for (; 1 <= e; e >>= 1)
			{
				if ((e & 1) != 0)
					a = ModAdd64(a, b, m);

				b = ModAdd64(b, b, m);
			}
			return a;
		}

		private static ulong ModAdd64(ulong a, ulong b, ulong m)
		{
			ulong r = (ulong.MaxValue % m + 1) % m;

			while (ulong.MaxValue - a < b)
			{
				unchecked { a += b; }
				b = r;
			}
			return (a + b) % m;
		}
	}
}
