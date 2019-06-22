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

    
    void Start()
    {
        // 接続中のIPアドレスを取得
        var ipAddress = GetIPAddress();
        // 指定したポートを開く
        Listen(ipAddress, port);


    }

    void Update()
    {
        
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