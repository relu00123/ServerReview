using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace DummyClient
{


    class Program
    {
        static void Main(string[] args)
        {
            // DNS (Domain Name System ) 
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];

            // 첫번째 인자는 ip : 식당 이름
            // 두번째 인자는 포트번호 : 식당 정문인지 후문인지
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);


            Connector connector = new Connector();

            // 마지막인자는 몇개의 Client가 접속할 것인지? 일반적인 게임이라면 1개이겠지만
            // 이것은 DummyClient로 여러개의 Client접속 상황을 체크하기위해 사용한다. 
            connector.Connect(endPoint, () => { return SessionManager.Instance.Generate(); },
                100);


            while (true)
            {
                try
                {
                    SessionManager.Instance.SendForEach();
                }

                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(250);

            }
        }
    }
}