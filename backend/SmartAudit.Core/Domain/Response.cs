namespace SmartAudit.Core.Domain
{
    public enum ResponseStatus
    {
        Success,
        Error,
        NotFound,
        ValidationFailed,

        Forbidden,

        // Závislá služba (např. AI) není nastavená nebo dostupná
        ServiceUnavailable,
    }

    public class Response<T>
    {
        public ResponseStatus Status { get; }
        public bool IsSuccess => Status == ResponseStatus.Success;
        public string? Message { get; }
        public T? Data { get; }

        internal Response(ResponseStatus status, T? data, string? message)
        {
            Status = status;
            Data = data;
            Message = message;
        }

        public static implicit operator Response<T>(T data) => Response.Success(data);
    }

    public static class Response
    {
        public static Response<T> Success<T>(T data, string? message = null)
            => new(ResponseStatus.Success, data, message);

        public static Response<T> Error<T>(string message, ResponseStatus status = ResponseStatus.Error)
            => new(status, default, message);

        public static Response<bool> Success(string? message = null)
            => new(ResponseStatus.Success, true, message);

        public static Response<T> NotFound<T>(string? message = null, ResponseStatus status = ResponseStatus.NotFound)
            => new(status, default, message);

        public static Response<bool> Error(string message, ResponseStatus status = ResponseStatus.Error)
            => new(status, false, message);

        public static Response<T> ValidationFailed<T>(string? message = null, ResponseStatus status = ResponseStatus.ValidationFailed)
            => new(status, default, message);

        public static Response<T> Forbidden<T>(string? message = null, ResponseStatus status = ResponseStatus.Forbidden)
            => new(status, default, message);

        public static Response<T> Unavailable<T>(string message)
            => new(ResponseStatus.ServiceUnavailable, default, message);
    }
}
