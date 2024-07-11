using System;

namespace RhDev.Common.Web.Core.Composition.Factory.Definitions
{
    public class ContainerRegistrationLayerDefinition
    {
        public string LayerName { get; set; }
        public bool HasFrontendRegistration { get; set; }
        public bool HasBackendRegistration { get; set; }
        /// <summary>
        /// Public key token
        /// </summary>
        public string PKT { get; set; }
        /// <summary>
        /// DLL Version
        /// </summary>
        public Version Version { get; set; }

        public bool IsValid() => !string.IsNullOrWhiteSpace(LayerName);

        public override string ToString() => $"{LayerName},{PKT},{Version}";
    }
}
