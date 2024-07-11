using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Workflow
{
    public record WorkflowDocumentIdentificator(int entityId, string typeName)
    {
        public override string ToString() => $"{entityId}|{typeName}";
    }
}
