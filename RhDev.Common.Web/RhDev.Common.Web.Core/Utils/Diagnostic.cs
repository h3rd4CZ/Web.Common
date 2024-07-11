using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Utils
{
    public static class Diagnostic
    {
        public static string Id => System.Diagnostics.Activity.Current.RootId;
    }
}
