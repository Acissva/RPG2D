using System;
using System.Text;
using UnityEngine.Networking;

namespace RPG2D
{
    public sealed class LimitedDownloadHandler : DownloadHandlerScript
    {
        private readonly byte[] body;
        private int receivedCount;

        public int MaxBodyBytes { get; }
        public bool TooLarge { get; private set; }

        public LimitedDownloadHandler(int maxBodyBytes) : base(CreateReceiveBuffer(maxBodyBytes))
        {
            MaxBodyBytes = maxBodyBytes;
            body = new byte[maxBodyBytes];
        }

        private static byte[] CreateReceiveBuffer(int maxBodyBytes)
        {
            if (maxBodyBytes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBodyBytes), "响应正文上限必须大于 0。");
            }

            return new byte[4096];
        }

        protected override void ReceiveContentLengthHeader(ulong contentLength)
        {
            TooLarge = TooLarge || contentLength > (ulong)MaxBodyBytes;
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (TooLarge)
            {
                return false;
            }

            if (dataLength == 0)
            {
                return true;
            }

            if (data == null || dataLength < 0 || dataLength > data.Length)
            {
                return false;
            }

            if (dataLength > MaxBodyBytes - receivedCount)
            {
                TooLarge = true;
                return false;
            }

            //从 `data` 字节数组的开头，取出 `dataLength` 个字节，追加写入到 `body` 数组的 `receivedCount` 位置上
            Buffer.BlockCopy(data, 0, body, receivedCount, dataLength);
            receivedCount += dataLength;
            return true;
        }

        public string ReadUtf8Text()
        {
            return new UTF8Encoding(false, true).GetString(body, 0, receivedCount);
        }
    }
}
