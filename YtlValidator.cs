using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Ytl
{
    public static class YtlValidator
    {
        public static void ValidateDocument(YtlDocument document)
        {
            foreach (var block in document.Blocks) ValidateBlock(block, document);
        }

        private static void ValidateBlock(YtlBlock block, YtlDocument document)
        {
            foreach (var field in block.Fields) ValidateField(field, document);
            foreach (var child in block.Children) ValidateBlock(child, document);
        }

        private static void ValidateField(YtlField field, YtlDocument document)
        {
            if (field.IsEnv) return; // Пропускаем ENV

            string val = field.Value.Trim();
            if (string.IsNullOrEmpty(val) || val.StartsWith("ytl://") || val.StartsWith("http://") ||
                val.StartsWith("https://") || val.StartsWith("[") || val.StartsWith("\"")) return;

            // IPv4
            if (val.Count(c => c == '.') == 3 && !val.Contains(':') && !val.Contains('/') && !val.Any(char.IsLetter))
            {
                if (!IPAddress.TryParse(val, out _)) document.Errors.Add($"Line {field.Line}: Invalid IPv4 '{val}'");
            }
            // MAC
            else if ((val.Count(c => c == ':') == 5 || val.Count(c => c == '-') == 5) && val.Length == 17)
            {
                var parts = val.Split(val.Contains(':') ? ':' : '-');
                bool isValid = parts.All(p => p.Length == 2 && int.TryParse(p, System.Globalization.NumberStyles.HexNumber, null, out _));
                if (!isValid) document.Errors.Add($"Line {field.Line}: Invalid MAC '{val}'");
            }
            // CIDR
            else if (val.Contains('/'))
            {
                var parts = val.Split('/');
                if (parts.Length == 2 && !IPAddress.TryParse(parts[0], out _)) document.Errors.Add($"Line {field.Line}: Invalid CIDR '{val}'");
            }
            // UUID
            else if (val.Length == 36 && val.Count(c => c == '-') == 4)
            {
                if (!Regex.IsMatch(val, @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$"))
                    document.Errors.Add($"Line {field.Line}: Invalid UUID '{val}'");
            }
        }
    }
}