using System;
using System.Linq;

namespace Ytl
{
    public static class YtlBlockExtensions
    {
        public static string GetString(this YtlBlock block, string fieldName)
        {
            var field = block.Fields.FirstOrDefault(f => f.Name == fieldName);
            if (field == null) return null;

            if (field.IsEnv && field.Value.StartsWith("$"))
            {
                string envName = field.Value.Substring(1);
                string envValue = Environment.GetEnvironmentVariable(envName);
                return envValue ?? field.Value;
            }
            return field.Value;
        }

        public static int GetInt(this YtlBlock block, string fieldName, int defaultValue = 0)
            => int.TryParse(block.GetString(fieldName), out int r) ? r : defaultValue;

        public static float GetFloat(this YtlBlock block, string fieldName, float defaultValue = 0f)
            => float.TryParse(block.GetString(fieldName), out float r) ? r : defaultValue;

        public static bool GetBool(this YtlBlock block, string fieldName, bool defaultValue = false)
        {
            var val = block.GetString(fieldName);
            if (val != null && (val == "@true" || val.ToLower() == "true")) return true;
            if (val != null && (val == "@false" || val.ToLower() == "false")) return false;
            return defaultValue;
        }
    }
}