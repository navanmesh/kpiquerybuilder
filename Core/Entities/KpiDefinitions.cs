using System.Collections.Generic;

namespace Core.Entities
{
    public class KpiDefinitionIndex
    {
        public string Version { get; set; } = string.Empty;
        public KpiIndexMetadata Metadata { get; set; } = new();
        public List<KpiDefinition> Kpis { get; set; } = new();
    }

    public class KpiIndexMetadata
    {
        public string Description { get; set; } = string.Empty;
        public string LastUpdated { get; set; } = string.Empty;
    }

    public class KpiDefinition
    {
        public string KpiName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public KpiMetadata Metadata { get; set; } = new();
        public string KpiFormula { get; set; } = string.Empty;
        public string KpiGroup { get; set; } = string.Empty;
        public string KpiTemplate { get; set; } = string.Empty;
        public string DefaultBaseTableSchema { get; set; } = string.Empty;
        public string DefaultBaseTable { get; set; } = string.Empty;
        public ValidationRules ValidationRules { get; set; } = new();
    }

    public class KpiMetadata
    {
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class ValidationRules
    {
        public List<string> AllowedFilters { get; set; } = new();
        public List<string> RequiredFilters { get; set; } = new();
    }
}