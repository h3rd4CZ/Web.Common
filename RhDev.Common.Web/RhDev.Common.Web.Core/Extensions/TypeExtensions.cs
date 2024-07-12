using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsEnum(this Type type) => type.IsEnum;
    }
}
