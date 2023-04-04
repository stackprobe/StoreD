using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;
using Charlotte.Utilities;

namespace Charlotte.Tests
{
	/// <summary>
	/// Canvas.cs テスト
	/// </summary>
	public class Test0014
	{
		public void Test01()
		{
			Canvas canvas = new Canvas(100, 100);
			canvas.Fill(new Drawings.I4Color(255, 128, 0, 255));
			canvas.Save(SCommon.NextOutputPath() + ".png");
		}

		public void Test02()
		{
			Canvas canvas = new Canvas(300, 300);
			canvas.Fill(new Drawings.I4Color(255, 0, 0, 255));
			canvas.FillCircle(new Drawings.I4Color(0, 255, 0, 255), new Drawings.I2Point(150, 150), 100);
			canvas = canvas.SetMargin((dot, x, y) => dot.R == 255, new Drawings.I4Color(0, 0, 255, 255), 50);
			canvas.Save(SCommon.NextOutputPath() + ".png");
		}
	}
}
