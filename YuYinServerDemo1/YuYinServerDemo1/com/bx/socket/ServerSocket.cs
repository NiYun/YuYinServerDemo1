using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityGameServer.com.bx.proto;
using ProtoBuf;

namespace UnityGameServer.com.bx.socket
{
    class ServerSocket
    {

        private static ServerSocket _instance;
            
        private Socket server;
        private Thread serverThread;
        private ClientMappingScoket clientMappingScoket;
        private static Dictionary<string, ClientMappingScoket> clientThreadsDic;
        private int clientIndex = 0;

        public static ServerSocket GetInstance() {
            if (_instance == null) {
                clientThreadsDic = new Dictionary<string, ClientMappingScoket>();
                _instance = new ServerSocket();
            }
            return _instance;
        }

        public ServerSocket() {
        }

        public void StartServer()
        {
            IPAddress ip = IPAddress.Parse(SocketTool.SERVER_IP);
            IPEndPoint ipe = new IPEndPoint(ip, SocketTool.SERVER_PORT);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(ipe);
            server.Listen(0);

            serverThread = new Thread(ListeningHandel);
            serverThread.Start();

            Console.WriteLine("Voice server start success...");
        }

        public void stop() {
            serverThread.Abort();
            foreach (ClientMappingScoket item in clientThreadsDic.Values)
            {
                item.destory();
            }

            server = null;
            clientThreadsDic = null;
        }

        private void ListeningHandel() {
            while (true) {
                Socket client = server.Accept();
                if (client != null) {
                    clientMappingScoket = new ClientMappingScoket(clientIndex, client);
                    string key = clientIndex + "";
                    ServerSocket.clientThreadsDic.Add(key, clientMappingScoket);
                    Console.WriteLine("new client connect success...");
                    clientIndex++;
                }
            }
        }

        public void DiscountClient(int clientIndex) {
            if (ServerSocket.clientThreadsDic.ContainsKey(clientIndex + ""))
            {
                ServerSocket.clientThreadsDic.Remove(clientIndex + "");
                Console.WriteLine(clientIndex + " 断开连接...");
            }
        }

        public void SynVoiceData(byte[] bytes) {
            foreach (ClientMappingScoket item in ServerSocket.clientThreadsDic.Values)
            {
                item.SendMsg(bytes);
            }
        }

    }

}
