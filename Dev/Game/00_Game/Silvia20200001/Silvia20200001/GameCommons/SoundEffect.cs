using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using Charlotte.Commons;
using Charlotte.GameSettings;

namespace Charlotte.GameCommons
{
	public class SoundEffect
	{
		private static List<SoundEffect> Instances = new List<SoundEffect>();

		public static void UnloadAll()
		{
			foreach (SoundEffect instance in Instances)
				instance.Unload();
		}

		private Func<byte[]> FileDataGetter;

		private List<int> Handles; // null == 未ロード
		private int HandleIndex;
		private int LastVolume;

		public SoundEffect(string resPath)
		{
			this.FileDataGetter = () => DD.GetResFileData(resPath);
			this.Handles = null;

			Instances.Add(this);
		}

		private void LoadIfNeeded()
		{
			if (this.Handles == null)
			{
				byte[] fileData = this.FileDataGetter();
				int handle = -1;

				DU.PinOn(fileData, p => handle = DX.LoadSoundMemByMemImage(p, (ulong)fileData.Length));

				if (handle == -1) // ? 失敗
					throw new Exception("LoadSoundMemByMemImage failed");

				this.Handles = new List<int>();
				this.Handles.Add(handle);
				this.HandleIndex = 0;
				this.LastVolume = -1;
			}
		}

		public void Unload()
		{
			if (this.Handles != null)
			{
				// HACK: 再生中にアンロードされることを想定していない。

				foreach (int handle in DU.Reverse(this.Handles)) // 拡張したハンドルから削除しなければならない。なので逆順
					if (DX.DeleteSoundMem(handle) != 0) // ? 失敗
						throw new Exception("DeleteSoundMem failed");

				this.Handles = null;
			}
		}

		private static List<SoundEffect> PlayList = new List<SoundEffect>();

		public static void EachFrame()
		{
			if (1 <= PlayList.Count)
			{
				SoundEffect se = SCommon.DesertElement(PlayList, 0);

				if (se != null)
				{
					se.DoPlay();
				}
			}
		}

		public void Play()
		{
			if (PlayList.Where(v => v == this).Count() < 2)
			{
				PlayList.Add(this);
				PlayList.Add(null);
			}
		}

		private void DoPlay()
		{
			this.LoadIfNeeded();

			this.HandleIndex++;
			this.HandleIndex %= this.Handles.Count;

			if (IsPlaying(this.Handles[this.HandleIndex]))
			{
				int index = SCommon.IndexOf(this.Handles, v => !IsPlaying(v));

				if (index == -1)
				{
					this.Extend();
					PlayByHandle(this.Handles[this.Handles.Count - 1]);
				}
				else
				{
					this.ChangeVolumeIfNeeded();
					PlayByHandle(this.Handles[index]);
				}
			}
			else
			{
				this.ChangeVolumeIfNeeded();
				PlayByHandle(this.Handles[this.HandleIndex]);
			}
		}

		private static bool IsPlaying(int handle)
		{
			switch (DX.CheckSoundMem(handle))
			{
				case 1: // ? 再生中
					return true;

				case 0: // ? 停止
					return false;

				case -1: // ? チェック失敗
					throw new Exception("CheckSoundMem failed");

				default: // ? エラー
					throw new Exception("CheckSoundMem error");
			}
		}

		private static void PlayByHandle(int handle)
		{
			if (DX.ChangeVolumeSoundMem(SCommon.ToInt(GameSetting.SEVolume * 255.0), handle) != 0) // ? 失敗
				throw new Exception("ChangeVolumeSoundMem failed");

			if (DX.PlaySoundMem(handle, DX.DX_PLAYTYPE_BACK, 1) != 0) // ? 失敗
				throw new Exception("PlaySoundMem failed");
		}

		private void Extend()
		{
			this.LoadIfNeeded();

			int handle = DX.DuplicateSoundMem(this.Handles[0]);

			if (handle == -1) // ? 失敗
				throw new Exception("DuplicateSoundMem failed");

			if (DX.ChangeVolumeSoundMem(SCommon.ToInt(GameSetting.SEVolume * 255.0), handle) != 0) // ? 失敗
				throw new Exception("ChangeVolumeSoundMem failed");

			this.Handles.Add(handle);
		}

		private void ChangeVolumeIfNeeded()
		{
			if (this.Handles != null) // ? ロードされている。
			{
				int volume = SCommon.ToInt(GameSetting.SEVolume * 255.0);

				if (this.LastVolume != volume) // ? 前回の音量と違う -> 音量が変更されたので、新しい音量を全てのハンドルに適用する。
				{
					foreach (int handle in this.Handles)
						if (DX.ChangeVolumeSoundMem(volume, handle) != 0) // ? 失敗
							throw new Exception("ChangeVolumeSoundMem failed");

					this.LastVolume = volume;
				}
			}
		}
	}
}
