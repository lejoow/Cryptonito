using Newtonsoft.Json;

namespace Cryptonito.Connectivity.Cryptsy.Entities
{
    public class OrderResponse : BaseResponse
    {
        [JsonProperty("orderid")]
        public string Orderid { get; set; }

        [JsonProperty("moreinfo")]
        public string Moreinfo { get; set; }

        public decimal OrderedPrice { get; set; }
    }
}