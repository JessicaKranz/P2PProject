using CommonLogic;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace PeerToPeerCloneC
{
    class Program
    {
        static void Main(string[] args)
        {
            new Thread(o => TcpConnection.Server(13002)).Start();
            new Thread(o => TcpConnection.Server(13003)).Start();

            //Servers must be running before clients may connect
            Thread.Sleep(1000);

            List<TcpClient> tcpClients = new List<TcpClient>
            {
                new TcpClient("127.0.0.1", 13000),
                new TcpClient("127.0.0.1", 13001)
            };
            new Thread(o => TcpConnection.Client(tcpClients)).Start();
        }     
    }
}