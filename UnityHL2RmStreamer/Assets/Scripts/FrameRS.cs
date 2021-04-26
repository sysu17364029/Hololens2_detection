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
    public Text myText;
    public Text myText2;
    public Text myText3;
    private Socket client;
    private string host = "127.0.0.1";
    private int port = 23940;
    private byte[] messTmp;
    private int header_size = 24;
    //private Stream hdata;
    //private Stream idata;
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
        //messTmp = new byte[96];
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(new IPEndPoint(IPAddress.Parse(host), port));
        //Receive(client);
        while(client.Connected)
        {
            GetData();
        }
    }

    /*
    void ReceiveFrame()
    {
        var count = client.Receive(messTmp);
        if (count != 0)
        {
            // 写文件
            string mes = BitConverter.ToString(messTmp);
            string filename = string.Format(@"Receive_{0}.txt", Time.time);
            //string path = "Application.persistentDataPath/log.txt";
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
                sw = new StreamWriter(fs);
                sw.Write(mes);
            }
            catch(Exception e)
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

            Debug.Log(count);
            //string message = BitConverter.ToInt32(messTmp, 0).ToString();
            byte[] tsTmp = new byte[8];
            Array.Copy(messTmp, tsTmp, 8);
            string message = BitConverter.ToUInt64(tsTmp, 0).ToString();
            Debug.Log(message);
            Console.WriteLine(message);
            myText.GetComponent<Text>().text = message;
            
        }
    }
    */
    
    void GetData()
    {
        hdata = ReceiveAll(header_size);

        if (hdata.Length == 0)
        {
            Debug.LogError("ERROR: Failed to receive data from stream.");
        }

            //BinaryFormatter bf = new BinaryFormatter();
            //List<FrameHeader> frameHeaders = bf.Deserialize(hdata) as List<FrameHeader>;
        Debug.LogError("Come to Header!\n");
            //Console.WriteLine("Header\n");
        Header();
        Debug.LogError("Header is over!\n");

            //string message = frameHeaders[0].Timestamp.ToString();
        string message = Timestamp.ToString();
        Debug.Log(message);
        Console.WriteLine(message);
        myText.GetComponent<Text>().text = message;

            //int image_size = frameHeaders[0].ImageHeight*frameHeaders[0].RowStride;
        int image_size = ImageHeight * RowStride;
        myText3.GetComponent<Text>().text = image_size.ToString();
        idata = ReceiveAll(image_size);
        
        int showlen = idata.Length;
        myText2.GetComponent<Text>().text = showlen.ToString();
    }

    byte[] ReceiveAll(int data_size)
    {
        byte[] msg = new byte[data_size];
        byte[] part = new byte[100];
        int len = 0;
        int count = 0;
        //int result = 0;
        //Stream stream = null;
        while (len < data_size)
        {
            //result = Small(100, (data_size - len));
            count = client.Receive(part, 100 < (data_size - len) ? 100 : (data_size - len), SocketFlags.None);
            if (count == 0)
            {
                break;
            }
            Array.Copy(part, 0, msg, len, count);
            len += count;
        }

        //stream = new MemoryStream(msg);

        // 写入文件
        if (count != 0)
        {
            string mes = BitConverter.ToString(msg);
            string filename = string.Format(@"Receive_{0}.txt", DateTime.Now.TimeOfDay.ToString().Replace(":","."));
            //string filename = string.Format(@"Receive_{0}.txt", Time.time);
            //string path = "Application.persistentDataPath/log.txt";
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

        //return stream;
        return msg;
    }

    int Small(int a, int b)
    {
        int result;
        if (a <= b)
        {
            result = a;
        }
        else
        {
            result = b;
        }
        return result;
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

     /*
        for (int j = 0; i < 8; i++)
        {
            tsBytes[j] = hdata[i];
            j++;
        }
        for (int j = 0; i < 12; i++)
        {
            iwBytes[j] = hdata[i];
            j++;
        }
        for (int j = 0; i < 16; i++)
        {
            ihBytes[j] = hdata[i];
            j++;
        }
        for (int j = 0; i < 20; i++)
        {
            psBytes[j] = hdata[i];
            j++;
        }
        for (int j = 0; i < 24; i++)
        {
            rsBytes[j] = hdata[i];
            j++;
        }
     */
    }

    // Update is called once per frame
    void Update()
    {
        //ReceiveFrame();
    }
    
    //[Serializable]
    //public class FrameHeader
    //{
    //    public UInt64 Timestamp;
    //    public Int32 ImageWidth;
    //    public Int32 ImageHeight;
    //    public Int32 PixelStride;
    //    public Int32 RowStride;
    //    public Single fx;
    //    public Single fy;
    //    public Single PVtoWorldtransformM11;
    //    public Single PVtoWorldtransformM12;
    //    public Single PVtoWorldtransformM13;
    //    public Single PVtoWorldtransformM14;
    //    public Single PVtoWorldtransformM21;
    //    public Single PVtoWorldtransformM22;
    //    public Single PVtoWorldtransformM23;
    //    public Single PVtoWorldtransformM24;
    //    public Single PVtoWorldtransformM31;
    //    public Single PVtoWorldtransformM32;
    //    public Single PVtoWorldtransformM33;
    //    public Single PVtoWorldtransformM34;
    //    public Single PVtoWorldtransformM41;
    //    public Single PVtoWorldtransformM42;
    //    public Single PVtoWorldtransformM43;
    //    public Single PVtoWorldtransformM44;
    //}
    
}
