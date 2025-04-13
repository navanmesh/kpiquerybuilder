using System.Collections.Generic;

namespace Core.Entities
{
    public class KpiQueryContext
    {
        public Dictionary<string, KpiInfo> Kpis { get; set; } = new();
        public Dictionary<string, DimensionInfo> Dimensions { get; set; } = new();
        public List<string> GroupBy { get; set; } = new();
        public Dictionary<string, object> Filters { get; set; } = new();
        public string FromTable { get; set; } = "transactions t";
    }

    public class KpiInfo
    {
        public string Expression { get; set; }
        public string Alias { get; set; }
    }

    public class DimensionInfo
    {
        public string Type { get; set; }
        public string Select { get; set; }
        public string Where { get; set; }
        public string Join { get; set; }
        public string GroupBy { get; set; }
        public object Value { get; set; }
    }
}