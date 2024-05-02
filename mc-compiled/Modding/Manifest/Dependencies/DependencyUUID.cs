using System;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Manifest.Dependencies
{
    /// <summary>
    /// Dependency that references another BP/RP by UUID.
    /// </summary>
    public sealed class DependencyUUID : Dependency
    {
        /// <summary>
        /// The Guid of the pack this dependency references.
        /// </summary>
        internal readonly Guid dependsOnUUID;
        
        public DependencyUUID(Guid dependsOnUuid, ManifestVersion version) : base(version)
        {
            this.dependsOnUUID = dependsOnUuid;
        }
        public DependencyUUID(Manifest dependsOnPack, ManifestVersion version) : base(version)
        {
            this.dependsOnUUID = dependsOnPack.header.uuid;
        }
        
        public override JObject ToJSON()
        {
            return new JObject()
            {
                ["uuid"] = this.dependsOnUUID.ToString(),
                ["version"] = this.version.ToJSON()
            };
        }
    }
}