using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

public class LexicalAnalyzer
{
    public static void Main(string[] args)
    {
        string filePath = "C:\\Users\\dog79\\source\\repos\\SYS_LAB_31\\SYS_LAB_3\\code.cs";
        string code;

        try
        {
            code = File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
            return;
        }

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
        if (string.IsNullOrEmpty(code) || code.Contains("\0"))
        {
            throw new ArgumentException("Invalid input string.");
        }

        List<Token> tokens = new List<Token>();
        HashSet<string> declaredVariables = new HashSet<string>();
        bool expectingIdentifierOrString = false;
        bool isInString = false;
        bool isInDirective = false;
        string currentString = "";

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

                if (isInDirective)
                {
                    tokens.Add(new Token(word, TokenType.Directive));
                    isInDirective = false;
                    continue;
                }

                if (word.StartsWith("#"))
                {
                    isInDirective = true;
                    tokens.Add(new Token(word, TokenType.Directive));
                    continue;
                }

                if (isInString)
                {
                    currentString += word;
                    if (word.EndsWith("\""))
                    {
                        tokens.Add(new Token(currentString, TokenType.String));
                        isInString = false;
                        currentString = "";
                    }
                    continue;
                }

                if (word.StartsWith("\""))
                {
                    isInString = true;
                    currentString = word;
                    if (word.EndsWith("\""))
                    {
                        tokens.Add(new Token(currentString, TokenType.String));
                        isInString = false;
                        currentString = "";
                    }
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
                else if (IsOperator(word))
                {
                    type = TokenType.Operator;
                }
                else if (expectingIdentifierOrString)
                {
                    if (IsIdentifier(word))
                    {
                        declaredVariables.Add(word);
                        type = TokenType.Identifier;
                    }
                    else
                    {
                        Console.WriteLine($"Error: Expected identifier or string after type, found {word}");
                        type = TokenType.Error;
                    }
                    expectingIdentifierOrString = false;
                }
                else if (Regex.IsMatch(word, reservedWordsPattern))
                {
                    type = TokenType.Reserved;
                    expectingIdentifierOrString = true;
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

                if (!isInString)
                {
                    tokens.Add(new Token(word, type));
                }
            }
        }

        return tokens;
    }

    public static bool IsNumber(string word)
    {
        return Regex.IsMatch(word, @"^\d+(\.\d+)?(f|F)?|0[xX][0-9a-fA-F]+|0[oO][0-7]+|0[bB][01]+$");
    }

    public static bool IsOperator(string word)
    {
        return Regex.IsMatch(word, @"^(\+|-|\*|/|%|=|<|>|!|&|\||\(|\)|{|}|,|\[|\]|\.|\;)$");
    }

    public static bool IsIdentifier(string word)
    {
        return Regex.IsMatch(word, @"^[a-zA-Z_]\w*$");
    }
}
