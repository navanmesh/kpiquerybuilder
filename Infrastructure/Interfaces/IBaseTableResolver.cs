using System;
using Core.Entities;

namespace Infrastructure.Interfaces;

public interface IBaseTableResolver
{
    BaseTableInfo ResolveBaseTable(KpiRequest kpiRequest, string companyId, string judgeId, List<KpiDefinition> kpiDefinitions);
}
