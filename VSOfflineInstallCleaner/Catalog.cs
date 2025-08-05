using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSOfflineInstallCleaner
{
    public class Catalog
    {
        [JsonProperty("manifestVersion")]
        public string ManifestVersion { get; set; }

        [JsonProperty("engineVersion")]
        public string EngineVersion { get; set; }

        [JsonProperty("info")]
        public Info Info { get; set; }

        [JsonProperty("packages")]
        public List<Package> Packages { get; set; }
    }


    public partial class Info
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("buildBranch")]
        public string BuildBranch { get; set; }

        [JsonProperty("buildVersion")]
        public string BuildVersion { get; set; }

        [JsonProperty("localBuild")]
        public string LocalBuild { get; set; }

        [JsonProperty("manifestName")]
        public string ManifestName { get; set; }

        [JsonProperty("manifestType")]
        public string ManifestType { get; set; }

        [JsonProperty("productDisplayVersion")]
        public string ProductDisplayVersion { get; set; }

        [JsonProperty("productLine")]
        public string ProductLine { get; set; }

        [JsonProperty("productLineVersion")]
        public string ProductLineVersion { get; set; }

        [JsonProperty("productMilestone")]
        public string ProductMilestone { get; set; }

        [JsonProperty("productMilestoneIsPreRelease")]
        public string ProductMilestoneIsPreRelease { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("productPatchVersion")]
        public string ProductPatchVersion { get; set; }

        [JsonProperty("productPreReleaseMilestoneSuffix")]
        public string ProductPreReleaseMilestoneSuffix { get; set; }

        [JsonProperty("productRelease")]
        public string ProductRelease { get; set; }

        [JsonProperty("productSemanticVersion")]
        public string ProductSemanticVersion { get; set; }
    }

    public partial class Package
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("chip")]
        public string Chip { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("machineArch")]
        public string MachineArch { get; set; }

        [JsonProperty("productArch")]
        public string ProductArch { get; set; }
    }
}
