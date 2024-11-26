using System;

namespace Web.Services.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string message) : base(message, "NOT_FOUND")
    {
    }

    public NotFoundException(string message, Exception innerException) 
        : base(message, innerException, "NOT_FOUND")
    {
    }
}
