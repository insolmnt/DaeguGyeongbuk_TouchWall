using UnityEngine;
using System.Net.NetworkInformation;
using System;

namespace StartPage
{

    public static class CustomUtils
    {
        public static string GetMacAddress()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                Debug.Log(
                    "Found MAC Address: " + nic.GetPhysicalAddress() +
                    " Type: " + nic.NetworkInterfaceType);
                if (nic.NetworkInterfaceType.Equals(NetworkInterfaceType.Ethernet))
                {
                    return nic.GetPhysicalAddress().ToString();
                }
            }
            
            return "";
        }

        public static T[] ShuffleArray<T>(T[] array, int seed = -1)
        {
            System.Random rand;
            if (seed == -1)
            {
                rand = new System.Random();
            }
            else
            {
                rand = new System.Random(seed);
            }

            for (int i = 0; i < array.Length - 1; i++)
            {
                int randIndex = rand.Next(i, array.Length);
                T temp = array[randIndex];
                array[randIndex] = array[i];
                array[i] = temp;
            }

            return array;
        }


        public static string GetTodayStr()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");// + UnityEngine.Random.Range(10, 30)
        }

        //public static string GetIP()
        //{
        //    return Network.player.ipAddress;
        //}
    }


    [System.Serializable]
    public class RandValue
    {
        public float min;
        public float max;

        public float Value;

        public float SetRandomValue()
        {
            Value = UnityEngine.Random.Range(min, max);
            return Value;
        }
    }

    [System.Serializable]
    public class RandPosition
    {
        [SerializeField]
        private Transform[] PointList;
        private int index;

        public Vector3 SetRandomPoint()
        {
            index = UnityEngine.Random.Range(0, PointList.Length);
            return PointList[index].position;
        }

        public Vector3 GetPoint()
        {
            return PointList[index].position;
        }
    }

}