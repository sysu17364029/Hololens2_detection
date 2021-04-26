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

public class FrameRS : MonoBehaviour
{
    private Socket client;
    private string host = "127.0.0.1";
    private int port = 23940;
    private byte[] messTmp;
    private int header_size = 24;
    private byte[] hdata;
    private byte[] idata;
    private UInt64 Timestamp;
    private Int32 ImageWidth;
    private Int32 ImageHeight;
    private Int32 PixelStride;
    private Int32 RowStride;

    // Start is called before the first frame update
    void Start()
    {
        Thread.Sleep(3000);
        messTmp = new byte[65536];
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(new IPEndPoint(IPAddress.Parse(host), port));
        while(client.Connected)
        {
            GetData();
        }
    }
    
    void GetData()
    {
        hdata = ReceiveAll(header_size);

        if (hdata.Length == 0)
        {
            Debug.LogError("ERROR: Failed to receive data from stream.");
        }

        //Debug.LogError("Come to Header!\n");
        Header();
        //Debug.LogError("Header is over!\n");
        
        string message = Timestamp.ToString();

        int image_size = ImageHeight * RowStride;
        idata = ReceiveAll(image_size);
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

        // 写入文件
        if (count != 0)
        {
            string mes = BitConverter.ToString(msg);
            string filename = string.Format(@"Receive_{0}.txt", DateTime.Now.TimeOfDay.ToString().Replace(":","."));
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
                sw = new StreamWriter(fs);
                sw.Write(mes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Debug.LogError(e.Message);
            }
            finally
            {
                if (fs != null)
                {
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                }
            }
        }
        return msg;
    }

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

    // Update is called once per frame
    void Update()
    {
        
    }  
}
