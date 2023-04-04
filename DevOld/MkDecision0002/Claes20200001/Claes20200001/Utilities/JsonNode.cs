using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Charlotte.Commons;

namespace Charlotte.Utilities
{
	/// <summary>
	/// JSON-ツリーのノード
	/// 読み込み時 (LoadFromFile, Load) は Array, Map, StringValue, WordValue のどれか一つを not null にセットする。
	/// 書き出し時 (WriteToFile, GetString) は Array, Map, StringValue, WordValue の順で見て最初に not null であった値が書き出される。
	/// </summary>
	public class JsonNode
	{
		/// <summary>
		/// マップ用要素
		/// </summary>
		public class Pair
		{
			/// <summary>
			/// 名前
			/// null-不可
			/// 空白系文字 OK
			/// 改行 OK
			/// </summary>
			public string Name;

			/// <summary>
			/// 値
			/// null-不可
			/// 空白系文字 OK
			/// 改行 OK
			/// </summary>
			public JsonNode Value;
		}

		/// <summary>
		/// 配列
		/// null-要素不可
		/// </summary>
		public List<JsonNode> Array;

		/// <summary>
		/// マップ
		/// null-要素不可
		/// </summary>
		public List<Pair> Map;

		/// <summary>
		/// 文字列
		/// 空白系文字 OK
		/// 改行 OK
		/// </summary>
		public string StringValue;

		/// <summary>
		/// 単語
		/// 空白系文字 NG
		/// 改行 NG
		/// </summary>
		public string WordValue;

		public static JsonNode LoadFromFile(string file)
		{
			return LoadFromFile(file, GetFileEncoding(file));
		}

		public static JsonNode LoadFromFile(string file, Encoding encoding)
		{
			return Load(File.ReadAllText(file, encoding));
		}

		public static JsonNode Load(string text)
		{
			return new Reader(text).GetNode();
		}

		private class Reader
		{
			private string Text;
			private int Index = 0;

			public Reader(string text)
			{
				this.Text = text;
			}

			private char Next()
			{
				return this.Text[this.Index++];
			}

			private char NextNS()
			{
				char chr;

				do
				{
					chr = this.Next();
				}
				while (chr <= ' ');

				return chr;
			}

			public JsonNode GetNode()
			{
				char chr = this.NextNS();
				JsonNode node = new JsonNode();

				if (chr == '[') // ? Array
				{
					node.Array = new List<JsonNode>();

					if ((chr = this.NextNS()) != ']')
					{
						for (; ; )
						{
							this.Index--;
							node.Array.Add(this.GetNode());
							chr = this.NextNS();

							if (chr == ']')
								break;

							if (chr != ',')
								throw new Exception("JSON format error: Array ','");

							chr = this.NextNS();

							if (chr == ']')
							{
								ProcMain.WriteLog("JSON format warning: ',' before ']'");
								break;
							}
						}
					}
				}
				else if (chr == '{') // ? Map
				{
					node.Map = new List<Pair>();

					if ((chr = this.NextNS()) != '}')
					{
						for (; ; )
						{
							this.Index--;
							JsonNode nameNode = this.GetNode();
							string name = nameNode.StringValue ?? nameNode.WordValue;

							if (name == null)
								throw new Exception("JSON format error: Map name");

							if (this.NextNS() != ':')
								throw new Exception("JSON format error: Map ':'");

							node.Map.Add(new Pair()
							{
								Name = name,
								Value = this.GetNode(),
							});

							chr = this.NextNS();

							if (chr == '}')
								break;

							if (chr != ',')
								throw new Exception("JSON format error: Map ','");

							chr = this.NextNS();

							if (chr == '}')
							{
								ProcMain.WriteLog("JSON format warning: ',' before '}'");
								break;
							}
						}
					}
				}
				else if (chr == '"' || chr == '\'') // ? String-Value
				{
					StringBuilder buff = new StringBuilder();
					char chrEnclStr = chr;

					if (chrEnclStr == '\'')
						ProcMain.WriteLog("JSON format warning: String enclosed in single quotes");

					for (; ; )
					{
						chr = this.Next();

						if (chr == chrEnclStr)
							break;

						if (chr == '\\')
						{
							chr = this.Next();

							if (chr == 'b')
							{
								chr = '\b';
							}
							else if (chr == 'f')
							{
								chr = '\f';
							}
							else if (chr == 'n')
							{
								chr = '\n';
							}
							else if (chr == 'r')
							{
								chr = '\r';
							}
							else if (chr == 't')
							{
								chr = '\t';
							}
							else if (chr == 'u')
							{
								char c1 = this.Next();
								char c2 = this.Next();
								char c3 = this.Next();
								char c4 = this.Next();

								chr = (char)Convert.ToInt32(new string(new char[] { c1, c2, c3, c4 }), 16);
							}
						}
						buff.Append(chr);
					}
					node.StringValue = buff.ToString();
				}
				else // ? Word-Value
				{
					StringBuilder buff = new StringBuilder();

					this.Index--;

					while (this.Index < this.Text.Length)
					{
						chr = this.Next();

						if (
							chr == '}' ||
							chr == ']' ||
							chr == ',' ||
							chr == ':'
							)
						{
							this.Index--;
							break;
						}
						buff.Append(chr);
					}
					node.WordValue = buff.ToString().Trim();
				}
				return node;
			}
		}

		private static Encoding GetFileEncoding(string file)
		{
			using (FileStream reader = new FileStream(file, FileMode.Open, FileAccess.Read))
			{
				byte[] buff = new byte[4];
				int readSize = reader.Read(buff, 0, 4);

				// ? UTF-32 BE
				if (
					4 <= readSize &&
					buff[0] == 0x00 &&
					buff[1] == 0x00 &&
					buff[2] == 0xfe &&
					buff[3] == 0xff
					)
					return Encoding.UTF32;

				// ? UTF-32 LE
				if (
					4 <= readSize &&
					buff[0] == 0xff &&
					buff[1] == 0xfe &&
					buff[2] == 0x00 &&
					buff[3] == 0x00
					)
					return Encoding.UTF32;

				// ? UTF-16 BE
				if (
					2 <= readSize &&
					buff[0] == 0xfe &&
					buff[1] == 0xff
					)
					return Encoding.Unicode;

				// ? UTF-16 LE
				if (
					2 <= readSize &&
					buff[0] == 0xff &&
					buff[1] == 0xfe
					)
					return Encoding.Unicode;

				return Encoding.UTF8;
			}
		}

		public void WriteToFile(string file)
		{
			this.WriteToFile(file, Encoding.UTF8);
		}

		public void WriteToFile(string file, Encoding encoding, bool shortJsonMode = false)
		{
			File.WriteAllText(file, this.GetString(shortJsonMode), encoding);
		}

		public string GetString(bool shortJsonMode)
		{
			StringBuilder buff = new StringBuilder();
			new Writer(buff) { ShortJsonMode = shortJsonMode }.WriteRoot(this);
			return buff.ToString();
		}

		private class Writer
		{
			public bool ShortJsonMode;

			private StringBuilder Buff;
			private int Depth = 0;

			public Writer(StringBuilder buff)
			{
				this.Buff = buff;
			}

			public void WriteRoot(JsonNode node)
			{
				this.Write(node);
				this.WriteNewLine();
			}

			public void Write(JsonNode node)
			{
				if (node.Array != null) // ? Array
				{
					this.Write('[');
					this.WriteNewLine();
					this.Depth++;

					for (int index = 0; index < node.Array.Count; index++)
					{
						this.WriteIndent();
						this.Write(node.Array[index]);

						if (index < node.Array.Count - 1)
							this.Write(',');

						this.WriteNewLine();
					}
					this.Depth--;
					this.WriteIndent();
					this.Write(']');
				}
				else if (node.Map != null) // ? Map
				{
					this.Write('{');
					this.WriteNewLine();
					this.Depth++;

					for (int index = 0; index < node.Map.Count; index++)
					{
						this.WriteIndent();
						this.Write(new JsonNode() { StringValue = node.Map[index].Name });
						this.Write(':');
						this.WriteSpace();
						this.Write(node.Map[index].Value);

						if (index < node.Map.Count - 1)
							this.Write(',');

						this.WriteNewLine();
					}
					this.Depth--;
					this.WriteIndent();
					this.Write('}');
				}
				else if (node.StringValue != null) // ? String-Value
				{
					this.Write('"');

					foreach (char chr in node.StringValue)
					{
						if (chr == '"')
						{
							this.Write("\\\"");
						}
						else if (chr == '\\')
						{
							this.Write("\\\\");
						}
						else if (chr == '\b')
						{
							this.Write("\\b");
						}
						else if (chr == '\f')
						{
							this.Write("\\f");
						}
						else if (chr == '\n')
						{
							this.Write("\\n");
						}
						else if (chr == '\r')
						{
							this.Write("\\r");
						}
						else if (chr == '\t')
						{
							this.Write("\\t");
						}
						else
						{
							this.Write(chr);
						}
					}
					this.Write('"');
				}
				else if (node.WordValue != null) // ? Word-Value
				{
					this.Write(node.WordValue);
				}
				else
				{
					throw new Exception("JSON node error");
				}
			}

			private void WriteIndent()
			{
				if (!this.ShortJsonMode)
					for (int index = 0; index < this.Depth; index++)
						this.Write('\t');
			}

			private void WriteNewLine()
			{
				if (!this.ShortJsonMode)
					this.Write("\r\n");
			}

			private void WriteSpace()
			{
				if (!this.ShortJsonMode)
					this.Write(' ');
			}

			private void Write(string str)
			{
				this.Buff.Append(str);
			}

			private void Write(char chr)
			{
				this.Buff.Append(chr);
			}
		}
	}
}
