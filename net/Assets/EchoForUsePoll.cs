using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class EchoForUsePoll : MonoBehaviour
{
    [Header("UI空间")]
    [Tooltip("编辑文本框")] public InputField inputfield;
    [Tooltip("连接按钮")] public Button connBtn;
    [Tooltip("发送按钮")] public Button sendBtn;
    [Tooltip("消息文本")] public Text msgTextBox;

    //定义套接字
    private Socket socket;

    string recvStr="";

    /// <summary>
    /// 连接
    /// </summary>
    public void Connection()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(GetLocalIP().Trim(), 8888);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    public void Send()
    {
        if(socket.Poll(0,SelectMode.SelectWrite))
        {
            string send_content = inputfield.text;
            byte[] sendBytes = System.Text.Encoding.Default.GetBytes(send_content);
            socket.Send(sendBytes);
        }

        //阻塞方法
        /*
        byte[] readBuff = new byte[1024];
        int count = socket.Receive(readBuff);
        string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
        Debug.Log("[接收到服务器的消息]" + recvStr);
        socket.Close();
        */
    }
     
    private void Start()
    {
        connBtn.onClick.AddListener(Connection);
        sendBtn.onClick.AddListener(Send);
    }

    private void Update()
    {
        if (socket == null)
            return;

        if (socket.Poll(0, SelectMode.SelectRead))
        {
            byte[] readBuff = new byte[1024];
            int count = socket.Receive(readBuff);
            string s = System.Text.Encoding.Default.GetString(readBuff, 0, count);
            Debug.Log("[接收到服务器的消息]" + recvStr);
            recvStr = "<color=red>" + s + "</color>" + "\n" + recvStr.Replace("<color=red>", "<color=black>");
            msgTextBox.text = recvStr;
        }
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
