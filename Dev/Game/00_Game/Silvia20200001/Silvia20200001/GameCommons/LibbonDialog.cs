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
	/// -- SetMessage()
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

		public static Thread Th;
		public static bool AliveFlag = true;

		private static object SYNCROOT = new object();
		private static bool ChangeFlag = false;
		private static I4Rect P_TargetMonitor;
		private static string P_Message = null;

		/// <summary>
		/// メッセージの表示・非表示を行う。
		/// 以下を経由して呼び出すこと。
		/// -- DD.SetLibbon()
		/// </summary>
		/// <param name="message">メッセージ</param>
		public static void SetMessage(string message)
		{
			lock (SYNCROOT)
			{
				ChangeFlag = true;
				P_TargetMonitor = DD.TargetMonitor;
				P_Message = message;
			}
		}

		private static LibbonDialog Instance = null;

		public static void MainTh()
		{
			while (AliveFlag)
			{
				if (ChangeFlag)
				{
					lock (SYNCROOT)
					{
						ChangeFlag = false;

						// メインスレッドで参照されるので、ローカル変数に退避する。
						I4Rect targetMonitor = P_TargetMonitor;
						string message = P_Message;

						DD.RunOnUIThread(() =>
						{
							P_Hide();

							if (!string.IsNullOrEmpty(P_Message))
							{
								Instance = new LibbonDialog();
								Instance.TargetMonitor = targetMonitor;
								Instance.Message = message;
								Instance.Show();
							}
						});
					}
					Thread.Sleep(500); // リボンの最短表示時間待ち
				}
				else
				{
					Thread.Sleep(100); // ループ待機待ち
				}
			}

			DD.RunOnUIThread(() =>
			{
				P_Hide();
			});

			Thread.Sleep(100); // リボンが閉じるのを待つ // HACK: 同期していない。
		}

		private static void P_Hide()
		{
			if (Instance != null)
			{
				Instance.Close();
				Instance = null;
			}
		}

		private I4Rect TargetMonitor;
		private string Message;

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
			this.MessageLabel.Font = new Font("メイリオ", fontSize);
			this.MessageLabel.ForeColor = Color.FromArgb(255, 255, 255);
			this.MessageLabel.Text = this.Message;

			const int MARGIN = 30;

			this.Width = this.TargetMonitor.W;
			this.Height = MARGIN + this.MessageLabel.Height + MARGIN;
			this.Left = this.TargetMonitor.L;
			this.Top = (this.TargetMonitor.H - this.Height) / 2;
			this.MessageLabel.Left = (this.Width - this.MessageLabel.Width) / 2;
			this.MessageLabel.Top = MARGIN;
		}
	}
}
