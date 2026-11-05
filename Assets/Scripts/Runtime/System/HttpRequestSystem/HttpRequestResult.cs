namespace RPG2D
{
    public sealed class HttpRequestResult
    {
        //创建 / 接收 / 解码阶段的失败类别
        public HttpRequestFailure Failure { get; }

        //实际 HTTP 状态，未取得响应时通常为 0
        public long StatusCode { get; }

        //现有业务实际需要的 Content-Type 响应头，可能为空
        public string ContentType { get; }

        //经过字节限制和严格 UTF-8 读取的原始正文，可能为空；业务层负责 JSON 解析
        public string Body { get; }

        //技术诊断信息，业务层决定日志与界面文案
        public string Error { get; }

        public HttpRequestResult(HttpRequestFailure failure, long statusCode, string contentType, string body, string error)
        {
            Failure = failure;
            StatusCode = statusCode;
            ContentType = contentType ?? string.Empty;
            Body = body ?? string.Empty;
            Error = error ?? string.Empty;
        }
    }
}
