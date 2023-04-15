using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using DxLibDLL;
using Charlotte.Commons;
using Charlotte.Drawings;

namespace Charlotte.GameCommons
{
	/// <summary>
	/// 編集可能な画像リソース
	/// このクラスのインスタンスはプロセスで有限個であること。
	/// 原則的に任意のクラスの静的フィールドとして植え込むこと。
	/// </summary>
	public class VariablePicture
	{
		private string _imageFile = null;

		private string ImageFile
		{
			get
			{
				if (_imageFile == null)
					_imageFile = DU.WD.MakePath();

				return _imageFile;
			}
		}

		/// <summary>
		/// イメージデータを取得する。
		/// </summary>
		/// <returns>イメージデータ</returns>
		public byte[] GetImageData()
		{
			byte[] imageData;

			if (File.Exists(this.ImageFile))
				imageData = File.ReadAllBytes(this.ImageFile);
			else
				imageData = DD.GetResFileData(@"General\Dummy.png"); // イメージデータ未設定につき、適当な画像データを返す。

			return imageData;
		}

		/// <summary>
		/// イメージデータを設定する。
		/// </summary>
		/// <param name="imageData">イメージデータ</param>
		public void SetImageData(byte[] imageData)
		{
			if (imageData == null)
				throw new Exception("Bad imageData");

			File.WriteAllBytes(this.ImageFile, imageData);

			if (this.Picture != null)
				this.Picture.Unload();
		}

		/// <summary>
		/// 現在の描画先スクリーンの指定領域をイメージデータとして設定する。
		/// </summary>
		/// <param name="rect">指定領域</param>
		public void ToImageData(I4Rect rect)
		{
			string bmpFile = DU.WD.MakePath();

			DX.SaveDrawScreenToBMP(rect.L, rect.T, rect.R, rect.B, bmpFile);

			using (Bitmap bmp = (Bitmap)Bitmap.FromFile(bmpFile))
			{
				bmp.Save(this.ImageFile, ImageFormat.Png);
			}

			SCommon.DeletePath(bmpFile);

			if (this.Picture != null)
				this.Picture.Unload();
		}

		private Picture Picture = null;

		public Picture GetPicture()
		{
			if (this.Picture == null)
				this.Picture = new Picture(() => DU.GetPictureData(this.GetImageData()));

			return this.Picture;
		}
	}
}
