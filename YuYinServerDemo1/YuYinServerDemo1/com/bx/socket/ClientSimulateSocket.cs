using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityGameServer.com.bx.proto;
using ProtoBuf;
using System.Threading;

/// <summary>
/// Socket客户端模拟类
/// </summary>
namespace UnityGameServer.com.bx.socket
{
    class ClientSimulateSocket
    {
        private string _name;
        private Socket _client;
        private Thread _interActiveThread;
        public ClientSimulateSocket(string name) {
            _name = name;
        }

        public void Connect(string serverIp,int port) {
            IPAddress ip = IPAddress.Parse(serverIp);
            IPEndPoint ipe = new IPEndPoint(ip, port);
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                _client.Connect(ipe);
                _interActiveThread = new Thread(ReceiveMsg);
                _interActiveThread.Start();

                SendMsg();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        private void SendMsg(string content = "") {
            testBuf f = new testBuf();
            f.name = _name;
            f.age = 30;
            byte[] body = ProtocolTool.PackageProto(Command.RegisterUser, f);
            _client.Send(body, body.Length, 0);
        }

        private void ReceiveMsg() {
            while (true)
            {
                while (_client.Available > 0)
                {
                    byte[] bytes = new byte[_client.Available];
                    int count = _client.Receive(bytes, _client.Available, 0);
                    testBuf t = (testBuf)ProtoBufftool.Deserialize<testBuf>(bytes);
                    Console.WriteLine("msg from server : " + t.name + " : " + t.age);
                }
            }
        }

    }

}
