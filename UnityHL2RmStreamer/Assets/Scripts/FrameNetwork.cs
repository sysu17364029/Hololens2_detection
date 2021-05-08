using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using Newtonsoft.Json;

// 消费者类(将图片数据发到服务器)
public class Consumer
{
    List<Array> container = new List<Array>();  // 共享缓冲区
    public static byte[] sendata = null;    // 发送缓冲区

    public Consumer(List<Array> container)
    {
        this.container = container;
    }

    public void Consumption()
    {
        sendata = new byte[this.container[0].Length];   
        Array.Copy(this.container[0], sendata, this.container[0].Length);   // 取出共享缓冲区的图片
        //Debug.LogError("Copy Product!\n");
        this.container.RemoveAt(0); // 清空共享缓冲区
        //Debug.LogError("Write Product!\n");
    }

    // 将要发送的数据打包成 Json 格式
    public string FrameJson()
    {
        FrameNetwork.i = FrameNetwork.i + 1;
        ImageFrame iframe = new ImageFrame();
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        iframe.FrameID = FrameNetwork.i;
        iframe.Frame = sendata;
        iframe.SendTime = Convert.ToInt64(ts.TotalMilliseconds);
        //JavaScriptSerializer js = new JavaScriptSerializer();
        //Debug.LogError("Try to change Json!\n");
        string imfo = JsonConvert.SerializeObject(iframe).ToString();
        //Debug.LogError("Get Json!\n");
        return imfo;
    }
}

public class ImageFrame
{
    private int frameID;
    private byte[] frame;
    private Int64 sendTime;
    public int FrameID
    {
        set { frameID = value; }
        get { return frameID; }
    }
    public byte[] Frame
    {
        set { frame = value; }
        get { return frame; }
    }
    public Int64 SendTime
    {
        set { sendTime = value; }
        get { return sendTime; }
    }
}

public class FrameNetwork : MonoBehaviour
{
    public Text myText;
    private Socket client;
    private string host = "127.0.0.1";
    private int port = 23940;

    private byte[] messTmp;
    private int header_size = 24;
    private byte[] hdata;
    public static byte[] idata;

    public static UInt64 Timestamp = 0;
    private Int32 ImageWidth;
    private Int32 ImageHeight;
    private Int32 PixelStride;
    private Int32 RowStride;

    List<Array> container = new List<Array>();
    Consumer consumer = null;
    public static int i = 0;    // 发给服务器的帧id
    public static int ii = 0;   // 实际的帧id
    public static ClientWebSocket webSocket = null;


    // Start is called before the first frame update
    void Start()
    {
        Thread.Sleep(3000);
        messTmp = new byte[65536];
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(new IPEndPoint(IPAddress.Parse(host), port));    // 和 C++ 进行socket连接

        webSocket = new ClientWebSocket();

        Thread t1 = new Thread(new ThreadStart(ThreadGetData));
        t1.Start(); // 启动从 C++ 获取数据的线程

        Thread t2 = new Thread(new ThreadStart(ThreadConsumption));
        t2.Start(); // 启动将数据发送到服务器的线程
    }

    void ThreadGetData()
    {
        while(client.Connected)
        {
            GetData();
        }
    }
    
    void GetData()
    {
        //Debug.LogError("GetData!\n");
        hdata = ReceiveAll(header_size);

        //Debug.LogError("Come to Header!\n");
        Header();
        //Debug.LogError("Header is over!\n");

        string message = Timestamp.ToString();
        Debug.LogError(message);

        int image_size = ImageHeight * RowStride;
        Debug.LogError(image_size.ToString());
        idata = ReceiveAll(image_size);
        ii = ii + 1;
        //string filename1 = string.Format(@"New_{0}_{1}.txt", ii, DateTime.Now.TimeOfDay.ToString().Replace(":", "."));
        //string filePath1 = System.IO.Path.Combine("C:/Data/Users/edwin3280@163.com/AppData/Local/Packages/Template3D_pzq3xp76mxafg/LocalState/", filename1);
        //FileStream fs1 = null;
        //fs1 = new FileStream(filePath1, FileMode.Create, FileAccess.ReadWrite);
        //if (fs1 != null) { fs1.Close(); }

        int showlen = idata.Length;
        Debug.LogError(showlen.ToString());

        if (image_size != 0)
        {
            //Debug.LogError("GetData Lock!\n");
            lock (this)
            {
                if (this.container.Count == 0)
                {
                    this.container.Add(idata);
                    Debug.LogError("New Product!\n");
                    //Monitor.Pulse(this);
                    //Debug.LogError("Release GD Lock!\n");
                }
                else
                {
                    this.container.RemoveAt(0);
                    this.container.Add(idata);
                    Debug.LogError("Renew Product!\n");
                    string filename1 = string.Format(@"Renew_{0}_{1}.txt", ii, DateTime.Now.TimeOfDay.ToString().Replace(":", "."));
                    string filePath1 = System.IO.Path.Combine("C:/Data/Users/edwin3280@163.com/AppData/Local/Packages/Template3D_pzq3xp76mxafg/LocalState/", filename1);
                    FileStream fs1 = null;
                    fs1 = new FileStream(filePath1, FileMode.Create, FileAccess.ReadWrite);
                    if (fs1 != null) { fs1.Close(); }
                    //Monitor.Pulse(this);
                }
                //Monitor.Wait(this);
                //Debug.LogError("Release GD Lock!\n");
            }
        }
    }

    byte[] ReceiveAll(int data_size)
    {
        byte[] msg = new byte[data_size];
        byte[] part = new byte[100];
        int len = 0;
        int count = 0;

        while (len < data_size)
        {
            count = client.Receive(part, 100 < (data_size - len) ? 100 : (data_size - len), SocketFlags.None);
            if (count == 0)
            {
                break;
            }
            Array.Copy(part, 0, msg, len, count);
            len += count;
        }
        return msg;
    }

    // 解析接收到的信息头
    void Header()
    {
        byte[] tsBytes = new byte[8];
        byte[] iwBytes = new byte[4];
        byte[] ihBytes = new byte[4];
        byte[] psBytes = new byte[4];
        byte[] rsBytes = new byte[4];

        Array.Copy(hdata, 0, tsBytes, 0, 8);
        Array.Copy(hdata, 8, iwBytes, 0, 4);
        Array.Copy(hdata, 12, ihBytes, 0, 4);
        Array.Copy(hdata, 16, psBytes, 0, 4);
        Array.Copy(hdata, 20, rsBytes, 0, 4);

        Timestamp = BitConverter.ToUInt64(tsBytes, 0);
        ImageWidth = BitConverter.ToInt32(iwBytes, 0);
        ImageHeight = BitConverter.ToInt32(ihBytes, 0);
        PixelStride = BitConverter.ToInt32(psBytes, 0);
        RowStride = BitConverter.ToInt32(rsBytes, 0);
    }
    
    // 定义一个线程方法取出图像帧数据
    public async void ThreadConsumption()
    {
        // 创建一个消费者
        consumer = new Consumer(this.container);
        //Debug.LogError("New Consumer!\n");
        await Connect(webSocket, "ws://192.168.43.123:12345");  

        while (true)
        {
            if (webSocket.State != WebSocketState.Open)
            {
                Debug.LogError(webSocket.State.ToString());
                Debug.LogError(webSocket.CloseStatusDescription);
            }

            //if ((Consumer.sendata == null) && (WebSocketReceive.confirm == 1)) 
            //if (Consumer.sendata == null) 
            if ((Consumer.sendata == null) && (Global.ok == 1)) 
            {
                //Debug.LogError("Consumer Lock is Coming!\n");
                lock (this)
                {
                    //Debug.LogError("Consumer Lock!\n");
                    if (this.container.Count != 0)
                    {
                        //Debug.LogError("Consumer container had data!\n");
                        consumer.Consumption();
                        //Debug.LogError("Get Product!\n");
                        //Monitor.Pulse(this);
                    }
                    //Monitor.Wait(this);
                    //Debug.LogError("Release Consumer Lock!\n");
                }
                if (Consumer.sendata != null) 
                {
                    //Debug.LogError("sendata get data!\n");
                    string sendframe = consumer.FrameJson();
                    byte[] byteArray = Encoding.UTF8.GetBytes(sendframe);
                    //Debug.LogError("Already to Send!\n");
                    await Send(webSocket, byteArray);   // 发送数据到服务器
                    Consumer.sendata = null;
                    //Debug.LogError("Clear sendata!\n");
                    lock (Global.locker)
                    {
                        Global.ok = 0;
                    }
                }
            }
        }
    }

    public static async Task Connect(ClientWebSocket webSocket, string uri)
    {
        //webSocket = null;
        try
        {
            //webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
            //Debug.LogError("webSocket!\n");
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Debug.LogError("Failed to Connect!\n");
        }
    }

    static async Task Send(ClientWebSocket webSocket, byte[] msg)
    {
        if (webSocket.State == WebSocketState.Open)
        {
            try
            {
                await webSocket.SendAsync(new ArraySegment<byte>(msg), WebSocketMessageType.Binary, true, CancellationToken.None);
                //Debug.LogError("Send to Server!\n");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                Debug.LogError("Failed to Send!\n");
            }
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
    
}
