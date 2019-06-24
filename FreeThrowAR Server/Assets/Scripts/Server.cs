﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using MiniJSON;


enum GameState
{
    Matching,
    Playing,
    Finish,
    Reset
}

public struct ClientPoint
{
    TcpClient client;
    int point;

    public ClientPoint(TcpClient c, int i)
    {
        client = c;
        point = i;
    }
}

public class Server : ServerNetwork
{
    public int port = 30000;

    private GameState currentState;
    private List<ClientPoint> pointList = new List<ClientPoint>(); 

    
    void Start()
    {
        GameMatching();


    }

    void Update()
    {
        
        
    }

    // クライアントからパケットを受け取ったら実行
    protected override void OnMessage(TcpClient client, string str)
    {
        Dictionary<string, object> msgJson = stringToDict(str);
        
        if ((string)msgJson["name"] == "matching")
        {
            GameMatching();

        }
        else if ((string) msgJson["name"] == "start" && currentState == GameState.Matching)
        {
            GameStart();

        }
        else if ((string)msgJson["name"] == "throw" && currentState == GameState.Playing)
        {
            bcastMessage(client, str);

        }
        else if ((string)msgJson["name"] == "finish" && currentState == GameState.Playing)
        {
            ClientPoint cp = new ClientPoint(client, (int)msgJson["value"]);
            GameFinish(cp);
        }else if ((string)msgJson["name"] == "reset")
        {

        }
        
        
    }



    private void GameMatching()
    {
        currentState = GameState.Matching;
        
        // ipアドレス取得
        var ipAddress = GetIPAddress();
        // ポートを開いて待ち受け開始
        Listen(ipAddress, port);
        
    }

    private void GameStart()
    {
        currentState = GameState.Playing;

        setAcceptionEnd();
        
    }

    // ランキング出す予定
    private void GameFinish(ClientPoint cp)
    {
        currentState = GameState.Finish;
        
        // clientとポイントをリストにイン
        pointList.Add(cp);
        // ポイントで降順ソート

        // ランキングをclientに送信
        
    }

    private void GameReset()
    {
        currentState = GameState.Reset;

        ResetConnectionData();
        
    }

    

    public void bcastMessage(TcpClient client, string str)
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

    
    public Dictionary<string, object> stringToDict(string str)
    {
        return Json.Deserialize(str) as Dictionary<string, object>;
    }


    // 一番上のIPv4アドレスを返す
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