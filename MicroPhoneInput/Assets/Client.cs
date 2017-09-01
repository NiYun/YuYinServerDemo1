using UnityEngine;
using System.Collections;
using System.Net;
using System.Collections.Generic;
using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public class Client : MonoBehaviour {

    static string Ip = "192.168.0.195";
    static int Port = 9999;
    static string ClientUID;

    ClientSimulateSocket socketClient;
    public static string msg = "";

    private static Client _instance;
    private int msgHeight = Screen.height;
    private int msgContentHeight = 0;
    private int msgY = 0;

    private byte[] newVoice = null;
    private int height = 100;

    public static Client GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Client();
        }
        return _instance;
    }

	void Start () 
    {
       socketClient = new ClientSimulateSocket();
       socketClient.Connect(Client.Ip, Client.Port);
       Client.ClientUID = GetLocalIP();

       VoiceModel.GetInstance().DataReceiveCompleteEvent += StoreNewVoiceData;

       height = Screen.height / 6;

       _instance = this;
    }

    public void StoreNewVoiceData(byte[] data) {
        this.AddOperationMsg("收到新的语音消息...");
        newVoice = data;
    }

    public string GetLocalIP()
    {
        return Network.player.ipAddress;
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, height*0, Screen.width / 2, height), "Record"))
        {
            this.AddOperationMsg("[" + Time.time + "] 开始录音...");
            MicroPhoneInput.getInstance().StartRecord();
        }
        if (GUI.Button(new Rect(0, height * 1, Screen.width / 2, height), "Play"))
        {
            this.AddOperationMsg("[" + Time.time + "] 播放录音...");
            MicroPhoneInput.getInstance().PlayRecord();
        }
        if (GUI.Button(new Rect(0, height * 2, Screen.width / 2, height), "Store"))
        {
            this.AddOperationMsg("[" + Time.time + "] 保存录音...");
            MicroPhoneInput.getInstance().SaveRecord();
        }
        if (GUI.Button(new Rect(0, height * 3, Screen.width / 2, height), "Send"))
        {
           byte[] data = MicroPhoneInput.getInstance().GetClipData();

           this.AddOperationMsg("[" + Time.time + "] 发送录音...数据量:" + data.Length + "   ," + (data.Length/1024) + "k");
           socketClient.SendMsg(data);
        }
        if (GUI.Button(new Rect(0, height * 4, Screen.width / 2, height), "PlayerReceiveAudio"))
        {
            if (newVoice != null)
            {
                MicroPhoneInput.getInstance().PlayClipData(newVoice);
            }else{
                this.AddOperationMsg("未接收到语音数据...");
            }
        }
        if (GUI.Button(new Rect(0, height * 5, Screen.width / 2, height), "ClearLog"))
        {
            msgHeight = Screen.height;
            msgContentHeight = 0;
            msgY = 0;
            msg = "";

            MicroPhoneInput.getInstance().WaveToMP3();
        }

        GUI.TextArea(new Rect(Screen.width / 2, msgY, Screen.width / 2, msgHeight), msg);

        GUI.Label(new Rect(Screen.width - 200, 0, 200, 100), "客户端Id:" + socketClient.UID);
    }

    public void AddOperationMsg(string info) {
        msg = msg + info + "\n";
        msgContentHeight = msgContentHeight + 16;
        if (msgContentHeight > Screen.height)
        {
            msgHeight = msgHeight + 16;
            msgY = msgY - 16;
        }
    }

    void OnDestroy()
    {
        Debug.Log("OnDestroy...");
        socketClient.destroy();
        socketClient = null;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
