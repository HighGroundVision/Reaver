using System;
using System.Runtime.Serialization;

[Serializable]
public class AccountNotLinkedException : Exception
{
    public AccountNotLinkedException()
    {
    }

    public AccountNotLinkedException(string message)
        : base(message)
    {
    }

    public AccountNotLinkedException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected AccountNotLinkedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}