using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Timers;
using System.Collections.Concurrent;
using System.Net;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Coin
{
	using System.Threading.Tasks;

	public partial class FormMain : Form
    {
		delegate void SetTextCallback(Control control, String text);
        delegate void SetOpacityCallback(double value);

		Int32 _viewMode = 0;
		System.Timers.Timer _timer = new System.Timers.Timer();
		System.Timers.Timer _refreshSecTimer = new System.Timers.Timer(1000);
		public static ConcurrentDictionary<String, String> KorPrices = new ConcurrentDictionary<string, string>();

		static readonly String[] _columnNames = { "btcusd", "btc", "eth", "bch", "ltc", "xrp", "iota", "etc", "qtum", "eos", "trx" };
        static String _sourceUrl = "https://api.coinone.co.kr/ticker?currency=all";
        static Int32 _updateInterval = 5 * 60 * 1000;
        const Int32 MIN_INTERVAL_SEC = 30;

	    System.Timers.Timer _exchangeRateTimer = new System.Timers.Timer(60000);
		private static Double _exchangeRateUSDKRW = 0;

	    static readonly String[] _bitfinexCurrencies = { "btc", "eth", "bch", "ltc", "xrp", "iot", "etc", "qtm", "eos", "trx" };
		static readonly ConcurrentDictionary<String, String> _bitfinexPrices = new ConcurrentDictionary<String, String>();
	    static readonly ConcurrentDictionary<String, String> _premiums = new ConcurrentDictionary<String, String>();

		public FormMain()
        {
            InitializeComponent();

            try
            {
                LoadConfig("./res/Config.json");

                _timer.Interval = _updateInterval;
                _timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                _timer.Start();

				_refreshSecTimer.Elapsed += (object sender, ElapsedEventArgs e) =>
				{
					String secStr = Regex.Replace(remainSeconds.Text, @"\D", "");
					Int32 secInt = 0;
					if (Int32.TryParse(secStr, out secInt))
					{
						secInt = Math.Max(0, secInt - 1);
					}
					
					SetText(remainSeconds, secInt + "s");
					if (_viewMode == 1)
					{
						SetText(time, $"{DateTime.Now:HH:mm:ss}");
					}
				};
				_refreshSecTimer.Start();

	            _exchangeRateTimer.Elapsed += (object sender, ElapsedEventArgs e) =>
	            {
		            UpdateExchangeRate();
	            };
	            _refreshSecTimer.Start();

	            UpdateExchangeRate();
				GetPriceBitfinex();
				GetPrice();
	            CalcPremium();
				UpdateServerObjectData();
			}
            catch (Exception ex)
            {
                Program.Log(LogType.Error, ex.ToString());
            }
        }

		public void SetText(Control control, String text)
		{
			try
			{
				if (control.InvokeRequired)
				{
					SetTextCallback callback = new SetTextCallback(SetText);
					this.Invoke(callback, new object[] { control, text });
				}
				else
				{
					control.Text = text;
				}
			}
			catch (Exception ex)
			{
				Program.Log(LogType.Error, ex.ToString());
			}
		}

        void LoadConfig(String fileName)
        {
            if (File.Exists(fileName) == false) return;

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(fs))
                {
                    String res = reader.ReadToEnd();
                    //Console.WriteLine(res);

                    reader.Close();

                    var objJson = JObject.Parse(res);
                    var sourceUrl = objJson["sourceUrl"];
                    var updateInterval = objJson["updateInterval"];
					var devMode = objJson["devmode"];
					var viewMode = objJson["viewMode"];
					if (viewMode != null)
					{
						_viewMode = Int32.Parse(objJson["viewMode"].ToString());
						if (_viewMode == 1)
						{
							columnHeaderKind.Text = "";
						}
						else
						{
							columnHeaderKind.Text = "Currency";
						}
					}
					Console.WriteLine($@"sourceUrl: {sourceUrl}, updateInterval: {updateInterval}");

                    if (sourceUrl != null)
                    {
                        var sourceUrlValue = sourceUrl.ToString();
                        if (sourceUrl.ToString().Length > 0) _sourceUrl = sourceUrlValue;
                    }
                    if (updateInterval != null)
                    {
                        var updateIntervalValue = Int32.Parse(updateInterval.ToString());
						if (devMode != null)
						{
							_updateInterval = Math.Max(updateIntervalValue, 3) * 1000;
						}
						else
						{
							_updateInterval = Math.Max(updateIntervalValue, MIN_INTERVAL_SEC) * 1000;
						}
                    }
                }

                fs.Close();
            }
        }

        void UpdateServerObjectData()
        {
            if (listView.InvokeRequired == true)
            {
                listView.Invoke(new MethodInvoker(delegate
                {
                    listView.Items.Clear();
                }));
            }
            else
            {
                listView.Items.Clear();
            }

            foreach (var filter in _columnNames)
            {
                var key = filter.ToUpper();

                var value = KorPrices[key];
                if (value == null) continue;

                ListViewItem lvi = new ListViewItem(key);

				String price = value;
	            if (_premiums.ContainsKey(key))
	            {
		            price += $"({_premiums[key]})";
	            }

				lvi.SubItems.Add(price);

				if (listView.InvokeRequired == true)
                {
                    listView.Invoke(new MethodInvoker(delegate
                    {
                        listView.Items.Add(lvi);
                    }));
                }
                else
                {
                    listView.Items.Add(lvi);
                }
            }

			SetText(remainSeconds, (Int32)(_updateInterval / 1000) + "s");
		}
		
        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var formMain = (FormMain)Application.OpenForms["FormMain"];
                KorPrices.Clear();

	            GetPriceBitfinex();
				GetPrice();
				CalcPremium();
                formMain?.UpdateServerObjectData();
            }
            catch (Exception ex)
            {
                Program.Log(LogType.Error, ex.ToString());
            }
        }

        static void GetPrice()
        {
	        Parallel.ForEach(new List<Action> { UpdateExchangeRate, GetPriceElse, GetPriceTron, GetPriceBtcUsd },
	                         (getPrice) => { getPrice(); });
        }

	    static void CalcPremium()
	    {
			foreach (var bitfinexCurrency in _bitfinexCurrencies)
		    {
			    String korCurrency = bitfinexCurrency;
			    if (String.CompareOrdinal(korCurrency, "iot") == 0)
			    {
				    korCurrency = "iota";
			    }
			    else if (String.CompareOrdinal(korCurrency, "qtm") == 0)
			    {
				    korCurrency = "qtum";
			    }

			    korCurrency = korCurrency.ToUpper();
				
				if (KorPrices.ContainsKey(korCurrency) == false)
			    {
				    continue;
			    }

				try
			    {
				    Double korPrice = Double.Parse(KorPrices[korCurrency]);
				    Double bitfinexPrice = Double.Parse(_bitfinexPrices[bitfinexCurrency]);
				    Double overseasPrice = (Int64)(bitfinexPrice * _exchangeRateUSDKRW);

				    Double premium = 0;
				    String sign = "";

				    // 김프.
				    if (overseasPrice < korPrice)
				    {
					    premium = ((korPrice / overseasPrice) - 1) * 100;
					    sign = "+";
				    }
				    // 역프.
				    else if (overseasPrice > korPrice)
				    {
					    premium = ((overseasPrice / korPrice) - 1) * 100;
					    sign = "-";
				    }

				    _premiums.TryAdd(korCurrency, $"{sign}{Math.Round(premium, 2)}%");
				}
			    catch (Exception e)
			    {
				    _premiums.TryAdd(korCurrency, "-%");
				    Console.WriteLine(e);
				}
		    }
	    }

	    static void UpdateExchangeRate()
	    {
			try
			{
				WebRequest request = WebRequest.Create("http://earthquake.kr/exchange/");
				request.Credentials = CredentialCache.DefaultCredentials;

				WebResponse response = request.GetResponse();
				//Console.WriteLine(((HttpWebResponse)response).StatusDescription);

				Stream dataStream = response.GetResponseStream();
				StreamReader reader = new StreamReader(dataStream);
				String res = reader.ReadToEnd();
				//Console.WriteLine(res);

				reader.Close();
				response.Close();

				var objJson = JObject.Parse(res);
				foreach (var objData in objJson)
				{
					if (String.CompareOrdinal("USDKRW", objData.Key) != 0)
					{
						continue;
					}

					_exchangeRateUSDKRW = Double.Parse(objData.Value[0].ToString());
				}
			}
			catch (Exception ex)
			{
				Program.Log(LogType.Error, ex.ToString());
			}
		}

		static void GetPriceElse()
		{
			try
			{
				WebRequest request = WebRequest.Create(_sourceUrl);
				request.Credentials = CredentialCache.DefaultCredentials;

				WebResponse response = request.GetResponse();
				//Console.WriteLine(((HttpWebResponse)response).StatusDescription);

				Stream dataStream = response.GetResponseStream();
				StreamReader reader = new StreamReader(dataStream);
				String res = reader.ReadToEnd();
				//Console.WriteLine(res);

				reader.Close();
				response.Close();

				var objJson = JObject.Parse(res);
				foreach (var objData in objJson)
				{
					if (Array.Exists(_columnNames, element => element == objData.Key) == false) continue;

					var item = objData.Value;
					var currency = item["currency"].ToString();
					var last = item["last"].ToString();

					var price = Int64.Parse(last).ToString("N0");
					Console.WriteLine($@"currency: {currency}, price: {price}");

					KorPrices.TryAdd(currency.ToUpper(), price);
				}
			}
			catch (Exception ex)
			{
				Program.Log(LogType.Error, ex.ToString());
			}
		}

	    // 트론 조회 전용.
		static void GetPriceTron()
		{
			const String currency = "TRX";

			try
			{
				WebRequest request = WebRequest.Create("https://api.bithumb.com/public/ticker/trx");
				request.Credentials = CredentialCache.DefaultCredentials;

				WebResponse response = request.GetResponse();
				//Console.WriteLine(((HttpWebResponse)response).StatusDescription);

				Stream dataStream = response.GetResponseStream();
				StreamReader reader = new StreamReader(dataStream);
				String res = reader.ReadToEnd();
				//Console.WriteLine(res);

				reader.Close();
				response.Close();

				var objJson = JObject.Parse(res);
				foreach (var objData in objJson)
				{
					if (String.Compare(objData.Key, "data") != 0)
					{
						continue;
					}

					var last = objData.Value["buy_price"].ToString();
					var price = Int64.Parse(last).ToString("N0");
					Console.WriteLine($"currency: {currency}, price: {price}");

					KorPrices.TryAdd(currency, price);
				}
			}
			catch (Exception ex)
			{
				KorPrices.TryAdd(currency, "-");
				Program.Log(LogType.Error, ex.ToString());
			}
		}

	    static void GetPriceBitfinex()
	    {
		    Parallel.ForEach(_bitfinexCurrencies, (currency) =>
		    {
				try
				{
					Int32 sleepMs = Array.FindIndex(_bitfinexCurrencies, (c) => String.CompareOrdinal(c, currency) == 0);
					Thread.Sleep(sleepMs * 50);

					// Console.WriteLine($"t{currency.ToUpper()}USD");

					WebRequest request = WebRequest.Create($"https://api.bitfinex.com/v2/ticker/t{currency.ToUpper()}USD");
					request.Credentials = CredentialCache.DefaultCredentials;

					WebResponse response = request.GetResponse();
					//Console.WriteLine(((HttpWebResponse)response).StatusDescription);

					Stream dataStream = response.GetResponseStream();
					StreamReader reader = new StreamReader(dataStream);
					String res = reader.ReadToEnd();

					// Console.WriteLine(res);

					reader.Close();
					response.Close();

					var last = res.Replace("[", "").Replace("]", "").Split(',')[0];
					var price = Double.Parse(last).ToString();
					Console.WriteLine($@"bitfinex currency: {currency}, price: {price}");

					_bitfinexPrices.AddOrUpdate(currency, price, (key, oldValue) => price);
				}
				catch (Exception ex)
				{
					Program.Log(LogType.Error, ex.ToString());
				}
			});
	    }

		// 해외 비트코인 조회 전용.
		static void GetPriceBtcUsd()
		{
			const String currency = "BTCUSD";

			try
			{
				if (_bitfinexPrices.ContainsKey("btc"))
				{
					KorPrices.TryAdd(currency, Double.Parse(_bitfinexPrices["btc"]).ToString("N1"));
					return;
				}

				WebRequest request = WebRequest.Create("https://api.bitfinex.com/v2/ticker/tBTCUSD");
				request.Credentials = CredentialCache.DefaultCredentials;

				WebResponse response = request.GetResponse();
				//Console.WriteLine(((HttpWebResponse)response).StatusDescription);

				Stream dataStream = response.GetResponseStream();
				StreamReader reader = new StreamReader(dataStream);
				String res = reader.ReadToEnd();
				//Console.WriteLine(res);

				reader.Close();
				response.Close();

				var last = res.Replace("[", "").Replace("]", "").Split(',')[0];
				var price = Double.Parse(last).ToString("N1");
				Console.WriteLine($@"currency: {currency}, price: {price}");

				KorPrices.TryAdd(currency, price);
			}
			catch (Exception ex)
			{
				KorPrices.TryAdd(currency, "-");
				Program.Log(LogType.Error, ex.ToString());
			}
		}

		public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void FormMain_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        ReleaseCapture();
                        SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                        break;
                }
            }
            catch (Exception ex)
            {
                Program.Log(LogType.Error, ex.ToString());
            }
        }

        private void FormMain_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                switch (e.Button)
                {
                    case MouseButtons.Right:
                        if (MessageBox.Show("Exit to board. Right?", "Exit", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            Close();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Program.Log(LogType.Error, ex.ToString());
            }
        }

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            const double MaxValue = 1.0f;
            const double MinValue = 0.1f;
            const double changeValue = 0.05f;

            Console.WriteLine($"KeyCode: {e.KeyCode.ToString()}, KeyValue: {e.KeyValue.ToString()}");

            switch (e.KeyCode)
            {
                case Keys.Add:
                case Keys.Oemplus:
                    SetOpacity(Math.Min(MaxValue, Opacity + changeValue));
                    break;

                case Keys.Subtract:
                case Keys.OemMinus:
                    SetOpacity(Math.Max(MinValue, Opacity - changeValue));
                    break;
            }
        }

        public void SetOpacity(double value)
        {
            try
            {
                if (InvokeRequired)
                {
                    SetOpacityCallback callback = new SetOpacityCallback(SetOpacity);
                    this.Invoke(callback, new object[] { value });
                }
                else
                {
                    Opacity = value;
                }
            }
            catch (Exception ex)
            {
                Program.Log(LogType.Error, ex.ToString());
            }
        }
    }
}
