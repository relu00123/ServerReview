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
    class Packet
    {
        public ushort size;

        // Packet ID만 보고 얼만큼의 크기인지는 파악하기 힘듬. 따라서 Size가 필요
        public ushort packetId;
    }

    class LoginOkPacket : Packet
    {

    }

    class GameSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected bytes: {endPoint}");

            Packet packet = new Packet() { size = 100, packetId = 10 };

            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            //byte[] buffer = BitConverter.GetBytes(packet.size);
            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
            //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);


            // 100명
            // 1 -> 이동패킷이 100명
            // 100명이라면 이동패킷이 100 * 100  100'000;
            // Send(sendBuff);
            Thread.Sleep(5000);
            Disconnect();
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            // 여기까지 왔으면 유효한 범위가 온다.
            // 즉 처음 2바이트는 Size, 다음 2Bytes느 Packet ID, ... 해서 하나의 패킷까지
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);
            Console.WriteLine($"RecvPacketId: {id}, Size {size}");

        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected bytes: {endPoint}");
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