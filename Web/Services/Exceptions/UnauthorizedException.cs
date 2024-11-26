using System;

namespace Web.Services.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message) : base(message, "UNAUTHORIZED")
    {
    }

    public UnauthorizedException(string message, Exception innerException) 
        : base(message, innerException, "UNAUTHORIZED")
    {
    }
}
