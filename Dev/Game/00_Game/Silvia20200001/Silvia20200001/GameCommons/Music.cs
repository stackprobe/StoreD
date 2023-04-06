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

		public static void UnloadAll()
		{
			foreach (Music instance in Instances)
				instance.Unload();
		}

		private Func<byte[]> FileDataGetter;

		private int Handle; // -1 == 未ロード

		public Music(string resPath)
		{
			this.FileDataGetter = () => DD.GetResFileData(resPath);
			this.Handle = -1;

			Instances.Add(this);
		}

		public Music(string resPath, int loopStartPosition, int loopLength)
		{
			throw null; // TODO
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
			}
			return this.Handle;
		}

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

		private static List<Func<bool>> Tasks = new List<Func<bool>>();

		public static void EachFrame()
		{
			if (1 <= Tasks.Count && !Tasks[0]())
			{
				Tasks.RemoveAt(0);
			}
		}

		private static Music Playing = null;

		public void Play()
		{
			if (Playing != this)
			{
				Fadeout();
				Tasks.Add(SCommon.Supplier(this.E_Play()));
				Playing = this;
			}
		}

		public static void Fadeout()
		{
			if (Playing != null)
			{
				Tasks.Add(SCommon.Supplier(Playing.E_Fadeout()));
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
