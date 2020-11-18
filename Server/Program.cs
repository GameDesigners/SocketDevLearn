﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// 1.基础的连接代码
/// 2.服务器各个步骤异步API的运用
/// </summary>
namespace Server
{
    class Program
    {
        private static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();    //客户端列表
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //初始化Socket
            Socket listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //绑定
            IPAddress ipAdr = IPAddress.Parse(GetLocalIP().Trim());
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
            listenfd.Bind(ipEp);

            //监听
            listenfd.Listen(0);
            Console.WriteLine("[服务器]启动成功");

            #region 同步的Accept和消息处理方法
            /*
            while (true)
            {
                //接收
                Socket connfd = listenfd.Accept();
                Console.WriteLine("[服务器]连接到客户端");

                //接收消息
                byte[] readBuff = new byte[1024];
                int count = connfd.Receive(readBuff);
                string readStr = System.Text.Encoding.Default.GetString(readBuff,0,count);
                Console.WriteLine("[接收到客户端消息]" + readStr);

                //重新发送回客户端
                byte[] sendByte = System.Text.Encoding.Default.GetBytes(readStr);
                connfd.Send(sendByte);
            }
            */
            #endregion

            #region 异步的Accept和消息处理方法
            listenfd.BeginAccept(AcceptCallback, listenfd);
            #endregion

            Console.ReadLine();
        }

        /// <summary>
        /// Accept的回调函数,处理三件事
        /// 1.给新的连接分配ClientState，并把它添加到clients中
        /// 2.异步接收数据
        /// 3.再次调用BeginAccept实现循环
        /// </summary>
        /// <param name="ar"></param>
        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("[服务器]连接到客户端");
                Socket listenfd = (Socket)ar.AsyncState;
                Socket clientfd = listenfd.EndAccept(ar);

                //clients列表
                ClientState state = new ClientState();
                state.socket = clientfd;
                clients.Add(clientfd,state);
                clientfd.BeginReceive(state.readBuff, 0, 1024, 0, ReceiveCallback, state);

                //继续Accept
                listenfd.BeginAccept(AcceptCallback, listenfd);
            }
            catch(SocketException ex)
            {
                Console.WriteLine("Socket连接失败：" + ex.ToString());
            }
        }

        /// <summary>
        /// Accept的回调函数,处理三件事
        /// 1.服务器收到消息，回应客户端
        /// 2.如果客户端关闭连接的信号“if(count==0)”,断开连接
        /// 3.继续调用BenginReceive接收下一个数据
        /// </summary>
        /// <param name="ar"></param>
        public static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                ClientState state = (ClientState)ar.AsyncState;
                Socket clientfd = state.socket;
                int count = clientfd.EndReceive(ar);

                //客户端关闭
                if(count==0)
                {
                    clientfd.Close();
                    clients.Remove(clientfd);
                    Console.WriteLine("Socket Close");
                    return;
                }

                string recvStr = System.Text.Encoding.Default.GetString(state.readBuff, 0, count);
                string sendStr = clientfd.RemoteEndPoint.ToString() + ":" + recvStr;
                Console.WriteLine("[接收到客户端消息]" + sendStr);

                //发送
                byte[] sendByte = System.Text.Encoding.Default.GetBytes(sendStr);
                //变例每一个客户端
                foreach(ClientState s in clients.Values)
                {
                    s.socket.Send(sendByte);//减少代码量，不用异步
                }
                clientfd.BeginReceive(state.readBuff, 0, 1024, 0, ReceiveCallback, state);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Socket接收信息失败：" + ex.ToString());
            }
        }

        /// <summary>
        /// 获取本地IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIP()
        {
            string AddressIP = string.Empty;
            foreach(IPAddress _ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_ipAddress.AddressFamily.ToString() == "InterNetwork")
                    AddressIP = _ipAddress.ToString();
            }
            return AddressIP;
        }
    }
}
