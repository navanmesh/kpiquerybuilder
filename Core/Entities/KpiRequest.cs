namespace Core.Entities
{
    public class KpiRequest
    {
        public string[] Kpis { get; set; }
        public FilterCriteria FilterBy { get; set; }
        public string[] GroupBy { get; set; }
    }

    public class FilterCriteria
    {
        public ProductAttributes ProductAttributes { get; set; }
        public StoreAttributes StoreAttributes { get; set; }
        public TimeAttributes TimeAttributes { get; set; }
    }

    public class ProductAttributes
    {
        public int? L0Id { get; set; }
        public int? L1Id { get; set; }
        public int? L2Id { get; set; }
        public int? L3Id { get; set; }
        public int? L4Id { get; set; }
        public int? L5Id { get; set; }
        public int? L6Id { get; set; }
        public int? L7Id { get; set; }
        public int? L8Id { get; set; }
        public int? L9Id { get; set; }
    }

    public class StoreAttributes
    {
        public int? StoreId { get; set; }
    }

    public class TimeAttributes
    {
        public int? FcWeek { get; set; }
        public int? FcPeriod { get; set; }
        public TimePeriodType? TimePeriod { get; set; }
    }

    public enum TimePeriodType
    {
        Roll4,
        Roll13,
        Week26,
        Week52
    }
}