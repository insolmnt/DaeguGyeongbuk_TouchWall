using InsolDefaultProject;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SensorLidarGManager : MonoBehaviour
{
    static public SensorLidarGManager Instance;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }



    private float SensorCheckTime = 0;
    private bool IsChecking = false;
    private int CheckingCount = 0;

    public IEnumerator FindLidar()
    {
        var list = SerialPort.GetPortNames();
        for(int i=list.Length - 1; i>=0;i --)
        {
            var com = list[i];

            Debug.Log("" + com + " 체크");

            bool check = false;

            foreach (var sensor in SensorManager.Instance.SensorList)
            {
                if (sensor.SensorId.Model == SensorModel.LidarG4)
                {
                    var g4 = (SensorG4)sensor;
                    if (com == g4.PortName && g4.IsConnect)
                    {
                        check = true;
                    }
                }
            }

            if (check == false)
            {
                Debug.Log("" + com + " 연결 시도");
                ConnectG6(com, true, true);
                yield return new WaitForSeconds(2f);
                ConnectG6(com, false, true);
            }
        }
    }

    public IEnumerator CheckSensor()
    {
        if (IsChecking)
        {
            yield break;
        }
        IsChecking = true;


        var list = SerialPort.GetPortNames();
        for (int i = list.Length - 1; i >= 0; i--)
        {
            var com = list[i];

            Debug.Log("" + com + " 체크");
            bool check = false;

            foreach(var sensor in SensorManager.Instance.SensorList)
            {
                if(sensor.SensorId.Model == SensorModel.LidarG4)
                {
                    var g4 = (SensorG4)sensor;
                    if(com == g4.PortName && g4.IsConnect)
                    {
                        check = true;
                    }
                }
            }

            if (check == false)
            {
                Debug.Log("" + com + " 연결 시도");
                ConnectG6(com, true);
                yield return new WaitForSeconds(2f);
                ConnectG6(com, false);
            }
        }

        CheckingCount++;
        IsChecking = false;
    }

    public void ConnectG6(string com, bool isG6, bool isSearch = false)
    {
        Debug.Log("<color=white>[LidarG" + (isG6 ? "6" : "4") + " Connect] </color>" + com);

        int baudate = isG6 ? 512000 : 230400; // 230400;
        int timeout = 2000;

        var _serialPort = new SerialPort();
        //_serialPort.PortName = portname;
        _serialPort.BaudRate = baudate;
        _serialPort.Parity = Parity.None;
        _serialPort.DataBits = 8;
        _serialPort.StopBits = StopBits.One;
        _serialPort.ReadTimeout = timeout;
        _serialPort.WriteTimeout = timeout;

        _serialPort.DtrEnable = true;

        _serialPort.PortName = com;
        try
        {
            _serialPort.Open();

            Debug.Log("시리얼 연결!!");

            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            //_serialPort.Write(SensorG4.msgScanStop, 0, SensorG4.msgScanStop.Length);
            _serialPort.Write(SensorG4.msgGetInfo, 0, SensorG4.msgGetInfo.Length);
            Debug.Log("정보요청");
            //5 23 3 2022021800075267
            //모델(1b, 05=g4), Firmware Version(2b), Hardware version(1b), Serial number(16b)
            var info = ReadData(_serialPort, 27);
            int model = info[7];
            string firmwareVersion = info[9] + "." + info[8];
            int hardwareVersion = info[10];
            string serial = "";
            //foreach(var d in info)
            //{
            //    Debug.Log("" + d.ToString("x2"));
            //}
            for (int i = 11; i < info.Length; i++)
            {
                serial += info[i].ToString();
            }

            Debug.Log("model : " + model);
            Debug.Log("firmwareVersion : " + firmwareVersion);
            Debug.Log("hardwareVersion : " + hardwareVersion);
            Debug.Log("serial : " + serial);

            if (model != 5 && model != 13)
            {
                Debug.Log("G4 또는 G6 센서 아님 : " + com);
                DisConnectSerial(_serialPort);
                return;
            }

            //Key = serial;
            //Data.Key = serial;


            if (SensorManager.Instance.Data.IsLidarMaxHz)
            {
                int count = 0;
                while (count < 20)
                {
                    _serialPort.Write(SensorG4.msgAddHz1, 0, SensorG4.msgAddHz1.Length);
                    var data = ReadData(_serialPort, 11);

                    float hz = (data[8] << 8 | data[7]) * 0.01f;
                    Debug.Log("hz : " + hz);
                    if (hz > 12)
                    {
                        break;
                    }
                    count++;
                }
            }



            foreach (var sensor in SensorManager.Instance.SensorList)
            {
                if (sensor.SensorId.Model == SensorModel.LidarG4 && serial == ((SensorG4)sensor).Data.SerialNo)
                {
                    ((SensorG4)sensor).IsG4 = !isG6;
                    ((SensorG4)sensor).SetSerialPort(_serialPort);
                    return;
                }
            }
            if (isSearch)
            {
                var sensor = (SensorG4)SensorManager.Instance.AddSensor(new SensorData(SensorModel.LidarG4, serial).Id, true);
                sensor.IsG4 = !isG6;
                sensor.Data.SerialNo = serial;
                sensor.SetSerialPort(_serialPort);
                sensor.Save();

                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Lidar G 센서 오류 : " + com + " / " + e);
            DisConnectSerial(_serialPort);
            return;
        }

        Debug.Log("Lidar 목록에 없음 : " + com);
        DisConnectSerial(_serialPort);
    }
    private void DisConnectSerial(SerialPort serial)
    {
        if (serial == null)
        {
            return;
        }
        Debug.Log("Lidar DisConnect " + serial.PortName);
        if (serial.IsOpen)
        {
            {
                System.Threading.Thread myThread = new System.Threading.Thread(delegate ()
                {
                    //if (_scanThread != null)
                    //    _scanThread.Join();
                    serial.DiscardInBuffer();
                    serial.Write(SensorG4.msgScanStop, 0, SensorG4.msgScanStop.Length);
                    serial.Close();
                });
                myThread.Start();
                myThread.Join();
            }
        }
    }


    byte[] ReadData(SerialPort serialPort, int len)
    {
        byte[] read = new byte[len];
        int offset = 0;
        while (offset < len)
        {
            int readLen = serialPort.Read(read, offset, len - offset);
            offset += readLen;
        }

        return read;
    }


}
