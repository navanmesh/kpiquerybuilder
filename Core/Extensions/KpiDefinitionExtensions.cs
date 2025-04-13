using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Entities;

namespace Core.Extensions
{
    public static class KpiDefinitionExtensions
    {
        public static void SetProperties(this KpiDefinition kpi, Dictionary<string, object> properties)
        {
            if (properties == null) return;

            var type = typeof(KpiDefinition);
            foreach (var property in properties)
            {
                var propInfo = type.GetProperty(property.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (propInfo != null && propInfo.CanWrite)
                {
                    try
                    {
                        var value = Convert.ChangeType(property.Value, propInfo.PropertyType);
                        propInfo.SetValue(kpi, value);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Failed to set property {property.Key}: {ex.Message}");
                    }
                }
            }
        }

        public static void SetNestedProperties(this KpiDefinition kpi, Dictionary<string, object> properties)
        {
            if (properties == null) return;

            foreach (var property in properties)
            {
                var propertyParts = property.Key.Split('.');
                if (propertyParts.Length == 1)
                {
                    SetSingleProperty(kpi, property.Key, property.Value);
                }
                else
                {
                    SetNestedProperty(kpi, propertyParts, property.Value);
                }
            }
        }

        private static void SetSingleProperty(object target, string propertyName, object value)
        {
            var propInfo = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (propInfo?.CanWrite == true)
            {
                try
                {
                    var convertedValue = Convert.ChangeType(value, propInfo.PropertyType);
                    propInfo.SetValue(target, convertedValue);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Failed to set property {propertyName}: {ex.Message}");
                }
            }
        }

        private static void SetNestedProperty(object target, string[] propertyPath, object value)
        {
            object current = target;
            for (int i = 0; i < propertyPath.Length - 1; i++)
            {
                var propInfo = current.GetType().GetProperty(propertyPath[i], 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (propInfo == null)
                    return;

                var next = propInfo.GetValue(current);
                if (next == null)
                {
                    next = Activator.CreateInstance(propInfo.PropertyType);
                    propInfo.SetValue(current, next);
                }
                current = next;
            }

            SetSingleProperty(current, propertyPath[^1], value);
        }
    }
}