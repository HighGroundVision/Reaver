using System;
using System.Runtime.Serialization;

[Serializable]
public class UserFriendlyException : Exception
{
    public UserFriendlyException()
    {
    }

    public UserFriendlyException(string message)
        : base(message)
    {
    }

    public UserFriendlyException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected UserFriendlyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}