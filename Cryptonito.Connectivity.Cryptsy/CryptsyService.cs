using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cryptonito.Connectivity.Cryptsy.Entities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using Cryptonito.Connectivity.Cryptsy.Enums;

namespace Cryptonito.Connectivity.Cryptsy
{
    public class CryptsyService
    {
        private HMACSHA512 hmAcSha;

        /// <summary>
        /// Cryptsy public key
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// Cryptsy private key
        /// </summary>
        public string PrivateKey { get; set; }

        public CryptsyService()
        { 
            this.PublicKey = "b996f33a1b998be6594c463aa6909f9776f041ae";
            this.PrivateKey = "4b14e5c20177e4960d7dbc0bb5ee92facc3ef5409da37a7f53260acf6390cc39a963f43c67b7ef87";
            hmAcSha = new HMACSHA512(Encoding.ASCII.GetBytes(PrivateKey));
        }



        /// <summary>
        /// Get info on your cryptsy account
        /// </summary>
        /// <returns>The info object</returns>
        public async Task<Info> GetInfo()
        {
            int maxRetryCount = 5;
            while (true)
            {
                try
                {
                    var json = await Base("getinfo");
                    var jObject = JObject.Parse(json);
                    var value = jObject.GetValue("return");

                    if (!value.HasValues)
                    {
                        return new Info();
                    }

                    var info = value.ToObject<Info>();

                    return info;
                }
                catch (Exception exception)
                {
                    if (--maxRetryCount == 0)
                    {
                        return null;
                    }
                }
            }
        }


        /// <summary>
        /// Creates an order
        /// </summary>
        /// <param name="marketId">The market to place the order</param>
        /// <param name="orderType">Buy or Sell option</param>
        /// <param name="quantity">The quantity to buy/Sell</param>
        /// <param name="price">The offer price to Buy/Sell</param>
        /// <returns>The order response value from the api</returns>
        public async Task<OrderResponse> CreateOrder(string marketId, OrderType orderType, decimal quantity, decimal price, int maxRetryCount = 3)
        {
            while (true)
            {
                try
                {
                    var pairs = new Dictionary<string, string>
                    {
                        {"marketid", marketId},
                        {"ordertype", orderType.ToString()},
                        {"quantity", quantity.ToString()},
                        {"price", price.ToString()}
                    };

                    var json = await Base("createorder", pairs);
                    var o = JObject.Parse(json);
                    var result = o.ToObject<OrderResponse>();
                    result.OrderedPrice = price;
                    return result;
                }
                catch (Exception exception)
                {
                    if (--maxRetryCount == 0)
                    {
                        throw exception;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all trades 
        /// </summary>
        /// <param name="marketId">Optional marketid to return trades from single market only</param>
        /// <returns>List of all trades</returns>
        public async Task<List<Trade>> GetTrades(string marketId = "0", int maxRetryCount = 3)
        {
            var pairs = new Dictionary<string, string>
                {
                    {"marketid", marketId}
                };

            var json = marketId == "0" ? await Base("allmytrades", maxRetryCount: maxRetryCount) : await Base("mytrades", pairs, maxRetryCount);

            var jObject = JObject.Parse(json);
            var value = jObject.GetValue("return");

            if (!value.HasValues)
            {
                return new List<Trade>();
            }

            var trades = value.ToObject<List<Trade>>();

            return trades;
        }


        /// <summary>
        ///  Array of Deposits and Withdrawals on your account 
        /// </summary>
        /// <param name="maxRetryCount">Max retry before exception</param>
        /// <returns></returns>
        public async Task<List<Transaction>> GetMyTransactions(int maxRetryCount = 3)
        {
            var json = await Base("mytransactions", maxRetryCount: maxRetryCount);

            var jObject = JObject.Parse(json);
            var ret = jObject.GetValue("return");

            if (ret == null)
            {
                return new List<Transaction>();
            }

            if (!ret.HasValues)
            {
                return new List<Transaction>();
            }

            var result = ret.ToObject<List<Transaction>>();

            return result;
        }


        /// <summary>
        ///   Array of last 1000 Trades for this Market, in Date Decending Order  
        /// </summary>
        /// <param name="marketId">the marketid to query</param>
        /// <param name="maxRetryCount">Max retry before exception</param>
        /// <returns>Array of last 1000 Trades for this Market, in Date Decending Order  </returns>
        public async Task<List<MarketTrade>> GetMarketTrades(string marketId, int maxRetryCount = 3)
        {
            while (true)
            {
                try
                {
                    var pairs = new Dictionary<string, string>
                    {
                        {"marketid", marketId}
                    };

                    var json = await Base("markettrades", pairs, maxRetryCount);

                    var jObject = JObject.Parse(json);
                    var ret = jObject.GetValue("return");

                    if (ret == null)
                    {
                        return new List<MarketTrade>();
                    }

                    if (!ret.HasValues)
                    {
                        return new List<MarketTrade>();
                    }

                    var result = ret.ToObject<List<MarketTrade>>();

                    return result;
                }
                catch (Exception exception)
                {
                    if (--maxRetryCount == 0)
                    {
                        throw exception;
                    }
                }
            }
        }

        /// <summary>
        /// Lists all orders in all markets
        /// </summary>
        /// <param name="maxRetryCount">Max retry before exception</param>
        /// <returns></returns>
        public async Task<List<Order>> GetAllMyOrders(int maxRetryCount = 3)
        {
            while (true)
            {
                try
                {
                    var json = await Base("allmyorders", maxRetryCount: maxRetryCount);

                    var jObject = JObject.Parse(json);
                    var ret = jObject.GetValue("return");

                    if (ret == null)
                    {
                        return new List<Order>();
                    }

                    if (!ret.HasValues)
                    {
                        return new List<Order>();
                    }

                    var result = ret.ToObject<List<Order>>();

                    return result;
                }
                catch (Exception exception)
                {
                    if (--maxRetryCount == 0)
                    {
                        throw exception;
                    }
                }
            }
        }


        public async Task<OrderStatus> GetOrderStatus(string orderId)
        {
            var pairs = new Dictionary<string, string>
                {
                    {"orderid", orderId}
                };

            var json = await Base("getorderstatus", pairs);

            var jObject = JObject.Parse(json);
            var ret = jObject.GetValue("return");

            if (ret == null || !ret.HasValues)
            {
                return new OrderStatus();
            }

            var result = ret.ToObject<OrderStatus>();
            return result;
        }


       
        public async Task<List<Order>> GetAllMyOrders(string marketId, int maxRetryCount = 3)
        {
            while (true)
            {
                try
                {
                    var pairs = new Dictionary<string, string>
                    {
                        {"marketid", marketId}
                    };

                    var json = await Base("myorders", pairs);

                    var jObject = JObject.Parse(json);
                    var ret = jObject.GetValue("return");

                    if (ret == null)
                    {
                        return new List<Order>();
                    }

                    if (!ret.HasValues)
                    {
                        return new List<Order>();
                    }

                    var result = ret.ToObject<List<Order>>();

                    return result;
                }
                catch (Exception exception)
                {
                    if (--maxRetryCount == 0)
                    {
                        throw exception;
                    }
                }
            }
        }


        /// <summary>
        /// Cancells all orders in all markets
        /// </summary>
        /// <param name="maxRetryCount">Max retry before exception</param>
        /// <returns></returns>
        public async Task<List<string>> CancellAllOrders(int maxRetryCount = 3)
        {

            while (true)
            {
                try
                {
                    var json = await Base("cancelallorders");

                    var jObject = JObject.Parse(json);
                    var ret = jObject.GetValue("return");

                    if (ret == null)
                    {
                        return new List<string>();
                    }

                    if (!ret.HasValues)
                    {
                        return new List<string>();
                    }

                    var result = ret.ToObject<List<string>>();

                    return result;
                }
                catch (Exception exception)
                {
                    if (--maxRetryCount == 0)
                    {
                        throw exception;
                    }
                }
            }

        }

        public async Task<bool> CancelOrder(string orderId, int maxRetryCount = 3)
        {
            while (true)
            {
                try
                {
                    var pairs = new Dictionary<string, string>
                     {
                        {"orderid", orderId}
                    };

                    var json = await Base("cancelorder", pairs);
                    var jObject = JObject.Parse(json);
                    string status = jObject.ToString();
                    return !String.IsNullOrEmpty(status);
                }
                catch (Exception exception)
                {
                    if (--maxRetryCount == 0)
                    {
                        throw exception;
                    }
                }
            }
        }

        private async Task<string> Base(string method, Dictionary<string, string> paramList = null, int maxRetryCount = 3)
        {
            if (string.IsNullOrEmpty(PublicKey) || string.IsNullOrEmpty(PrivateKey))
            {
                throw new Exception("Please set public and private key using the static properties");
            }

            var request = WebRequest.Create("https://api.cryptsy.com/api");
            var postData = String.Format("method={0}&nonce={1}", method, Environment.TickCount);

            if (paramList != null)
            {
                postData = paramList.Aggregate(postData, (current, pair) => current + String.Format("&{0}={1}", pair.Key, pair.Value.Replace(",", ".")));
            }

            var messagebyte = Encoding.ASCII.GetBytes(postData);
            var hashmessage = hmAcSha.ComputeHash(messagebyte);
            var sign = BitConverter.ToString(hashmessage);
            sign = sign.Replace("-", "");

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = messagebyte.Length;
            request.Method = "POST";
            request.Headers.Add("Key", PublicKey);
            request.Headers.Add("Sign", sign.ToLower());

            try
            {
                var stream = request.GetRequestStream();
                stream.Write(messagebyte, 0, messagebyte.Length);
                stream.Close();

                var response = await Task.Run<WebResponse>(() => request.GetResponseAsync());
                var postreqreader = new StreamReader(response.GetResponseStream());
                var json = postreqreader.ReadToEnd();
                return json;
            }
            catch (Exception exception)
            {
                if (--maxRetryCount == 0)
                    throw exception;
            }

            return null;
        }

        
        /// <summary>
        /// Returns the market data 
        /// </summary>
        /// <param name="marketId">Market id</param>
        /// <param name="maxRetryCount">Number of retrys, defaults to 3</param>
        /// <returns>A single market</returns>
        public async Task<Market> GetSingleMarket(string marketId = "", int maxRetryCount = 3)
        {
            var market = await GetMarketRetry(marketId);
            return market.FirstOrDefault();
        }

        /// <summary>
        /// Returns the market data 
        /// </summary>
        /// <param name="marketId">Optional parameter for single market</param>
        /// <param name="maxRetryCount">Number of retrys, defaults to 3</param>
        /// <returns>A list of all markets</returns>
        public async Task<List<Market>> GetAllMarkets(int maxRetryCount = 3)
        {
            while (true)
            {
                try
                {
                    var markets = await GetMarketRetry("");
                    return markets;
                }
                catch (Exception exception)
                {
                    if (--maxRetryCount == 0)
                    {
                        throw exception;
                    }
                }
            }
        }

        public async Task<List<Order>> GetOrders(string marketId = "")
        {
            var url = String.Empty;

            if (marketId == String.Empty)
            {
                url = "http://pubapi.cryptsy.com/api.php?method=marketdatav2";
            }
            else
            {
                url = string.Format("http://pubapi.cryptsy.com/api.php?method=singleorderdata&marketid={0}", marketId);
            }

            var json = await DownloadStringAsync(url);
            JObject jObject = JObject.Parse(json);
            JToken value = jObject.GetValue("return");
            return value.First.First.Select(x => x.First.ToObject<Order>()).ToList();
        }

        public async Task<List<Market>> GetMarketRetry(string marketId = "")
        {
            var url = String.Empty;
            if (marketId == String.Empty)
            {
                url = "http://pubapi.cryptsy.com/api.php?method=marketdatav2";
            }
            else
            {
                url = string.Format("http://pubapi.cryptsy.com/api.php?method=singlemarketdata&marketid={0}", marketId);
            }

            var json = await DownloadStringAsync(url);
            var jObject = JObject.Parse(json);
            var value = jObject.GetValue("return");
            return value.First.First.Select(x => x.First.ToObject<Market>()).ToList();
        }

        private Task<string> DownloadStringAsync(string url)
        {
            var client = new WebClient();
            client.Headers.Add("User-Agent", "Nobody");
            return client.DownloadStringTaskAsync(new Uri(url));
        }
    }
}
