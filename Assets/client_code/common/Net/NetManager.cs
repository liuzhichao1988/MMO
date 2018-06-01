using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using neil.mmo.net;


public class NetManager : Singleton<NetManager>
{
    public NetManager()
    {
        
    }

    INetworkSupport m_netClient;

    /* 负责将数据处理和socket结合 */
}
