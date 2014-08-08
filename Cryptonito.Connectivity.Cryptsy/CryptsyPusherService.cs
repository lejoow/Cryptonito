using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PusherClient;
using Cryptonito.Connectivity.Cryptsy.Entities;
namespace Cryptonito.Connectivity.Cryptsy
{
    public class CryptsyPusherService
    {
        private static string AppKey="cb65d0a7a72cd94adf1f";

        public bool IsConnected{get;set;}
        public ConnectionState ConnectionState{get;set;}
        public string ChannelId { get; set; }
        public Action<PusherMessage> Listener { get; set; }
        public Action<string> StatusListener { get; set; }
        public Pusher Pusher { get; set; }

        public CryptsyPusherService(string channelId, Action<PusherMessage> listener, Action<string> connectionStatusListener)
        {
            this.ChannelId = channelId;
            this.StatusListener = connectionStatusListener;
            this.Listener = listener;
            Pusher = new Pusher(AppKey, new PusherOptions { Encrypted = true });
            Init();
        }

        private void Init()
        {
            Pusher.Connected += Connected;
            Pusher.ConnectionStateChanged += Pusher_ConnectionStateChanged;
        }

        void Pusher_ConnectionStateChanged(object sender, ConnectionState state)
        {
            StatusListener(state.ToString());
        }

        public void Connect()
        {
            Pusher.Connect();
        }

        public void Disconnect()
        {
            Pusher.Disconnect();
        }

        public void Connected(object sender)
        {
            IsConnected = true;
            Channel channel = Pusher.Subscribe(ChannelId);
            channel.Bind("message", new Action<dynamic>(UpdateMessage));
        }

        public void UpdateMessage(dynamic value)
        {
            PusherMessage message = PusherMessage.Deserialize(value.ToString());
            Listener(message);
        }
    }
}
