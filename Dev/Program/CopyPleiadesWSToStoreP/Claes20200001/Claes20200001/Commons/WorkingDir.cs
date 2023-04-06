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
			private string Dir;
			private bool Created = false;

			public RootInfo(string dir)
			{
				this.Dir = dir;
			}

			public string GetDir()
			{
				if (!this.Created)
				{
					SCommon.DeletePath(this.Dir);
					SCommon.CreateDir(this.Dir);

					this.Created = true;
				}
				return this.Dir;
			}

			public void Delete()
			{
				if (this.Created)
				{
					try
					{
						Directory.Delete(this.Dir, true);
					}
					catch (Exception e)
					{
						ProcMain.WriteLog(e);
					}

					this.Created = false;
				}
			}
		}

		public static RootInfo CreateProcessRoot()
		{
			// 環境変数 TMP のパスは ProcMain.CheckLogonUserAndTmp() で検査している。-- ProcMain.GUIMain() の場合のみ

			return new RootInfo(Path.Combine(Environment.GetEnvironmentVariable("TMP"), ProcMain.APP_IDENT + "_" + Process.GetCurrentProcess().Id));
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
