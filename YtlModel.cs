using System.Collections.Generic;

namespace Ytl
{
    public abstract class YtlNode
    {
        public int Line { get; set; }
    }

    public class YtlField : YtlNode
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsEnv { get; set; } = false; // <-- Для поддержки $VAR
    }

    public class YtlBlock : YtlNode
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Parents { get; set; } = new List<string>();
        public List<YtlField> Fields { get; set; } = new List<YtlField>();
        public List<YtlBlock> Children { get; set; } = new List<YtlBlock>();
        public YtlBlock ParentBlock { get; set; }
    }

    public class YtlDocument
    {
        public List<YtlBlock> Blocks { get; set; } = new List<YtlBlock>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}