using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Session;
using ServerCore;

namespace Server
{
    class GameRoom : IJobQueue
    {
        // 멀티쓰레드 환경에서 List Add, Remove는 정상적으로 작동하지 않는다.
        // 여기에 작성되는 코드들은 멀티 쓰레드 환경에서 돌아가는 코드들이다. 
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }


        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat}  I am {packet.playerId}";

            ArraySegment<byte> segment = packet.Write();

            foreach (ClientSession s in _sessions)
                s.Send(segment);
        }


        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
        }
    }
}
