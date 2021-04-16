using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using HoloToolkit.Unity;
using System.Runtime.InteropServices;

public class StartReceive : MonoBehaviour
{
    public Text myText;
    private string message;
    private Socket server;
    private Socket worker;
    private string host = "127.0.0.1";
    //private string host = "0.0.0.0";
    private int port = 5000;
    private byte[] messTmp;
    private List<string> DataList = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        messTmp = new byte[1024];

        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(new IPEndPoint(IPAddress.Parse(host), port));
        server.Listen(1);
        server.BeginAccept(new AsyncCallback(Accept), server);
    }

    void Accept(IAsyncResult ia)
    {
        server = ia.AsyncState as Socket;
        worker = server.EndAccept(ia);
        server.BeginAccept(new AsyncCallback(Accept), server);

        try
        {
            worker.BeginReceive(messTmp, 0, messTmp.Length, SocketFlags.None, new AsyncCallback(Receive), worker);
        }
        catch
        { throw; }
    }

    void Receive(IAsyncResult ia)
    {
        worker = ia.AsyncState as Socket;
        int count = worker.EndReceive(ia);
        worker.BeginReceive(messTmp, 0, messTmp.Length, SocketFlags.None, new AsyncCallback(Receive), worker);
        if (count > 0)
        {
            Debug.Log(count);
            message = Encoding.UTF8.GetString(messTmp, 0, count);
            Debug.Log(message);
            Display(message);
        }
    }

    void Display(string data)
    {
        DataList.Add(data);
    }

    void ParseItemJson(string jsonStr)
    {
        jsonStr = "{\"list\":" + jsonStr + "}";
        Response<detection> detectList = JsonUtility.FromJson<Response<detection>>(jsonStr);
        Debug.Log(detectList.list.Count);
        foreach (detection item in detectList.list)
        {
            Debug.Log(item.X1);
            Debug.Log(item.Y1);
            Debug.Log(item.X2);
            Debug.Log(item.Y2);
            Debug.Log(item.Conf);
            Debug.Log(item.Name);
            myText.GetComponent<Text>().text = item.Name;
            GameObject gameObject = GameObject.Find("Main Camera"); 
            TextToSpeech textToSpeech = gameObject.GetComponent<TextToSpeech>();
            textToSpeech.StartSpeaking(item.Name);
        }
    }
    public class Response<T>
    {
        public List<T> list;
    }

    [Serializable]
    public class detection
    {
        public double X1;
        public double Y1;
        public double X2;
        public double Y2;
        public double Unknown;
        public double Conf;
        public string Name;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (DataList.Count > 0) 
        {
            try
            {
                ParseItemJson(DataList[0]);
            }
            catch {}
            finally
            {
                DataList.RemoveAt(0);
            }
        }
    }
}
