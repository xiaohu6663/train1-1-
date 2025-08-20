using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine;
using System; // 添加这一行解决 Exception 引用问题


public class UDPManager : MonoBehaviour
{
    [Header("网络设置")]
    public int localPort = 8880;
    [Header("远程目标ip")]
    public string remoteIp= "10.89.3.68";
    [Header("远程目标端口")]
    public int remotePort = 8881;
    
    
    public static UDPManager instance;


    //定义默认私有的成员
    Socket socket;//目标socket
    EndPoint clientEnd;//客户端IP
    EndPoint ipEnd;//服务器端口
    public string recvStr = null; //接收到的字符串
    string sendStr;//发送的字符串
    byte[] recvData = new byte[1024];//接收的数据，字节为
    byte[] sendData = new byte[1024];//发送的数据，字节为
    int recvLen;//接收的数据长度
    Thread connectThread;//连接线程


    private void Awake()
    {
        UDPManager.instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitSocket();
    }

    private void OnEnable()
    {
        // 确保在启用时重新初始化
        if (socket != null)
        {
            SocketQuit();
        }
        InitSocket();
    }
    private void OnApplicationQuit()
    {
        SocketQuit();
    }


    //初始化__________________________________________________________________________________________________________

    void InitSocket()
    {
        try
        {
            // 先关闭之前的连接
            if (socket != null)
            {
                SocketQuit();
            }

            // 创建新的socket
            ipEnd = new IPEndPoint(IPAddress.Any, localPort);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            
            // 设置socket选项，允许地址重用
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            
            socket.Bind(ipEnd);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, remotePort);
            clientEnd = (EndPoint)sender;
            
            // 启动接收线程
            if (connectThread != null && connectThread.IsAlive)
            {
                connectThread.Abort();
            }
            connectThread = new Thread(new ThreadStart(SocketReceive));
            connectThread.Start();
            
            Debug.Log($"[UDP] Socket初始化成功，监听端口: {localPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[UDP] Socket初始化失败: {e.Message}");
            // 尝试使用备用端口
            TryAlternativePort();
        }
    }

    private void TryAlternativePort()
    {
        try
        {
            localPort += 1; // 尝试下一个端口
            if (localPort > 8890) // 限制最大端口号
            {
                localPort = 8880;
                Debug.LogError("[UDP] 无法找到可用端口，请检查网络设置");
                return;
            }
            
            Debug.Log($"[UDP] 尝试使用备用端口: {localPort}");
            InitSocket();
        }
        catch (Exception e)
        {
            Debug.LogError($"[UDP] 备用端口也失败: {e.Message}");
        }
    }


    //发送回复________________________________________________________________________________________________________

    public void SocketSend(string sendStr)
    {
        //发送数据给客户端
        sendData = new byte[1024];//数据转字节
        sendData = Encoding.UTF8.GetBytes(sendStr);//发送给指定客户端
        socket.SendTo(sendData, sendData.Length, SocketFlags.None, clientEnd);
    }

    // 在UDPManager.cs里修改接收方法
    void SocketReceive()
    {
        while (true)
        {
            recvData = new byte[1024];
            recvLen = socket.ReceiveFrom(recvData, ref clientEnd);
            string utf8Attempt = Encoding.UTF8.GetString(recvData, 0, recvLen);
            recvStr = NormalizeAndDecode(recvData, recvLen, utf8Attempt);
            Debug.Log("[UDP] 收到消息: " + recvStr);

            // 添加列车消息处理
            if (recvStr.StartsWith("{") && recvStr.EndsWith("}"))
            {
                MainThreadDispatcher.RunOnMainThread(() => {
                    try
                    {
                        // 使用新的TrackManager系统创建列车
                        if (TrackManager.Instance != null)
                        {
                            TrackManager.Instance.CreateTrain(recvStr);
                        }
                        else if (TrainManager.Instance != null)
                        {
                            // 回退到旧的TrainManager系统
                            TrainManager.Instance.CreateTrain(recvStr);
                        }
                        else
                        {
                            Debug.LogError("没有找到可用的列车管理器");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"处理列车消息失败: {ex.Message}");
                    }
                });
            }
            // 直接将UDP收到的内容作为文件名，访问F:/Graffiti
            else if (!string.IsNullOrEmpty(recvStr))
            {
                MainThreadDispatcher.RunOnMainThread(() => {
                    GraffitiManager.Instance.AddGraffitiFromFile(recvStr.Trim());
                });
            }
        }
    }

    // 规范化并尝试多编码解码（UTF-8 → GBK 回退），并替换全角标点为半角
    private string NormalizeAndDecode(byte[] data, int length, string utf8Attempt)
    {
        string message = utf8Attempt ?? string.Empty;

        // 若包含常见的乱码占位符，则尝试GBK/GB18030
        if (message.Contains("��") || (!message.StartsWith("{") && !message.EndsWith("}")))
        {
            try
            {
                var gbk = System.Text.Encoding.GetEncoding(936); // GBK
                message = gbk.GetString(data, 0, length);
            }
            catch {}
            
            if (string.IsNullOrEmpty(message) || message.Contains("��"))
            {
                try
                {
                    var gb18030 = System.Text.Encoding.GetEncoding("GB18030");
                    message = gb18030.GetString(data, 0, length);
                }
                catch {}
            }
        }

        // 标点半角化
        if (!string.IsNullOrEmpty(message))
        {
            message = message.Trim();
            message = message
                .Replace('：', ':')
                .Replace('；', ';')
                .Replace('，', ',')
                .Replace('（', '(')
                .Replace('）', ')');
        }

        return message;
    }

    //发送____________________________________________________________________________________________________________
    public void SendToTarget(string msg)
    {
        // 创建一个目标接收节点（IP + 端口），用于指定要发送到的远程地址
        IPEndPoint targetEndPoint = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);
        // 将字符串消息转换为 UTF-8 字节数组，UDP 发送的是字节
        byte[] data = Encoding.UTF8.GetBytes(msg);
        // 通过 socket 向指定的目标接收节点发送数据
        socket.SendTo(data, data.Length, SocketFlags.None, targetEndPoint);
        Debug.Log($"[UDP] 向目标:{remoteIp}:{remotePort},发送内容:{msg}");
    }


    //连接关闭_________________________________________________________________________________________________________
    void SocketQuit()
    {
        //关闭线程
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        // 关闭socket
        if (socket != null)
            socket.Close();
        print("disconnect-1");
    }

}