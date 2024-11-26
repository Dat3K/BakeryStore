using System;

namespace Web.Services.Exceptions;

public class BusinessException : BaseException
{
    public BusinessException(string message) : base(message, "BUSINESS_ERROR")
    {
    }

    public BusinessException(string message, Exception innerException) 
        : base(message, innerException, "BUSINESS_ERROR")
    {
    }
}
