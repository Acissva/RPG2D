using System;
using System.Text;
using UnityEngine.Networking;

namespace RPG2D
{
    public sealed class GameServerDownloadHandler : DownloadHandlerScript
    {
        public const int MaxBodyBytes = 16 * 1024;

        private readonly byte[] body = new byte[MaxBodyBytes];
        private int receivedCount;

        public bool TooLarge { get; private set; }

        /// <summary>
        /// 构造函数
        /// base(new byte[4096])：给父类DownloadHandlerScript提供4KB的接收缓冲，减少GC
        /// Unity会用这个4KB数组作为分片接收的临时缓冲区
        /// </summary>
        public GameServerDownloadHandler() : base(new byte[4096])
        {

        }

        /// <summary>
        /// 回调：收到HTTP响应头 Content‑Length（服务端声明的内容长度）
        /// </summary>
        /// <param name="contentLength">http头里的内容长度</param>
        protected override void ReceiveContentLengthHeader(ulong contentLength)
        {
            TooLarge = TooLarge || contentLength > (ulong)MaxBodyBytes;
        }

        /// <summary>
        /// 核心回调：网络每下载到一块分片数据就调用这个方法
        /// 返回true代表继续接收；返回false终止本次下载
        /// data：父类传入的临时缓冲区（就是构造给的4KB数组）
        /// dataLength：这一次分片实际有效的字节数
        /// </summary>
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

            // Buffer.BlockCopy(源数组,源起始,目标数组,目标起始,拷贝字节数)
            Buffer.BlockCopy(data, 0, body, receivedCount, dataLength);
            receivedCount += dataLength;
            return true;
        }

        /// <summary>
        /// 将收到的二进制字节，转为UTF‑8字符串对外提供读取
        /// UTF8Encoding(false, true):
        ///  false = 不输出UTF‑8 BOM头
        ///  true = 遇到非法UTF8字节直接抛异常
        /// </summary>
        public string ReadUtf8Text()
        {
            return new UTF8Encoding(false, true).GetString(body, 0, receivedCount);
        }
    }
}
