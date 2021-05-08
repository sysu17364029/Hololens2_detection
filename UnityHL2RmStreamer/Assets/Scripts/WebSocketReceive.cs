using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HoloToolkit.Unity;
using System.Runtime.InteropServices;

public class WebSocketReceive : MonoBehaviour
{
    public Text myText;
    private static string message;
    private static List<string> DataList = new List<string>();
    private const int receiveChunkSize = 1024;
    private static int count = 0;
    //public static int confirm = 1;
    //public static object locker = new object();


    // Start is called before the first frame update
    void Start()
    {
        //Debug.LogError("WebSocketReceive\n");
        Thread.Sleep(2000);
        ReceiveJson();
    }

    public async void ReceiveJson()
    {
        try
        {
            while (true)
            {
                //Debug.LogError("Hello?\n");
                if (FrameNetwork.webSocket != null)
                {
                    await Receive(FrameNetwork.webSocket);
                }
                //if (Begging.webSocket != null)
                //{
                //    //Debug.LogError("Hello?\n");
                //    await Receive(Begging.webSocket);
                //}
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            Debug.LogError(e.Message);
        }
    }

    static async Task Receive(ClientWebSocket webSocket)
    {
        byte[] buffer = new byte[receiveChunkSize];
        //Debug.LogError(webSocket.State);
        if (webSocket.State == WebSocketState.Open)
        {
            //Debug.LogError("plzplz");

            Global.confirm = 0;

           // confirm = 0;
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            //Debug.LogError("Receive");
            //Console.WriteLine(BitConverter.ToString(buffer, 0, result.Count));
            //Debug.LogError("resule.Count = " + result.Count);
            if (result.Count > 0)
            {
                Global.confirm = 1;
                count = count + 1;
                string filename1 = string.Format(@"Receive_{0}_{1}.txt", count, DateTime.Now.TimeOfDay.ToString().Replace(":", "."));
                string filePath1 = System.IO.Path.Combine("C:/Data/Users/edwin3280@163.com/AppData/Local/Packages/Template3D_pzq3xp76mxafg/LocalState/", filename1);
                FileStream fs1 = null;
                fs1 = new FileStream(filePath1, FileMode.Create, FileAccess.ReadWrite);
                if (fs1 != null) { fs1.Close(); }
            }
            if(Global.confirm == 1)
            {
                lock (Global.locker)
                {
                    Global.ok = 1;
                }
            }
            message = Encoding.UTF8.GetString(buffer);
            Display(message);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            //else
            //{
            //    Debug.LogError(message);
            //}
        }
    }

    static void Display(string data)
    {
        DataList.Add(data);
        //Debug.LogError(DataList.Count);
    }

    void ParseItemJson(string jsonStr)
    {
        //Debug.LogError("json?");
        jsonStr = "{\"list\":" + jsonStr.Replace("\0","") + "}";
        //Debug.LogError(jsonStr);

        Response<detection> detectList = JsonUtility.FromJson<Response<detection>>(jsonStr);
        //Debug.LogError("Are u ok?");
        //Debug.LogError(detectList.list.Count);
        foreach (detection item in detectList.list)
        {
            Debug.Log(item.X1);
            Debug.Log(item.Y1);
            Debug.Log(item.X2);
            Debug.Log(item.Y2);
            Debug.Log(item.Conf);
            Debug.Log(item.Name);
            myText.GetComponent<Text>().text = item.Name;
            string filename1 = string.Format(@"Text_{0}_{1}.txt", count, DateTime.Now.TimeOfDay.ToString().Replace(":", "."));
            string filePath1 = System.IO.Path.Combine("C:/Data/Users/edwin3280@163.com/AppData/Local/Packages/Template3D_pzq3xp76mxafg/LocalState/", filename1);
            FileStream fs1 = null;
            fs1 = new FileStream(filePath1, FileMode.Create, FileAccess.ReadWrite);
            if (fs1 != null) { fs1.Close(); }
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
        //public double Unknown;
        public double Conf;
        public string Name;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //GetMessage();
        if (DataList.Count > 0)
        {
            try { ParseItemJson(DataList[0]); }
            catch { }
            finally { DataList.RemoveAt(0); }
        }
    }
}
