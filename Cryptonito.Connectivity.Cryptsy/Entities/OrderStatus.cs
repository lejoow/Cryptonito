using Newtonsoft.Json;
using System.Collections.Generic;

namespace Cryptonito.Connectivity.Cryptsy.Entities
{
    public class OrderStatus:BaseResponse
    {
        [JsonProperty("tradeinfo")]
        public List<Trade> TradeInfo { get; set; }

        [JsonProperty("orderinfo")]
        public OrderInfo OrderInfo { get; set; }
    }
}
