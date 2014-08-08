using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Cryptonito.Connectivity.Cryptsy.Enums;


namespace Cryptonito.Connectivity.Cryptsy.Entities
{
    public class MarketOrder
    {
        /// <summary>
        ///A unique ID for the trade
        /// </summary>
        [JsonProperty("orderid")]
        public string OrderId { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("ordertype")]
        public OrderType OrderType { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        [JsonProperty("total")]
        public decimal Total { get; set; }

        [JsonProperty("orig_quantity")]
        public decimal OriginalQuantity { get; set; }
       
    }
}
