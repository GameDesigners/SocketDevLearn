using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class Echo : MonoBehaviour
{
    [Header("UI控件")]
    [Tooltip("编辑文本框")]public InputField inputfield;
    [Tooltip("连接按钮")] public Button connBtn;
    [Tooltip("发送按钮")] public Button sendBtn;
    [Tooltip("消息文本")] public Text msgTextBox;

    [Header("服务器IP4地址")] public string IPV4Address;

    //定义套接字
    private Socket socket;

    //接受缓冲区
    byte[] readBuff = new byte[1024];
    string recvStr = "";

    public void Connection()
    {
        //初始化Socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);  //网络类型：IP4 

        
        //异步客户端代码
        socket.BeginConnect(IPV4Address, 8888, ConnectCallback, socket);
    }

    //Connect的回调
    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket连接成功");

            //异步接受消息
            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch(SocketException ex)
        {
            Debug.Log("Socket连接失败：" + ex.ToString());
        }
    }

    //Receive的回调
    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            string s = System.Text.Encoding.Default.GetString(readBuff, 0, count);
            recvStr = "<color=red>" + s + "</color>" + "\n" + recvStr.Replace("<color=red>", "<color=black>");
            Debug.Log("[接收到服务器的消息]" + recvStr);
            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch(SocketException ex)
        {
            Debug.Log("Socket接收消息失败：" + ex.ToString());
        }
    }

    private void SendCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndSend(ar);
            Debug.Log("Socket发送成功");
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket发送消息失败：" + ex.ToString());
        }
    }

    public void Send()
    {
        string send_content = inputfield.text;
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(send_content);
        socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, socket);

        //模拟大数据发送
        /*
        for (int i = 0; i < 10000; i++)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, socket);
            Debug.Log("发送次数：" + i);
        }
        */

        #region 发送消息之堵塞方法
        /*
        //发送消息
        string send_content = "Send MSG";
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(send_content);
        //socket.Send(sendBytes);

        //模拟大数据发送
        for (int i = 0; i < 1000000; i++)
        {
            socket.Send(sendBytes);
            Debug.Log("发送次数：" + i);
        }
        */
        #endregion

        #region 接受消息之阻塞方法
        /*
        //接收消息
        byte[] readBuff = new byte[1024];
        int count = socket.Receive(readBuff);
        string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
        Debug.Log("[接收到服务器的消息]" + recvStr);
        socket.Close();
        */
        #endregion

    }


    private void Start()
    {
        connBtn.onClick.AddListener(Connection);
        sendBtn.onClick.AddListener(Send);
    }

    private void Update()
    {
        msgTextBox.text = recvStr;
    }

    /// <summary>
    /// 获取本地IP地址
    /// </summary>
    /// <returns></returns>
    public string GetLocalIP()
    {
        string AddressIP = string.Empty;
        foreach (IPAddress _ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (_ipAddress.AddressFamily.ToString() == "InterNetwork")
                AddressIP = _ipAddress.ToString();
        }
        return AddressIP;
    }
}
