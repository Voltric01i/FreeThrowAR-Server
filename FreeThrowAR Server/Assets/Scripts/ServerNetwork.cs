﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class ServerNetwork : MonoBehaviour    
{
    private TcpListener listener;
    private List<TcpClient> clients = new List<TcpClient>();
    private bool endAcceptConnection = false;

    public void Listen(string host, int port)
    {
        Debug.Log("IPAddress is " + host);
        var ip = IPAddress.Parse(host);
        listener = new TcpListener(ip, port);
        listener.Start();
        listener.BeginAcceptSocket(DoAcceptTcpClientCallback, listener);
        Debug.Log("Wating for connection...");
    }

    // 接続処理
    private void DoAcceptTcpClientCallback(IAsyncResult ar)
    {
        //Debug.Log("Wating for connection...");
        var listener = (TcpListener) ar.AsyncState;
        var client = listener.EndAcceptTcpClient(ar);
        clients.Add(client);
        Debug.Log("Connect:" + client.Client.RemoteEndPoint);

        // 次の人を待ち受ける
        if (endAcceptConnection == false)
        {
            listener.BeginAcceptSocket(DoAcceptTcpClientCallback, listener);
        }

        var stream = client.GetStream();
        var reader = new StreamReader(stream, Encoding.UTF8);


        // 接続が切れるまで受信を繰り返す
        while (client.Connected)
        {
            while (!reader.EndOfStream)
            {
                var str = reader.ReadLine();
                OnMessage(str);

                // 受け取ったものを他のクライアントにも送信する
                foreach(var distination in clients)
                {
                    if(client != distination)
                    {
                        SendMessageToClient(str, distination);
                    }
                }
            }

            // 接続が切れてたら
            if (client.Client.Poll(1000, SelectMode.SelectRead) && (client.Client.Available == 0))
            {
                Debug.Log("Disconnect: " + client.Client.RemoteEndPoint);
                client.Close();
                clients.Remove(client);
                break;
            }

        }
    }


    protected void OnMessage(string str)
    {
        Debug.Log("MessageReceived: " + str);
    }

    // 指定した一つのclientにmsgを送る
    protected void SendMessageToClient(string msg, TcpClient client)
    {
        if(clients.Count == 0)
        {
            return;
        }

        var body = Encoding.UTF8.GetBytes(msg);
        client.GetStream().Write(body, 0, body.Length);



    }

    protected void OnApplicationQuit()
    {
        if(listener == null)
        {
            return;
        }

        if(clients.Count != 0)
        {
            foreach(var client in clients)
            {
                client.Close();
            }
        }

        listener.Stop();

    }

}