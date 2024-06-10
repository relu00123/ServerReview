using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory;

        public void init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
        {
            // 문지기
            // 첫번째 인자 : 네트워크 주소, AddressFamily에는 Ipv4인지 Ipv6인지
            // 사용하던 그대로 들어가 있다. 
            // 두번째 인자, 세번째 인자 : tcp or udp
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            // 문지기 교육  
            // endpoint에는 식당주소 (ip) 와 정문인지 후문인지(포트번호)
            // 등의 정보가 들어있다.
            _listenSocket.Bind(endPoint);

            // 영업시작
            // 인자는 최대 대기수를 의미한다. 
            // 전화 대기열이 10명이 넘어가면 바로 컷트 (Fail)
            _listenSocket.Listen(backlog);


            // register는 문지기의 개수
            for (int i = 0; i < register; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                // EventHandler방식이다. Event방식 즉, Callback으로 전달해준다.
                // Delegate 형식이다. 
                // Client가 Connect요청이 왔다면 Callback방식으로 On AcceptCompleted가 호출될 것임.
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted); // 물고기가 물면 낚아챈다.

                // 최초로 낚시대를 던졌음 ... 
                RegisterAccept(args);
            }
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // 두번째이상의 낚시대를 던졌을때, 첫번째에 던졌던 낚시대의 내용들을 reset해야 한다.
            args.AcceptSocket = null;


            // 손님을 입장 시킨다.
            // 반환인자가 Socket인데, 대리인의 Socket을 의미한다.
            // 이제 방금 입장한 손님과 대화하고 싶으면 이 소켓을 이용해서 대화한다.
            // 이 함수는 Blocking 함수이다. Client가 입장하지 않으면 이 아래로 
            // 코드 진행이 되지 않는다. 
            // 게임을 만들때에는 Blocking 계열의 함수를 사용하는 것을 최대한 피해야 한다. 
            // 따라서 NonBlocking 함수 (비동기 함수로 바꿀 것이다) 
            //return _listenSocket.Accept();

            // 비동기 방식은 AcceptAsync
            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)   // 낚시대를 던지자 마자 잡힌것
            {
                // 지연 없이 바로 Accept가 된 것
                // 즉 극악의 확률로 열자마자 Client가 접속했다고 보면됨
                OnAcceptCompleted(null, args);
            }


        }

        // Callback 함수는 주 Thread (While문을 계속 돌고있음) 와 달리
        // 우리가 Task나 Thread를 만들지 않았는데도 자기들이 만들어서 그 쓰레드에서
        // 수행중이다. 즉 Race Condition이 일어날 수 있다. 
        // 나중에 동기화 문제가 일어날 수 있는 코드를 작성하면 Lock을 걸어야 한다. 
        void OnAcceptCompleted(Object sender, SocketAsyncEventArgs args)
        {
            // 낚시에 걸린 물고기를 통에 집어넣음 
            // 에러가 없이 모든게 잘 처리됨
            if (args.SocketError == SocketError.Success)
            {
                // 유저가 왔으면 무엇을 해야하는가? 
                // args.AccpetSocket이 유저. _onAcceptHandler에게 유저를 전달. 
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
                Console.WriteLine(args.SocketError.ToString());

            // 다음 Client를 위해 등록을 해준 것임. 
            // 낚시대를 다시 던지는 것임 
            RegisterAccept(args);
        }
    }
}
