using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Security.Permissions;
using System.Windows.Forms;
using Charlotte.Commons;
using Charlotte.Drawings;
using Charlotte.GameCommons;

namespace Charlotte.GameCommons
{
	public partial class LibbonDialog : Form
	{
		#region WndProc

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		protected override void WndProc(ref Message m)
		{
			const int WM_SYSCOMMAND = 0x112;
			const long SC_CLOSE = 0xF060L;

			if (m.Msg == WM_SYSCOMMAND && (m.WParam.ToInt64() & 0xFFF0L) == SC_CLOSE)
				return;

			base.WndProc(ref m);
		}

		#endregion

		public I4Rect P_TargetMonitor;
		public string P_Message;

		public LibbonDialog()
		{
			InitializeComponent();
		}

		private void LibbonDialog_Load(object sender, EventArgs e)
		{
			// none
		}

		private void LibbonDialog_Shown(object sender, EventArgs e)
		{
			float fontSize;

			if (
				this.P_TargetMonitor.W < 1920 ||
				this.P_TargetMonitor.H < 1080
				)
				fontSize = 24f;
			else
				fontSize = 48f;

			this.BackColor = Color.FromArgb(0, 64, 64);
			this.FormBorderStyle = FormBorderStyle.None;
			this.Message.Font = new Font("メイリオ", fontSize);
			this.Message.ForeColor = Color.FromArgb(255, 255, 255);
			this.Message.Text = this.P_Message;

			const int MARGIN = 30;

			this.Width = this.P_TargetMonitor.W;
			this.Height = MARGIN + this.Message.Height + MARGIN;
			this.Left = this.P_TargetMonitor.L;
			this.Top = (this.P_TargetMonitor.H - this.Height) / 2;
			this.Message.Left = (this.Width - this.Message.Width) / 2;
			this.Message.Top = MARGIN;

			this.P_Timer(500);
		}

		private void P_Timer(int millis)
		{
			Thread th = new Thread(() =>
			{
				Thread.Sleep(millis);

				DD.RunOnUIThread(() =>
				{
					if (string.IsNullOrEmpty(this.P_Message))
					{
						this.Close();
					}
					else
					{
						this.P_Timer(100); // HACK: このスレッドの終了を待たない。
					}
				});
			});

			th.Start();
		}
	}
}
