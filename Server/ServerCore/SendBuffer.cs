using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 4096;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (CurrentBuffer.Value == null)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            if (CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    // 다루는 강좌에서 SendBuffer는 재사용을 하지 않는다. 만약 FreeSize를 초과하는 만큼을
    // 요구하면 새로 하나 만들어서 준다.
    // 이것에 대해 최적화를 하려면 Pool을 만들어서 할수는 있을 것이지만 강좌에서는 다루지 않음을 명시하였음.
    public class SendBuffer
    {

        // [u][][][][][][][][][]
        byte[] _buffer;
        int _usedSize = 0; // RecvBuffer에서 WriteCursor역할에 해당

        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }

        // 얼마만큼 최대치로 사용할 것인지 예약을 해주는 것이 매개변수
        // 예약을 할 수 없다면 null을 return
        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > FreeSize)
                return null;

            // 예약한 영역을 반환 - 예약한 영역만큼을 다 사용한다고 보장할 수는 없음
            // 그냥 큰 파이를 미리 할당받고 야금야금 쓰는 것임
            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        // 다 쓴 다음에는 사용한 사이즈를 넣어준다. 
        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }


    }
}
