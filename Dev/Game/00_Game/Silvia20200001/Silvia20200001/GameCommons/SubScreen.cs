using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using Charlotte.Commons;
using Charlotte.Drawings;

namespace Charlotte.GameCommons
{
	/// <summary>
	/// スクリーン
	/// このクラスのインスタンスはプロセスで有限個であること。
	/// 原則的に任意のクラスの静的フィールドとして植え込むこと。
	/// </summary>
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

		private int Handle; // -1 == 未ロード

		public SubScreen(int w, int h)
		{
			if (w < 1 || SCommon.IMAX < w)
				throw new Exception("Bad w");

			if (h < 1 || SCommon.IMAX < h)
				throw new Exception("Bad h");

			this.W = w;
			this.H = h;
			this.Handle = -1;

			Instances.Add(this);
		}

		public int GetHandle()
		{
			if (this.Handle == -1)
			{
				this.Handle = DX.MakeScreen(this.W, this.H, 0); // 幅, 高さ, 画像の透明度を有効にするか/1:有効/0:無効

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

				if (this.Picture != null)
					this.Picture.Unload();
			}
		}

		public void ChangeDrawScreenToThis()
		{
			if (DX.SetDrawScreen(this.GetHandle()) != 0) // ? 失敗
				throw new Exception("SetDrawScreen failed");
		}

		public static void ChangeDrawScreenToBack()
		{
			if (DX.SetDrawScreen(DX.DX_SCREEN_BACK) != 0) // ? 失敗
				throw new Exception("SetDrawScreen failed");
		}

		private Picture Picture = null;

		public Picture GetPicture()
		{
			if (this.Picture == null)
				this.Picture = new Picture(this.W, this.H, this.GetHandle);

			return this.Picture;
		}

		public bool IsLoaded()
		{
			return this.Handle != -1;
		}

		public static IEnumerable<SubScreen> GetAllSubScreen()
		{
			return Instances;
		}

		public object StoredData = null;
	}
}
