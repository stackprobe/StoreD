using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Charlotte.Commons;

namespace Charlotte.Utilities
{
	public class HTTPClient
	{
		private HttpWebRequest Inner;

		public HTTPClient(string url)
		{
			if (!InitOnceDone)
			{
				InitOnce();
				InitOnceDone = true;
			}

			this.Inner = (HttpWebRequest)HttpWebRequest.Create(url);
			this.SetProxyNone();
		}

		private static bool InitOnceDone;

		private static void InitOnce()
		{
			// どんな証明書も許可する。
			ServicePointManager.ServerCertificateValidationCallback =
				(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

			// TLS 1.2
			ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
		}

		/// <summary>
		/// 接続を試みてから応答ヘッダを受信し終えるまでのタイムアウト_ミリ秒
		/// </summary>
		public int ConnectTimeoutMillis = 60000; // 1 min

		/// <summary>
		/// 接続を試みてから全て送受信し終えるまでのタイムアウト_ミリ秒
		/// </summary>
		public int TimeoutMillis = 86400000; // 1 day

		/// <summary>
		/// 応答ヘッダを受信し終えてから全て送受信し終えるまでの間の無通信タイムアウト_ミリ秒
		/// </summary>
		public int IdleTimeoutMillis = 180000; // 3 min

		/// <summary>
		/// 応答ボディ最大サイズ_バイト数
		/// </summary>
		public long ResBodySizeMax = 100000000000000; // 100 TB

		/// <summary>
		/// 応答ボディ出力ファイル
		/// null == 出力しない。
		/// </summary>
		public string ResFile = null;

		/// <summary>
		/// HTTP versions
		/// </summary>
		public enum Version_e
		{
			/// <summary>
			/// HTTP 1.0
			/// </summary>
			V_1_0,

			/// <summary>
			/// HTTP 1.1
			/// </summary>
			V_1_1,
		};

		public void SetVersion(Version_e version)
		{
			switch (version)
			{
				case Version_e.V_1_0:
					this.Inner.ProtocolVersion = HttpVersion.Version10;
					break;

				case Version_e.V_1_1:
					this.Inner.ProtocolVersion = HttpVersion.Version11;
					break;

				default:
					throw null;
			}
		}

		/// <summary>
		/// 100-Continue-を行わないようにする。
		/// </summary>
		public void Disable100Continue()
		{
			this.Inner.ServicePoint.Expect100Continue = false;
		}

		public void SetAuthorization(string user, string password)
		{
			string plain = user + ":" + password;
			string enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(plain));
			this.AddHeader("Authorization", "Basic " + enc);
		}

		public void AddHeader(string name, string value)
		{
			if (SCommon.EqualsIgnoreCase(name, "Accept"))
			{
				this.Inner.Accept = value;
			}
			else if (SCommon.EqualsIgnoreCase(name, "Connection"))
			{
				this.Inner.Connection = value;
			}
			else if (SCommon.EqualsIgnoreCase(name, "Content-Length"))
			{
				throw null; // never
				//this.Inner.ContentLength = long.Parse(value); // 送信時に設定する。
			}
			else if (SCommon.EqualsIgnoreCase(name, "Content-Type"))
			{
				this.Inner.ContentType = value;
			}
			else if (SCommon.EqualsIgnoreCase(name, "Expect"))
			{
				throw null; // never
				//this.Inner.Expect = value; // -> Disable100Continue
			}
			else if (SCommon.EqualsIgnoreCase(name, "Date"))
			{
				this.Inner.Date = SCommon.SimpleDateTime.FromTimeStamp(long.Parse(value)).ToDateTime();
			}
			else if (SCommon.EqualsIgnoreCase(name, "Host"))
			{
				this.Inner.Host = value;
			}
			else if (SCommon.EqualsIgnoreCase(name, "If-Modified-Since"))
			{
				this.Inner.IfModifiedSince = SCommon.SimpleDateTime.FromTimeStamp(long.Parse(value)).ToDateTime();
			}
			else if (SCommon.EqualsIgnoreCase(name, "Range"))
			{
				this.Inner.AddRange(int.Parse(value));
			}
			else if (SCommon.EqualsIgnoreCase(name, "Referer"))
			{
				this.Inner.Referer = value;
			}
			else if (SCommon.EqualsIgnoreCase(name, "Transfer-Encoding"))
			{
				this.Inner.TransferEncoding = value;
			}
			else if (SCommon.EqualsIgnoreCase(name, "User-Agent"))
			{
				this.Inner.UserAgent = value;
			}
			else
			{
				this.Inner.Headers.Add(name, value);
			}
		}

		public void SetProxyNone()
		{
			this.Inner.Proxy = null;
			//this.Inner.Proxy = GlobalProxySelection.GetEmptyWebProxy(); // 古い実装
		}

		public void SetIEProxy()
		{
			this.Inner.Proxy = WebRequest.GetSystemWebProxy();
		}

		public void SetProxy(string host, int port)
		{
			this.Inner.Proxy = new WebProxy(host, port);
		}

		/// <summary>
		/// HEAD-リクエストを実行
		/// </summary>
		public void Head()
		{
			this.GetOrPost(null, "HEAD");
		}

		/// <summary>
		/// GET-リクエストを実行
		/// </summary>
		public void Get()
		{
			this.GetOrPost(null, "GET");
		}

		/// <summary>
		/// POST-リクエストを実行
		/// </summary>
		/// <param name="bodyFile">リクエストボディファイル</param>
		public void Post(string bodyFile)
		{
			this.GetOrPost(bodyFile, "POST");
		}

		private void GetOrPost(string bodyFile, string method)
		{
			ProcMain.WriteLog("HTTPClient-ST");

			DateTime timeoutTime = DateTime.Now + TimeSpan.FromMilliseconds((double)TimeoutMillis);

			// 2022.5.29
			// 送信ファイルをメモリに読み込まない。
			// これをしないと、送信ファイルをメモリに読み込んでから送信しようとする。-> でかいファイルでメモリ不足になる。
			{
				this.Inner.AllowWriteStreamBuffering = false;
			}

			this.Inner.Timeout = this.ConnectTimeoutMillis;
			this.Inner.Method = method;

			if (bodyFile != null)
			{
				ProcMain.WriteLog("HTTPClient-SendBody-ST");

				if (bodyFile == "")
					throw new Exception("Bad bodyFile");

				if (!File.Exists(bodyFile))
					throw new Exception("no bodyFile");

				this.Inner.ContentLength = new FileInfo(bodyFile).Length;

				using (Stream reader = new FileStream(bodyFile, FileMode.Open, FileAccess.Read))
				using (Stream writer = this.Inner.GetRequestStream())
				{
					SCommon.ReadToEnd(reader.Read, writer.Write);
				}

				ProcMain.WriteLog("HTTPClient-SendBody-ED");
			}
			using (WebResponse res = this.Inner.GetResponse())
			{
				this.ResHeaders = SCommon.CreateDictionaryIgnoreCase<string>();

				// 受信ヘッダ
				{
					const int RES_HEADERS_LEN_MAX = 612000;
					const int WEIGHT = 1000;

					int roughResHeaderLength = 0;

					foreach (string name in res.Headers.Keys)
					{
						if (RES_HEADERS_LEN_MAX < name.Length)
							throw new Exception("受信ヘッダが長すぎます。");

						roughResHeaderLength += name.Length + WEIGHT;

						if (RES_HEADERS_LEN_MAX < roughResHeaderLength)
							throw new Exception("受信ヘッダが長すぎます。");

						string value = res.Headers[name];

						if (RES_HEADERS_LEN_MAX < value.Length)
							throw new Exception("受信ヘッダが長すぎます。");

						roughResHeaderLength += value.Length + WEIGHT;

						if (RES_HEADERS_LEN_MAX < roughResHeaderLength)
							throw new Exception("受信ヘッダが長すぎます。");

						this.ResHeaders.Add(name, res.Headers[name]);
					}
				}

				// 受信ボディ
				{
					ProcMain.WriteLog("HTTPClient-RecvResBody-ST");

					long totalSize = 0L;
					long recvCount = 0L;

					FileStream writer =
						this.ResFile == null ?
							null :
							new FileStream(this.ResFile, FileMode.Create, FileAccess.Write);

					try
					{
						using (Stream reader = res.GetResponseStream())
						{
							reader.ReadTimeout = this.IdleTimeoutMillis; // この時間経過すると reader.Read() が例外を投げる。

							byte[] buff = new byte[20000000]; // 20 MB

							for (; ; )
							{
								int readSize = reader.Read(buff, 0, buff.Length);

								if (readSize <= 0)
									break;

								if (timeoutTime < DateTime.Now)
									throw new Exception("受信タイムアウト");

								totalSize += (long)readSize;
								recvCount++;

								if (this.ResBodySizeMax < totalSize)
									throw new Exception("受信データが長すぎます。");

								if (writer != null)
									writer.Write(buff, 0, readSize);
							}
						}
					}
					finally
					{
						if (writer != null)
						{
							writer.Dispose();
							writer = null;
						}
					}

					ProcMain.WriteLog("HTTPClient-RecvResBody-ED " + totalSize + " (" + recvCount + ")");
				}
			}

			ProcMain.WriteLog("HTTPClient-ED");
		}

		public Dictionary<string, string> ResHeaders;
	}
}
