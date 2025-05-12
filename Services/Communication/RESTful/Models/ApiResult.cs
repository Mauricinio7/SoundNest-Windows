using System.Net;

namespace Services.Communication.RESTful.Models
{
    public class ApiResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; } 
        public string? Message { get; set; }     
        public HttpStatusCode? StatusCode { get; set; }

        public static ApiResult<T> Success(T data, string? message = null, HttpStatusCode code = HttpStatusCode.OK) => new()
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            StatusCode = code
        };

        public static ApiResult<T> Failure(string error, string? userMessage = null, HttpStatusCode? code = null) => new()
        {
            IsSuccess = false,
            ErrorMessage = error,
            Message = userMessage,
            StatusCode = code
        };
    }
}
