using RhDev.Common.Web.Core.Composition;
using System;
using System.Collections.Generic;

namespace RhDev.Common.Web.Core.Composition.Factory.Definitions
{
    public interface IContainerRegistrationDefinition : IService
    {
        IEnumerable<Type> BuildTypes();
    }
}
