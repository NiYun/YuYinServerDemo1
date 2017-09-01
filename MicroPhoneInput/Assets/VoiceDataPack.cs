using UnityEngine;
using System.Collections;

public class VoiceDataPacket : MonoBehaviour{
    public static int ContentSize = 1024 * 7;
    public static int Head = 99999999;

    public int clientUID;//Ip
    public int voiceId;//clientUID+timeStr
    public int count; //整个消息的数据块数量
    public int id;//index
    public byte[] content;

    public static byte[] PackData(int clientUID, int voiceId, int count, int id = 0, byte[] bytes = null)
    {
        ByteBuffer buff = new ByteBuffer(28 + bytes.Length);
        buff.WriteInt(VoiceDataPacket.Head);
        buff.WriteInt(MsgId.Voice);
        buff.WriteInt(16 + bytes.Length);
        buff.WriteInt(clientUID);
        buff.WriteInt(voiceId);
        buff.WriteInt(count);
        buff.WriteInt(id);
        buff.WriteBytes(bytes);
        return buff.ToArray();
    }

    public string Tostring() {
        string str = "";
        str = str + "clientUID : " + this.clientUID + "\n";
        str = str + "voiceId : " + this.voiceId + "\n";
        str = str + "count : " + this.count + "\n";
        str = str + "id : " + this.id + "\n";
        str = str + "Length : " + this.content.Length + "\n";
        return str;
    }

    public static VoiceDataPacket UnpackData(byte[] bytes)
    {
        int length = bytes.Length;
        ByteBuffer buff = new ByteBuffer(length);
        buff.WriteBytes(bytes, bytes.Length);

        VoiceDataPacket packet = new VoiceDataPacket();
        packet.clientUID = buff.ReadInt();
        packet.voiceId = buff.ReadInt();
        packet.count = buff.ReadInt();
        packet.id = buff.ReadInt();

        int contentSize = length - 16;
        byte[] content = new byte[contentSize];
        buff.ReadBytes(content, 0, contentSize);
        packet.content = content;

        return packet;
    }
}

