using System.Collections.Generic;

namespace Core.Entities
{
    public class KpiOverrideRules
    {
        public string Version { get; set; } = string.Empty;
        public KpiOverrideMetadata Metadata { get; set; } = new();
        public List<KpiOverrideRule> Rules { get; set; } = new();
    }

    public class KpiOverrideMetadata
    {
        public string Description { get; set; } = string.Empty;
        public string LastUpdated { get; set; } = string.Empty;
    }

    public class KpiOverrideRule
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string Condition { get; set; } = string.Empty;
        public KpiOverrideActions Actions { get; set; } = new();
    }

    public class KpiOverrideActions
    {
        public GroupLevelChanges GroupLevelChanges { get; set; } = new();
        public List<KpiSpecificOverride> KpiSpecificOverrides { get; set; } = new();
    }

    public class GroupLevelChanges
    {
        public Dictionary<string, string> Properties { get; set; } = new();
    }

    public class KpiSpecificOverride
    {
        public string KpiName { get; set; } = string.Empty;
        public KpiOverrideProperties Properties { get; set; } = new();
    }

    public class KpiOverrideProperties
    {
        public string KpiFormula { get; set; } = string.Empty;
    }
}