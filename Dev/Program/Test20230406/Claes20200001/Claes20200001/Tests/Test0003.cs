using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Utilities;
using Charlotte.Commons;

namespace Charlotte.Tests
{
	public class Test0003
	{
		public void Test01()
		{
			Canvas canvas = Canvas.LoadFromFile(@"C:\temp\KAZUKIcghvbnkm.jpg");

			canvas = canvas.Expand(960, 540);

			canvas.Save(SCommon.NextOutputPath() + ".png");
		}
	}
}
