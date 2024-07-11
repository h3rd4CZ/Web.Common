using System;

namespace RhDev.Common.Web.Core.Exceptions
{
    public class InvalidCharactersInFileNameException : Exception
    {
        public InvalidCharactersInFileNameException() : base("Entity name contains invalid characters, please rename the entity and try again.") { }
        public InvalidCharactersInFileNameException(string msg) : base(msg) { }

    }
}
