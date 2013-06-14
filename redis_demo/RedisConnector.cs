using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;

namespace redis_demo 
{
    class RedisConnector
    {
        private int _port;
        private string _ipAddress;
        private string _name;
        //private string _authHash;

        private Socket _generalSocket; //for SET, GET, PUBLISH
        //private Socket _subscribeSocket;  //for SUBSCRIBE

        public RedisConnector()
        {
        
        }
        // JF
        public String Name {
            get {return _name;}
            set {_name = value;}
        }
        public void connect(string ipAddress, int port)
        {
            _port = port;
            _ipAddress = ipAddress;
            
            // create the SET GET PUBLISH socket 
            _generalSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            // create endpoint 
            var ipAdd = IPAddress.Parse(_ipAddress);
            var endpoint = new IPEndPoint(ipAdd, _port);

            // create event args 
            var args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = endpoint;
            args.Completed += GeneralSocketConnected;
            //args.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
            args.UserToken = _generalSocket;    //jf

            // check if the completed event will be raised. If not, invoke the handler manually. 
            if (!_generalSocket.ConnectAsync(args))
                GeneralSocketConnected(args.ConnectSocket, args);
        }
        /// <summary>
        /// A single callback is used for all socket operations. This method forwards execution on to the correct handler 
        /// based on the type of completed operation
        /// </summary>
        static void SocketEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    //ProcessConnect(e);
                    break;
                case SocketAsyncOperation.Receive:
                    //ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    //ProcessSend(e);
                    break;
                default:
                    throw new Exception("Invalid operation completed");
            }
        }
        private void GeneralSocketConnected(object sender, SocketAsyncEventArgs e)
        {
            // check for errors 
            if (e.SocketError != System.Net.Sockets.SocketError.Success)
            {
                Debug.WriteLine("ERROR CONNECTING WITH GENERAL SOCKET WITH SERVER");
                return;
            }
            else
            {
               // Debug.WriteLine("GENERAL SOCKET CONNECTED WITH SERVER");

                if (m_connected != null)
                {
                    m_connected(this, new RedisEventArgs("connected"));
                }

            }
            e.Completed -= GeneralSocketConnected;

            listenAndReceive();
        }
        private void listenAndReceive() {
            //-----------------------------------------------------------------
            //jf
            // 
            // Know that this instance of a RedisConnector
            //-----------------------------------------------------------------
            if(_name == "im_the_subscriber_bitch") {
                SocketAsyncEventArgs receiveSocketEventArg = new SocketAsyncEventArgs();
                receiveSocketEventArg.RemoteEndPoint = _generalSocket.RemoteEndPoint;
                // Setup the buffer to receive the data
                receiveSocketEventArg.SetBuffer(new byte[512], 0, 512);
                receiveSocketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(onGeneralSocketMessageIn);

                if (!_generalSocket.ReceiveAsync(receiveSocketEventArg))
                    onGeneralSocketMessageIn(this, receiveSocketEventArg);

            }
        }

        void onGeneralSocketMessageIn(object sender, SocketAsyncEventArgs e)
        {
            if(_name == "im_the_subscriber_bitch") {
                Debug.WriteLine("onGenerateSocketMessageIn: I'm the subscriber...");
            }

	            if (e.BytesTransferred > e.Count)
                    new Exception("ERROR onGeneralSocketMessageIn");

                e.Completed -= onGeneralSocketMessageIn;

                string response = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                response = response.Trim('\0');               

                if (m_messageReceived != null)
                {
                    m_messageReceived(this, new RedisEventArgs(response));
                }
            }



        public void sendCommand(string[] commandArgs)
        {           
                SocketAsyncEventArgs receiveSocketEventArg = new SocketAsyncEventArgs();
                receiveSocketEventArg.RemoteEndPoint = _generalSocket.RemoteEndPoint;
                // Setup the buffer to receive the data
                receiveSocketEventArg.SetBuffer(new byte[512], 0, 512);
                receiveSocketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(onGeneralSocketMessageIn);

                if (!_generalSocket.ReceiveAsync(receiveSocketEventArg))
                    onGeneralSocketMessageIn(this, receiveSocketEventArg);

                string bufferStr = "";
                bufferStr += "*" + commandArgs.Length.ToString() + "\r\n";

                for (int i = 0; i < commandArgs.Length; i++)
                {
                    string str = commandArgs[i];
                    bufferStr += "$" + str.Length.ToString() + "\r\n";
                    bufferStr += str + "\r\n";
                }
                Debug.WriteLine("SENDING : " + bufferStr);

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(bufferStr);

                SocketAsyncEventArgs sendSocketEventArg = new SocketAsyncEventArgs();
               
                sendSocketEventArg.SetBuffer(buffer, 0, buffer.Length);


                bool completedAsync = false;

                try
                {
                    completedAsync = _generalSocket.SendAsync(sendSocketEventArg);
                }
                catch (SocketException se)
                {
                    Debug.WriteLine("Socket Exception: " + se.ErrorCode + " Message: " + se.Message);
                }
        }

        public void CleanUp()
        {
           _generalSocket.Shutdown(SocketShutdown.Both);
            _generalSocket.Close();
        }



        private EventHandler<RedisEventArgs> m_connected;

        public event EventHandler<RedisEventArgs> onConnected
        {
            add { m_connected += value; }
            remove { m_connected -= value; }
        }

        private EventHandler<RedisEventArgs> m_messageReceived;

        public event EventHandler<RedisEventArgs> onMessageReceived
        {
            add { m_messageReceived += value; }
            remove { m_messageReceived -= value; }
        }

    }

    public class RedisEventArgs : EventArgs
    {
        private string msg;

        public RedisEventArgs(string messageData)
        {
            msg = messageData;
        }
        public string Message
        {
            get { return msg; }
            set { msg = value; }
        }
    }

}
