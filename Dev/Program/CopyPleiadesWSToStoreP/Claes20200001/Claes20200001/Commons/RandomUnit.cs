using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Charlotte.Commons
{
	public class RandomUnit : IDisposable
	{
		public interface IRandomNumberGenerator : IDisposable
		{
			byte[] GetBlock();
		}

		private IRandomNumberGenerator Rng;

		public RandomUnit(IRandomNumberGenerator rng)
		{
			this.Rng = rng;
		}

		public void Dispose()
		{
			if (this.Rng != null)
			{
				this.Rng.Dispose();
				this.Rng = null;
			}
		}

		private byte[] Cache = SCommon.EMPTY_BYTES;
		private int RIndex = 0;

		public byte GetByte()
		{
			if (this.Cache.Length <= this.RIndex)
			{
				this.Cache = this.Rng.GetBlock();
				this.RIndex = 0;
			}
			return this.Cache[this.RIndex++];
		}

		public byte[] GetBytes(int length)
		{
			byte[] dest = new byte[length];

			for (int index = 0; index < length; index++)
				dest[index] = this.GetByte();

			return dest;
		}

		public uint GetUInt16()
		{
			byte[] r = GetBytes(2);

			return
				((uint)r[0] << 0) |
				((uint)r[1] << 8);
		}

		public uint GetUInt24()
		{
			byte[] r = GetBytes(3);

			return
				((uint)r[0] << 0) |
				((uint)r[1] << 8) |
				((uint)r[2] << 16);
		}

		public uint GetUInt()
		{
			byte[] r = GetBytes(4);

			return
				((uint)r[0] << 0) |
				((uint)r[1] << 8) |
				((uint)r[2] << 16) |
				((uint)r[3] << 24);
		}

		public ulong GetULong()
		{
			byte[] r = GetBytes(8);

			return
				((ulong)r[0] << 0) |
				((ulong)r[1] << 8) |
				((ulong)r[2] << 16) |
				((ulong)r[3] << 24) |
				((ulong)r[4] << 32) |
				((ulong)r[5] << 40) |
				((ulong)r[6] << 48) |
				((ulong)r[7] << 56);
		}

		public ulong GetULong_M(ulong modulo)
		{
			if (modulo == 0ul)
				throw new Exception("Bad modulo");

			ulong m = (ulong.MaxValue % modulo + 1ul) % modulo;
			ulong r;

			do
			{
				r = this.GetULong();
			}
			while (r < m);

			r %= modulo;

			return r;
		}

		public uint GetUInt_M(uint modulo)
		{
			return (uint)this.GetULong_M((ulong)modulo);
		}

		public long GetLong(long modulo)
		{
			return (long)this.GetULong_M((ulong)modulo);
		}

		public int GetInt(int modulo)
		{
			return (int)this.GetUInt_M((uint)modulo);
		}

		public int GetRange(int minval, int maxval)
		{
			return this.GetInt(maxval - minval + 1) + minval;
		}

		/// <summary>
		/// 真偽値をランダムに返す。
		/// </summary>
		/// <returns>真偽値</returns>
		public bool GetBoolean()
		{
			return this.GetInt(2) == 1;
		}

		/// <summary>
		/// -1 または 1 をランダムに返す。
		/// </summary>
		/// <returns>-1 または 1</returns>
		public int GetSign()
		{
			return this.GetInt(2) * 2 - 1;
		}

		/// <summary>
		/// 0.0 ～ 1.0 の乱数を返す。
		/// </summary>
		/// <returns>乱数</returns>
		public double GetReal1()
		{
			return (double)this.GetUInt() / uint.MaxValue;
		}

		/// <summary>
		/// -1.0 ～ 1.0 の乱数を返す。
		/// </summary>
		/// <returns>乱数</returns>
		public double GetReal2()
		{
			return GetReal1() * 2.0 - 1.0;
		}

		/// <summary>
		/// minval ～ maxval の乱数を返す。
		/// </summary>
		/// <returns>乱数</returns>
		public double GetReal3(double minval, double maxval)
		{
			return GetReal1() * (maxval - minval) + minval;
		}

		public T ChooseOne<T>(IList<T> list)
		{
			return list[this.GetInt(list.Count)];
		}

		public void Shuffle<T>(IList<T> list)
		{
			for (int index = list.Count; 1 < index; index--)
			{
				SCommon.Swap(list, this.GetInt(index), index - 1);
			}
		}
	}
}
