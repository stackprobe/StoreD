using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using Charlotte.Commons;

namespace Charlotte.GameCommons
{
	public class Music
	{
		private static List<Music> Instances = new List<Music>();

		public static void TouchAll()
		{
			foreach (Music instance in Instances)
				instance.GetHandle();
		}

		/// <summary>
		/// このメソッド実行時、全てのインスタンスは再生終了(未再生・停止)していること。
		/// </summary>
		public static void UnloadAll()
		{
			foreach (Music instance in Instances)
				instance.Unload();
		}

		private Func<byte[]> FileDataGetter;
		private Action<Music> PostLoaded = instance => { };

		private int Handle; // -1 == 未ロード

		/// <summary>
		/// リソースから音楽をロードする。
		/// </summary>
		/// <param name="resPath">リソースのパス</param>
		public Music(string resPath)
		{
			this.FileDataGetter = () => DD.GetResFileData(resPath);
			this.Handle = -1;

			Instances.Add(this);
		}

		/// <summary>
		/// リソースからループありの音楽をロードする。
		/// </summary>
		/// <param name="resPath">リソースのパス</param>
		/// <param name="loopStartPosition">ループ開始位置</param>
		/// <param name="loopLength">ループの長さ</param>
		public Music(string resPath, long loopStartPosition, long loopLength)
			: this(resPath)
		{
			long loopEndPosition = loopStartPosition + loopLength;

			this.PostLoaded = instance =>
			{
				if (DX.SetLoopSamplePosSoundMem(loopStartPosition, this.Handle) != 0) // ? 失敗 // ループ開始位置
					throw new Exception("SetLoopSamplePosSoundMem failed");

				if (DX.SetLoopStartSamplePosSoundMem(loopEndPosition, this.Handle) != 0) // ? 失敗 // ループ終了位置
					throw new Exception("SetLoopStartSamplePosSoundMem failed");
			};
		}

		public int GetHandle()
		{
			if (this.Handle == -1)
			{
				byte[] fileData = this.FileDataGetter();
				int handle = -1;

				DU.PinOn(fileData, p => handle = DX.LoadSoundMemByMemImage(p, (ulong)fileData.Length));

				if (handle == -1) // ? 失敗
					throw new Exception("LoadSoundMemByMemImage failed");

				this.Handle = handle;
				this.PostLoaded(this);
			}
			return this.Handle;
		}

		/// <summary>
		/// このメソッド実行時、再生終了(未再生・停止)していること。
		/// </summary>
		public void Unload()
		{
			if (this.Handle != -1)
			{
				// HACK: 再生中にアンロードされることを想定していない。

				if (DX.DeleteSoundMem(this.Handle) != 0) // ? 失敗
					throw new Exception("DeleteSoundMem failed");

				this.Handle = -1;
			}
		}

		private static LinkedList<Func<bool>> TaskSequence = new LinkedList<Func<bool>>();
		private static int LastVolume = -1;

		public static void EachFrame()
		{
			if (!DD.ExecuteTaskSequence(TaskSequence) && Playing != null) // ? アイドル状態 && 再生中
			{
				int volume = DD.RateToByte(GameSetting.MusicVolume);

				if (LastVolume != volume) // ? 前回の音量と違う -> 音量が変更されたので、新しい音量を適用する。
				{
					if (DX.ChangeVolumeSoundMem(DD.RateToByte(GameSetting.MusicVolume), Playing.GetHandle()) != 0) // ? 失敗
						throw new Exception("ChangeVolumeSoundMem failed");

					LastVolume = volume;
				}
			}
		}

		private static Music Playing = null;

		public void Play()
		{
			if (Playing != this)
			{
				Fadeout();
				TaskSequence.AddLast(SCommon.Supplier(this.E_Play()));
				Playing = this;
			}
		}

		public static void Fadeout()
		{
			if (Playing != null)
			{
				TaskSequence.AddLast(SCommon.Supplier(Playing.E_Fadeout()));
				Playing = null;
			}
		}

		private IEnumerable<bool> E_Play()
		{
			if (DX.ChangeVolumeSoundMem(0, this.GetHandle()) != 0) // ? 失敗
				throw new Exception("ChangeVolumeSoundMem failed");

			yield return true;

			if (DX.PlaySoundMem(this.GetHandle(), DX.DX_PLAYTYPE_LOOP, 1) != 0) // ? 失敗
				throw new Exception("PlaySoundMem failed");

			yield return true;
			yield return true;
			yield return true;

			if (DX.ChangeVolumeSoundMem(DD.RateToByte(GameSetting.MusicVolume), this.GetHandle()) != 0) // ? 失敗
				throw new Exception("ChangeVolumeSoundMem failed");
		}

		private IEnumerable<bool> E_Fadeout()
		{
			foreach (Scene scene in Scene.Create(60))
			{
				if (DX.ChangeVolumeSoundMem(DD.RateToByte(GameSetting.MusicVolume * (1.0 - scene.Rate)), this.GetHandle()) != 0) // ? 失敗
					throw new Exception("ChangeVolumeSoundMem failed");

				yield return true;
			}

			if (DX.StopSoundMem(this.GetHandle()) != 0) // ? 失敗
				throw new Exception("StopSoundMem failed");
		}
	}
}
