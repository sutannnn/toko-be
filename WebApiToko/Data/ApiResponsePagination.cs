namespace WebApiToko.Data
{
    public class ApiResponsePagination<T>
    {
        public string statusCode { get; set; }
        public string statusDesc { get; set; }
        public int pageSize { get; set; }
        public int pageNumber { get; set; }
        public int totalData { get; set; }
        public T data { get; set; }
        public ApiResponsePagination(string code, string message, int size, int number, int total, T _data)
        {
            statusCode = code;
            statusDesc = message;
            pageSize = size;
            pageNumber = number;
            totalData = total;
            data = _data;
        }
    }
}
