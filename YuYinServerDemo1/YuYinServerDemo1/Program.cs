using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityGameServer.com.bx.proto;
using UnityGameServer.com.bx.socket;

namespace YuYinServerDemo1
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerSocket.GetInstance().StartServer();

           string ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault<IPAddress>(a => a.AddressFamily.ToString().Equals("InterNetwork")).ToString();
           Console.WriteLine("服务器Ip.." + ip);
        }
    }
}
