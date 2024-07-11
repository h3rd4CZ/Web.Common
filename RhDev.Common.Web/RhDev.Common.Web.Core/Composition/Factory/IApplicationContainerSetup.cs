namespace RhDev.Common.Web.Core.Composition.Factory
{
    public interface IApplicationContainerSetup
    {
        IApplicationContainer Frontend { get; }
        IApplicationContainer Backend { get; }
    }
}
