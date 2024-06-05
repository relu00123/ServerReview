using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class RecvBuffer
    {
        // [r][][][][][w][][][][]
        ArraySegment<byte> _buffer;
        // 실재로 read하는 것은 해당 패킷의 데이터가 모두 도착했을때이다.
        int _readPos;  // 쓰고 있는 커서
        int _writePos; // 읽고 있는 커서
        // 커서가 맨뒤로 가면 문제가 되니까 나중에 앞으로 땡겨준다. 

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize { get { return _writePos - _readPos; } }

        // FreeSize는 버퍼에 남은 공간을 의미. (여유공간)
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        // 데이터의 유효범위
        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }

        // 데이터를 쓸 수 있는 유효범위
        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clean()
        {
            int dataSize = DataSize;
            if (dataSize == 0)
            {
                // 남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
                _readPos = _writePos = 0;
            }
            else
            {
                // 남은 찌끄래기가 있으면 시작 위치로 복사
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        // 성공적으로 읽으면 커서 위치를 옮긴다.
        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
                return false;
            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
                return false;
            _writePos += numOfBytes;
            return true;
        }
    }
}
