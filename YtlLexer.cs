using System;
using System.Collections.Generic;

namespace Ytl
{
    public enum TokenType
    {
        Word, String, Number, OpenBrace, CloseBrace, OpenBracket, CloseBracket,
        Colon, Comma, NewLine, Indent, Comment, EOF
    }

    public struct Token
    {
        public TokenType Type;
        public string Value;
        public int Line;
        public int Indent;
    }

    public class YtlLexer
    {
        private readonly string _text;
        private int _pos;
        private int _line;
        private int _lineStart;

        public YtlLexer(string text)
        {
            _text = text;
            _pos = 0;
            _line = 1;
            _lineStart = 0;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_pos < _text.Length)
            {
                char c = _text[_pos];

                if (c == ' ' || c == '\t')
                {
                    int start = _pos;
                    while (_pos < _text.Length && (_text[_pos] == ' ' || _text[_pos] == '\t')) _pos++;
                    if (start == _lineStart)
                        tokens.Add(new Token { Type = TokenType.Indent, Value = _text.Substring(start, _pos - start), Line = _line });
                    continue;
                }

                if (c == '\n')
                {
                    tokens.Add(new Token { Type = TokenType.NewLine, Value = "\n", Line = _line });
                    _pos++; _line++; _lineStart = _pos; continue;
                }

                if (c == '/' && _pos + 1 < _text.Length && _text[_pos + 1] == '/')
                {
                    int start = _pos;
                    while (_pos < _text.Length && _text[_pos] != '\n') _pos++;
                    tokens.Add(new Token { Type = TokenType.Comment, Value = _text.Substring(start, _pos - start).Trim(), Line = _line });
                    continue;
                }

                if (c == '{') { tokens.Add(new Token { Type = TokenType.OpenBrace, Value = "{", Line = _line }); _pos++; continue; }
                if (c == '}') { tokens.Add(new Token { Type = TokenType.CloseBrace, Value = "}", Line = _line }); _pos++; continue; }
                if (c == '[') { tokens.Add(new Token { Type = TokenType.OpenBracket, Value = "[", Line = _line }); _pos++; continue; }
                if (c == ']') { tokens.Add(new Token { Type = TokenType.CloseBracket, Value = "]", Line = _line }); _pos++; continue; }
                if (c == ':') { tokens.Add(new Token { Type = TokenType.Colon, Value = ":", Line = _line }); _pos++; continue; }
                if (c == ',') { tokens.Add(new Token { Type = TokenType.Comma, Value = ",", Line = _line }); _pos++; continue; }

                if (c == '"')
                {
                    int start = _pos;
                    _pos++;
                    while (_pos < _text.Length && _text[_pos] != '"') _pos++;
                    if (_pos < _text.Length) _pos++;
                    tokens.Add(new Token { Type = TokenType.String, Value = _text.Substring(start, _pos - start), Line = _line });
                    continue;
                }

                if (char.IsLetterOrDigit(c) || c == '_' || c == '@' || c == '$' || c == '.' || c == '-')
                {
                    int start = _pos;
                    while (_pos < _text.Length && (char.IsLetterOrDigit(_text[_pos]) || _text[_pos] == '_' || _text[_pos] == '@' || _text[_pos] == '$' || _text[_pos] == '.' || _text[_pos] == '-')) _pos++;
                    tokens.Add(new Token { Type = TokenType.Word, Value = _text.Substring(start, _pos - start), Line = _line });
                    continue;
                }
                _pos++;
            }
            tokens.Add(new Token { Type = TokenType.EOF, Value = "", Line = _line });
            return tokens;
        }
    }
}