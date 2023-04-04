using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;

namespace Charlotte.Utilities
{
	public class CtrCipher : IDisposable
	{
		public static CtrCipher CreateTemporary()
		{
			return new CtrCipher(SCommon.CRandom.GetBytes(32), SCommon.CRandom.GetBytes(16));
		}

		private AESCipher Transformer;
		private byte[] InitializationVector;
		private byte[] Counter = new byte[16];
		private byte[] Buffer = new byte[16];
		private int Index;

		public CtrCipher(byte[] rawKey, byte[] iv)
		{
			if (
				rawKey == null ||
				iv == null ||
				iv.Length != 16
				)
				throw new ArgumentException();

			this.Transformer = new AESCipher(rawKey);
			this.InitializationVector = iv;
			this.Reset();
		}

		public void Reset()
		{
			Array.Copy(this.InitializationVector, this.Counter, 16);
			this.Index = 16;
		}

		public byte Next()
		{
			if (this.Index == 16)
			{
				this.Transformer.EncryptBlock(this.Counter, this.Buffer);
				this.Increment();
				this.Index = 0;
			}
			return this.Buffer[this.Index++];
		}

		private void Increment()
		{
			for (int index = 0; index < 16; index++)
			{
				if (this.Counter[index] < 255)
				{
					this.Counter[index]++;
					break;
				}
				this.Counter[index] = 0;
			}
		}

		public void Mask(byte[] data, int offset = 0)
		{
			this.Mask(data, offset, data.Length - offset);
		}

		public void Mask(byte[] data, int offset, int count)
		{
			for (int index = 0; index < count; index++)
			{
				data[offset + index] ^= this.Next();
			}
		}

		public void Dispose()
		{
			if (this.Transformer != null)
			{
				this.Transformer.Dispose();
				this.Transformer = null;
			}
		}
	}
}
