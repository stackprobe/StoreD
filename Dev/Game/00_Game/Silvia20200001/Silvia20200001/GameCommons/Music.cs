using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using Charlotte.Commons;
using Charlotte.GameSettings;

namespace Charlotte.GameCommons
{
	public class Music
	{
		private static List<Music> Instances = new List<Music>();

		/// <summary>
		/// このメソッド実行時、全てのインスタンスは再生終了(未再生・停止)していること。
		/// </summary>
		public static void UnloadAll()
		{
			foreach (Music instance in Instances)
				instance.Unload();
		}

		private Func<byte[]> FileDataGetter;
		private long[] LoopRange = null;

		private int Handle; // -1 == 未ロード

		public Music(string resPath)
		{
			this.FileDataGetter = () => DD.GetResFileData(resPath);
			this.Handle = -1;

			Instances.Add(this);
		}

		public Music(string resPath, long loopStartPosition, long loopLength)
			: this(resPath)
		{
			this.LoopRange = new long[]
			{
				loopStartPosition,
				loopStartPosition + loopLength,
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

				if (this.LoopRange != null)
				{
					DX.SetLoopSamplePosSoundMem(this.LoopRange[0], handle); // ループ開始位置
					DX.SetLoopStartSamplePosSoundMem(this.LoopRange[1], handle); // ループ終了位置
				}
				this.Handle = handle;
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
				if (DX.DeleteSoundMem(this.Handle) != 0) // ? 失敗
					throw new Exception("DeleteSoundMem failed");

				this.Handle = -1;
			}
		}

		private static List<Func<bool>> TaskList = new List<Func<bool>>();
		private static int LastVolume = -1;

		public static void EachFrame()
		{
			if (1 <= TaskList.Count && !TaskList[0]()) // Busy
			{
				TaskList.RemoveAt(0);
			}
			else // Idle
			{
				if (Playing != null) // ? 再生中
				{
					int volume = SCommon.ToInt(GameSetting.MusicVolume * 255.0);

					if (LastVolume != volume) // ? 前回の音量と違う -> 音量が変更されたので、新しい音量を適用する。
					{
						if (DX.ChangeVolumeSoundMem(SCommon.ToInt(GameSetting.MusicVolume * 255.0), Playing.GetHandle()) != 0) // ? 失敗
							throw new Exception("ChangeVolumeSoundMem failed");

						LastVolume = volume;
					}
				}
			}
		}

		private static Music Playing = null;

		public void Play()
		{
			if (Playing != this)
			{
				Fadeout();
				TaskList.Add(SCommon.Supplier(this.E_Play()));
				Playing = this;
			}
		}

		public static void Fadeout()
		{
			if (Playing != null)
			{
				TaskList.Add(SCommon.Supplier(Playing.E_Fadeout()));
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

			if (DX.ChangeVolumeSoundMem(SCommon.ToInt(GameSetting.MusicVolume * 255.0), this.GetHandle()) != 0) // ? 失敗
				throw new Exception("ChangeVolumeSoundMem failed");
		}

		private IEnumerable<bool> E_Fadeout()
		{
			foreach (DD.Scene scene in DD.CreateScene(30))
			{
				if (DX.ChangeVolumeSoundMem(SCommon.ToInt(GameSetting.MusicVolume * 255.0 * (1.0 - scene.Rate)), this.GetHandle()) != 0) // ? 失敗
					throw new Exception("ChangeVolumeSoundMem failed");

				yield return true;
			}

			if (DX.StopSoundMem(this.GetHandle()) != 0) // ? 失敗
				throw new Exception("StopSoundMem failed");
		}
	}
}
