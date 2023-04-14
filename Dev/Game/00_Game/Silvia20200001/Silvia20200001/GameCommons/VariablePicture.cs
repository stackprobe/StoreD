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
	public class VariablePicture
	{
		private string _imageFile = null;

		private string ImageFile
		{
			get
			{
				if (_imageFile == null)
					_imageFile = new WorkingDir().MakePath() + ".png";

				return _imageFile;
			}
		}

		/// <summary>
		/// 現在の描画先スクリーンの指定領域をイメージデータとして設定する。
		/// </summary>
		/// <param name="rect">指定領域</param>
		public void ToImageData(I4Rect rect)
		{
			using (WorkingDir wd = new WorkingDir())
			{
				string bmpFile = wd.MakePath() + ".bmp";

				DX.SaveDrawScreenToBMP(rect.L, rect.T, rect.R, rect.B, bmpFile);

				using (Bitmap bmp = (Bitmap)Bitmap.FromFile(bmpFile))
				{
					bmp.Save(this.ImageFile, ImageFormat.Png);
				}
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

		private Picture Picture = null;

		public Picture GetPicture()
		{
			if (this.Picture == null)
				this.Picture = new Picture(() => DU.GetPictureData(this.GetImageData()));

			return this.Picture;
		}
	}
}
