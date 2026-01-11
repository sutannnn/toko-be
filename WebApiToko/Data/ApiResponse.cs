namespace WebApiToko.Data
{
    public class ApiResponse<T>
    {
        public string statusCode { get; set; }
        public string statusDescHeading { get; set; }
        public string statusDesc { get; set; }
        public T data { get; set; }

        public ApiResponse(string code, string message, T _data)
        {
            statusCode = code;
            statusDesc = message;
            data = _data;
        }
        public ApiResponse(string code, string statusDescHead, string message)
        {
            statusCode = code;
            statusDescHeading = statusDescHead;
            statusDesc = message;
        }
        public ApiResponse(string code, string statusDescHead, string message, T _data)
        {
            statusCode = code;
            statusDescHeading = statusDescHead;
            statusDesc = message;
            data = _data;
        }
    }
}
