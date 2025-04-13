namespace Infrastructure.Interfaces
{
    public interface IKpiDefinitionResolver
    {
        List<KpiDefinition> ResolveKpis(KpiRequest request);
        KpiDefinition GetKpiDefinition(string kpiKey);
    }
}