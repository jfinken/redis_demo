using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace redis_demo 
{
    class RedisManager
    {
        private static RedisManager _instance;

        private RedisConnector _redisPublisher;

        private RedisConnector _redisSubscriber;

        public static RedisManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RedisManager();
                }
                return _instance;
            }
        }

        public void start()
        {
            _redisPublisher = new RedisConnector();
            _redisPublisher.onConnected += _redisPublisher_onConnected;
            _redisPublisher.onMessageReceived += _redisPublisher_onMessageReceived;
            _redisPublisher.connect("54.235.99.151", 24400);

            _redisSubscriber = new RedisConnector();
            _redisSubscriber.Name = "im_the_subscriber_bitch";
            _redisSubscriber.onConnected += _redisSubscriber_onConnected;
            _redisSubscriber.onMessageReceived += _redisSubscriber_onMessageReceived;
            _redisSubscriber.connect("54.235.99.151", 24400);
        }

        private void _redisPublisher_onConnected(object sender, RedisEventArgs e)
        {
            Debug.WriteLine("_redisPublisher CONNECTED = " + e.Message);
            _redisPublisher.sendCommand(new string[] { "AUTH", "IR1rq9w3NUIxMy1aw5EWF90WxKKcmF3g2vpdOIPLlCOwMuMeFu+3YB9N7P9ni31vjIvZA5YHcEW3" });

        }

        private void _redisSubscriber_onConnected(object sender, RedisEventArgs e)
        {
            Debug.WriteLine("_redisSubscriber CONNECTED = " + e.Message);
            _redisSubscriber.sendCommand(new string[] { "AUTH", "IR1rq9w3NUIxMy1aw5EWF90WxKKcmF3g2vpdOIPLlCOwMuMeFu+3YB9N7P9ni31vjIvZA5YHcEW3" });
            _redisSubscriber.sendCommand(new string[] { "SUBSCRIBE", "vehicle_status_channel" });
        }

        private void _redisSubscriber_onMessageReceived(object sender, RedisEventArgs e)
        {
            Debug.WriteLine("SUBSCRIBER RECEIVED = " + e.Message);
        }

        private void _redisPublisher_onMessageReceived(object sender, RedisEventArgs e)
        {
            Debug.WriteLine("PUBLISHER RECEIVED = " + e.Message);
        }

        public void sendCommute()
        {
            Random rnd1 = new Random();
            int maVar = rnd1.Next(100);
            _redisPublisher.sendCommand(new string[] { "PUBLISH", "vehicle_status_channel", "status_" + maVar.ToString() });
        }

        public void sendDowntime()
        {
            Random rnd1 = new Random();
            int maVar = rnd1.Next(100);
            _redisPublisher.sendCommand(new string[] { "PUBLISH", "vehicle_status_channel", "down!" + maVar.ToString() });
        }

        public void sendSet()
        {
            Random rnd1 = new Random();
            int maVar = rnd1.Next(100);
            _redisPublisher.sendCommand(new string[] { "SET", "josh", "finken_" + maVar.ToString() });

        }

        public void sendGet()
        {            
            _redisPublisher.sendCommand(new string[] { "GET", "josh" });
        }

        public void closeConnection()
        {
            _redisSubscriber.sendCommand(new string[] { "UNSUBSCRIBE", "vehicle_status_channel" });
            _redisPublisher.CleanUp();
            _redisSubscriber.CleanUp();
        }


    }
}
