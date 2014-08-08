using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cryptonito.Connectivity.Cryptsy.Entities
{
    public class OrderInfo:BaseResponse
    {
        [JsonProperty("active")]
        public bool IsActive { get; set; }

        [JsonProperty("remainqty")]
        public decimal RemainQuantity { get; set; }
    }
}
