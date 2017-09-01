using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using ICSharpCode.SharpZipLib.BZip2;
using NAudio.Lame;
using NAudio.Wave.WZT;

[RequireComponent (typeof(AudioSource))]
public class MicroPhoneInput : MonoBehaviour 
{

	private static MicroPhoneInput m_instance;

	public float sensitivity=100;
	public float loudness=0;

    private static string[] micArray=null;

    const int HEADER_SIZE = 44;

    const int RECORD_TIME = 10;
    const int SIMPLE_RATE = 8000; //22050 8000 44100

	void Start () {
	}

	public static MicroPhoneInput getInstance()
	{
		if (m_instance == null) 
		{
            micArray = Microphone.devices;
            if (micArray.Length == 0)
            {
                Debug.LogError ("Microphone.devices is null");
            }
            foreach (string deviceStr in Microphone.devices)
            {
                Debug.Log("device name = " + deviceStr);
            }
			if(micArray.Length==0)
			{
				Debug.LogError("no mic device");
			}

			GameObject MicObj=new GameObject("MicObj");
			m_instance= MicObj.AddComponent<MicroPhoneInput>();
		}
		return m_instance;
	}

	public void StartRecord()
	{
        GetComponent<AudioSource>().Stop();
        if (micArray.Length == 0)
        {
            Debug.Log("No Record Device!");
            return;
        }
        int minFreq, maxFreq;
        Microphone.GetDeviceCaps(Microphone.devices[0], out minFreq, out maxFreq);
        Debug.Log("设备最小频率 = " + minFreq.ToString() + " 最大频率 = " + maxFreq.ToString());

		GetComponent<AudioSource>().loop = false;
        GetComponent<AudioSource>().mute = true;
        GetComponent<AudioSource>().clip = null;
        GetComponent<AudioSource>().clip = Microphone.Start(null, false, RECORD_TIME, SIMPLE_RATE); 
		while (!(Microphone.GetPosition(null)>0)) {
		}
		GetComponent<AudioSource>().Play ();
        Debug.Log("StartRecord");
        //倒计时
        StartCoroutine(TimeDown());
	}

	public  void StopRecord()
	{
        if (micArray.Length == 0)
        {
            Debug.Log("No Record Device!");
            return;
        }
        if (!Microphone.IsRecording(null))
        {
            return;
        }
		Microphone.End (null);
        GetComponent<AudioSource>().Stop();

        Debug.Log("StopRecord");

	}

    /// <summary>
    /// 保存录音
    /// </summary>
    public void SaveRecord()
    {
        try
        {
            Util.Save(GetComponent<AudioSource>().clip, Application.dataPath + "//record.wav");
            Debug.Log("保存完毕");
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message + ex.StackTrace);
        }
    }

    /// <summary>
    /// 写入Mp3
    /// </summary>
    /// <param name="waveFileName1"></param>
    /// <param name="mp3FileName1"></param>
    /// <param name="bitRate"></param>
    public void WaveToMP3(string waveFileName1 = "", string mp3FileName1 = "", LAMEPreset bitRate = LAMEPreset.ABR_128)
    {
        string waveFileName = Application.dataPath + "//record.wav";
        string mp3FileName = Application.dataPath + "//record.mp3";
        using (var reader = new WaveFileReader(waveFileName))
        using (var writer = new LameMP3FileWriter(mp3FileName, reader.WaveFormat, bitRate))
            reader.CopyTo(writer);
    }

    public static byte[] ZlipCompress(byte[] bytes)
    {
        MemoryStream ms = new MemoryStream();
        BZip2OutputStream BZip2 = new BZip2OutputStream(ms);
        BZip2.Write(bytes, 0, bytes.Length);
        BZip2.Close();
        return ms.ToArray();
    }

    public static byte[] ZlipDeCompress(byte[] bytes)
    {
        BZip2InputStream gzi = new BZip2InputStream(new MemoryStream(bytes));
        MemoryStream re = new MemoryStream();
        int count = 0;
        byte[] data = new byte[4096];
        while ((count = gzi.Read(data, 0, data.Length)) != 0)
        {
            re.Write(data, 0, count);
        }
        return re.ToArray();
    }

	public Byte[] GetClipData()
    {
        if (GetComponent<AudioSource>().clip == null)
        {
            Debug.Log("GetClipData audio.clip is null");
            return null; 
        }

        float[] samples = new float[GetComponent<AudioSource>().clip.samples];
        GetComponent<AudioSource>().clip.GetData(samples, 0);
		Byte[] outData = new byte[samples.Length * 2];
        //Int16[] intData = new Int16[samples.Length];
        //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

        int rescaleFactor = 32767; //to convert float to Int16
        for (int i = 0; i < samples.Length; i++)
        {
            short temshort = (short)(samples[i] * rescaleFactor);
			Byte[] temdata=System.BitConverter.GetBytes(temshort);
			outData[i*2]=temdata[0];
			outData[i*2+1]=temdata[1];
        }
		if (outData == null || outData.Length <= 0)
        {
            Debug.Log("GetClipData intData is null");
            return null; 
        }

        Debug.Log("ZlipCompress Before:" + outData.Length);
        byte[] outData1 = ZlipCompress(outData);
        Debug.Log("ZlipCompress Result:" + outData1.Length + " compress percent:" + (outData.Length / outData1.Length) + " finalDataSize(kb)：" + (outData1.Length / 1024));
        //return intData;
        return outData1;
    }

    public void PlayClipData(byte[] bytes)
    {
        byte[] data = ZlipDeCompress(bytes);
        PlayClipData(VoiceChatUtils.ByteToHexStr(data));
    }

    public void PlayClipData(Int16[] intArr)
    {
        string aaastr = intArr.ToString();
        long  aaalength=aaastr.Length;
		string aaastr1 = Convert.ToString (intArr);
		aaalength = aaastr1.Length;
        if (intArr.Length == 0)
        {
            Debug.Log("get intarr clipdata is null");
            return;
        }
        //从Int16[]到float[]
        float[] samples = new float[intArr.Length];
        int rescaleFactor = 32767;
        for (int i = 0; i < intArr.Length; i++)
        {
            samples[i] = (float)intArr[i] / rescaleFactor;
        }
        
        //从float[]到Clip
        AudioSource audioSource = this.GetComponent<AudioSource>();
        if (audioSource.clip == null)
        {
            audioSource.clip = AudioClip.Create("playRecordClip", intArr.Length, 1, 44100, false, false);
        }
        audioSource.clip.SetData(samples, 0);
        audioSource.mute = false;
        audioSource.Play();
    }
    public void PlayRecord()
	{
        if (GetComponent<AudioSource>().clip == null)
        {
            Debug.Log("audio.clip=null");
            return;
        }
		GetComponent<AudioSource>().mute = false;
		GetComponent<AudioSource>().loop = false;
		GetComponent<AudioSource>().Play ();
        Debug.Log("PlayRecord");
	}

	public  float GetAveragedVolume()
	{
		float[] data=new float[256];
		float a=0;
		GetComponent<AudioSource>().GetOutputData(data,0);
		foreach(float s in data)
		{
			a+=Mathf.Abs(s);
		}
		return a/256;
	}
	
	// Update is called once per frame
	void Update ()
    {
		loudness = GetAveragedVolume () * sensitivity;
		if (loudness > 1) 
		{
			Debug.Log("loudness = "+loudness);
		}
	}

    private IEnumerator TimeDown()
    {
        Debug.Log(" IEnumerator TimeDown()");

        int time = 0;
        while (time < RECORD_TIME)
        {
			if (!Microphone.IsRecording (null)) 
			{ //如果没有录制
				Debug.Log ("IsRecording false");
				yield break;
			}
            Debug.Log("yield return new WaitForSeconds "+time);
            Client.GetInstance().AddOperationMsg("录音倒计时:" + (RECORD_TIME - time));
            yield return new WaitForSeconds(1);
            time++;
        }
        if (time >= 10)
        {
            Debug.Log("RECORD_TIME is out! stop record!");
            StopRecord();
            Client.GetInstance().AddOperationMsg("录音结束...");
        }
        yield return 0;
    }
}
