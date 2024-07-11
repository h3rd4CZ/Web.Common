using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Impl.Notification
{
    public record struct MailTemplate(string body, string? header = default, string? footer = default);
}
