using System;
using System.Net;
using System.Net.Sockets;
using neil.mmo.debug;
using System.Threading;

namespace neil.mmo.net{
    
    public class TcpClient : INetworkSupport
    {

        private const bool c_Support_IPV6 = true;

        public const int c_Async_Recv_Buffer_Size = 256*1024;
        public const int c_Async_Send_Buffer_Size = 128*1024;

        private Socket m_socket = null;

        private EConnectionStatus m_connectState = EConnectionStatus.None;
        private int m_sendingFlag = 0;
        private int m_socketTag = 1;

        private byte[] m_socketRecvBuffer;
        private byte[] m_socketSendBuffer;
        private SocketAsyncEventArgs m_socketRecvArgs;
        private SocketAsyncEventArgs m_socketSendArgs;

        private DataProcess m_onRecieveData = null;


        public bool IsConnecting { get { return (m_connectState == EConnectionStatus.Connected && m_socket != null && m_socket.Connected); } }
        public DataProcess onReceiveData { get { return m_onRecieveData; } set { m_onRecieveData = value; } }

        public TcpClient()
        {
            m_socketRecvBuffer = new byte[c_Async_Recv_Buffer_Size];
            m_socketSendBuffer = new byte[c_Async_Send_Buffer_Size];
        }


        public void Connect(string ip, int port)
        {
            Close();

            if(string.IsNullOrEmpty(ip)){
                Debug.LogError("Empty IP Address to Connect....");
                return;
            }

            ip = ip.Trim();

            try
            {
                if (c_Support_IPV6)
                {
                    IPAddress[] addr = Dns.GetHostAddresses(ip);
                    if (addr == null || addr.Length <= 0)
                    {
                        Debug.LogError("Invalid server Ip:" + ip);
                        Close();
                        return;
                    }

                    if (addr[0].AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        m_socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    }
                    else
                    {
                        m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    }

                    m_socket.SendBufferSize = c_Async_Send_Buffer_Size;
                    m_socket.ReceiveBufferSize = c_Async_Recv_Buffer_Size;
                    m_socket.BeginConnect(addr, port, null, this);
                }
                else
                {
                    m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    m_socket.SendBufferSize = c_Async_Send_Buffer_Size;
                    m_socket.ReceiveBufferSize = c_Async_Recv_Buffer_Size;
                    m_socket.BeginConnect(ip, port, null, this);
                }

                m_connectState = EConnectionStatus.Conecting;
                Debug.Log(string.Format("Try Connect -- {0}:{1}", ip, port));
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.Message);
                Close();
            }
        }

        public void Close()
        {
            m_connectState = EConnectionStatus.Closed;
            if (m_socket == null)
            {
                return;
            }
            try
            {
                if(m_socket.Connected)
                {
                    m_socket.Shutdown(SocketShutdown.Both);
                }
                m_socket.Close();
                m_socket = null;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

        }

        public void SendData(byte[] buffer, int dataSize)
        {
            if(m_socket == null || m_socket.Connected == false)
            {
                Debug.LogError("Socket Send Data Error");
                return;
            }

            if(buffer == null || dataSize < 0)
            {
                return;
            }

            if(dataSize > c_Async_Send_Buffer_Size)
            {
                Debug.LogError("Message Data oversize...");
                return;
            }

            try
            {
                if(Interlocked.CompareExchange(ref m_sendingFlag, 1, 0) == 0)
                {
                    Array.Copy(buffer, m_socketSendBuffer, dataSize);
                    m_socketSendArgs.SetBuffer(m_socketSendBuffer, 0, dataSize);
                    if(!m_socket.SendAsync(m_socketSendArgs))
                    {
                        AsyncCompleteNotify(null, m_socketRecvArgs);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.Message);
                Close();
            }

        }


        private void OnConnected(IAsyncResult result)
        {
            try
            {
                result.AsyncWaitHandle.Close();
                if(m_socket.Connected)
                {
                    Debug.Log("Connect Sucess...");

                    //结束异步连接请求
                    m_socket.EndConnect(result);
                    //开始异步等待数据
                    m_socketTag++;

                    m_socketRecvArgs = new SocketAsyncEventArgs();
                    m_socketRecvArgs.UserToken = m_socketTag;
                    m_socketRecvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AsyncCompleteNotify);
                    m_socketRecvArgs.SetBuffer(m_socketRecvBuffer, 0, m_socketRecvBuffer.Length);

                    m_socketSendArgs = new SocketAsyncEventArgs();
                    m_socketSendArgs.UserToken = m_socketTag;
                    m_socketSendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AsyncCompleteNotify);
                    m_socketSendArgs.SetBuffer(m_socketSendBuffer, 0, m_socketSendBuffer.Length);

                    m_socket.ReceiveAsync(m_socketRecvArgs);
                    m_connectState = EConnectionStatus.Connected;
                }

            }
            catch(Exception ex)
            {
                Debug.LogError("Connect Fail:" + ex.Message);
                m_connectState = EConnectionStatus.Conected_Failed;
            }
        }

        private void AsyncCompleteNotify(Object sender, SocketAsyncEventArgs args)
        {
            //只处理当前的异步连接，之前发出的异步连接忽略掉
            if(args == null || (int)args.UserToken != m_socketTag)
            {
                return;
            }

            if(args.SocketError != SocketError.Success)
            {
                if(m_connectState == EConnectionStatus.Connected)
                {
                    Debug.LogError("Socket Recieve Error:" + args.SocketError);
                    Close();
                }
                return;
            }

            if(args.LastOperation == SocketAsyncOperation.Receive)
            {
                try
                {
                    //解析数据 args.Buffer, args.BytesTransferred
                    if(m_onRecieveData != null)
                    {
                        m_onRecieveData(args.Buffer, args.BytesTransferred);
                    }

                    if (m_socket != null && m_connectState == EConnectionStatus.Connected)
                    {
                        //立即异步读取下一条消息
                        if (!m_socket.ReceiveAsync(m_socketRecvArgs))
                        {
                            AsyncCompleteNotify(null, m_socketRecvArgs);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Debug.LogError(ex.Message);
                    Close();
                    return;
                }
            }
            else if(args.LastOperation == SocketAsyncOperation.Send)
            {
                //发送完毕,可继续发送
                Interlocked.Exchange(ref m_sendingFlag, 0);
            }
        }
            
    }
}

