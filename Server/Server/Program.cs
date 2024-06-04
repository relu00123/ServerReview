using System.Net;
using System.Text;
using System;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using ServerCore;

namespace Server
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected bytes: {endPoint}");

            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !");
            Send(sendBuff);

            Thread.Sleep(1000);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected bytes: {endPoint}");
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {recvData}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }

    class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            // DNS (Domain Name System ) 
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];

            // 첫번째 인자는 ip : 식당 이름
            // 두번째 인자는 포트번호 : 식당 정문인지 후문인지
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.init(endPoint, () => { return new GameSession(); });

            while (true)
            {

            }

        }
    }
}