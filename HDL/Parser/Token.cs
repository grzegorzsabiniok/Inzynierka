using System;
using System.Collections.Generic;
using System.Text;

namespace HDL.Parser
{
    class Token
    {
        private string value;

        public static List<string> Keywords { get; set; }

        public enum TokenType
        {
            Unknown,
            Identifier,
            Keyword,
            Separator,
        }

        public TokenType Type { get; set; }

        public string Value
        {
            get => value;
            set
            {
                if (Type == TokenType.Unknown)
                {
                    if (Keywords.Contains(value))
                    {

                        Type = TokenType.Keyword;
                    }
                    else
                    {
                        Type = TokenType.Identifier;
                    }
                }
                this.value = value;
            }
        }

        public File File { get; set; }

        public int Line { get; set; }

        public int Position { get; set; }

        public override string ToString()
        {
            return $"({Value}|{Type}|{Line}|{Position}|{File.Name})";
        }
    }
}
