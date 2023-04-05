using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DxLibDLL;
using Charlotte.Commons;
using Charlotte.Drawings;

namespace Charlotte.GameCommons
{
	/// <summary>
	/// ゲームに関する共通機能・便利機能はできるだけこのクラスに集約する。
	/// </summary>
	public static class DD
	{
		public static List<Action> Finalizers = new List<Action>();
		public static string MainWindowTitle;
		public static I4Rect TargetMonitor;
		public static I2Size RealScreenSize;
		public static SubScreen MainScreen;
		public static SubScreen LastMainScreen;
		public static SubScreen KeptMainScreen;
		public static long FrameStartTime;
		public static long HzChaserTime;
		public static int ProcFrame;
		public static bool WindowIsActive;

		private static Func<string, byte[]> ResFileDataGetter = null;

		public static byte[] GetResFileData(string resPath)
		{
			if (ResFileDataGetter == null)
				ResFileDataGetter = GetResFileDataGetter();

			return ResFileDataGetter(resPath);
		}

		private static Func<string, byte[]> GetResFileDataGetter()
		{
			string clusterFile = Path.Combine(ProcMain.SelfDir, "Resource.dat");
			Func<string, byte[]> getter;

			if (File.Exists(clusterFile))
			{
				ResourceCluster rc = new ResourceCluster(clusterFile);
				getter = resPath => rc.GetData(resPath);
			}
			else
			{
				getter = resPath => File.ReadAllBytes(Path.Combine(@"..\..\..\..\Resource", resPath));
			}
			return getter;
		}

		public static void EachFrame()
		{
			// TODO
			// TODO
			// TODO

			GC.Collect();

			DX.ScreenFlip();

			if (DX.CheckHitKey(DX.KEY_INPUT_ESCAPE) == 1 || DX.ProcessMessage() == -1)
			{
				throw new Exception("ゲーム中断");
			}

			// TODO
			// TODO
			// TODO
		}
	}
}
