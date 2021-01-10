﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace MultiDiceGame
{
    class Server
    {
        private static Socket server;
        private static Socket client;
        public static bool IsOpen { get; set; }
        public static string RcvMsg { get; set; }

        public static void OpenServer()
        {
            // TCP서버 소켓 생성
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // IP포트 설정
            IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 14150);

            // IP포트를 소켓에 바인딩
            server.Bind(ep);

            // 클라이언트의 연결 대기
            server.Listen(1);
        }

        public static async void AcceptClient(Form form)
        {
            // 클라이언트의 연결 수락
            client = await Task.Factory.FromAsync(server.BeginAccept, server.EndAccept, null);

            // 수락 후 게임폼 열기
            FormGame formGame = new FormGame();
            Player.User = User.Server;
            formGame.Owner = form;
            formGame.Show();
        }

        public static async void Receive(Action<string> callBack)
        {
            // 클라이언트로 부터 데이터를 받기위해 비동기로 대기
            while (true)
            {
                byte[] rcvData = new byte[1000];
                var length = await Task.Factory.FromAsync(
                    client.BeginReceive(rcvData, 0, rcvData.Length, SocketFlags.None, null, null),
                    client.EndReceive);
                RcvMsg = Encoding.UTF8.GetString(rcvData, 0, length);
                callBack(RcvMsg);
            }
        }

        public static async void SendToClient<T>(T _msg)
        {
            // 클라이언트로 데이터를 전송
            dynamic msg = _msg;
            byte[] sendData = Encoding.UTF8.GetBytes(msg);
            await Task.Factory.FromAsync(
                client.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, null, null),
                client.EndSend);
        }

        /*public static async void SendToMe<T>(T _msg)
        {
            // 클라이언트로 데이터를 전송
            dynamic msg = _msg;
            byte[] sendData = Encoding.UTF8.GetBytes(msg);
            await Task.Factory.FromAsync(
                server.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, null, null),
                server.EndSend);
        }*/

        public static void Close()
        {
            if (server != null)
                server.Close();
        }
    }
}
