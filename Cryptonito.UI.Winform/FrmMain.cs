using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cryptonito.Connectivity.Cryptsy.Enums;
using Cryptonito.Connectivity.Cryptsy.Entities;
using System.Text.RegularExpressions;
using Cryptonito.Connectivity.Cryptsy;
using Cryptonito.Services.Arb;
using System.Threading;
namespace Cryptonito.UI.Winform
{
    public partial class FrmMain : Form
    {

        private static string DRK_BTC_MARKET_ID = "155";
        private static string DRK_LTC_MARKET_ID = "214";
        private static string BTC_LTC_MARKET_ID = "3";

        private CryptsyPusherService _pusher1Client;
        private CryptsyPusherService _pusher2Client;
        private CryptsyPusherService _pusher3Client;

        private decimal _drkToLtc = 0m;
        private decimal _ltcToBtc = 0m;
        private decimal _btcToDrk = 0m;

        private CancellationTokenSource _tokenSourceBalanceThread;
        private CryptsyService _service;

        public FrmMain()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            System.Net.ServicePointManager.Expect100Continue = false;
            _tokenSourceBalanceThread = new System.Threading.CancellationTokenSource();
            _service = new CryptsyService();
        }

        public void NotifyMarketOrdersReceived(string marketId, List<MarketOrder> myOrders)
        {
            if (myOrders != null)
            {
                if (marketId.Equals(DRK_BTC_MARKET_ID))
                {
                    this.dataGridViewLtc.Invoke(new Action(() => this.dataGridViewLtc.DataSource = myOrders));
                }
                else if (marketId.Equals(DRK_LTC_MARKET_ID))
                {
                    this.dataGridViewDrkOrder.Invoke(new Action(() => this.dataGridViewDrkOrder.DataSource = myOrders));
                }
                else
                {
                    this.dataGridViewBtc.Invoke(new Action(() => this.dataGridViewBtc.DataSource = myOrders));
                }
            }
        }

        public void NotifyArbEvent(bool arbExist, decimal totalReturn, decimal drkToLtcRate, decimal ltcToBtcRate, decimal btcToDrkRate)
        {
            this.txtBoxArbExists.Invoke(new Action(() => txtBoxArbExists.Text = arbExist.ToString().ToUpper()));
            this.txtBoxExpectedReturn.Invoke(new Action(() => txtBoxExpectedReturn.Text = string.Format("{0:0.00000}%", 100M * Decimal.Parse(totalReturn.ToString()))));
            this.btnStartArbExecution.Invoke(new Action(() => btnStartArbExecution.Enabled = arbExist));
            if (!arbExist)
            {
                StopTopOrderThread();
            }
            else
            {
                this.Invoke(new Action(() => Blink()));
            }
        }

        public void UpdateMyInfo(Info info)
        {
            if (info != null)
            {
                string avaBal = info.BalancesAvailable.ToString();
                string[] balances = avaBal.Split(',');
                avaBal = Regex.Replace(avaBal, "[^0-9a-zA-Z]+", "");

                foreach (string bal in balances)
                {
                    string[] keys = bal.Split(':');
                    string newKey = Regex.Replace(keys[0], "[^0-9a-zA-Z]+", "");
                    string newBal = keys[1].Trim().Replace(@"\", "").Replace(@"/", "").Replace(@"""", "");
                    if (newKey == "DRK")
                    {
                        this.txtBoxAvalBalDrk.Invoke(new Action(() => this.txtBoxAvalBalDrk.Text = newBal));
                    }
                    else if (newKey == "BTC")
                    {
                        this.txtBoxAvalBalBTC.Invoke(new Action(() => this.txtBoxAvalBalBTC.Text = newBal));
                    }
                    else if (newKey == "LTC")
                    {
                        this.txtBoxAvalBalLTC.Invoke(new Action(() => this.txtBoxAvalBalLTC.Text = newBal));
                    }
                }

                if (info.BalancesHold == null)
                {
                    this.txtBoxBalHoldingDRK.Invoke(new Action(() => this.txtBoxBalHoldingDRK.Text = ""));
                    this.txtBoxBalHoldingBTC.Invoke(new Action(() => this.txtBoxBalHoldingBTC.Text = ""));
                    this.txtBoxBalHoldingLTC.Invoke(new Action(() => this.txtBoxBalHoldingLTC.Text = ""));
                    return;
                }

                avaBal = info.BalancesHold.ToString();
                balances = avaBal.Split(',');
                avaBal = Regex.Replace(avaBal, "[^0-9a-zA-Z]+", "");

                foreach (string bal in balances)
                {
                    string[] keys = bal.Split(':');
                    string newKey = Regex.Replace(keys[0], "[^0-9a-zA-Z]+", "");
                    string newBal = keys[1].Trim().Replace(@"\", "").Replace(@"/", "").Replace(@"""", "").Replace(@"}", "");

                    if (newKey == "DRK")
                    {
                        this.txtBoxBalHoldingDRK.Invoke(new Action(() => this.txtBoxBalHoldingDRK.Text = newBal));
                    }
                    else if (newKey == "BTC")
                    {
                        this.txtBoxBalHoldingBTC.Invoke(new Action(() => this.txtBoxBalHoldingBTC.Text = newBal));
                    }
                    else if (newKey == "LTC")
                    {
                        this.txtBoxBalHoldingLTC.Invoke(new Action(() => this.txtBoxBalHoldingLTC.Text = newBal));
                    }
                }
            }
        }

        private void btnExecuteMarketMonitor_Click(object sender, EventArgs e)
        {
            if (btnExecuteMarketMonitor.Text.Equals("Start Arb Monitor"))
            {
                StartMarketDataReceivers();
            }
            else
            {
                AbortMarketDataReceivers();
            }
        }

        private async void StartMarketDataReceivers()
        {
            _service.GetSingleMarket("214").ContinueWith(m => 
                {
                    this.txtBoxPusher1Sell.Text = m.Result.LowestSellPrice.ToString();
                    this.txtBoxPusher1Buy.Text = m.Result.HighestBuyPrice.ToString();
                }, TaskScheduler.FromCurrentSynchronizationContext());

            _service.GetSingleMarket("3").ContinueWith(m =>
            {
                this.txtBoxPusher2Sell.Text = m.Result.LowestSellPrice.ToString();
                this.txtBoxPusher2Buy.Text = m.Result.HighestBuyPrice.ToString();
            }, TaskScheduler.FromCurrentSynchronizationContext());

            _service.GetSingleMarket("155").ContinueWith(m =>
            {
                this.txtBoxPusher3Sell.Text = m.Result.LowestSellPrice.ToString();
                this.txtBoxPusher3Buy.Text = m.Result.HighestBuyPrice.ToString();
            }, TaskScheduler.FromCurrentSynchronizationContext());

            btnExecuteMarketMonitor.Text = "Running... Press to Stop";

            _pusher1Client = new CryptsyPusherService("ticker.214", ProcessPusherMessage, new Action<String>(s => ProcessPusherStatus(1, s)));
            _pusher2Client = new CryptsyPusherService("ticker.155", ProcessPusherMessage, new Action<String>(s => ProcessPusherStatus(2, s)));
            _pusher3Client = new CryptsyPusherService("ticker.3", ProcessPusherMessage, new Action<String>(s => ProcessPusherStatus(3, s)));
            
            _pusher1Client.Connect();
            _pusher2Client.Connect();
            _pusher3Client.Connect();
        }

        private void ProcessPusherMessage(PusherMessage msg)
        {
            switch(msg.Trade.MarketId)
            {
                case 214:
                    this._drkToLtc = msg.Trade.TopSell.Price;
                    this.txtBoxPusher1Sell.Invoke(new Action(() => this.txtBoxPusher1Sell.Text = msg.Trade.TopSell.Price.ToString()));
                    this.txtBoxPusher1Buy.Invoke(new Action(() => this.txtBoxPusher1Buy.Text = msg.Trade.TopBuy.Price.ToString()));
                    break;
                case 3:
                    this._ltcToBtc = msg.Trade.TopBuy.Price;
                    this.txtBoxPusher2Sell.Invoke(new Action(() => this.txtBoxPusher2Sell.Text = msg.Trade.TopSell.Price.ToString()));
                    this.txtBoxPusher2Buy.Invoke(new Action(() => this.txtBoxPusher2Buy.Text = msg.Trade.TopBuy.Price.ToString()));

                    break;
                case 155:
                    this._btcToDrk = msg.Trade.TopBuy.Price;
                    this.txtBoxPusher3Sell.Invoke(new Action(() => this.txtBoxPusher3Sell.Text = msg.Trade.TopSell.Price.ToString()));
                    this.txtBoxPusher3Buy.Invoke(new Action(() => this.txtBoxPusher3Buy.Text = msg.Trade.TopBuy.Price.ToString()));
                    break;
                default :
                    break;
            }

            decimal expectedReturn = ArbFormula.CalculateExpectedReturn(_drkToLtc, _ltcToBtc, _btcToDrk);
            this.txtBoxExpectedReturn.Invoke(new Action(() => this.txtBoxExpectedReturn.Text = string.Format("{0:0.00000}%", 100M * Decimal.Parse(expectedReturn.ToString()))));
        }

        private void ProcessPusherStatus(int pusherId, string status)
        {
            switch (pusherId)
            { 
                case 1:
                    labelPusher1Status.Invoke(new Action(() => labelPusher1Status.Text = status));
                    break;
                case 2:
                    labelPusher2Status.Invoke(new Action(() => labelPusher2Status.Text = status));
                    break;
                case 3:
                    labelPusher3Status.Invoke(new Action(() => labelPusher3Status.Text = status));
                    break;

                default:
                    break;
            }
        }

        private void AbortMarketDataReceivers()
        {
            btnExecuteMarketMonitor.Text = "Start Arb Monitor";
            this.btnStartArbExecution.Invoke(new Action(() => btnStartArbExecution.Enabled = false));
            _pusher1Client.Disconnect();
            _pusher2Client.Disconnect();
            _pusher3Client.Disconnect();
        }

        private void btnRefreshBalance_Click(object sender, EventArgs e)
        {
            if (btnRefreshBalance.Text.Equals("Refresh Balance"))
            {
                StartBalanceThread();
                btnRefreshBalance.Text = "Running.. Press to Stop";
            }
            else
            {
                StopBalanceThread();
                btnRefreshBalance.Text = "Refresh Balance";
            }
        }

        private void StartBalanceThread()
        {
           
            Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        Info i = await _service.GetInfo();
                        UpdateMyInfo(i);
                        await Task.Delay(500, _tokenSourceBalanceThread.Token);
                    }
                }, _tokenSourceBalanceThread.Token);
            //service.GetInfo().ContinueWith(i =>
            //{
            //    UpdateMyInfo(i.Result);
            //}, new System.Threading.CancellationToken(), TaskContinuationOptions.Lon
            
            //my_info_thread = new Thread(new ThreadStart(myInfoReceiver.MonitorMyInfo));
            //my_info_thread.Start();
        }

        private void StopBalanceThread()
        {
            _tokenSourceBalanceThread.Cancel();
            //my_info_thread.Suspend();
        }

        private void btnRefreshOrder_Click(object sender, EventArgs e)
        {
            if (btnRefreshOrder.Text.Equals("Refresh Order"))
            {
                StartRefreshOrderThreads();
                btnRefreshOrder.Text = "Running.. Press to Stop";
            }
            else
            {
                StopRefreshOrderThreads();
                btnRefreshOrder.Text = "Refresh Order";
            }
        }

        private void StopRefreshOrderThreads()
        {
            //my_drk_market_order_thread.Join(100);
            //my_ltc_market_order_thread.Join(100);
            //my_btc_market_order_thread.Join(100);
        }
        private void StartRefreshOrderThreads()
        {
            //my_drk_market_order_thread = new Thread(new ThreadStart(drk_order_receiver.MonitorMyOrder));
            //my_ltc_market_order_thread = new Thread(new ThreadStart(ltc_order_receiver.MonitorMyOrder));
            //my_btc_market_order_thread = new Thread(new ThreadStart(btc_order_receiver.MonitorMyOrder));
            //my_drk_market_order_thread.Start();
            //my_ltc_market_order_thread.Start();
            //my_btc_market_order_thread.Start();
        }

        public void CancelAllOrders(List<string> cancelStatus)
        {

            if (cancelStatus == null || cancelStatus.Count == 0)
            {
                MessageBox.Show("No Orders to Cancel");
            }
            else
            {
                MessageBox.Show(string.Format("[{0}] Order(s) Terminated", cancelStatus.Count));
            }
            StopCancelAllMyOrdersThread();
        }

        private void btnCancelAllOrders_Click(object sender, EventArgs e)
        {
            if (btnCancelAllOrders.Text.Equals("Cancel All Orders"))
            {
                StartCancelAllMyOrdersThread();
            }
            else
            {
                StopCancelAllMyOrdersThread();
            }
        }

        private void StartCancelAllMyOrdersThread()
        {
            //cancel_order_thread = new Thread(new ThreadStart(myInfoReceiver.CancelMyOrders));
            //cancel_order_thread.Start();
            btnCancelAllOrders.Text = "Running... Press to Stop";
        }

        private void StopCancelAllMyOrdersThread()
        {
            //try
            //{
            //    cancel_order_thread.Join(100);
            //}
            //catch (Exception ex)
            //{
            //}
            //finally
            //{
            //    btnCancelAllOrders.Invoke(new Action(() => this.btnCancelAllOrders.Text = "Cancel All Orders"));
            //}
        }

        public void NotifyMarketOrderCreated(OrderResponse response, string marketId, OrderType orderType, decimal quantity, decimal orderedPrice)
        {

        }

        private void btnStartArbExecution_Click(object sender, EventArgs e)
        {
            if (this.btnStartArbExecution.Text.Equals("EXECUTE ARB"))
            {
                StartTopOrderThread();
            }
            else
            {
                StopTopOrderThread();
            }
        }

        private void StopTopOrderThread()
        {
            //try
            //{
            //    if (top_order_thread.IsAlive)
            //    {
            //        StartCancelAllMyOrdersThread();
            //    }
            //    top_order_thread.Join(100);
            //    if (top_order_thread != null)
            //    {
            //        top_order_thread.Abort();
            //        top_order_thread = null;
            //    }

            //}
            //catch (Exception ex)
            //{
            //}
            //finally 
            //{
            //    this.btnStartArbExecution.Invoke(new Action(() => this.btnStartArbExecution.Text = "EXECUTE ARB"));
            //}
        }

        private void StartTopOrderThread()
        {
            //decimal manualPrice = 0M;
            //decimal.TryParse(this.txtBoxOverridePrice.Text, out manualPrice);
            //decimal orderQty = 0M;
            //if (!decimal.TryParse(this.txtBoxArbOrderQty.Text, out orderQty)) return;
            //ltc_order_receiver.OrderType = OrderType.Sell;
            //ltc_order_receiver.Quantity = orderQty;
            //if (manualPrice > 0M)
            //{
            //    ltc_order_receiver.ManualPrice = manualPrice;
            //    top_order_thread = new Thread(new ThreadStart(ltc_order_receiver.CreateOrderWithManualPrice));
            //}
            //else
            //{
            //    top_order_thread = new Thread(new ThreadStart(ltc_order_receiver.CreateTopOrderAndMaintainTopPosition));
            //}

            //DialogResult result = MessageBox.Show(string.Format("You are about to create the [{0}] Sell Order to buy LTC with DRK, Quantity is [{1}], Price is [{2}], Confirm?", (manualPrice>0M?"MANUAL":"AUTO-TOP-ORDER"), ltc_order_receiver.Quantity, (manualPrice>0M?manualPrice.ToString():"AUTO")), "Order", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            //if (result != System.Windows.Forms.DialogResult.Yes) return;
            //top_order_thread.Start();
            //this.btnStartArbExecution.Invoke(new Action(() => this.btnStartArbExecution.Text = "Running..., Press to Stop"));
        }

        private void txtBoxArbOrderQty_TextChanged(object sender, EventArgs e)
        {
            decimal val = 0M;
            string text = this.txtBoxArbOrderQty.Text;
            if (!Decimal.TryParse(text, out val)) this.txtBoxArbOrderQty.Text = "";
        }

        private void txtBoxCalcDrkToLtcRate_TextChanged(object sender, EventArgs e)
        {
            CalculatorRun();
        }

        private void CalculatorRun()
        {
            decimal drkToLtcRate = 0M;
            decimal ltcToBtcRate = 0M;
            decimal btcToDrkRate = 0M;

            //if (decimal.TryParse(txtBoxCalcDrkToLtcRate.Text, out drkToLtcRate) && decimal.TryParse(txtBoxCalcLtcToBtcRate.Text, out ltcToBtcRate) && decimal.TryParse(txtBoxCalcBtcToDrkRate.Text, out btcToDrkRate))
            //{
            //    decimal expectedReturn = ArbFormula.CalculateExpectedReturn(drkToLtcRate, ltcToBtcRate, btcToDrkRate);
            //    this.txtBoxCalcExpectedReturn.Text = string.Format("{0:0.00000}%", 100M * Decimal.Parse(expectedReturn.ToString()));
            //}
        }

        private void txtBoxCalcLtcToBtcRate_TextChanged(object sender, EventArgs e)
        {
            CalculatorRun();
        }

        private void txtBoxCalcBtcToDrkRate_TextChanged(object sender, EventArgs e)
        {
            CalculatorRun();
        }

        private void txtBoxMinimumReturnReq_TextChanged(object sender, EventArgs e)
        {

        }

        private void ChangeMinimumRequirement(decimal newReq)
        {
            //if (drk_btc_market_thread != null && drk_btc_market_thread.IsAlive)
            //{
            //    AbortMarketDataReceivers();
            //    arbMonitor.ChangeMinimumRequirement(newReq);
            //    StartMarketDataReceivers();
            //}
            //else
            //{
            //    arbMonitor.ChangeMinimumRequirement(newReq);
            //}
        }

        private void btnCopyCalculatorValue_Click(object sender, EventArgs e)
        {
            decimal expectedReturn = 0M;
            if (decimal.TryParse(txtBoxCalcExpectedReturn.Text.Replace("%", ""), out expectedReturn))
            {
                if (expectedReturn <= 0M)
                {
                    MessageBox.Show("Unable to copy the rate from Calculator, expected return is not larger than 0%!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    txtBoxOverridePrice.Text = this.txtBoxCalcDrkToLtcRate.Text;
                    this.txtBoxMinimumReturnReq.Text = this.txtBoxCalcExpectedReturn.Text.Replace("%", "");
                    ButtonApplyMinChangeClick();
                }
            }
        }

        private void txtBoxMinimumReturnReq_Leave(object sender, EventArgs e)
        {

        }

        private void btnApplyMinReturnChange_Click(object sender, EventArgs e)
        {
            ButtonApplyMinChangeClick();
        }

        private void ButtonApplyMinChangeClick()
        {
            decimal newMinimumArbReturn = 0M;
            if (!decimal.TryParse(txtBoxMinimumReturnReq.Text, out newMinimumArbReturn))
            {
                txtBoxMinimumReturnReq.Text = "";
            }
            newMinimumArbReturn = newMinimumArbReturn / 100.00M;
            ChangeMinimumRequirement(newMinimumArbReturn);
        }

        public void Blink()
        {
            if (this.txtBoxArbExists.BackColor == Color.Red)
            {
                txtBoxArbExists.BackColor = Color.White;
            }
            else
            {
                txtBoxArbExists.BackColor = Color.Red;
            }
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Environment.Exit(Environment.ExitCode);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                this.Close();
            }
        }

    }
}
