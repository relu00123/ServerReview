using System.Net;
using System.Text;
using System;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using ServerCore;
using Server.Session;

namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();
        public static GameRoom Room = new GameRoom();

        static void FlushRoom()
        {
            Room.Push(() => Room.Flush());
            JobTimer.Instance.Push(FlushRoom, 250);
        }

        static void Main(string[] args)
        {


            // DNS (Domain Name System ) 
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];

            // 첫번째 인자는 ip : 식당 이름
            // 두번째 인자는 포트번호 : 식당 정문인지 후문인지
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening...");


            // 재귀적으로 호출 
            //FlushRoom();
            // 아래와 위는 같은 뜻이다.
            JobTimer.Instance.Push(FlushRoom);


            while (true)
            {
                JobTimer.Instance.Flush();
            }
        }
    }
}