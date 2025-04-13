using System;
using Core.Entities;
using Infrastructure.Interfaces;

namespace Infrastructure.KpiQueryPipeline;

public class KpiDefinitionStep : IPipelineStep<KpiQueryContext>
{
    private readonly IKpiDefinitionResolver _resolver;

    public KpiDefinitionStep(IKpiDefinitionResolver resolver)
    {
        _resolver = resolver;
    }

    public KpiQueryContext Process(KpiQueryContext context)
    {
        context.KpiDefinitions = _resolver.ResolveKpiDefintions(context.KpiRequest);
        return context;
    }
}