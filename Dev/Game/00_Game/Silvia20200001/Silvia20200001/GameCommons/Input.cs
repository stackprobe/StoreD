using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charlotte.GameCommons
{
	public class Input
	{
		public int Key;
		public int Button;
		public string Description;

		public Input(int key, int button, string description)
		{
			this.Key = key;
			this.Button = button;
		}

		// MEMO: ボタン・キー押下は 1 マウス押下は -1 で判定する。

		public int GetInput()
		{
			int value = Keyboard.GetInput(this.Key);

			if (value == 0 && Pad.PrimaryPad != -1)
				value = Pad.GetInput(Pad.PrimaryPad, this.Button);

			return value;
		}

		public bool IsPound()
		{
			return DU.IsPound(this.GetInput());
		}
	}
}
