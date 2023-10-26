using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SensorUST10 : LidarSensor
{
    public const int PORT_NUMBER = 10940;

    private TcpClient m_Client;
    private Thread th;
    private NetworkStream stream = null;

    private bool IsSensorStop = false;
    public float SensorCheckCurrentTime = 0;


    private bool connectMsg = false;
    private bool connectFailMsg = false;
    private bool IsTryConnect = false;


    [Header("Data")]
    public float AngleResolution = 0.25f;
    public string Model;
    public int StartStep = 0;
    public int EndStep = 1080;

    public override void Connect()
    {
        Data.Addr = SensorId.Port;
        Debug.Log("실제 연결 시도 : " + Data.Addr + " / " + DateTime.Now);
        if (IsTryConnect)
        {
            Debug.Log("이미 연결 시도중");
            return;
        }
        if (IsConnect)
        {
            Debug.Log("이미 연결됨 : " + Data.Addr);
            return;
        }

        IsTryConnect = true;
        //UIManager.Instance.ShowSmallMessage(1798, true); //센서 연결중 입니다.

        //new Thread(() => {
        try
        {
            m_Client = new TcpClient();
            m_Client.BeginConnect(Data.Addr, PORT_NUMBER, onCompleteConnect, m_Client);
        }
        catch (Exception e)
        {
            Debug.LogError("[Lidar Jp 40] Conenct Err : " + e);
            Stop();
            return;
        }
    }

    TcpClient tcpc;
    void onCompleteConnect(IAsyncResult iar)
    {
        try
        {
            tcpc = (TcpClient)iar.AsyncState;
            tcpc.EndConnect(iar);

            IsConnect = true;
            connectMsg = true;

        }
        catch (Exception e)
        {
            Debug.LogError("[Lidar Jp 40] Conenct Err : " + e);
            Stop();
            return;
        }
    }


    public override void DisConnect()
    {
        base.DisConnect();
        Debug.Log("[DisConnect] : " + Data.Addr);
        Stop();
    }

    public override void Update()
    {
        base.Update();


        if (connectMsg)
        {
            connectMsg = false;

            IsSensorStop = false;
            IsTryConnect = false;

            if (th != null && th.IsAlive)
            {
                stream = m_Client.GetStream();
            }
            else
            {
                th = new Thread(() => LoopThread());
                th.Start();
            }


            Debug.Log("SensorStartSuccess");

            //UIManager.Instance.ShowMessage(1799); //센서 연결 성공!
        }
        if (connectFailMsg)
        {
            connectFailMsg = false;

            IsTryConnect = false;
            //SensorSetting.Instance.AddLidarSensor(this, false);
            //UIManager.Instance.ShowSmallMessage(1797); //센서 연결에 실패하였습니다.
        }

        if (IsTryConnect == false && IsConnect == false)
        {
            if (Data == null || string.IsNullOrEmpty(Data.Addr))
            {
                return;
            }

            SensorCheckCurrentTime += Time.deltaTime;
            if (SensorCheckCurrentTime > 5f) //연결이 끊긴다면 5초마다 재연결 시도
            {
                SensorCheckCurrentTime = 0;

                Connect();
            }
        }
    }
    private string read_line(NetworkStream stream)
    {
        if (stream.CanRead)
        {
            StringBuilder sb = new StringBuilder();
            bool is_NL2 = false;
            bool is_NL = false;
            do
            {
                char buf = (char)stream.ReadByte();
                if (buf == '\n')
                {
                    if (is_NL)
                    {
                        is_NL2 = true;
                    }
                    else
                    {
                        is_NL = true;
                    }
                }
                else
                {
                    is_NL = false;
                }
                sb.Append(buf);
            } while (!is_NL2);

            return sb.ToString();
        }
        else
        {
            return null;
        }
    }
    private void LoopThread()
    {
        try
        {
            if (m_Client == null || m_Client.Connected == false)
            {
                Stop();
            }
            stream = m_Client.GetStream();
            stream.ReadTimeout = 500;
            stream.WriteTimeout = 500;

            write(stream, SCIP_Writer.VV());
            string read = read_line(stream); // ignore echo back
            Debug.Log("VV " + read);

            string model = GetStrData(read, "PROD:");
            Model = model;
            string serial = GetStrData(read, "SERI:");
            Data.SerialNo = serial;
            if (model.Contains("H02"))
            {
                //Data.Type = LidarType.UST_10LX_H02;
                AngleResolution = 0.125f;
                EndStep = 2160;
            }
            else
            {
                //Data.Type = LidarType.UST_10LX;
                AngleResolution = 0.25f;
                EndStep = 1080;
            }

            //
            //
            //write(stream, SCIP_Writer.PP());
            //read = read_line(stream); // ignore echo back
            //Debug.Log("PP " + read);

            //write(stream, SCIP_Writer.II());
            //read = read_line(stream); // ignore echo back
            //Debug.Log("II " + read);


            write(stream, SCIP_Writer.SCIP2());
            read = read_line(stream); // ignore echo back
            Debug.Log("SCIP2 " + read);


            write(stream, SCIP_Writer.MD(StartStep, EndStep));
            read_line(stream);  // ignore echo back

            Thread.Sleep(100);
            List<long> DistanceList = new List<long>();
            while (true)
            {
                if (IsSensorStop)
                    break;

                DistanceList.Clear();
                long time_stamp = 0;
                string receive_data = read_line(stream);
                if (!SCIP_Reader.MD(receive_data, ref time_stamp, ref DistanceList))
                    break;
                //Debug.Log("" + time_stamp + " / " + receive_data);
                if (DistanceList.Count == 0)
                {
                    continue;
                }
                else
                {
                    List<DistanceInfo> list = new List<DistanceInfo>();
                    DistanceInfo info = new DistanceInfo();
                    float Angle = 315;
                    //if(Data.Type == LidarType.UST_10LX_H02)
                    //{
                    //    Angle = 0;
                    //}

                    for (int i = 0; i < DistanceList.Count; i++)
                    {
                        info.Angle = Angle;
                        info.Distance = (DistanceList[i] - 60) / 10.0f;
                        list.Add(info);

                        Angle += AngleResolution;
                        //if (Angle >= 360)
                        //    Angle %= 360;
                    }
                    Fps++;
                    ReadData(list);
                }

                Thread.Sleep(1);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("SensorThreadErr : " + ex);
            Stop();
        }
    }

    private bool write(NetworkStream stream, string data)
    {
        if (stream.CanWrite)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            stream.Write(buffer, 0, buffer.Length);
            return true;
        }
        else
        {
            return false;
        }
    }
    private string GetStrData(string str, string key)
    {
        int index = str.IndexOf(key);
        if (index < 0)
        {
            return "";
        }

        int lastIndex = str.IndexOf(';', index);

        int startIndex = index + key.Length;
        return str.Substring(startIndex, lastIndex - startIndex);
    }

    public void Stop()
    {
        connectFailMsg = true;
        if (IsConnect)
        {
            try
            {
                NetworkStream stream = m_Client.GetStream();
                write(stream, SCIP_Writer.QT());    // stop measurement mode
                //read_line(stream); // ignore echo back
                stream.Close();
                m_Client.Close();
            }
            catch (Exception e)
            {
                Debug.LogError("Stop Err : " + e.Message);
            }
        }

        IsConnect = false;
        //IsUse = false;
        IsSensorStop = true;
        //if (th != null && th.IsAlive)
        //    th.Abort();

        Debug.Log("SensorStop : " + Data.Addr);
        SensorCheckCurrentTime = 0;
        IsTryConnect = false;
    }

}

public class SCIP_Reader
{
    /// <summary>
    /// read MD command
    /// </summary>
    /// <param name="get_command">received command</param>
    /// <param name="time_stamp">timestamp data</param>
    /// <param name="distances">distance data</param>
    /// <returns>is successful</returns>
    public static bool MD(string get_command, ref long time_stamp, ref List<long> distances)
    {
        distances.Clear();
        string[] split_command = get_command.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        if (!split_command[0].StartsWith("MD"))
        {
            return false;
        }

        if (split_command[1].StartsWith("00"))
        {
            return true;
        }
        else if (split_command[1].StartsWith("99"))
        {
            time_stamp = SCIP_Reader.decode(split_command[2], 4);
            distance_data(split_command, 3, ref distances);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// read distance data
    /// </summary>
    /// <param name="lines"></param>
    /// <param name="start_line"></param>
    /// <returns></returns>
    public static bool distance_data(string[] lines, int start_line, ref List<long> distances)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = start_line; i < lines.Length; ++i)
        {
            sb.Append(lines[i].Substring(0, lines[i].Length - 1));
        }
        return SCIP_Reader.decode_array(sb.ToString(), 3, ref distances);
    }

    /// <summary>
    /// decode part of string 
    /// </summary>
    /// <param name="data">encoded string</param>
    /// <param name="size">encode size</param>
    /// <param name="offset">decode start position</param>
    /// <returns>decode result</returns>
    public static long decode(string data, int size, int offset = 0)
    {
        long value = 0;

        for (int i = 0; i < size; ++i)
        {
            value <<= 6;
            value |= (long)data[offset + i] - 0x30;
        }

        //using (System.IO.StreamWriter outputFile = new System.IO.StreamWriter("Test.txt", true))
        //{
        //    outputFile.WriteLine(offset + " / " + data[offset] + " " + data[offset + 1] + " " + data[offset + 2] + " : " + value);
        //}
        return value;
    }

    /// <summary>
    /// decode multiple data
    /// </summary>
    /// <param name="data">encoded string</param>
    /// <param name="size">encode size</param>
    /// <returns>decode result</returns>
    public static bool decode_array(string data, int size, ref List<long> decoded_data)
    {
        //using (System.IO.StreamWriter outputFile = new System.IO.StreamWriter("Test.txt", true))
        //{
        //    outputFile.WriteLine("\nCheck!\n");
        //}
        for (int pos = 0; pos <= data.Length - size; pos += size)
        {
            var l = decode(data, size, pos);
            decoded_data.Add(l);

        }
        return true;
    }
}


public class SCIP_Writer
{
    /// <summary>
    /// Create MD command
    /// </summary>
    /// <param name="start">measurement start step</param>
    /// <param name="end">measurement end step</param>
    /// <param name="grouping">grouping step number</param>
    /// <param name="skips">skip scan number</param>
    /// <param name="scans">get scan numbar</param>
    /// <returns>created command</returns>
    public static string MD(int start, int end, int grouping = 1, int skips = 0, int scans = 0)
    {
        return "MD" + start.ToString("D4") + end.ToString("D4") + grouping.ToString("D2") + skips.ToString("D1") + scans.ToString("D2") + "\n";
    }

    public static string VV()
    {
        return "VV\n";
    }

    public static string II()
    {
        return "II\n";
    }

    public static string PP()
    {
        return "PP\n";
    }

    public static string SCIP2()
    {
        return "SCIP2.0" + "\n";
    }

    public static string QT()
    {
        return "QT\n";
    }

    public static string BM()
    {
        return "BM\n";
    }
}