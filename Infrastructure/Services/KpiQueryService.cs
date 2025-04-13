using Core.Entities;
using Infrastructure.Interfaces;
using Scriban;
using Scriban.Runtime;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class KpiQueryService : IKpiQueryService
    {
        private readonly IKpiDefinitionResolver _kpiResolver;
        private readonly string _templatePath;
        private readonly string _dimensionsPath;
        private readonly string _timeDimensionsPath;
        private readonly string _kpisPath;

        public KpiQueryService()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _templatePath = Path.Combine(baseDir, "Templates", "Promo", "aggregate", "aggregate_kpi_template.sbn");
            _dimensionsPath = Path.Combine(baseDir, "Templates", "Promo", "aggregate", "aggregate_dimension.json");
            _timeDimensionsPath = Path.Combine(baseDir, "Templates", "Promo", "aggregate", "time_dimension.json");
            _kpisPath = Path.Combine(baseDir, "Templates", "Promo", "aggregate", "aggregate_kpis.json");
        }

        public string GenerateQuery(KpiQueryContext kpiQueryContext)
        {
        
            var context = CreateQueryContext(kpiQueryContext.KpiRequest);
            var templateContent = File.ReadAllText(_templatePath);
            var template = Template.Parse(templateContent);
            
            // Create a model object that Scriban can easily access
            var model = new ScriptObject();
            model.Add("kpis", context.Kpis);
            model.Add("dimensions", context.Dimensions);
            model.Add("group_by", context.GroupBy);
            model.Add("filters", context.Filters);
            model.Add("fromTable", context.FromTable);

            var templateContext = new TemplateContext();
            templateContext.PushGlobal(model);
            
            return template.Render(templateContext);
        }

        private string ConvertToDimensionName(string propertyName)
        {
            return propertyName
                .Replace("Id", "_id")
                .Replace("store", "store")  // preserve 'store' as is
                .Replace("Fc", "fc_")
                .Replace("Period", "period")
                .Replace("Week", "week")
                .Replace("Time", "time_")
                .Replace("Roll", "roll")
                .ToLower();
        }

        private DimensionInfo? LoadTimeDimension(string timeDimensionKey)
        {
            if (string.IsNullOrEmpty(timeDimensionKey)) return null;

            var timeDimensionsJson = File.ReadAllText(_timeDimensionsPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var timeDimensionsRoot = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, DimensionInfo>>>(timeDimensionsJson, options);
            
            return timeDimensionsRoot?["dimensions"]?.GetValueOrDefault(timeDimensionKey);
        }

        private void AddTimeDimension(KpiQueryContext context, KpiRequest request, string timeDimensionKey, DimensionInfo? dimensionInfo)
        {
            if (dimensionInfo == null) return;

            var timeAttrs = request.FilterBy?.TimeAttributes;
            if (timeAttrs == null) return;

            // Set the appropriate value based on the dimension type
            if (timeAttrs.FcWeek.HasValue && TimeDimensionHelper.IsWeeklyDimension(timeDimensionKey))
            {
                dimensionInfo.Value = timeAttrs.FcWeek.Value;
            }
            else if (timeAttrs.FcPeriod.HasValue && TimeDimensionHelper.IsPeriodDimension(timeDimensionKey))
            {
                dimensionInfo.Value = timeAttrs.FcPeriod.Value;
            }
            else if (timeAttrs.TimePeriod.HasValue && TimeDimensionHelper.IsRollingPeriod(timeDimensionKey))
            {
                dimensionInfo.Value = true; // For rolling periods, just need a non-null value
            }

            context.Dimensions[timeDimensionKey] = dimensionInfo;
        }

        private void AddDimensionsFromFilters(KpiQueryContext context, Dictionary<string, DimensionInfo> dimensions, KpiRequest request)
        {
            // Handle product attributes
            if (request.FilterBy?.ProductAttributes != null)
            {
                var props = request.FilterBy.ProductAttributes.GetType().GetProperties();
                foreach (var prop in props)
                {
                    var value = prop.GetValue(request.FilterBy.ProductAttributes);
                    if (value != null)
                    {
                        var dimensionName = ConvertToDimensionName(prop.Name);
                        if (dimensions.TryGetValue(dimensionName, out var dimensionInfo))
                        {
                            var dimension = dimensionInfo;
                            dimension.Value = value;
                            context.Dimensions[dimensionName] = dimension;
                        }
                    }
                }
            }

            // Handle store attributes
            if (request.FilterBy?.StoreAttributes != null)
            {
                var props = request.FilterBy.StoreAttributes.GetType().GetProperties();
                foreach (var prop in props)
                {
                    var value = prop.GetValue(request.FilterBy.StoreAttributes);
                    if (value != null)
                    {
                        var dimensionName = ConvertToDimensionName(prop.Name);
                        if (dimensions.TryGetValue(dimensionName, out var dimensionInfo))
                        {
                            var dimension = dimensionInfo;
                            dimension.Value = value;
                            context.Dimensions[dimensionName] = dimension;
                        }
                    }
                }
            }
        }

        private void AddGroupByDimensions(KpiQueryContext context, Dictionary<string, DimensionInfo> dimensions, KpiRequest request)
        {
            if (request.GroupBy == null) return;

            foreach (var group in request.GroupBy)
            {
                var dimensionName = ConvertToDimensionName(group);
                
                // Add dimension to context if not already present
                if (!context.Dimensions.ContainsKey(dimensionName))
                {
                    if (dimensions.TryGetValue(dimensionName, out var dimensionInfo))
                    {
                        context.Dimensions[dimensionName] = dimensionInfo;
                    }
                }

                // Always add to GroupBy if it's a valid dimension
                if (dimensions.ContainsKey(dimensionName))
                {
                    context.GroupBy.Add(dimensionName);
                }
            }
        }

        private KpiQueryContext CreateQueryContext(KpiRequest request)
        {
            var context = new KpiQueryContext();
            context.FromTable = "transactions t";

            var dimensionsJson = File.ReadAllText(_dimensionsPath);
            var kpisJson = File.ReadAllText(_kpisPath);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dimensionsRoot = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, DimensionInfo>>>(dimensionsJson, options);
            var dimensions = dimensionsRoot?["dimensions"];
            var kpis = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(kpisJson, options)?["kpis"];

            if (dimensions == null || kpis == null)
            {
                throw new InvalidOperationException("Failed to load dimensions or kpis configuration");
            }

            // Add requested kpis
            if (request.Kpis != null)
            {
                foreach (var kpi in request.Kpis)
                {
                    if (kpis.TryGetValue(kpi, out var expression))
                    {
                        context.Kpis[kpi] = new KpiInfo
                        {
                            Expression = expression,
                            Alias = kpi
                        };
                    }
                }
            }

            // Handle dimensions from filters
            AddDimensionsFromFilters(context, dimensions, request);

            // Handle time dimension
            var timeDimensionKey = TimeDimensionHelper.GetTimeDimensionKey(request);
            if (!string.IsNullOrEmpty(timeDimensionKey))
            {
                var timeDimension = LoadTimeDimension(timeDimensionKey);
                AddTimeDimension(context, request, timeDimensionKey, timeDimension);
            }

            // Handle group by dimensions (must be done after all dimensions are added)
            AddGroupByDimensions(context, dimensions, request);

            return context;
        }
    }
}