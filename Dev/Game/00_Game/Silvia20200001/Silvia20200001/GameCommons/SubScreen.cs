using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using Charlotte.Commons;
using Charlotte.Drawings;

namespace Charlotte.GameCommons
{
	public class SubScreen
	{
		private static List<SubScreen> Instances = new List<SubScreen>();

		public static void UnloadAll()
		{
			foreach (SubScreen instance in Instances)
				instance.Unload();
		}

		public int W { get; private set; }
		public int H { get; private set; }
		public bool AFlag { get; private set; }
		private int Handle;

		public SubScreen(int w, int h, bool aFlag = false)
		{
			if (w < 1 || SCommon.IMAX < w)
				throw new Exception("Bad w");

			if (h < 1 || SCommon.IMAX < h)
				throw new Exception("Bad h");

			this.W = w;
			this.H = h;
			this.AFlag = aFlag;
			this.Handle = -1;

			Instances.Add(this);
		}

		public int GetHandle()
		{
			if (this.Handle == -1)
			{
				this.Handle = DX.MakeScreen(this.W, this.H, this.AFlag ? 1 : 0);

				if (this.Handle == -1) // ? 失敗
					throw new Exception("MakeScreen failed");
			}
			return this.Handle;
		}

		public void Unload()
		{
			if (this.Handle != -1)
			{
				if (DX.DeleteGraph(this.Handle) != 0) // ? 失敗
					throw new Exception("DeleteGraph failed");

				this.Handle = -1;
			}
		}

		public void ChangeDrawScreenToThis()
		{
			if (DX.SetDrawScreen(this.GetHandle()) != -1) // ? 失敗
				throw new Exception("SetDrawScreen failed");
		}

		public static void ChangeDrawScreenToBack()
		{
			if (DX.SetDrawScreen(DX.DX_SCREEN_BACK) != -1) // ? 失敗
				throw new Exception("SetDrawScreen failed");
		}
	}
}
