using System;

namespace RhDev.Common.Web.Core
{
    public class EntityNotFoundException : Exception
    {
        public string Name { get; private set; }

        public EntityNotFoundException() : this("[Unknown]") { }

        public EntityNotFoundException(string name)
            : base()
        {
            Name = name;
        }

        public EntityNotFoundException(string name, string message)
            : base(message)
        {
            Name = name;
        }

        public EntityNotFoundException(string name, Exception inner)
            : base(string.Empty, inner)
        {
            Name = name;
        }

        public EntityNotFoundException(string name, string message, Exception inner)
            : base(message, inner)
        {
            Name = name;
        }
    }
}
