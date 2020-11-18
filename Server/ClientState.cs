using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    /// <summary>
    /// 客户端的信息类
    /// </summary>
    class ClientState
    {
        public Socket socket;                     //连接某客户端所需的Socket
        public byte[] readBuff = new byte[1024];  //用于填充BeginReceive参数的读缓冲区readBuff

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_socket">连接某客户端所需的Socket</param>
        /// <param name="_readBuff">用于填充BeginReceive参数的读缓冲区readBuff</param>
        public ClientState(Socket _socket, byte[] _readBuff)
        {
            socket = _socket;
            readBuff = _readBuff;
        }

        public ClientState() { }
    }
}
