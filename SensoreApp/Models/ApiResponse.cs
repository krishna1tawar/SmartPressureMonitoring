using System.Net;

namespace SensoreApp.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "")
            => new()
            {
                Success = true,
                Data = data,
                Message = message,
                StatusCode = HttpStatusCode.OK
            };

        public static ApiResponse<T> Fail(string message,
            HttpStatusCode code = HttpStatusCode.BadRequest)
            => new()
            {
                Success = false,
                Message = message,
                StatusCode = code
            };
    }
}