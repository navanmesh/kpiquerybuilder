using System;
using Core.Entities;
using Infrastructure.Interfaces;

namespace Infrastructure.KpiQueryPipeline;

public class KpiQueryPipeline
{
    private readonly IList<IPipelineStep<KpiQueryContext>> _steps;

    public KpiQueryPipeline(
        IKpiDefinitionResolver kpiResolver,
        IBaseTableResolver baseTableResolver,
        IDimensionResolver dimensionResolver,
        IKpiQueryService queryService)
    {
        _steps = new List<IPipelineStep<KpiQueryContext>>
        {
            new KpiDefinitionStep(kpiResolver),
            new BaseTableStep(baseTableResolver),
            new DimensionStep(dimensionResolver),
            new QueryGenerationStep(queryService)
        };
    }

    public string ExecutePipeline(KpiRequest request, string companyId, string judgeId, bool isCpgUser)
    {
        var context = new KpiQueryContext
        {
            KpiRequest = request,
            companyId = companyId,
            judgeId = judgeId,
            IsCpgUser = isCpgUser
        };

        foreach (var step in _steps)
        {
            context = step.Process(context);
        }

        return context.GeneratedQuery;
    }
}
