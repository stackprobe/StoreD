using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using Charlotte.Commons;

namespace Charlotte.GameCommons
{
	/// <summary>
	/// 画像リソース
	/// このクラスのインスタンスはプロセスで有限個であること。
	/// 原則的に以下のクラスの静的フィールドとして植え込むこと。
	/// -- Pictures
	/// </summary>
	public class Picture
	{
		private static DU.Collector<Picture> Instances = new DU.Collector<Picture>();

		public static void TouchAll()
		{
			foreach (Picture instance in Instances.Iterate())
				instance.GetHandle();
		}

		public static void UnloadAll()
		{
			foreach (Picture instance in Instances.Iterate())
				instance.Unload();
		}

		public class PictureDataInfo
		{
			public int Handle;
			public int W;
			public int H;
		}

		private Func<PictureDataInfo> PictureDataGetter;
		private Action<int> HandleUnloader;

		private PictureDataInfo PictureData = null;

		/// <summary>
		/// リソースから画像を作成する。
		/// </summary>
		/// <param name="resPath">リソースのパス</param>
		public Picture(string resPath)
		{
			this.PictureDataGetter = () => DU.GetPictureData(DD.GetResFileData(resPath));
			this.HandleUnloader = handle =>
			{
				if (DX.DeleteGraph(handle) != 0) // ? 失敗
					throw new Exception("DeleteGraph failed");
			};

			Instances.Add(this);
		}

		/// <summary>
		/// ローダーを指定して画像を作成する。
		/// </summary>
		/// <param name="loader">ローダー</param>
		public Picture(Func<PictureDataInfo> loader)
		{
			this.PictureDataGetter = loader;
			this.HandleUnloader = handle =>
			{
				if (DX.DeleteGraph(handle) != 0) // ? 失敗
					throw new Exception("DeleteGraph failed");
			};

			Instances.Add(this);
		}

		/// <summary>
		/// アンロード不要な画像を作成する。
		/// </summary>
		/// <param name="w">幅</param>
		/// <param name="h">高さ</param>
		/// <param name="handleGetter">ハンドル取得メソッド</param>
		public Picture(int w, int h, Func<int> handleGetter)
		{
			if (w < 1 || SCommon.IMAX < w)
				throw new Exception("Bad w");

			if (h < 1 || SCommon.IMAX < h)
				throw new Exception("Bad h");

			if (handleGetter == null)
				throw new Exception("Bad handleGetter");

			this.PictureDataGetter = () => new PictureDataInfo()
			{
				Handle = handleGetter(),
				W = w,
				H = h,
			};
			this.HandleUnloader = handle => { };

			Instances.Add(this);
		}

		private PictureDataInfo GetPictureData()
		{
			if (this.PictureData == null)
				this.PictureData = this.PictureDataGetter();

			return this.PictureData;
		}

		public void Unload()
		{
			if (this.PictureData != null)
			{
				this.HandleUnloader(this.PictureData.Handle);
				this.PictureData = null;
			}
		}

		public int GetHandle()
		{
			return this.GetPictureData().Handle;
		}

		public int W
		{
			get
			{
				return this.GetPictureData().W;
			}
		}

		public int H
		{
			get
			{
				return this.GetPictureData().H;
			}
		}
	}
}
