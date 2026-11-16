using System;
using System.Collections.Generic;
using System.Linq;

namespace Ytl
{
    public class YtlParser
    {
        private readonly List<Token> _tokens;
        private int _pos;
        private int _currentIndent;
        private Stack<YtlBlock> _blockStack;
        private YtlDocument _document;
        private Dictionary<string, string> _externalFiles;

        public YtlParser(string text, Dictionary<string, string> externalFiles = null)
        {
            var lexer = new YtlLexer(text);
            _tokens = lexer.Tokenize();
            _pos = 0;
            _currentIndent = 0;
            _blockStack = new Stack<YtlBlock>();
            _document = new YtlDocument();
            _externalFiles = externalFiles ?? new Dictionary<string, string>();
        }

        public YtlDocument Parse()
        {
            // 1. Строим дерево
            while (_pos < _tokens.Count)
            {
                var token = _tokens[_pos];
                if (token.Type == TokenType.EOF) break;
                if (token.Type == TokenType.Indent) { _currentIndent = token.Value.Length; _pos++; continue; }
                if (token.Type == TokenType.NewLine || token.Type == TokenType.Comment) { _pos++; continue; }
                if (token.Type == TokenType.Word || token.Type == TokenType.String) ParseLine();
                else _pos++;
            }

            // 2. Применяем Include
            ProcessIncludes();

            // 3. Применяем наследование и Realm
            ResolveInheritance();

            // 4. Валидация данных
            YtlValidator.ValidateDocument(_document);

            return _document;
        }

        private void ParseLine()
        {
            var firstToken = _tokens[_pos];
            _pos++;

            var parents = new List<string>();
            if (PeekType() == TokenType.OpenBrace)
            {
                _pos++;
                while (_pos < _tokens.Count && PeekType() != TokenType.CloseBrace)
                {
                    if (PeekType() == TokenType.Word) parents.Add(_tokens[_pos].Value);
                    _pos++;
                }
                _pos++;
            }

            if (PeekType() == TokenType.Colon)
            {
                _pos++;
                var block = new YtlBlock { Name = firstToken.Value, Line = firstToken.Line };
                block.Parents.AddRange(parents);

                while (_blockStack.Count > 0 && _currentIndent <= _blockStack.Peek().Line) _blockStack.Pop();

                if (_blockStack.Count > 0)
                {
                    var parent = _blockStack.Peek();
                    block.ParentBlock = parent;
                    parent.Children.Add(block);
                }
                else _document.Blocks.Add(block);

                _blockStack.Push(block);
                return;
            }

            var field = new YtlField { Name = firstToken.Value, Line = firstToken.Line };
            while (_pos < _tokens.Count && PeekType() != TokenType.NewLine && PeekType() != TokenType.EOF)
            {
                var t = _tokens[_pos];
                if (t.Type == TokenType.Word || t.Type == TokenType.String || t.Type == TokenType.Number)
                    field.Value += (field.Value.Length > 0 ? " " : "") + t.Value;
                _pos++;
            }

            if (field.Value.StartsWith("$")) field.IsEnv = true;

            if (_blockStack.Count > 0) _blockStack.Peek().Fields.Add(field);
            else _document.Errors.Add($"Line {firstToken.Line}: Field '{field.Name}' outside of a block.");
        }

        private TokenType PeekType() => _pos < _tokens.Count ? _tokens[_pos].Type : TokenType.EOF;

        private void ProcessIncludes()
        {
            var includedBlocks = new List<YtlBlock>();
            foreach (var block in _document.Blocks.ToList())
            {
                foreach (var field in block.Fields.ToList())
                {
                    if (field.Name == "Include" && field.Value.StartsWith("ytl://"))
                    {
                        string fileName = field.Value.Replace("ytl://", "");
                        if (_externalFiles.TryGetValue(fileName, out string includedText))
                        {
                            var includedDoc = new YtlParser(includedText).Parse();
                            includedBlocks.AddRange(includedDoc.Blocks);
                        }
                    }
                }
            }
            foreach (var incBlock in includedBlocks)
            {
                if (!_document.Blocks.Any(b => b.Name == incBlock.Name)) _document.Blocks.Add(incBlock);
            }
        }

        private void ResolveInheritance()
        {
            var allBlocks = new Dictionary<string, YtlBlock>();
            foreach (var block in _document.Blocks) CollectBlocks(block, allBlocks);
            foreach (var block in allBlocks.Values) ResolveBlock(block, allBlocks);
        }

        private void CollectBlocks(YtlBlock block, Dictionary<string, YtlBlock> allBlocks)
        {
            if (!allBlocks.ContainsKey(block.Name)) allBlocks[block.Name] = block;
            foreach (var child in block.Children) CollectBlocks(child, allBlocks);
        }

        private void ResolveBlock(YtlBlock block, Dictionary<string, YtlBlock> allBlocks)
        {
            foreach (var parentName in block.Parents)
            {
                if (allBlocks.TryGetValue(parentName, out var parentBlock))
                {
                    foreach (var parentField in parentBlock.Fields)
                    {
                        if (!block.Fields.Any(f => f.Name == parentField.Name))
                        {
                            block.Fields.Add(new YtlField { Name = parentField.Name, Value = parentField.Value, Line = parentField.Line });
                        }
                    }
                }
            }

            foreach (var realm in block.Children)
            {
                if (realm.Name.StartsWith("Realm"))
                {
                    foreach (var realmField in realm.Fields)
                    {
                        var existing = block.Fields.FirstOrDefault(f => f.Name == realmField.Name);
                        if (existing != null) existing.Value = realmField.Value;
                        else block.Fields.Add(realmField);
                    }
                }
            }
        }
    }
}