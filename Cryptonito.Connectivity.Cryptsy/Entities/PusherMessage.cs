using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Cryptonito.Connectivity.Cryptsy.Entities
{
    public class PusherMessage
    {
        public string Channel { get; set; }
        public PusherTrade Trade { get; set; }

        public static PusherMessage Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<PusherMessage>(json);
        }
    }
}
