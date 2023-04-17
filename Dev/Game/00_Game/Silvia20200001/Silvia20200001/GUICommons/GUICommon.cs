using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Charlotte.GUICommons
{
	/// <summary>
	/// フォームアプリケーションに関する共通機能・便利機能はできるだけこのクラスに集約する。
	/// </summary>
	public static class GUICommon
	{
		/// <summary>
		/// フォームを表示し終えたら任意でこれを呼ぶ。
		/// </summary>
		/// <param name="f">フォーム</param>
		public static void PostShown(Form f)
		{
			SetDefaultContextMenu(f);
		}

		public static void SetDefaultContextMenu(Form f)
		{
			foreach (Control control in GetAllControl(f))
			{
				if (
					control is TextBox ||
					control is NumericUpDown
					)
				{
					if (control.ContextMenuStrip == null)
					{
						ContextMenuStrip menu = new ContextMenuStrip();
						ToolStripMenuItem item = new ToolStripMenuItem();

						item.Text = "項目なし";
						item.Enabled = false;

						menu.Items.Add(item);
						control.ContextMenuStrip = menu;
					}
				}
			}
		}

		public static IEnumerable<Control> GetAllControl(Form f)
		{
			Queue<Control.ControlCollection> q = new Queue<Control.ControlCollection>();

			q.Enqueue(f.Controls);

			while (1 <= q.Count)
			{
				foreach (Control control in q.Dequeue())
				{
					Panel p = control as Panel;

					if (p != null)
						q.Enqueue(p.Controls);

					GroupBox gb = control as GroupBox;

					if (gb != null)
						q.Enqueue(gb.Controls);

					TabControl tc = control as TabControl;

					if (tc != null)
						foreach (TabPage tp in tc.TabPages)
							q.Enqueue(tp.Controls);

					SplitContainer sc = control as SplitContainer;

					if (sc != null)
					{
						q.Enqueue(sc.Panel1.Controls);
						q.Enqueue(sc.Panel2.Controls);
					}
					yield return control;
				}
			}
		}
	}
}
