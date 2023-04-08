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
			if (key < 0 || Keyboard.KEY_MAX <= key)
				throw new Exception("Bad key");

			if (button < 0 || Pad.BUTTON_MAX <= button)
				throw new Exception("Bad button");

			if (string.IsNullOrEmpty(description))
				throw new Exception("Bad description");

			this.Key = key;
			this.Button = button;
			this.Description = description;
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
