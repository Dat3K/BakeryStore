using System;

namespace Web.Services.Exceptions;

public class BaseException : Exception
{
    public string Code { get; }

    public BaseException(string message, string code = "ERROR") : base(message)
    {
        Code = code;
    }

    public BaseException(string message, Exception innerException, string code = "ERROR") 
        : base(message, innerException)
    {
        Code = code;
    }
}
