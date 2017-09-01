using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.IO;

namespace UnityGameServer.com.bx.proto
{
    class ProtoBufftool
    {
        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="msg">protobuf 序列化对象</param>
        /// <returns>byte[]</returns>
        public static byte[] Serialize<T>(T msg)
        {
            byte[] result;
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize<T>(stream, msg);
                result = stream.ToArray();
            }
            return result;
        }

        /// <summary>
        /// 反序列化对象字节数组
        /// </summary>
        /// <param name="message">对象字节数组</param>
        /// <returns>实现IExtensible接口的protobuf对象实例</returns>
        public static T Deserialize<T>(byte[] message)
        {
            T result;
            using (var stream = new MemoryStream(message,true))
            {
                result = Serializer.Deserialize<T>(stream);
            }
            return result;
        }

        //use demo
        /*
            //proto
            package com.bx.proto;//包名
             message TestMsg {
                 required int64 Id=1; 
                required string Name=2;
             } 

            //序列化-发送数据
             Testmsg protoOut= new Testmsg ();
             protoOut.Id = 10046;
             protoOut.name= "beitown";
             byte[] bytes = Serialize(protoOut);
            //socket.send(bytes)之类种种，发送到字节流中...

             //接受数据-反序列化
             TestMsg protoIn = (TestMsg)Deserialize<TestMsg>(bytes);//强转成TestMsg类型
             Debug.log("Id: " + protoIn.Id);//获取Id
             Debug.log("Name: " + protoIn.Name);//获取Name
 
         */
    }
}
