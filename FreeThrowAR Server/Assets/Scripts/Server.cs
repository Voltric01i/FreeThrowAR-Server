using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using MiniJSON;


enum GameState
{
    Matching,
    Playing,
    Finish
}

public class GamePoint
{
    public TcpClient client { get; set; }
    public int point { get; set; }
    public GamePoint(TcpClient c, int p)
    {
        client = c;
        point = p;
    }
}

public class StatusText
{
    public string ip;
    public int port;
    public string state;
    public int players;

    public StatusText()
    {
        ip = "null";
        port = 0;
        state = "null";
        players = 0;
    }

    public string getText()
    {
        return "IPAddress is " + ip + ":" + port + "\n\nState: " + state + "\nPlayerCount: " + players;
    }
}

public class Server : ServerNetwork
{
    public int port = 30000;
    public Text textStatus;

    private float rankingSendTime = -1000;
    private GameState currentState;
    private List<GamePoint> pointList = new List<GamePoint>();
    private StatusText status = new StatusText();
    
    void Start()
    {
        GameMatching();

    }

    void Update()
    {
        status.players = clientList.Count;
        textStatus.text = status.getText();

        if(currentState == GameState.Finish)
        {
            if(rankingSendTime > 0)
            {
                rankingSendTime -= Time.deltaTime;
                if(pointList.Count == clientList.Count)
                {
                    sendRanking();
                }
            }else if(-100 < rankingSendTime && rankingSendTime < 0)
            {
                sendRanking();
                rankingSendTime = -1000;
            }
        }
        
    }

    // クライアントからパケットを受け取ったら実行
    protected override void OnMessage(TcpClient client, string str)
    {
        Dictionary<string, object> msgJson = stringToDict(str);
        
        if ((string) msgJson["name"] == "start" && currentState == GameState.Matching)
        {
            GameStart();

        }
        else if ((string)msgJson["name"] == "ball" && currentState == GameState.Playing)
        {
            bcastMessage(client, str);

        }
        else if ((string)msgJson["name"] == "finish" && currentState == GameState.Playing)
        {
            // pointをわたしてランキング計算
            // int.Parseしないとフリーズする!
            int point = int.Parse(msgJson["value"].ToString());
            GameFinish(client, point);

        }else if ((string)msgJson["name"] == "reset")
        {
            GameReset();
        }
        
        
    }



    private void GameMatching()
    {
        currentState = GameState.Matching;
        status.state = "Matching";
        
        // ipアドレス取得
        var ipAddress = GetIPAddress();
        // ポートを開いて待ち受け開始
        Listen(ipAddress, port);

        status.ip = ipAddress;
        status.port = port;
    }

    private void GameStart()
    {
        currentState = GameState.Playing;
        status.state = "Playing";

        setAcceptionEnd();
        
    }

    // ランキング出す予定
    private void GameFinish(TcpClient client, int point)
    {
        currentState = GameState.Finish;
        status.state = "Finish";

        // clientとポイントをリストにイン
        pointList.Add(new GamePoint(client, point));
        // ポイントで降順ソート
        pointList.Sort((a, b) => b.point - a.point);

        rankingSendTime = 20;
    }

    private void GameReset()
    {
        currentState = GameState.Matching;
        status.state = "Matching";

        ResetConnectionData();
        GameMatching();
        
    }

    
    private void sendRanking()
    {
        string json = "{";
        int i = 0;
        foreach(var gp in pointList){
            json += i + "{point:" + gp.point + "},";  // {0{point:4},
            i++;
        }                                             // {0{point:4},1{point:2},
        json = json.TrimEnd(',');                            // {0{point:4},1{point:2}
        json += "}";                                  // {0{point:4},1{point:2}}

        bcastMessage(new TcpClient(), json);
        pointList.Clear();
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

    public void ServerReset()
    {
        GameReset();
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