using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using Charlotte.GameCommons;

namespace Charlotte
{
	public static class Inputs
	{
		public static Input DIR_2 = new Input(DX.KEY_INPUT_DOWN, 0, "下");
		public static Input DIR_4 = new Input(DX.KEY_INPUT_LEFT, 1, "左");
		public static Input DIR_6 = new Input(DX.KEY_INPUT_RIGHT, 2, "右");
		public static Input DIR_8 = new Input(DX.KEY_INPUT_UP, 3, "上");
		public static Input A = new Input(DX.KEY_INPUT_Z, 4, "効果Ⅰ・決定");
		public static Input B = new Input(DX.KEY_INPUT_X, 7, "効果Ⅱ・キャンセル");
		public static Input C = new Input(DX.KEY_INPUT_C, 5, "特殊Ⅰ");
		public static Input D = new Input(DX.KEY_INPUT_V, 8, "特殊Ⅱ");
		public static Input E = new Input(DX.KEY_INPUT_B, 6, "特殊Ⅲ");
		public static Input F = new Input(DX.KEY_INPUT_N, 9, "特殊Ⅳ");
		public static Input L = new Input(DX.KEY_INPUT_LCONTROL, 10, "左補助");
		public static Input R = new Input(DX.KEY_INPUT_LSHIFT, 11, "右補助");
		public static Input PAUSE = new Input(DX.KEY_INPUT_SPACE, 13, "ポーズ");
		public static Input START = new Input(DX.KEY_INPUT_RETURN, 12, "スタート");

		/// <summary>
		/// 全ての入力を列挙する。
		/// </summary>
		/// <returns>全ての入力</returns>
		public static Input[] GetAllInput()
		{
			return new Input[]
			{
				DIR_8,
				DIR_2,
				DIR_4,
				DIR_6,
				A,
				B,
				C,
				D,
				E,
				F,
				L,
				R,
				PAUSE,
				START,
			};
		}
	}
}
