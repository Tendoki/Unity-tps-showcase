using System;

namespace Game.Scripts.Services.Auth
{
    public sealed class RestRequestException : Exception
    {
        public long StatusCode { get; }

        public bool IsUnauthorized => StatusCode == 401;

        public RestRequestException(string message, long statusCode = 0, Exception innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
