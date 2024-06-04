using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);


        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            // Reciev Buffer 설정 
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        // 매번 보내는 것이 아니라 차곡차곡 모았다가 한번만 보낼 것이다.
        // 누군가가 동시다발적으로 Send를 호출할 수 있기 때문에
        // Lock을 해줘야 한다. 
        public void Send(byte[] sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0) // 내가 1빠로 Send하는 경우
                    RegisterSend();
            }
        }

        // 쫓아낸다
        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both); // 듣기도 싫고 말하기도 싫다.
            _socket.Close();
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            // 뭉텅이로 보내버린다. 
            while (_sendQueue.Count > 0)
            {
                byte[] buff = _sendQueue.Dequeue();
                // Array Segment : Array의 일부, C#에는 포인터가 없어서 넣어줄 때 불편한..  (시작주소, offset, length인듯?)
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }

            _sendArgs.BufferList = _pendingList; 

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);
                         

                        if (_sendQueue.Count > 0 )
                            RegisterSend();
                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        void RegisterRecv()
        {
            // 기존에는 동기화 버전이였는데 이제는 비동기 버전으로 바꿀 것임
            bool pending = _socket.ReceiveAsync(_recvArgs);

            if (pending == false)
            {
                OnRecvCompleted(null, _recvArgs);
            }
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
              
                    RegisterRecv();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }

            else
            {
                Disconnect();
            }
            
        }

        #endregion
    }
}
