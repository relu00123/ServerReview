using Server.Session;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class PacketHandler
{
    // 패킷이 다 조립되면 무엇을 할 것인가?'

    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
            return;

        // 모든 Client에게 들어온 메세지를 뿌려준다. 
        clientSession.Room.Broadcast(clientSession, chatPacket.chat);
    }



}

