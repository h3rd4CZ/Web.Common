using RhDev.Common.Web.Core.Composition.Factory.Definitions;
using RhDev.Common.Web.Core.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace RhDev.Common.Web.Core.Composition.Factory.Builder
{
    public class ContainerRegistrationDefinitionLayerBuilder : IContainerRegistrationDefinitionBuilder<ContainerRegistrationLayerDefinition>
    {

        private const string LAYER_DATAACCESS_SHAREPOINT_NAME = "DataAccess.SharePoint";
        private const string LAYER_DATAACCESS_AZURE_NAME = "DataAccess.Azure";
        private const string LAYER_DATAACCESS_SHAREPOINTONLINE_NAME = "DataAccess.SharePoint.Online";
        private const string LAYER_IMPLEMENTATION_NAME = "Impl";

        private string layerName;
        private bool hasFrontend;
        private bool hasBackend;
        private string pkt;
        private Version version;
        private ContainerRegistrationDefinitionLayerBuilder(string ln) => layerName = ln;

        private string PktGenerator => string.Join(string.Empty, Assembly.GetExecutingAssembly().GetName().GetPublicKeyToken().Select(t => t.ToString("x")));
        private Version defaultVersion = new Version(1, 0, 0, 0);
        public ContainerRegistrationDefinitionLayerBuilder WithFrontendAndBackendRegistrations()
        {
            hasFrontend = true;
            hasBackend = true;
            return this;
        }

        public ContainerRegistrationDefinitionLayerBuilder WithFrontendRegistrations()
        {
            hasFrontend = true;
            return this;
        }

        public ContainerRegistrationDefinitionLayerBuilder WithBackendRegistrations()
        {
            hasBackend = true;
            return this;
        }

        public ContainerRegistrationDefinitionLayerBuilder WithPKT(string pkt)
        {
            Guard.StringNotNullOrWhiteSpace(pkt, nameof(pkt));

            this.pkt = pkt;
            return this;
        }
                
        public ContainerRegistrationDefinitionLayerBuilder WithVersion(Version version)
        {
            Guard.NotNull(version, nameof(version));

            this.version = version;
            return this;
        }

        public ContainerRegistrationDefinitionLayerBuilder WithDefaultVersion()
        {
            version = defaultVersion;
            return this;
        }

        public static ContainerRegistrationDefinitionLayerBuilder GetNativeDataAccessAzureLayer() => new ContainerRegistrationDefinitionLayerBuilder(LAYER_DATAACCESS_AZURE_NAME);
        public static ContainerRegistrationDefinitionLayerBuilder GetNativeDataAccessSharePointLayer() => new ContainerRegistrationDefinitionLayerBuilder(LAYER_DATAACCESS_SHAREPOINT_NAME);
        public static ContainerRegistrationDefinitionLayerBuilder GetNativeDataAccessSharePointOnlineLayer() => new ContainerRegistrationDefinitionLayerBuilder(LAYER_DATAACCESS_SHAREPOINTONLINE_NAME);
        public static ContainerRegistrationDefinitionLayerBuilder GetNativeImplementationLayer() => new ContainerRegistrationDefinitionLayerBuilder(LAYER_IMPLEMENTATION_NAME);
        public static ContainerRegistrationDefinitionLayerBuilder Get(string layerName) => new ContainerRegistrationDefinitionLayerBuilder(layerName);

        public ContainerRegistrationLayerDefinition Build()
        {
            return new ContainerRegistrationLayerDefinition()
            {
                LayerName = layerName,
                HasBackendRegistration = hasBackend,
                HasFrontendRegistration = hasFrontend,
                PKT = pkt,
                Version = version
            };
        }
    }
}
