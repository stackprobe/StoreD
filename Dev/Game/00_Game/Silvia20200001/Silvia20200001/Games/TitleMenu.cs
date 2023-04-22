using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using Charlotte.Commons;
using Charlotte.Drawings;
using Charlotte.GameCommons;

namespace Charlotte.Games
{
	public static class TitleMenu
	{
		private static string FONT_NAME = "木漏れ日ゴシック";
		private static SoundEffect[] TEST_SOUND_EFFECTS = new SoundEffect[]
		{
			SoundEffects.Save,
			SoundEffects.Load,
			SoundEffects.Buy,
		};

		private static Action DrawWall;

		public static void Run()
		{
			DD.SetCurtain(-1.0);
			DD.SetCurtainTarget(0.0);

			double z = 1.3;

			Musics.RemotestLibrary.Play();

			DrawWall = () =>
			{
				DD.Approach(ref z, 1.0, 0.999);

				DD.SetZoom(z);
				DD.Draw(Pictures.KAZUKIcghvbnkm, new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());
			};

			SimpleMenu menu = new SimpleMenu(40, 40, 40, 440, null, new string[]
			{
				"スタート",
				"コンテニュー",
				"設定",
				"終了",
			});

			for (; ; )
			{
				DD.FreezeInput();

				for (; ; )
				{
					DrawWall();

					if (menu.Draw())
						break;

					DD.EachFrame();
				}
				DD.FreezeInput();

				switch (menu.SelectedIndex)
				{
					case 0:
						StartTheGame();
						break;

					case 1:
						ContinueTheGame();
						break;

					case 2:
						Setting();
						break;

					case 3:
						goto endOfMenu;

					default:
						throw null; // never
				}
			}
		endOfMenu:

			DD.SetCurtainTarget(-1.0);
			Music.Fadeout(30);

			foreach (Scene scene in Scene.Create(40))
			{
				DrawWall();
				DD.EachFrame();
			}
		}

		private static void StartTheGame()
		{
			Game.Run();

			Musics.RemotestLibrary.Play();
		}

		private static void ContinueTheGame()
		{
			Game.Run();

			Musics.RemotestLibrary.Play();
		}

		private static void Setting()
		{
			SimpleMenu menu = new SimpleMenu(30, 40, 30, 540, "設定", new string[]
			{
				"ゲームパッドのボタン設定",
				"キーボードのキー設定",
				"ウィンドウサイズの変更",
				"ＢＭＧ音量",
				"ＳＥ音量",
				"マウスの使用／不使用",
				"戻る",
			});

			for (; ; )
			{
				DD.FreezeInput();

				for (; ; )
				{
					DrawWall();

					if (menu.Draw())
						break;

					DD.EachFrame();
				}
				DD.FreezeInput();

				switch (menu.SelectedIndex)
				{
					case 0:
						CustomizePad();
						break;

					case 1:
						CustomizeKeyboard();
						break;

					case 2:
						ChangeWindowSize();
						break;

					case 3:
						ChangeMusicVolume();
						break;

					case 4:
						ChangeSEVolume();
						break;

					case 5:
						ChangeMouseEnabled();
						break;

					case 6:
						goto endOfMenu;

					default:
						throw null; // never
				}
			}
		endOfMenu:

			DD.Save();
		}

		private static void CustomizePad()
		{
			DD.FreezeInput();

			Input[] inputs = Inputs.GetAllInput();
			int[] dest = new int[inputs.Length];

			for (int index = 0; index < inputs.Length; )
			{
				if (Keyboard.GetInput(DX.KEY_INPUT_SPACE) == 1)
					goto cancelled;

				int button = -1;

				for (int c = 0; c < Pad.BUTTON_MAX; c++)
					if (Pad.PrimaryPad != -1 && Pad.GetInput(Pad.PrimaryPad, c) == 1)
						button = c;

				for (int c = 0; c < index; c++)
					if (dest[c] == button)
						button = -1;

				if (button != -1)
				{
					dest[index] = button;
					index++;
				}

				// ここから描画

				DrawWall();
				DD.DrawCurtain(-0.5);

				DD.SetPrint(24, 20, 34, FONT_NAME, 20);
				DD.PrintLine("ゲームパッドのボタン設定");

				for (int c = 0; c < inputs.Length; c++)
				{
					DD.Print(c == index ? "[>] " : "[ ] ");
					DD.Print(inputs[c].Description);

					if (c < index)
					{
						DD.Print("　>>>　(" + dest[c] + ")");
					}
					DD.PrintRet();
				}

				DD.SetPrint(428, 428, 34, FONT_NAME, 20);
				DD.PrintLine("/// ＴＩＰＳ ///");
				DD.PrintLine("カーソルの指す機能に割り当てるボタンを押して下さい。");
				DD.SetPrintColor(new I3Color(255, 255, 0));
				DD.PrintLine("スペースキーを押すとキャンセルします。");

				DD.EachFrame();
			}

			using (DU.FreeScreen.Section())
			{
				DD.Draw(DD.LastMainScreen.GetPicture(), new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());
			}

			foreach (Scene scene in Scene.Create(30))
			{
				DD.Draw(DU.FreeScreen.GetPicture(), new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());
				DD.EachFrame();
			}

			DU.FreeScreen.Unload();

			for (int index = 0; index < inputs.Length; index++)
			{
				inputs[index].Button = dest[index];
			}

		cancelled:
			DD.FreezeInput();
		}

		private static void CustomizeKeyboard()
		{
			DD.FreezeInput();
			DX.SetMouseDispFlag(1);

			string[] KEY_NAMES = DU.KeyboardKeys.GetNames();

			Input[] inputs = Inputs.GetAllInput();
			int[] dest = new int[inputs.Length];

			for (int index = 0; index < inputs.Length; )
			{
				if (Mouse.R.GetInput() == -1)
					goto cancelled;

				int key = -1;

				for (int c = 0; c < Keyboard.KEY_MAX && key == -1; c++)
					if (Keyboard.GetInput(c) == 1)
						key = c;

				if (key == DX.KEY_INPUT_F11) // F11 --> フルスクリーン切り替えに使用
					key = -1;

				if (key == DX.KEY_INPUT_ESCAPE) // エスケープ --> ゲーム終了に使用
					key = -1;

				for (int c = 0; c < index; c++)
					if (dest[c] == key)
						key = -1;

				if (key != -1)
				{
					dest[index] = key;
					index++;
				}

				// ここから描画

				DrawWall();
				DD.DrawCurtain(-0.5);

				DD.SetPrint(24, 20, 34, FONT_NAME, 20);
				DD.PrintLine("キーボードのキー設定");

				for (int c = 0; c < inputs.Length; c++)
				{
					DD.Print(c == index ? "[>] " : "[ ] ");
					DD.Print(inputs[c].Description);

					if (c < index)
					{
						DD.Print("　>>>　");
						DD.Print(KEY_NAMES[dest[c]]);
					}
					DD.PrintRet();
				}

				DD.SetPrint(428, 428, 34, FONT_NAME, 20);
				DD.PrintLine("/// ＴＩＰＳ ///");
				DD.PrintLine("カーソルの指す機能に割り当てるキーを押して下さい。");
				DD.SetPrintColor(new I3Color(255, 255, 0));
				DD.PrintLine("画面を右クリックするとキャンセルします。");

				DD.EachFrame();
			}

			using (DU.FreeScreen.Section())
			{
				DD.Draw(DD.LastMainScreen.GetPicture(), new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());
			}

			foreach (Scene scene in Scene.Create(30))
			{
				DD.Draw(DU.FreeScreen.GetPicture(), new I2Point(GameConfig.ScreenSize.W / 2, GameConfig.ScreenSize.H / 2).ToD2Point());
				DD.EachFrame();
			}

			DU.FreeScreen.Unload();

			for (int index = 0; index < inputs.Length; index++)
			{
				inputs[index].Key = dest[index];
			}

		cancelled:
			DX.SetMouseDispFlag(GameSetting.MouseCursorShow ? 1 : 0);
			DD.FreezeInput();
		}

		private static void ChangeWindowSize()
		{
			I2Size[] sizes = new I2Size[]
			{
				GameConfig.ScreenSize,
				(GameConfig.ScreenSize.ToD2Size() * 1.1).ToI2Size(),
				(GameConfig.ScreenSize.ToD2Size() * 1.2).ToI2Size(),
				(GameConfig.ScreenSize.ToD2Size() * 1.3).ToI2Size(),
				(GameConfig.ScreenSize.ToD2Size() * 1.4).ToI2Size(),
				(GameConfig.ScreenSize.ToD2Size() * 1.5).ToI2Size(),
				(GameConfig.ScreenSize.ToD2Size() * 1.6).ToI2Size(),
				(GameConfig.ScreenSize.ToD2Size() * 1.7).ToI2Size(),
				(GameConfig.ScreenSize.ToD2Size() * 1.8).ToI2Size(),
			};

			string[] items = sizes.Select(size => size.W + " x " + size.H)
				.Concat(new string[] { "フルスクリーン", "戻る" })
				.ToArray();

			items[0] += " (デフォルト)";

			SimpleMenu menu = new SimpleMenu(24, 40, 16, 420, "ウィンドウサイズの変更", items);

			for (; ; )
			{
				DD.FreezeInput();

				for (; ; )
				{
					DrawWall();

					if (menu.Draw())
						break;

					DD.EachFrame();
				}
				DD.FreezeInput();

				if (menu.SelectedIndex < sizes.Length)
				{
					I2Size size = sizes[menu.SelectedIndex];

					GameSetting.UserScreenSize.W = size.W;
					GameSetting.UserScreenSize.H = size.H;
					GameSetting.FullScreen = false;

					DD.SetRealScreenSize(size.W, size.H);
				}
				else if (menu.SelectedIndex == sizes.Length)
				{
					GameSetting.FullScreen = true;

					DD.SetRealScreenSize(DD.TargetMonitor.W, DD.TargetMonitor.H);
				}
				else if (menu.SelectedIndex == sizes.Length + 1)
				{
					break;
				}
				else
				{
					throw null; // never
				}
			}
		}

		private static void ChangeMusicVolume()
		{
			ChangeVolume(
				"ＢＧＭ音量",
				GameSetting.MusicVolume,
				GameConfig.DefaultMusicVolume,
				value => GameSetting.MusicVolume = value,
				() =>
				{
					// noop
				}
				);
		}

		private static void ChangeSEVolume()
		{
			ChangeVolume(
				"ＳＥ音量",
				GameSetting.SEVolume,
				GameConfig.DefaultSEVolume,
				value => GameSetting.SEVolume = value,
				() =>
				{
					SCommon.CRandom.ChooseOne(TEST_SOUND_EFFECTS).Play();
				}
				);
		}

		private static void ChangeVolume(string title, double volume, double defaultVolume, Action<double> volumeSetter, Action pulse)
		{
			ChangeVolumePercent(
				title,
				SCommon.ToInt(volume * 100.0),
				SCommon.ToInt(defaultVolume * 100.0),
				value => volumeSetter(value / 100.0),
				pulse
				);
		}

		private static void ChangeVolumePercent(string title, int volume, int defaultVolume, Action<int> volumeSetter, Action pulse)
		{
			const int PULSE_FRAME = 60;

			DD.FreezeInput();

			for (; ; )
			{
				if (Inputs.A.GetInput() == 1 || (GameSetting.MouseEnabled && Mouse.L.GetInput() == -1))
				{
					break;
				}
				if (Inputs.B.GetInput() == 1 || (GameSetting.MouseEnabled && Mouse.R.GetInput() == -1))
				{
					if (volume == defaultVolume)
						break;

					volume = defaultVolume;
				}
				if (Inputs.DIR_8.IsPound())
				{
					volume += 3;
				}
				if (Inputs.DIR_2.IsPound())
				{
					volume -= 3;
				}
				if (Inputs.DIR_4.IsPound())
				{
					volume--;
				}
				if (Inputs.DIR_6.IsPound())
				{
					volume++;
				}
				volume += Mouse.Rot;
				volume = SCommon.ToRange(volume, 0, 100);
				volumeSetter(volume);

				if (DD.ProcFrame % PULSE_FRAME == 0)
				{
					pulse();
				}

				// ここから描画

				DrawWall();
				DD.DrawCurtain(-0.5);

				DD.SetPrint(40, 40, 40, FONT_NAME, 30);
				DD.PrintLine(title);
				DD.SetPrint(40, 170, 40, FONT_NAME, 20);
				DD.PrintLine(GetMeterString(volume / 100.0, 80));
				DD.SetPrint(280, 320, 40, FONT_NAME, 20); ;
				DD.PrintLine("/// ＴＩＰＳ ///");
				DD.PrintLine("右・上キーまたはホイール上　⇒　上げる");
				DD.PrintLine("左・下キーまたはホイール下　⇒　下げる");
				DD.PrintLine("調整が終わったら左クリックまたは決定ボタンを押して下さい。");
				DD.PrintLine("右クリックまたはキャンセルボタンを押すと変更をキャンセルします。");

				DD.EachFrame();
			}
			DD.FreezeInput();
		}

		private static string GetMeterString(double volume, int length)
		{
			int indicator = SCommon.ToInt(volume * (length - 1));

			StringBuilder buff = new StringBuilder();

			buff.Append("[");

			for (int index = 0; index < length; index++)
				buff.Append(index == indicator ? "■" : "-");

			buff.Append("] ");
			buff.Append(volume.ToString("F2"));

			return buff.ToString();
		}

		private static void ChangeMouseEnabled()
		{
			SimpleMenu menu = new SimpleMenu(30, 40, 30, 570, "マウスの使用／不使用の切り替え", new string[]
			{
				"マウスを使用する",
				"マウスを使用しない",
				"マウスカーソルを表示しない",
				"戻る",
			});

			DD.FreezeInput();

			for (; ; )
			{
				DrawWall();

				if (menu.Draw())
					break;

				DD.EachFrame();
			}
			DD.FreezeInput();

			switch (menu.SelectedIndex)
			{
				case 0:
					GameSetting.MouseCursorShow = true;
					GameSetting.MouseEnabled = true;
					break;

				case 1:
					GameSetting.MouseCursorShow = true;
					GameSetting.MouseEnabled = false;
					break;

				case 2:
					GameSetting.MouseCursorShow = false;
					GameSetting.MouseEnabled = false;
					break;

				case 3:
					break;

				default:
					throw null; // never
			}

			DX.SetMouseDispFlag(GameSetting.MouseCursorShow ? 1 : 0);
		}
	}
}
