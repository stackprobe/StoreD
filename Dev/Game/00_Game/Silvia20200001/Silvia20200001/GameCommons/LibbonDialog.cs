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
	/// <summary>
	/// リボン
	/// ゲーム画面より前面にリボン(横長の矩形領域)を表示してメッセージを表示する。
	/// 以下から表示・非表示の制御を行うこと。
	/// -- DD.SetLibbon()
	/// </summary>
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

		public I4Rect TargetMonitor;
		public string P_Message;

		public bool CloseFlag = false;

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

			// ? 開発環境のモニタよりも小さい
			if (
				this.TargetMonitor.W < 1920 ||
				this.TargetMonitor.H < 1080
				)
				fontSize = 24f; // 画面からはみ出ないように小さくする。
			else
				fontSize = 48f; // 想定フォントサイズ

			this.BackColor = Color.FromArgb(0, 64, 64);
			this.FormBorderStyle = FormBorderStyle.None;
			this.Message.Font = new Font("メイリオ", fontSize);
			this.Message.ForeColor = Color.FromArgb(255, 255, 255);
			this.Message.Text = this.P_Message;

			const int MARGIN = 30;

			this.Width = this.TargetMonitor.W;
			this.Height = MARGIN + this.Message.Height + MARGIN;
			this.Left = this.TargetMonitor.L;
			this.Top = (this.TargetMonitor.H - this.Height) / 2;
			this.Message.Left = (this.Width - this.Message.Width) / 2;
			this.Message.Top = MARGIN;

			this.WaitAndClose(500);
		}

		private void WaitAndClose(int millis)
		{
			Thread th = new Thread(() =>
			{
				Thread.Sleep(millis);

				DD.RunOnUIThread(() =>
				{
					if (this.CloseFlag)
					{
						this.Close();
					}
					else
					{
						this.WaitAndClose(100); // HACK: 上位のスレッド(th)の終了を待たない。
					}
				});
			});

			th.Start();
		}
	}
}
