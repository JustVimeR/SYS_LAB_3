using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class LexicalAnalyzer
{
    static void Main(string[] args)
    {
        string code = @"
            #define DEBUG
            int x = 42;
            string message = ""Hello, world!"";
            float pi = 3.14f;
            // This is a comment
        ";

        List<Token> tokens = Analyze(code);

        foreach (Token token in tokens)
        {
            Console.WriteLine($"< {token.Value}, {token.Type} >");
        }
    }

    public enum TokenType
    {
        Number,
        String,
        Operator,
        Identifier,
        Directive,
        Comment,
        Reserved,
        Delimiter,
        Error
    }

    public class Token
    {
        public string Value { get; }
        public TokenType Type { get; }

        public Token(string value, TokenType type)
        {
            Value = value;
            Type = type;
        }
    }

    public static List<Token> Analyze(string code)
    {
        List<Token> tokens = new List<Token>();

        // Regular expressions for directive, comments, reserved words, and delimiters
        string directivePattern = @"^#(\w+)";
        string commentPattern = @"//.*";
        string reservedWordsPattern = @"\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|using\sstatic|virtual|void|volatile|while)\b";
        string delimiterPattern = @"[,.;()\[\]{}]";

        string[] lines = code.Split('\n');

        foreach (string line in lines)
        {
            string[] words = Regex.Split(line, @"(\s+|" + delimiterPattern + @")");

            foreach (string word in words)
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    continue;
                }

                TokenType type = TokenType.Error;

                if (Regex.IsMatch(word, directivePattern))
                {
                    type = TokenType.Directive;
                }
                else if (Regex.IsMatch(word, commentPattern))
                {
                    type = TokenType.Comment;
                }
                else if (Regex.IsMatch(word, reservedWordsPattern))
                {
                    type = TokenType.Reserved;
                }
                else if (Regex.IsMatch(word, delimiterPattern))
                {
                    type = TokenType.Delimiter;
                }
                else if (IsNumber(word))
                {
                    type = TokenType.Number;
                }
                else if (IsString(word))
                {
                    type = TokenType.String;
                }
                else if (IsOperator(word))
                {
                    type = TokenType.Operator;
                }
                else if (IsIdentifier(word))
                {
                    type = TokenType.Identifier;
                }

                tokens.Add(new Token(word, type));
            }
        }

        return tokens;
    }

    private static bool IsNumber(string word)
    {
        return Regex.IsMatch(word, @"^\d+(\.\d+)?(f|F)?|0[xX][0-9a-fA-F]+$");
    }

    private static bool IsString(string word)
    {
        return Regex.IsMatch(word, @"^"".*""$");
    }

    private static bool IsOperator(string word)
    {
        return Regex.IsMatch(word, @"^(\+|-|\*|/|%|=|<|>|!|&|\||\(|\)|{|}|,|\[|\]|\.|\;)$");
    }

    private static bool IsIdentifier(string word)
    {
        return Regex.IsMatch(word, @"^[a-zA-Z_]\w*$");
    }
}

