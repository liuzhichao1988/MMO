using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace neil.mmo.net{

    public enum EConnectionStatus{
        None,
        Conecting,
        Connected,
        Conected_Failed,
        Closed
    }

    public delegate bool DataProcess(byte[] data, int length);


    public interface INetworkSupport
    {
        DataProcess onReceiveData { get; set; }
        void SendData(byte[] buff, int dataSize);
        void Connect(string ip, int port);
        void Close();
    }

}
