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

		/// <summary>
		/// このメソッド実行時、全てのインスタンスは再生終了(未再生・停止)していること。
		/// </summary>
		public static void UnloadAll()
		{
			foreach (SoundEffect instance in Instances)
				instance.Unload();
		}

		private Func<byte[]> FileDataGetter;

		private class HandleInfo
		{
			public int Value;
			public int LastVolume = -1;

			public HandleInfo(int value)
			{
				this.Value = value;
			}
		}

		private List<HandleInfo> Handles; // null == 未ロード
		private int LastIndex;

		/// <summary>
		/// リソースから効果音をロードする。
		/// </summary>
		/// <param name="resPath">リソースのパス</param>
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

				this.Handles = new List<HandleInfo>();
				this.Handles.Add(new HandleInfo(handle));
				this.LastIndex = 0;
			}
		}

		/// <summary>
		/// このメソッド実行時、再生終了(未再生・停止)していること。
		/// </summary>
		public void Unload()
		{
			if (this.Handles != null)
			{
				// HACK: 再生中にアンロードされることを想定していない。

				foreach (HandleInfo handle in DU.Reverse(this.Handles)) // 拡張したハンドルから削除しなければならない。なので逆順
					if (DX.DeleteSoundMem(handle.Value) != 0) // ? 失敗
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

			this.LastIndex++;
			this.LastIndex %= this.Handles.Count;

			if (IsPlaying(this.Handles[this.LastIndex].Value))
			{
				int index = SCommon.IndexOf(this.Handles, v => !IsPlaying(v.Value));

				if (index == -1)
				{
					this.Extend();
					PlayByHandle(this.Handles[this.Handles.Count - 1]);
				}
				else
				{
					PlayByHandle(this.Handles[index]);
				}
			}
			else
			{
				PlayByHandle(this.Handles[this.LastIndex]);
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

		private static void PlayByHandle(HandleInfo handle)
		{
			ChangeVolumeIfNeeded(handle);

			if (DX.PlaySoundMem(handle.Value, DX.DX_PLAYTYPE_BACK, 1) != 0) // ? 失敗
				throw new Exception("PlaySoundMem failed");
		}

		private static void ChangeVolumeIfNeeded(HandleInfo handle)
		{
			int volume = DU.RateToByte(GameSetting.SEVolume);

			if (handle.LastVolume != volume) // ? 前回の音量と違う -> 音量が変更されたので、新しい音量を適用する。
			{
				if (DX.ChangeVolumeSoundMem(volume, handle.Value) != 0) // ? 失敗
					throw new Exception("ChangeVolumeSoundMem failed");

				handle.LastVolume = volume;
			}
		}

		private void Extend()
		{
			int handle = DX.DuplicateSoundMem(this.Handles[0].Value);

			if (handle == -1) // ? 失敗
				throw new Exception("DuplicateSoundMem failed");

			this.Handles.Add(new HandleInfo(handle));
		}
	}
}
