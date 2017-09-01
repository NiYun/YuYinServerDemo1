using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class VoiceModel{

    private static VoiceModel _instance;
    //<clientId, <voiceId, packetList>>
    private Dictionary<string, List<VoiceDataPacket>> voiceData = new Dictionary<string, List<VoiceDataPacket>>();
    //<clientId, <voiceId, packetLength>>
    private Dictionary<string, int> voiceLength = new Dictionary<string, int>();

    public delegate void VoiceDataReceiveCompleteHandler(byte[] data);
    public event VoiceDataReceiveCompleteHandler DataReceiveCompleteEvent;

    public static VoiceModel GetInstance()
    {
        if (_instance == null)
        {
            _instance = new VoiceModel();
        }
        return _instance;
    }

    public void AddPacket(VoiceDataPacket packet) {
        string key = packet.clientUID + "_" + packet.voiceId;
        List<VoiceDataPacket> voicePackets;
        if (voiceData.ContainsKey(key))
        {
           voicePackets = voiceData[key];
        }
        else {
            voicePackets = new List<VoiceDataPacket>();
            voiceData.Add(key, voicePackets);
        }
        voicePackets.Add(packet);

        if (!voiceLength.ContainsKey(key))
        {
           voiceLength.Add(key, packet.count);
        }
        Util.Log("AddPacket..." + voicePackets.Count);

        if (voicePackets.Count == packet.count)
        {
            byte[] voiceDataT = GetVoicedata(packet.clientUID, packet.voiceId);
            Client.GetInstance().AddOperationMsg("语音消息【" + packet.clientUID + "_" + packet.voiceId + "】数据接收完毕..." + voiceDataT.Length);
            if (DataReceiveCompleteEvent != null)
            {
                DataReceiveCompleteEvent(voiceDataT);
            }
        }

    }

    public byte[] GetVoicedata(int clientId, int voiceId) {
        string key = clientId + "_" + voiceId;
        if (voiceData[key] == null) { return null; }
        if (voiceLength[key] != voiceData[key].Count) { return null; }
        List<VoiceDataPacket> packets = voiceData[key];
        packets.Sort(delegate(VoiceDataPacket x, VoiceDataPacket y) { return x.id.CompareTo(y.id); });
        byte[] content = packets[0].content;
        for (int i = 1; i < packets.Count;i++ )
        {
            content = content.Concat(packets[i].content).ToArray();
        }
        return content;
    }
}
