using System;
using Core.Entities;
namespace Infrastructure.Interfaces;

public interface IKpiQueryService
{
    string GenerateQuery(KpiQueryContext kpiQueryContext);
}
