using System.Text.Json;

namespace WebApiToko.Data
{
    public class ApiResponseError
    {
        public string statusCode { get; set; }
        public string statusDesc { get; set; }

        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
