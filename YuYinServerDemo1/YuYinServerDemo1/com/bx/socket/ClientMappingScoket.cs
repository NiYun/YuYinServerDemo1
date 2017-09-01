using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityGameServer.com.bx.proto;
using ProtoBuf;

/// <summary>
/// Socket客户端在服务端映射类
/// </summary>
namespace UnityGameServer.com.bx.socket
{
    class ClientMappingScoket
    {
        private int _UID;
        private Socket _mappingSocket;
        private Thread _interActiveThread;
        private Thread _heartbeatThread;
        private int _lastHeartbeatCount;
        private int _heartbeatCount = int.MinValue;
        private ByteBuffer _byteBuff;
        private int i = 0;

        private bool isDestroy = false;

        public ClientMappingScoket(int uid, Socket mappingSocket) {
            _UID = uid;
            _mappingSocket = mappingSocket;
            _mappingSocket.SendBufferSize = 1024 * 8;
            _mappingSocket.ReceiveBufferSize = 1024 * 8;

            SendClientId();
            _interActiveThread = new Thread(receiveMsg);
            _interActiveThread.Start();
            _byteBuff = new ByteBuffer(1024);

            //_heartbeatThread = new Thread(SendHeartbeat);
            //_heartbeatThread.Start();
        }

        public void SendHeartbeat()
        {
            while (isDestroy == false)
            {
                if (_lastHeartbeatCount == _heartbeatCount)
                {
                    destory();
                    return;
                }
                else {
                    ByteBuffer buffInPackage = new ByteBuffer(1024);
                    buffInPackage.WriteInt(99999999);
                    buffInPackage.WriteInt(3);
                    buffInPackage.WriteInt(1);
                    buffInPackage.WriteByte(0);
                    byte[] bytes = buffInPackage.ToArray();
                    _mappingSocket.Send(bytes, bytes.Length, 0);
                    _lastHeartbeatCount = _heartbeatCount;
                    Thread.Sleep(1000);
                }
            }
        }

        public void receiveMsg()
        {
            while (true)
            {
                while (_mappingSocket.Available > 0)
                {
                    i++;
                    int readCount = _mappingSocket.Available;
                    byte[] bytes = new byte[readCount];
                    //Console.WriteLine(_UID + " 接收到消息....." + readCount);
                    _mappingSocket.Receive(bytes, readCount, 0);

                    ServerSocket.GetInstance().SynVoiceData(bytes);
                }
            }
        }

        //public byte[] content;
        //public void receiveMsg()
        //{
        //    while (isDestroy == false)
        //    {
        //        while (_mappingSocket.Available > 0)
        //        {
        //            //Console.WriteLine("接收到数据....可读:" + _mappingSocket.Available);
        //            byte[] bytes = new byte[_mappingSocket.Available];
        //            int count = _mappingSocket.Receive(bytes, _mappingSocket.Available, 0);
        //            if (content == null)
        //            {
        //                content = bytes;
        //            }
        //            else
        //            {
        //                content = content.Concat(bytes).ToArray();
        //            }
        //            //Console.WriteLine("保存数据...." + content.Length);

        //            while (content.Length > 12)
        //            {
        //                byte[] copyData = new byte[content.Length];
        //                Array.Copy(content, copyData, content.Length);

        //                ByteBuffer buff = new ByteBuffer(content.Length);
        //                buff.WriteBytes(copyData, copyData.Length);

        //                int head = buff.ReadInt();
        //                //Console.WriteLine("读取包头...." + head + " :" + (VoiceDataPacket.Head == head));
        //                if (99999999 == head)
        //                {
        //                    int msgId = buff.ReadInt();
        //                    int length = buff.ReadInt();

        //                    //Console.WriteLine("协议Id：" + msgId + " 内容长度:" + length);
        //                    if (copyData.Length - 12 >= length)
        //                    {
        //                        switch (msgId)
        //                        {
        //                            case MsgId.Voice:
        //                                ServerSocket.GetInstance().SynVoiceData(copyData);
        //                                break;
        //                            case MsgId.Heartbeat:
        //                                //Console.WriteLine("心跳包...");
        //                                _heartbeatCount++;
        //                                if (_heartbeatCount > int.MaxValue)
        //                                {
        //                                    _heartbeatCount = int.MinValue;
        //                                }
        //                                break;
        //                        }

        //                        int fixLength = content.Length - 12 - length;
        //                        byte[] temp = new byte[fixLength];
        //                        buff.ReadBytes(temp, 0, fixLength);
        //                        content = temp;
        //                    }
        //                    else
        //                    {
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        public void SendMsg(byte[] bytes) {
            Console.WriteLine(_UID + " 同步语音消息....." + bytes.Length);
            _mappingSocket.Send(bytes, bytes.Length, 0);
        }

        public void SendClientId()
        {
            ByteBuffer buffInPackage = new ByteBuffer(1024);
            buffInPackage.WriteInt(99999999);
            buffInPackage.WriteInt(1);
            buffInPackage.WriteInt(4);
            buffInPackage.WriteInt(_UID);
            byte[] bytes = buffInPackage.ToArray();
            _mappingSocket.Send(bytes, bytes.Length, 0);
        }

        public void destory() {
            ServerSocket.GetInstance().DiscountClient(_UID);
            isDestroy = true;

            try {
                _interActiveThread.Abort();
            }catch(Exception e){
            
            }

            try
            {
                _heartbeatThread.Abort();
            }
            catch (Exception e)
            {

            }

            _mappingSocket.Close();
            _mappingSocket = null;

        }
    }
}
