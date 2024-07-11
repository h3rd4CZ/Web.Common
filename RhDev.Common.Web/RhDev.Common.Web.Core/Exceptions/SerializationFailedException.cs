using System;

namespace RhDev.Common.Web.Core.Exceptions
{
    public class SerializationFailedException : Exception
    {
        public SerializationFailedException(Exception inner) : base("An error occured when serialization", inner)
        {

        }
    }
}
