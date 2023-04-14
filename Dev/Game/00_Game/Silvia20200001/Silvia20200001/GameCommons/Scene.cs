using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charlotte.GameCommons
{
	public class Scene
	{
		public int Numer;
		public int Denom;

		public double Rate
		{
			get
			{
				return (double)this.Numer / this.Denom;
			}
		}

		public static IEnumerable<Scene> Create(int denom)
		{
			for (int numer = 0; numer <= denom; numer++)
			{
				yield return new Scene()
				{
					Numer = numer,
					Denom = denom,
				};
			}
		}
	}
}
