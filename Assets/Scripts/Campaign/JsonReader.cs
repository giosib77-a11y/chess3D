using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Chess.Campaign
{
    // Minimal RFC 8259 reader. Values: object -> List<KeyValuePair<string, object>> (duplicates preserved),
    // array -> List<object>, string, long (integer literal), double (other numbers), bool, null.
    // Iterative (explicit stack), max nesting depth 64. Every error -> CampaignSaveFormatException.
    internal sealed class JsonReader
    {
        public const int MaxDepth = 64;

        private sealed class Frame
        {
            public readonly List<object> Array;
            public readonly List<KeyValuePair<string, object>> Object;
            public string Key;

            public Frame(bool isObject)
            {
                if (isObject)
                {
                    Object = new List<KeyValuePair<string, object>>();
                }
                else
                {
                    Array = new List<object>();
                }
            }

            public bool IsObject
            {
                get { return Object != null; }
            }

            public object Container
            {
                get { return IsObject ? (object)Object : Array; }
            }
        }

        private readonly string _text;
        private int _pos;

        private JsonReader(string text)
        {
            _text = text;
        }

        public static object Parse(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var reader = new JsonReader(text);
            object value = reader.ParseDocument();
            reader.SkipWhitespace();
            if (reader._pos < text.Length)
            {
                throw reader.Error("unexpected text after the JSON value");
            }

            return value;
        }

        private CampaignSaveFormatException Error(string reason)
        {
            return new CampaignSaveFormatException(reason + " at position " + _pos.ToString(CultureInfo.InvariantCulture));
        }

        private void SkipWhitespace()
        {
            while (_pos < _text.Length)
            {
                char c = _text[_pos];
                if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                {
                    _pos++;
                }
                else
                {
                    break;
                }
            }
        }

        private char Peek()
        {
            SkipWhitespace();
            if (_pos >= _text.Length)
            {
                throw Error("unexpected end of input");
            }

            return _text[_pos];
        }

        private object ParseDocument()
        {
            var stack = new Stack<Frame>();

            while (true)
            {
                object value;
                char c = Peek();

                if (c == '{' || c == '[')
                {
                    if (stack.Count >= MaxDepth)
                    {
                        throw Error("nesting is deeper than " + MaxDepth.ToString(CultureInfo.InvariantCulture));
                    }

                    _pos++;
                    var frame = new Frame(c == '{');
                    char close = c == '{' ? '}' : ']';
                    if (Peek() == close)
                    {
                        _pos++;
                        value = frame.Container;
                    }
                    else
                    {
                        if (frame.IsObject)
                        {
                            frame.Key = ReadKey();
                        }

                        stack.Push(frame);
                        continue;
                    }
                }
                else
                {
                    value = ReadScalar();
                }

                // Attach the finished value to its parent; close parents as far as possible.
                while (true)
                {
                    if (stack.Count == 0)
                    {
                        return value;
                    }

                    Frame top = stack.Peek();
                    if (top.IsObject)
                    {
                        top.Object.Add(new KeyValuePair<string, object>(top.Key, value));
                    }
                    else
                    {
                        top.Array.Add(value);
                    }

                    char next = Peek();
                    _pos++;
                    if (next == ',')
                    {
                        if (top.IsObject)
                        {
                            top.Key = ReadKey();
                        }

                        break;
                    }

                    if (next == (top.IsObject ? '}' : ']'))
                    {
                        stack.Pop();
                        value = top.Container;
                        continue;
                    }

                    _pos--;
                    throw Error("expected ',' or closing bracket");
                }
            }
        }

        private string ReadKey()
        {
            if (Peek() != '"')
            {
                throw Error("expected a string key");
            }

            string key = ReadString();
            if (Peek() != ':')
            {
                throw Error("expected ':'");
            }

            _pos++;
            return key;
        }

        private object ReadScalar()
        {
            char c = Peek();
            if (c == '"')
            {
                return ReadString();
            }

            if (c == '-' || (c >= '0' && c <= '9'))
            {
                return ReadNumber();
            }

            if (ReadLiteral("true"))
            {
                return true;
            }

            if (ReadLiteral("false"))
            {
                return false;
            }

            if (ReadLiteral("null"))
            {
                return null;
            }

            throw Error("unexpected character '" + c + "'");
        }

        private bool ReadLiteral(string literal)
        {
            if (string.CompareOrdinal(_text, _pos, literal, 0, literal.Length) == 0)
            {
                _pos += literal.Length;
                return true;
            }

            return false;
        }

        private string ReadString()
        {
            _pos++; // opening quote
            var sb = new StringBuilder();
            while (true)
            {
                if (_pos >= _text.Length)
                {
                    throw Error("unterminated string");
                }

                char c = _text[_pos++];
                if (c == '"')
                {
                    return sb.ToString();
                }

                if (c < 0x20)
                {
                    _pos--;
                    throw Error("control character in string");
                }

                if (c != '\\')
                {
                    sb.Append(c);
                    continue;
                }

                if (_pos >= _text.Length)
                {
                    throw Error("unterminated string");
                }

                char e = _text[_pos++];
                switch (e)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        if (_pos + 4 > _text.Length)
                        {
                            throw Error("incomplete \\u escape");
                        }

                        int code = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            int digit = HexValue(_text[_pos + i]);
                            if (digit < 0)
                            {
                                throw Error("invalid \\u escape");
                            }

                            code = (code << 4) | digit;
                        }

                        _pos += 4;
                        sb.Append((char)code);
                        break;
                    default:
                        _pos--;
                        throw Error("invalid escape '\\" + e + "'");
                }
            }
        }

        private static int HexValue(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            return -1;
        }

        private object ReadNumber()
        {
            int start = _pos;
            bool isInteger = true;

            if (_text[_pos] == '-')
            {
                _pos++;
            }

            if (_pos >= _text.Length)
            {
                throw Error("invalid number");
            }

            if (_text[_pos] == '0')
            {
                _pos++;
            }
            else if (_text[_pos] >= '1' && _text[_pos] <= '9')
            {
                SkipDigits();
            }
            else
            {
                throw Error("invalid number");
            }

            if (_pos < _text.Length && _text[_pos] == '.')
            {
                isInteger = false;
                _pos++;
                RequireDigits();
            }

            if (_pos < _text.Length && (_text[_pos] == 'e' || _text[_pos] == 'E'))
            {
                isInteger = false;
                _pos++;
                if (_pos < _text.Length && (_text[_pos] == '+' || _text[_pos] == '-'))
                {
                    _pos++;
                }

                RequireDigits();
            }

            string literal = _text.Substring(start, _pos - start);
            if (isInteger)
            {
                long l;
                if (long.TryParse(literal, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out l))
                {
                    return l;
                }
            }

            double d;
            if (!double.TryParse(literal, NumberStyles.Float, CultureInfo.InvariantCulture, out d)
                || double.IsInfinity(d))
            {
                throw Error("number out of range");
            }

            return d;
        }

        private void SkipDigits()
        {
            while (_pos < _text.Length && _text[_pos] >= '0' && _text[_pos] <= '9')
            {
                _pos++;
            }
        }

        private void RequireDigits()
        {
            int before = _pos;
            SkipDigits();
            if (_pos == before)
            {
                throw Error("invalid number");
            }
        }
    }
}
