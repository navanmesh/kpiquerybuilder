using System.Text.Json;
using Infrastructure.Interfaces;
using Scriban;
using Scriban.Runtime;

namespace Infrastructure.Services
{
    public class KpiDefinitionResolver : IKpiDefinitionResolver
    {
        private readonly string _kpiIndexPath;
        private readonly string _baseTemplatePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public KpiDefinitionResolver()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _kpiIndexPath = Path.Combine(baseDir, "Templates", "Promo", "kpi_index.json");
            _baseTemplatePath = Path.Combine(baseDir, "Templates", "Promo");
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public List<KpiDefinition> ResolveKpiDefintions(KpiRequest request)
        {
            ValidateRequest(request);
            var kpiIndex = LoadKpiIndex();
            var processedKpis = ProcessRequestedKpis(request, kpiIndex);
            ValidateProcessedKpis(processedKpis);
            return processedKpis;
        }

        private void ValidateRequest(KpiRequest request)
        {
            if (request.Kpis == null || !request.Kpis.Any())
            {
                throw new ArgumentException("At least one kpi must be specified in the request.");
            }
        }

        private KpiDefinitionIndex LoadKpiIndex()
        {
            var content = ReadJsonFile(_kpiIndexPath);
            var kpiIndex = DeserializeJson<KpiDefinitionIndex>(content);

            if (kpiIndex == null || !kpiIndex.Kpis.Any())
            {
                throw new InvalidOperationException("Failed to load kpis from the kpi index file.");
            }

            return kpiIndex;
        }

        private List<KpiDefinition> ProcessRequestedKpis(KpiRequest request, KpiDefinitionIndex kpiIndex)
        {
            var processedKpis = new List<KpiDefinition>();
            foreach (var kpiName in request.Kpis)
            {
                var kpi = kpiIndex.Kpis.FirstOrDefault(m => 
                    m.KpiName.Equals(kpiName, StringComparison.OrdinalIgnoreCase));
                    
                if (kpi != null)
                {
                    ApplyOverrideRules(kpi, request);
                    processedKpis.Add(kpi);
                }
            }
            return processedKpis;
        }

        private void ValidateProcessedKpis(List<KpiDefinition> processedKpis)
        {
            if (!processedKpis.Any())
            {
                throw new KeyNotFoundException($"None of the requested kpis were found in the kpi index.");
            }
        }

        private void ApplyOverrideRules(KpiDefinition kpi, KpiRequest request)
        {
            var overrideRules = LoadOverrideRules(kpi);
            if (overrideRules?.Rules == null)
                return;

            foreach (var rule in overrideRules.Rules)
            {
                if (IsRuleMatch(rule, request))
                {
                    ApplyGroupLevelChanges(kpi, rule);
                    ApplyKpiSpecificOverrides(kpi, rule);
                }
            }
        }

        private KpiOverrideRules? LoadOverrideRules(KpiDefinition kpi)
        {
            var kpiGroupPath = Path.Combine(_baseTemplatePath, kpi.KpiGroup ?? string.Empty, "kpi_override_rules.json");
            if (!File.Exists(kpiGroupPath))
                return null;

            var content = ReadJsonFile(kpiGroupPath);
            return DeserializeJson<KpiOverrideRules>(content);
        }

        private bool IsRuleMatch(KpiOverrideRule rule, KpiRequest request)
        {
            var template = Template.Parse(rule.Condition);
            var templateContext = new TemplateContext();
            templateContext.PushGlobal(new ScriptObject { ["filterBy"] = request.FilterBy });

            var result = template.Render(templateContext);
            return bool.TryParse(result, out var isMatch) && isMatch;
        }

        private void ApplyGroupLevelChanges(KpiDefinition kpi, KpiOverrideRule rule)
        {
            if (rule.Actions.GroupLevelChanges?.Properties != null)
            {
                var groupProps = rule.Actions.GroupLevelChanges.Properties
                    .ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
                kpi.SetNestedProperties(groupProps);
            }
        }

        private void ApplyKpiSpecificOverrides(KpiDefinition kpi, KpiOverrideRule rule)
        {
            var kpiOverrides = rule.Actions.KpiSpecificOverrides?
                .Where(m => m.KpiName.Equals(kpi.KpiName, StringComparison.OrdinalIgnoreCase));

            if (kpiOverrides == null)
                return;

            foreach (var kpiOverride in kpiOverrides)
            {
                if (kpiOverride.Properties != null)
                {
                    var overrideProps = SerializeToDict(kpiOverride.Properties);
                    kpi.SetNestedProperties(overrideProps);
                }
            }
        }

        private string ReadJsonFile(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read file {filePath}: {ex.Message}", ex);
            }
        }

        private T? DeserializeJson<T>(string content)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        private Dictionary<string, object> SerializeToDict<T>(T obj)
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(
                JsonSerializer.Serialize(obj, _jsonOptions),
                _jsonOptions) ?? new Dictionary<string, object>();
        }
    }
}