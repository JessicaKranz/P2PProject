using CommonLogic;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace PeerToPeerCloneA
{
    class Program
    {
        static void Main(string[] args)
        {
            new Thread(o => TcpConnection.Server(13001)).Start();

            //Servers must be running before clients may connect
            Thread.Sleep(1000);

            List<TcpClient> tcpClients = new List<TcpClient>
            {
                new TcpClient("127.0.0.1", 13002)
            };
            new Thread(o => TcpConnection.Client(tcpClients)).Start();
        }
    }
}
