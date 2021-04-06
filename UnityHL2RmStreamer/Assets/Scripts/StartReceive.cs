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

public class StartReceive : MonoBehaviour
{
    public Text myText;
    private string message;
    private Socket client;
    private string host = "192.168.43.123";
    //private string host = "127.0.0.1";
    private int port = 5000;
    private byte[] messTmp;
    private GameObject LineRenderGameObject;
    private LineRenderer lineRenderer;
    private int lineLength = 5;

    // Start is called before the first frame update
    void Start()
    {
        Thread.Sleep(5000);
        messTmp = new byte[1024];
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            client.Connect(new IPEndPoint(IPAddress.Parse(host), port));
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }
        LineRenderGameObject = GameObject.Find("line");
        lineRenderer = (LineRenderer)LineRenderGameObject.GetComponent("LineRenderer");
        //lineRenderer.SetVertexCount(lineLength);
        lineRenderer.positionCount = lineLength;
    }

    void GetMessage()
    {
        var count = client.Receive(messTmp);

        if (count != 0)
        {
            Debug.Log(count);
            //string message = Encoding.Unicode.GetString(messTmp, 0, count);
            string message = Encoding.UTF8.GetString(messTmp, 0, count);
            Debug.Log(message);
            //myText.GetComponent<Text>().text = message;
            ParseItemJson(message);
        }
    }
    void ParseItemJson(string jsonStr)
    {
        jsonStr = "{\"list\":" + jsonStr + "}";
        Response<detection> detectList = JsonUtility.FromJson<Response<detection>>(jsonStr);
        foreach (detection item in detectList.list)
        {
            Debug.Log(item.X1);
            Debug.Log(item.Y1);
            Debug.Log(item.X2);
            Debug.Log(item.Y2);
            Debug.Log(item.Conf);
            Debug.Log(item.Name);
            myText.GetComponent<Text>().text = item.Name;
            //GameObject.Find("SpeechFrame").GetComponent<TextToSpeech>().SpeakSsml(item.Name);
            //TextToSpeech.SpeakSsml(item.Name);
            //GameObject.Find("SpeechFrame").GetComponent<TextToSpeech>().StartSpeaking(item.Name);
            //TextToSpeech.StartSpeaking(item.Name);
            GameObject gameObject = GameObject.Find("Main Camera"); 
            TextToSpeech textToSpeech = gameObject.GetComponent<TextToSpeech>();
            textToSpeech.StartSpeaking(item.Name);
            Vector3 v0 = new Vector3((float)item.X1, (float)item.Y1, 0.0f);
            Vector3 v1 = new Vector3((float)item.X2, (float)item.Y1, 0.0f);
            Vector3 v2 = new Vector3((float)item.X2, (float)item.Y2, 0.0f);
            Vector3 v3 = new Vector3((float)item.X1, (float)item.Y2, 0.0f);
            lineRenderer.SetPosition(0, v0);
            lineRenderer.SetPosition(1, v1);
            lineRenderer.SetPosition(2, v2);
            lineRenderer.SetPosition(3, v3);
            lineRenderer.SetPosition(4, v0);
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
        GetMessage();
    }
}
