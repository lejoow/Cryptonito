using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cryptonito.Connectivity.Cryptsy.Entities
{
    public class PusherTrade
    {
        public int Timestamp { get; set; }
        public string DateTime { get; set; }
        public int MarketId { get; set; }
        public PusherTop TopBuy { get; set; }
        public PusherTop TopSell { get; set; }
    }
}
