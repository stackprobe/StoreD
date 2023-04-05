using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DxLibDLL;
using Charlotte.Commons;

namespace Charlotte.GameCommons
{
	/// <summary>
	/// ゲームに関する共通機能・便利機能はできるだけこのクラスに集約する。
	/// </summary>
	public static class DD
	{
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

		private static Dictionary<string, List<SubScreen>> SubScreenPool = SCommon.CreateDictionary<List<SubScreen>>();

		private static string GetSubScreenPoolKey(int w, int h, bool aFlag)
		{
			return string.Join("_", w, h, aFlag);
		}

		public static SubScreen GetSubScreen(int w, int h, bool aFlag = false)
		{
			string key = GetSubScreenPoolKey(w, h, aFlag);

			if (!SubScreenPool.ContainsKey(key))
				SubScreenPool.Add(key, new List<SubScreen>());

			List<SubScreen> stack = SubScreenPool[key];

			if (stack.Count == 0)
				stack.Add(new SubScreen(w, h, aFlag));

			return SCommon.UnaddElement(stack);
		}

		public static void ReturnSubScreen(SubScreen subScreen)
		{
			SubScreenPool[GetSubScreenPoolKey(subScreen.W, subScreen.H, subScreen.AFlag)].Add(subScreen);
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

		// TODO
		// TODO
		// TODO
	}
}
