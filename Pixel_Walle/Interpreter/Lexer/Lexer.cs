using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pixel_Walle
{
    public class Lexer
    {
        private static Dictionary<string, Token.TokenType> KeyWords = new Dictionary<string, Token.TokenType>()
        {
            //KeyWords
            {"Spawn",Token.TokenType.Spawn},
            {"Color",Token.TokenType.Color},
            {"Size",Token.TokenType.Size},
            {"DrawLine",Token.TokenType.DrawLine},
            {"DrawCircle",Token.TokenType.DrawCircle},
            {"DrawRectangle",Token.TokenType.DrawRectangle},
            {"Fill",Token.TokenType.Fill},
            {"GoTo",Token.TokenType.GoTo},
            {"GetActualX",Token.TokenType.GetActualX},
            {"GetActualY",Token.TokenType.GetActualY},
            {"GetCanvasSize",Token.TokenType.GetCanvasSize},
            {"GetColorCount",Token.TokenType.GetColorCount},
            {"IsBrushColor",Token.TokenType.IsBrushColor},
            {"IsBrushSize",Token.TokenType.IsBrushSize},
            {"IsCanvasColor",Token.TokenType.IsCanvasColor},
            {"IsColor",Token.TokenType.IsColor},

            //Colors
            {"Red",Token.TokenType.Red},
            {"Blue",Token.TokenType.Blue},
            {"Green",Token.TokenType.Green},
            {"Yellow",Token.TokenType.Yellow},
            {"Orange",Token.TokenType.Orange},
            {"Purple",Token.TokenType.Purple},
            {"Black",Token.TokenType.Black},
            {"White",Token.TokenType.White},
            {"Transparent",Token.TokenType.Transparent},

            // Símbolos
            {"\\(", Token.TokenType.OpenParan},
            {"\\)", Token.TokenType.ClosedParan},
            {"\\[", Token.TokenType.OpenBracket},
            {"\\]", Token.TokenType.ClosedBracket},
            {"\"", Token.TokenType.Quote},
            {"<-", Token.TokenType.Assignment},
            {"<=", Token.TokenType.LessThanEqual},
            {">=", Token.TokenType.GreaterThanEqual},
            {"<", Token.TokenType.LessThan},
            {">", Token.TokenType.GreaterThan},
            {"==", Token.TokenType.Equal},
            {"=>", Token.TokenType.Arrow},
            {"\\:", Token.TokenType.Colon},
            {"\\.", Token.TokenType.Dot},
            {",", Token.TokenType.Comma},
            {";", Token.TokenType.SemiColon},

            //Operadores Aritméticos
            {"\\*\\*", Token.TokenType.Pow},
            {"\\+", Token.TokenType.Plus},
            {"-",Token.TokenType.Minus },
            {"\\*", Token.TokenType.Times},
            {"/",Token.TokenType.Divide },
            {"\\^",Token.TokenType.Pow },
            {"\\%",Token.TokenType.Module },

            // Operadores Lógicos
            {"&&", Token.TokenType.AND},
            {"\\|\\|", Token.TokenType.OR},

            //Bolean
            {"True", Token.TokenType.True},
            {"False", Token.TokenType.False},

            // Concatenación
            {"@@", Token.TokenType.ATAT},
            {"@", Token.TokenType.AT},

             // Keys Arbitrarias
            {"[a-zA-Z_]\\w*", Token.TokenType.UnKnown},
            {"\\d+", Token.TokenType.Digit},
        };
        private string[]? code;
        private List<Token> tokensList;
        public string[] Code { get; private set; }
        public Token[] TokensList
        {
            get
            {
                if (tokensList.Count == 0)
                    return GetLexer();
                else
                    return tokensList.ToArray();
            }
            private set { }
        }
        //Builder
        public Lexer(string[] code)
        {
            this.code = code;
            tokensList = new List<Token>();
        }
        //Methods
        public Token[] GetLexer()
        {
            string input = string.Join('\n'.ToString(), code);
            string pattern = $"{string.Join("|", KeyWords.Keys)}";
            MatchCollection matches = Regex.Matches(input, pattern);

            foreach (Match match in matches)
            {
                if (KeyWords.TryGetValue(match.Value, out var tokenType))
                    tokensList.Add(new Token(tokenType, match.Value, Token.SearchLine(input, match.Index), Token.SearchColumn(input, match.Index)));

                else if (match.Value.All(char.IsDigit) && Regex.IsMatch(match.Value, "\\d+"))
                    tokensList.Add(new Token(Token.TokenType.Digit, match.Value, Token.SearchLine(input, match.Index), Token.SearchColumn(input, match.Index)));

                else
                {
                    foreach (string item in KeyWords.Keys)
                    {
                        if (Regex.IsMatch(match.Value, item))
                        {
                            tokensList.Add(new Token(KeyWords[item], match.Value, Token.SearchLine(input, match.Index), Token.SearchColumn(input, match.Index)));
                            break;
                        }
                    }
                }
            }
            return tokensList.ToArray();
        }
    }
}
