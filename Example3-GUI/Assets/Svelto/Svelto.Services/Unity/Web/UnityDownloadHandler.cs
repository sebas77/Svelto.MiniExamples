#if UNITY_2017_4_OR_NEWER
using System;
using System.IO;
using System.Text;
using UnityEngine.Networking;

namespace Svelto.ServiceLayer.Experimental.Unity
{
    public class UnityDownloadHandler : DownloadHandlerScript
    {
        public string GetError()
        {
            if (String.IsNullOrEmpty(_errorString) == true)
            {
                MemoryStream errorBuffer = new MemoryStream(_dataLength);

                errorBuffer.Write(data, 0, _dataLength);

                _errorString = Encoding.UTF8.GetString(errorBuffer.GetBuffer(), 0, (int) errorBuffer.Length);

                errorBuffer.Dispose();
            }

            return _errorString;
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data.Length < 1)
                return false; // No need to receive data

            _data = data;
            _dataLength = dataLength;

            if (_handler == null)
                return false; // No need to receive data

            return _handler.ReceiveData(data, dataLength);
        }

        public UnityDownloadHandler(IResponseHandler handler)
        {
            _handler = handler;
        }

        protected override byte[] GetData()
        {
            return _data;
        }

        // Called when all data has been received from the server and delivered via ReceiveData.
        protected override void CompleteContent()
        {
            _handler?.CompleteContent();
        }

        // Called when a Content-Length header is received from the server.
        protected override void ReceiveContentLength(int contentLength)
        {
            _handler?.ReceiveContentLength(contentLength);
        }

        readonly IResponseHandler _handler;
        byte[]                    _data;
        int                       _dataLength;
        string                    _errorString;
    }
}
#endif