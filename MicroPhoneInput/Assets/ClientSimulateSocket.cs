using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// Socket客户端模拟类
/// </summary>

public class ClientSimulateSocket : MonoBehaviour{

    public int UID;
    private Socket _client;
    private Thread _interActiveThread;
    public byte[] content;

    public ClientSimulateSocket() {
    }

    public void Connect(string serverIp,int port) {
        IPAddress ip = IPAddress.Parse(serverIp);
        IPEndPoint ipe = new IPEndPoint(ip, port);
        _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _client.ReceiveBufferSize = 1024*8;
        try {
            _client.Connect(ipe);
            _interActiveThread = new Thread(ReceiveMsg);
            _interActiveThread.Start();
            Client.GetInstance().AddOperationMsg("连接成功...");

            //SendMsg();
        } catch (Exception e) {
            Debug.Log(e.Message);
        }
    }

    public void SendMsg(byte[] yuyinBytes)
    {
        //VoiceDataPacket.PacketSize
        int length = yuyinBytes.Length;
        int packetCount; 
        if(yuyinBytes.Length % VoiceDataPacket.ContentSize == 0){
            packetCount = length / VoiceDataPacket.ContentSize;
        }else{
            packetCount = length / VoiceDataPacket.ContentSize + 1;
        }

        byte[] sendData;
        byte[] packetData;
        int copyLength = 0;
        int copyIndex = 0;
        int voiceId = VoiceChatUtils.GetVoiceIdID();
        for(int i = 0; i < packetCount; i++){
            if (length < VoiceDataPacket.ContentSize)
            {
                copyLength = length;
            }else{
                copyLength = VoiceDataPacket.ContentSize;
            }
            packetData = new byte[copyLength];
            Array.Copy(yuyinBytes, copyIndex, packetData, 0, copyLength);
            copyIndex = copyIndex + copyLength;
            length = length - copyLength;

            sendData = VoiceDataPacket.PackData(UID, voiceId, packetCount, i + 1, packetData);
            Client.GetInstance().AddOperationMsg("发送语音数据片段..." + (i + 1) + " 数据大小:" + sendData.Length + " 内容大小:" + packetData.Length);
            Debug.Log("发送语音数据片段..." + (i + 1) + " 数据大小:" + sendData.Length + " 内容大小:" + packetData.Length);
            _client.Send(sendData, sendData.Length, 0);
        }
    }

    private void SendHeartbeat() {
        ByteBuffer buffInPackage = new ByteBuffer(1024);
        buffInPackage.WriteInt(99999999);
        buffInPackage.WriteInt(MsgId.Heartbeat);
        buffInPackage.WriteInt(1);
        buffInPackage.WriteByte(0);
        byte[] bytes = buffInPackage.ToArray();
        _client.Send(bytes, bytes.Length, 0);
    }

    private void ReceiveMsg() {
        while (true)
        {
            while (_client.Available > 0)
            {
                //Debug.Log("接收到数据....可读:" + _client.Available);
                byte[] bytes = new byte[_client.Available];
                int count = _client.Receive(bytes, _client.Available, 0);
                if (content == null)
                {
                    content = bytes;
                }else{
                    content = content.Concat(bytes).ToArray();
                }
                //Debug.Log("保存数据...." + content.Length);
                while (content.Length > 12)
                {
                    byte[] copyData = new byte[content.Length];
                    Array.Copy(content, copyData, content.Length);

                    ByteBuffer buff = new ByteBuffer(content.Length);
                    buff.WriteBytes(copyData, copyData.Length);

                    int head = buff.ReadInt();
                    //Debug.Log("读取包头...." + head + " :" + (VoiceDataPacket.Head == head));
                    if(VoiceDataPacket.Head == head)
                    {
                        int msgId = buff.ReadInt();
                        int length = buff.ReadInt();

                        //Debug.Log("协议Id：" + msgId + " 内容长度:" + length);
                        if (copyData.Length - 12 >= length)
                        {
                            switch (msgId)
                            {
                                case MsgId.Account:
                                    int clientId = buff.ReadInt();
                                    Debug.Log("客户端UID：" + clientId);
                                    UID = clientId;
                                    break;
                                case MsgId.Voice:
                                    byte[] voiceData = new byte[length];
                                    buff.ReadBytes(voiceData, 0, length);
                                    VoiceDataPacket voiceDataPacket = VoiceDataPacket.UnpackData(voiceData);
                                    VoiceModel.GetInstance().AddPacket(voiceDataPacket);
                                    break;
                                case MsgId.Heartbeat:
                                    Debug.Log("心跳...");
                                    SendHeartbeat();
                                    break;
                            }

                            int fixLength = content.Length - 12 - length;
                            byte[] temp = new byte[fixLength];
                            buff.ReadBytes(temp, 0, fixLength);
                            content = temp;
                        }
                        else {
                            break;
                        }
                    }
                }
            }
        }
    }

    public void destroy()
    {
        Debug.Log("destroy...");
        _interActiveThread.Abort();
        _client.Close();
        _client = null;
    }

}
