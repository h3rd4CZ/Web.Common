using Microsoft.AspNetCore.Identity;

namespace RhDev.Common.Web.Core.Notification
{
    public abstract record class Notification(IdentityUser recipient, string? subject = default);
    public record class SmsNotification(IdentityUser recipient, string text, string? subject = default) : Notification(recipient, subject);
    public record class EmailNotification(IdentityUser recipient, string text, string? subject = default) : Notification(recipient, subject);
}
