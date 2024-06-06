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
        public ushort packetId;
    }

    class PlayerInfoReq : Packet
    {
        public long playerId;
    }

    class PlayerInfoOk : Packet
    {
        public int hp;
        public int attack;
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }


    class ClientSession : PacketSession
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
            // 여기까지 왔으면 하나의 패킷이 도착한 것이다.
            // 즉 처음 2바이트는 Size, 다음 2Bytes느 Packet ID, ... 해서 하나의 패킷까지

            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            switch ((PacketID)id)
            {
                case PacketID.PlayerInfoReq:
                    {
                        long playerId = BitConverter.ToInt64(buffer.Array, buffer.Offset + count);
                        count += 8;
                        Console.WriteLine($"Player InfoReq: {playerId}");
                    }
                    break;
            }

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
}
