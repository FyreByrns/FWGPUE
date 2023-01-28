using System.Text;
using TT = FWGPUE.IO.TokenType;

namespace FWGPUE.IO {
    enum TokenType {
        None = 0,

        #region tokens used only for parsing
        p_set,
        p_arrayOrStringEnd,
        #endregion tokens used only for parsing

        #region tokens used for both parsing and as value types
        Comment,
        Value,
        String,
        Array,
        #endregion tokens used for both parsing and as value types
    }

    class Token {
        public TokenType Type { get; set; }
        public string? Name { get; set; }
        public string? Value { get; set; }
        public List<string> Values { get; } = new();

        public Token() { }
        public Token(string value) {
            Type = TokenHelper.Parse(value);
            Value = value;
        }

        public override string ToString() {
            string result = $"{Type}:{Name}{(Value != null ? $" {Value}" : "")}";

            if (Values.Count > 0) {
                result += " [";
                for (int i = 0; i < Values.Count; i++) {
                    string v = Values[i];
                    result += $"{v}{(i == Values.Count - 1 ? "" : ", ")}";
                }
                result += "]";
            }
            return result;
        }
    }
    static class TokenHelper {
        public static readonly Dictionary<string, TokenType> TokenDefinitions = new() {
            { "#", TT.Comment },
            { "=", TT.p_set },
            { "s[", TT.String },
            { "[", TT.Array },
            { "]", TT.p_arrayOrStringEnd },
        };

        public static IEnumerable<Token> Collapse(Queue<Token> tokens) {
            Token? current = null;
            bool moveNext() { try { return tokens.TryDequeue(out current); } catch { Log.Warn("couldn't move next while tokenizing"); } return false; }

            while (moveNext()) {
                Token result = new Token();
                Log.Info(current!);
                result.Type = current!.Type;

                switch (result.Type) {
                    case TT.Comment: {
                            while (moveNext()) {
                                if (current.Type != TT.Comment) {
                                    result.Value += $"{current.Value} ";
                                }
                                else {
                                    break;
                                }
                            }
                            break;
                        }

                    // if a value is being parsed
                    case TT.Value: {
                            // 
                            result.Name = current!.Value;

                            moveNext();
                            if (current.Type == TT.p_set) {
                                moveNext();
                                if (current.Type == TT.Value) {
                                    result.Value = current.Value;
                                    break;
                                }
                            }

                            if (current.Type == TT.String) {
                                while (moveNext()) {
                                    if (current.Type == TT.p_arrayOrStringEnd) {
                                        break;
                                    }
                                    result.Value += $"{current.Value} ";
                                    result.Type = TT.String;
                                }
                            }
                            if (current.Type == TT.Array) {
                                while (moveNext()) {
                                    if (current.Type == TT.p_arrayOrStringEnd) {
                                        break;
                                    }
                                    result.Values.Add(current.Value!);
                                }
                                result.Type = TT.Array;
                            }
                            break;
                        }
                }
                yield return result;
            }
        }

        public static TokenType Parse(string text) {
            if (TokenDefinitions.ContainsKey(text)) {
                return TokenDefinitions[text];
            }

            if (!string.IsNullOrEmpty(text)) {
                return TT.Value;
            }

            return TT.None;
        }
    }

    class DataMarkupFile : EngineFile {
        public Token[]? Tokens { get; set; }

        protected override void ReadData(byte[] data) {
            // parse data to an ascii string
            string contents = Encoding.ASCII.GetString(data)
                .Replace("\r\n", " ")
                .Replace("\t", "");
            // tokenize
            string[] split = contents.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Queue<Token> tokens = new();

            for (int i = 0; i < split.Length; i++) {
                Token current = new(split[i]);
                tokens.Enqueue(current);
            }

            Tokens = TokenHelper.Collapse(tokens).ToArray();
            foreach (Token token in Tokens) {
                Log.Info(token);
            }
        }

        protected override byte[] SaveData() {
            List<byte> data = new();
            void addData(string s) {
                data.AddRange(Encoding.ASCII.GetBytes(s));
            }

            foreach (Token token in Tokens!) {
                switch (token.Type) {
                    case TT.Comment: {
                            addData($"# {token.Value} #\r\n");
                            break;
                        }
                    case TT.String: {
                            addData($"{token.Name} = s[ {token.Value} ]\r\n");
                            break;
                        }
                    case TT.Array: {
                            addData($"{token.Name} = [ ");
                            foreach (string s in token.Values) {
                                addData($"{s} ");
                            }
                            addData("]\r\n");
                            break;
                        }
                }
            }

            return data.ToArray();
        }

        public DataMarkupFile(EngineFileLocation header) : base(header) { }
    }
}
