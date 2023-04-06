using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Charlotte.Commons
{
	public class WorkingDir : IDisposable
	{
		public static RootInfo Root = null;

		public class RootInfo
		{
			private string Dir = null;

			public string GetDir()
			{
				if (this.Dir == null)
				{
					string dir = GetRootDir();

					SCommon.DeletePath(dir);
					SCommon.CreateDir(dir);

					this.Dir = dir;
				}
				return this.Dir;
			}

			public void Delete()
			{
				if (this.Dir != null)
				{
					try
					{
						Directory.Delete(this.Dir, true);
					}
					catch (Exception e)
					{
						ProcMain.WriteLog(e);
					}

					this.Dir = null;
				}
			}
		}

		private static string GetRootDir()
		{
			return Path.Combine(Environment.GetEnvironmentVariable("TMP"), "{4c90ff11-7fe5-4664-a66d-62a440bb4826}_" + Process.GetCurrentProcess().Id);
		}

		private static ulong CtorCounter = 0UL;

		private string Dir = null;

		private string GetDir()
		{
			if (this.Dir == null)
			{
				if (Root == null)
					throw new Exception("Root is null");

				this.Dir = Path.Combine(Root.GetDir(), (CtorCounter++).ToString("x16"));

				SCommon.CreateDir(this.Dir);
			}
			return this.Dir;
		}

		public string GetPath(string localName)
		{
			return Path.Combine(this.GetDir(), localName);
		}

		private ulong PathCounter = 0UL;

		public string MakePath()
		{
			return this.GetPath((this.PathCounter++).ToString("x16"));
		}

		public void Dispose()
		{
			if (this.Dir != null)
			{
				try
				{
					Directory.Delete(this.Dir, true);
				}
				catch (Exception e)
				{
					ProcMain.WriteLog(e);
				}

				this.Dir = null;
			}
		}
	}
}
