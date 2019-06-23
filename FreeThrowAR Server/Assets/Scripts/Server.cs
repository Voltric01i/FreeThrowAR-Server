using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

public class Server : ServerNetwork
{
    public int port = 30000;

    // ステート用
    private bool stateGameMatching = false;
    private bool stateGamePlaying = false;
    private bool stateGameFinished = false;

    
    void Start()
    {
        // 接続中のIPアドレスを取得
        var ipAddress = GetIPAddress();
        // 指定したポートを開く
        Listen(ipAddress, port);
        stateGameMatching = true;



    }

    void Update()
    {
        
        
    }

    protected override void OnMessage(List<TcpClient> tcpClientList, TcpClient client, string str)
    {
        
        if (str == "")
        {
            listener.Stop();


        }
        else if(stateGamePlaying == true)
        {
            bcastMessage(tcpClientList, client, str);
            
        }
        else if (stateGameFinished == true)
        {

        }
        
        

        
        
    }

    public void bcastMessage(List<TcpClient> clientList, TcpClient client, string str)
    {
        // 受け取ったものを他のクライアントにも送信する
        foreach (var distination in clientList)
        {
            if (client != distination)
            {
                SendMessageToClient(str, distination);
            }
        }
    }



    private string GetIPAddress()
    {
        string hostname = Dns.GetHostName();
        IPAddress[] addressList = Dns.GetHostAddresses(hostname);
        foreach(IPAddress address in addressList)
        {
            bool isIpv4 = Regex.IsMatch(address.ToString(), "^[0-9]+.[0-9]+.[0-9]+.[0-9]+");
            if (isIpv4)
            {
                return address.ToString();
            }

        }
        return null;
    }
}