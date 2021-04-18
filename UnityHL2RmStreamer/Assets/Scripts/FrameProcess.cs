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

public class FrameProcess : MonoBehaviour
{
    public Text myText;
    private Socket client;
    private string host = "127.0.0.1";
    private int port = 23940;
    private byte[] messTmp;

    // Start is called before the first frame update
    void Start()
    {
        Thread.Sleep(5000);
        messTmp = new byte[1024];
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(new IPEndPoint(IPAddress.Parse(host), port));
        Receive(client);
    }

    //void ReceiveFrame()
    //{
    //    var count = client.Receive(messTmp);
    //    if(count!=0)
    //    {
    //        Debug.Log(count);
    //        string message = BitConverter.ToInt32(messTmp, 0).ToString();
    //        //string message = Encoding.UTF8.GetString(messTmp, 0, count);
    //        //Debug.Log(message);
    //        //myText.GetComponent<Text>().text = message;
    //    }
    //}

    void Receive(Socket socket)
    {
        Task.Factory.StartNew(() =>
        {
            var pack = new BytePkg();
            while (true)
            {
                try
                {
                    if (!socket.Connected)
                    {
                        Debug.Log("连接已断开，停止接收数据;");
                        break;
                    }
                    byte[] prevBytes = new byte[1024];
                    int len = socket.Receive(prevBytes, prevBytes.Length, SocketFlags.None);
                    Debug.Log(len);
                    var bytes = prevBytes.Take(len).ToArray();
                    Debug.Log(bytes);
                    this.RcvHeadData(pack, bytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        });
    }

    void RcvHeadData(BytePkg pack, byte[] bytes)
    {
        var len = bytes.Length;

        pack.headIndex += len;
        if(pack.headIndex<pack.headLen)
        {
            for (int x = 0; x < len; x++)
            {
                pack.headBuff[pack.headIndex - len + x] = bytes[x];
            }
        }
        else
        {
            var actualHeadLen = pack.headIndex - len;
            var skipHeadLen = pack.headLen - actualHeadLen;

            for (int x = 0; x < skipHeadLen; x++)
            {
                pack.headBuff[actualHeadLen + x] = bytes[x];
            }
            var bodyLen = len;
            if(skipHeadLen>0)
            {
                bodyLen = len - skipHeadLen;
                pack.InitBodyBuff();
            }
            this.RcvBodyData(pack, bytes.Skip(skipHeadLen).Take(bodyLen).ToArray());
        }
    }

    void RcvBodyData(BytePkg pack, byte[] bytes)
    {
        var len = bytes.Length;

        pack.bodyIndex += len;
        if (pack.bodyIndex < pack.bodyLen)
        {
            for (int x = 0; x < len; x++)
            {
                pack.bodyBuff[pack.bodyIndex - len + x] = bytes[x];
            }
        }
        else
        {
            var actualBodyLen = pack.bodyIndex - len;
            var skipBodyLen = pack.bodyLen - actualBodyLen;
            for (int x = 0; x < skipBodyLen; x++)
            {
                pack.bodyBuff[actualBodyLen + x] = bytes[x];
            }
            //this.OnReceiveMsg(pack.bodyBuff);
            pack.ResetData();
            this.RcvHeadData(pack, bytes.Skip(skipBodyLen).ToArray());
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        //ReceiveFrame();
    }

    public class BytePkg
    {
        //public int headLen = 96;
        public int headLen = sizeof(long) + 4 * sizeof(int) + 18 * sizeof(float);
        public byte[] headBuff = null;
        public int headIndex = 0;

        public int bodyLen = 0;
        public byte[] bodyBuff = null;
        public int bodyIndex = 0;

        public BytePkg()
        {
            headBuff = new byte[headLen];
        }

        public void InitBodyBuff()
        {
            FrameHeader frameHeader = ByteHelper.Deserialize<FrameHeader>(headBuff);
            string message = frameHeader.ImageWidth.ToString();
            Debug.Log(message);
            FrameProcess fp = new FrameProcess();
            fp.myText.GetComponent<Text>().text = message;
            bodyLen = frameHeader.ImageHeight * frameHeader.RowStride;
            bodyBuff = new byte[bodyLen];
        }

        public void ResetData()
        {
            headIndex = 0;
            bodyLen = 0;
            bodyBuff = null;
            bodyIndex = 0;
        }
    }

    public class ByteHelper
    {
        public static T Deserialize<T>(byte[] bs) where T : FrameHeader
        {
            using (MemoryStream ms = new MemoryStream(bs))
            {
                BinaryFormatter bf = new BinaryFormatter();
                T pkg = (T)bf.Deserialize(ms);
                return pkg;
            }
        }
    }

    [Serializable]
    public class FrameHeader
    {
        public long Timestamp;
        public int ImageWidth;
        public int ImageHeight;
        public int PixelStride;
        public int RowStride;
        public float fx;
        public float fy;
        public float PVtoWorldtransformM11;
        public float PVtoWorldtransformM12;
        public float PVtoWorldtransformM13;
        public float PVtoWorldtransformM14;
        public float PVtoWorldtransformM21;
        public float PVtoWorldtransformM22;
        public float PVtoWorldtransformM23;
        public float PVtoWorldtransformM24;
        public float PVtoWorldtransformM31;
        public float PVtoWorldtransformM32;
        public float PVtoWorldtransformM33;
        public float PVtoWorldtransformM34;
        public float PVtoWorldtransformM41;
        public float PVtoWorldtransformM42;
        public float PVtoWorldtransformM43;
        public float PVtoWorldtransformM44;
    }
}
