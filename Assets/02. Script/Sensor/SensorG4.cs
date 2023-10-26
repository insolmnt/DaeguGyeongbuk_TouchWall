using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SensorG4 : LidarSensor
{
    static public byte[] msgGetInfo = new byte[] { 0xA5, 0x90 };
    static public byte[] msgScanStart = new byte[] { 0xA5, 0x60 };
    static public byte[] msgScanStop = new byte[] { 0xA5, 0x65 };
    static public byte[] msgAddHz1 = new byte[] { 0xA5, 0x0B };
    static public byte[] msgMinusHz1 = new byte[] { 0xA5, 0x0C };
    static public byte[] msgGetHz = new byte[] { 0xA5, 0x0D };

    public bool IsG4;

    [Header("Data")]
    public SerialPort _serialPort;
    private Thread _scanThread = null;
    public string PortName;

    public override void DisConnect()
    {
        Debug.Log("LidarG4 DisConnect");
        if (IsConnect || (_serialPort != null && _serialPort.IsOpen))
        {
            IsConnect = false;
            Thread myThread = new System.Threading.Thread(delegate ()
            {
                _serialPort.DiscardInBuffer();
                _serialPort.Write(msgScanStop, 0, msgScanStop.Length);
                _serialPort.Close();
                _scanThread.Join();
            });
            myThread.Start();
            myThread.Join();
        }
    }

    public override void Update()
    {
        base.Update();

        if (read)
        {
            read = false;
            if(distListLast.Count > 0)
                ReadData(distListLast);
        }
    }

    public void SetSerialPort(SerialPort serial)
    {
        //Load();

        Debug.Log(name + " [SetSerialPort] " + serial.PortName);
        //Data.SerialNo = serialNum;

        _serialPort = serial;
        PortName = _serialPort.PortName;
        SensorName = (IsG4 ? "G4" : "G6" ) + " (" + PortName + ")";

        //Save();
        try
        {
            Debug.Log("스캔 시작");
            //if (IsSearching == false)
            {
                _serialPort.Write(msgScanStart, 0, msgScanStart.Length);
                var read = ReadData(7);
                for (int i = 0; i < read.Length; i++)
                {
                    Debug.Log("" + read[i].ToString("x2"));
                }

                IsConnect = true;
                _scanThread = new Thread(ScanThread);
                _scanThread.Start();
            }

            //SettingSensor.Instance.AddSensorItem(this, true);

        }
        catch (System.Exception e)
        {
            Debug.LogError("시리얼 통신 에러 : " + e.Message);
            IsConnect = false;
        }
    }
    public override void Connect()
    {
        if (IsConnect)
        {
            return;
        }

        Debug.Log("접속 시도 : " + PortName);

        if (string.IsNullOrEmpty(PortName))
        {
            StartCoroutine(SensorLidarGManager.Instance.CheckSensor());
        }
        else
        {
            SensorLidarGManager.Instance.ConnectG6(PortName, IsG4);
        }
    }
    byte[] ReadData(int len)
    {
        byte[] read = new byte[len];
        int offset = 0;
        while (offset < len)
        {
            int readLen = _serialPort.Read(read, offset, len - offset);
            //Debug.Log("readLen : " + readLen);
            offset += readLen;
        }

        return read;
    }

    bool read = false;
    List<DistanceInfo> distListLast = new List<DistanceInfo>();
    public void ScanThread()
    {
        float lastAngle = 0;
        List<DistanceInfo> distList = new List<DistanceInfo>();

        while (IsConnect)
        {
            try
            {
                byte[] data = ReadData(10);
                int CT = data[2] & 0x01;
                int LSN = data[3];
                int FSA = (data[5] << 8 | data[4]);
                int LSA = (data[7] << 8 | data[6]);

                float Angle_FSA = (FSA >> 1) / 64;
                float Angle_LSA = (LSA >> 1) / 64;
                float Angle_Diff = (Angle_LSA - Angle_FSA);

                if (Angle_Diff < 0) Angle_Diff = Angle_Diff + 360;

                if (lastAngle > Angle_FSA)
                {
                    distListLast.Clear();
                    foreach (var da in distList)
                    {
                        distListLast.Add(da);
                    }
                    Fps++;
                    read = true;
                    Thread.Sleep(10);
                    distList.Clear();
                }
                lastAngle = Angle_FSA;

                //Debug.Log("" + Angle_FSA + " - " + Angle_LSA + " / " + Angle_Diff);


                data = ReadData(LSN * 2);
                for (int i = 0; i < LSN; i++)
                {
                    int si = data[i * 2 + 1] << 8 | data[i * 2];
                    var dist = si >> 2;
                    var angle = i * Angle_Diff / (LSN - 1) + Angle_FSA;
                    if (dist > 0)
                    {
                        float AngCorrect = Mathf.Atan(21.8f * (155.3f - dist) / (155.3f * dist));
                        angle = angle + AngCorrect * 180 / Mathf.PI;
                        //Debug.Log("" + angle.ToString("F1") + " : " + dist);

                        if (angle >= 360) angle -= 360;

                        if (float.IsNaN(angle) == false)
                            distList.Add(new DistanceInfo() { Angle = angle, Distance = dist * 0.1f });
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("스캔 에러 : " + e.Message);
                return;
            }
        }
    }



}
