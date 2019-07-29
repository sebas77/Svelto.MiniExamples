#if UNITY_2017_4_OR_NEWER
using System.IO;
using System.Text;
using UnityEngine;

namespace Svelto.ServiceLayer.Experimental.Unity
{
    public class JsonResponseHandler<ResponseType>: IResponseHandler<ResponseType>
    {
        public ResponseType response { get; private set; }

        public bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || data.Length < 1)
            {
                return false;
            }

            if (_buffer == null)
                _buffer = new MemoryStream(dataLength);

            _buffer.Write(data, 0, dataLength);

            return true;
        }

        public void CompleteContent()
        {
            if (_buffer == null)
                response = default;
            else
            {
                try
                {
                    var utf8String = Encoding.UTF8.GetString(_buffer.GetBuffer(), 0, (int) _buffer.Length);
                    response = JsonUtility.FromJson<ResponseType>(utf8String);
                }
                finally
                {
                    _buffer.Dispose();
                    _buffer = null;
                }
            }
        }

        public void ReceiveContentLength(int contentLength)
        {
            if (_buffer == null)
                _buffer = new MemoryStream(contentLength);
            else
            if (_buffer.Capacity < contentLength)
                _buffer.Capacity = contentLength;
        }

        MemoryStream _buffer;
    }
}
#endif