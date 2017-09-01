using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityGameServer.com.bx.proto;

namespace UnityGameServer.com.bx.socket
{
    class ProtocolTool
    {
        public static float HEAD = 418481047041432;

        private static ByteBuffer buffInPackage = new ByteBuffer(1024);
        private static Type typeInPackage;
        private static byte[] typeBytesInPackage;
        private static int typeLenghtInPackage;
        private static byte[] bodyBytesInPackage;
        private static int bodyLengthInPackage;
        private static int totalLengthInPackage;
        /// <summary>
        /// 封包
        /// </summary>
        /// <param name="commandId">对象字节数组</param>
        /// <param name="msgBody">protobuf消息体</param>
        /// <desc>协议格式【head(4)+totalLenth(4)+commonId(4)+typeLength(4)+typeContent+bodyLength(4)+bodyContent】</desc>
        public static byte[] PackageProto(int commandId, IExtensible msgBody) {
            typeInPackage = msgBody.GetType();
            typeBytesInPackage = Encoding.ASCII.GetBytes(typeInPackage.ToString());
            typeLenghtInPackage = typeBytesInPackage.Length;

            bodyBytesInPackage = ProtoBufftool.Serialize<IExtensible>(msgBody);
            bodyLengthInPackage = bodyBytesInPackage.Length;

            totalLengthInPackage = 4 + 4 + 4 + 4 + typeLenghtInPackage + 4 + bodyLengthInPackage;

            buffInPackage.Clear();
            buffInPackage.WriteFloat(ProtocolTool.HEAD);
            buffInPackage.WriteInt(totalLengthInPackage);
            buffInPackage.WriteInt(commandId);
            buffInPackage.WriteInt(typeLenghtInPackage);
            buffInPackage.WriteBytes(typeBytesInPackage);
            buffInPackage.WriteInt(bodyLengthInPackage);
            buffInPackage.WriteBytes(bodyBytesInPackage);
            return buffInPackage.ToArray();
        }

        private static ByteBuffer buffInUnpack = new ByteBuffer(1024);
        private static int typeLenghtInUnpack;
        private static int bodyLengthInUnpack;
        private static int totalLengthInUnpack;
        private static float headInUnpack;
        /// <summary>
        /// 解包
        /// </summary>
        /// <param name="bytes">消息字节</param>
        /// <desc>协议格式【head(4)+totalLenth(4)+commonId(4)+typeLength(4)+typeContent+bodyLength(4)+bodyContent】</desc>
        public static IExtensible UnpackProto(byte[] bytes) {
            buffInUnpack.WriteBytes(bytes, bytes.Length);
            headInUnpack = buffInUnpack.ReadFloat();
            totalLengthInUnpack = buffInUnpack.ReadInt();
            if (headInUnpack == ProtocolTool.HEAD && bytes.Length == totalLengthInUnpack)
            {
                int commonId = buffInUnpack.ReadInt();
                typeLenghtInUnpack = buffInUnpack.ReadInt();
                byte[] typeBytesInUnpack = new byte[typeLenghtInUnpack];
                buffInUnpack.ReadBytes(typeBytesInUnpack, 0, typeLenghtInUnpack);
                bodyLengthInUnpack = buffInUnpack.ReadInt();
                byte[] bodyBytesInUnpack = new byte[bodyLengthInUnpack];
                buffInUnpack.ReadBytes(bodyBytesInUnpack, 0, bodyLengthInUnpack);

                Type type = Type.GetType(Encoding.ASCII.GetString(typeBytesInUnpack));
                
                IExtensible msg = ProtoBufftool.Deserialize<IExtensible>(bodyBytesInUnpack);

                //buffInUnpack.Clear();
                //typeBytesInUnpack = null;
                //bodyBytesInUnpack = null;
                
                return null;
            }
            return null;
        }
    }
}
