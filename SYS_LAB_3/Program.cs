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
            float pi = ac;
            // This is a comment
            sdfrhgdshed
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
        HashSet<string> declaredVariables = new HashSet<string>();
        bool expectingIdentifier = false;

        string directivePattern = @"^#(\w+)";
        string commentPattern = @"//.*";
        string reservedWordsPattern = @"\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|using\sstatic|virtual|void|volatile|while)\b";
        string delimiterPattern = @"[,.;()\[\]{}]";

        string[] lines = code.Split('\n');

        foreach (string line in lines)
        {
            bool isComment = false;
            string[] words = Regex.Split(line, @"(\s+|" + delimiterPattern + @")");

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (string.IsNullOrWhiteSpace(word))
                {
                    continue;
                }

                if (isComment)
                {
                    tokens.Add(new Token(word, TokenType.Comment));
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
                    isComment = true;
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
                else if (expectingIdentifier)
                {
                    if (IsIdentifier(word))
                    {
                        declaredVariables.Add(word);
                        type = TokenType.Identifier;
                    }
                    else
                    {
                        Console.WriteLine($"Error: Expected identifier after type, found {word}");
                        type = TokenType.Error;
                    }
                    expectingIdentifier = false;
                }
                else if (Regex.IsMatch(word, reservedWordsPattern))
                {
                    type = TokenType.Reserved;
                    expectingIdentifier = true; // Expecting an identifier next
                }
                else if (IsIdentifier(word))
                {
                    if (declaredVariables.Contains(word))
                    {
                        type = TokenType.Identifier;
                    }
                    else
                    {
                        type = TokenType.Error;
                        Console.WriteLine($"Error: Undeclared variable {word}");
                    }
                }

                tokens.Add(new Token(word, type));
            }
        }

        return tokens;
    }

    private static bool IsNumber(string word)
    {
        return Regex.IsMatch(word, @"^\d+(\.\d+)?(f|F)?|0[xX][0-9a-fA-F]+|0[oO][0-7]+|0[bB][01]+$");
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
