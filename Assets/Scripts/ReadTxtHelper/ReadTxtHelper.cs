using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

#if NETFX_CORE
using Windows.Storage;
using StreamWriter = WinRTLegacy.IO.StreamWriter;
using StreamReader = WinRTLegacy.IO.StreamReader;
#else
using System.Xml;
using StreamWriter = System.IO.StreamWriter;
using StreamReader = System.IO.StreamReader;
#endif

public class ReadTxtHelper : Singleton<ReadTxtHelper> {

    private Vector3 offsetPosition = Vector3.zero;
    private Vector3 offsetRotation = Vector3.zero;
    private string ip = "";
    public Vector3 OffsetPosition
    {
        get
        {
            return offsetPosition;
        }

        set
        {
            offsetPosition = value;
        }
    }

    public Vector3 OffsetRotation
    {
        get
        {
            return offsetRotation;
        }

        set
        {
            offsetRotation = value;
        }
    }

    public string Ip
    {
        get
        {
            return ip;
        }

        set
        {
            ip = value;
        }
    }

    public List<float> danceDurations = new List<float>();

    /// <summary>
    /// 测试
    /// </summary>
    [Range(3f,8.0f)]
    public float[] durationlist;

    void Awake ()
    {
#if NETFX_CORE
    ReadOffset();
#else
        //for (int i = 0; i < durationlist.Length; i++)
        //{
        //    danceDurations.Add(durationlist[i]);
        //}
        ReadTxtContent(Application.dataPath + "/IpConfig/IpConfig.txt");
#endif
    }





#if NETFX_CORE
    public async void ReadOffset()
    {
        StorageFolder docLib = ApplicationData.Current.LocalFolder;
        Stream stream = await docLib.OpenStreamForReadAsync("\\IpConfig.txt");
        byte[] content = new byte[stream.Length];
        await stream.ReadAsync(content, 0, (int)stream.Length);
        stream.Dispose();
        string result = Encoding.UTF8.GetString(content, 0, content.Length);
        string[] configs = result.Split('/');
        string resip = configs[0];
        string res = configs[1];
        string resdance = configs[2];
        ip = resip;
        string x = res.Split(',')[0];
        string y = res.Split(',')[1];
        string z = res.Split(',')[2];
        offsetPosition = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
        string offsetrotationx = res.Split(',')[3];
        string offsetrotationy = res.Split(',')[4];
        string offsetrotationz = res.Split(',')[5];
        offsetRotation = new Vector3(float.Parse(offsetrotationx), float.Parse(offsetrotationy), float.Parse(offsetrotationz));
        InitializtionDurations(resdance);
    }



    void InitializtionDurations(string durations)
    {
        string[] durs = durations.Split(',');
        for (int i = 0; i < durs.Length; i++)
        {
            float dur = float.Parse(durs[i]);
            danceDurations.Add(dur);
        }
    }


#else
    /// <summary>
    /// 读取txt文件内容
    /// </summary>
    /// <param name="Path">文件地址</param>
    public void ReadTxtContent(string Path)
    {
        StreamReader sr = new StreamReader(Path, Encoding.Default);
        string content;
        while ((content = sr.ReadLine()) != null)
        {
            InitializtionDurations(content);
        }
    }



    void InitializtionDurations(string durations)
    {
        string[] result = durations.Split('/');
        string[] durs = result[2].Split(',');
        for (int i = 0; i < durs.Length; i++)
        {
            float dur = float.Parse(durs[i]);
            danceDurations.Add(dur);
        }
    }


#endif



}
