using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Walle
{
    public class Token
    {
        public enum TokenType
        {
            //Instructions
            Spawn,
            Color,
            Size,
            DrawLine,
            DrawCircle,
            DrawRectangle,
            Fill,
            GoTo,

            //Functions

            GetActualX,
            GetActualY,
            GetCanvasSize,
            GetColorCount,
            IsBrushColor,
            IsBrushSize,
            IsCanvasColor,
            IsColor,

            // Símbolos
            OpenParan,        // ( 
            ClosedParan,      // )
            OpenBracket,      // [
            ClosedBracket,    // ]
            Quote,            // "
            GreaterThan,      // >
            LessThan,         // <
            LessThanEqual,    // <=
            GreaterThanEqual, // =>
            Assignment,       // <-
            Equal,            // == 
            Colon,            // :
            Dot,              // .
            Comma,            // ,
            SemiColon,        // ;
            Arrow,            // =>
            Comment,          // //

            // Operadores Aritméticos
            Pow,              // **
            Plus,             // +
            Minus,            // -
            Times,            // *
            Divide,           // /
            Module,           // %        

            //Boolean
            True,
            False,

            // Operadores Lógicos
            AND,              // &&
            OR,               // ||

            // Concatenación
            ATAT,             // @@
            AT,               // @

            Digit,           // 1354 
            UnKnown,          // UserText

        }
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public Token(TokenType Type, string Value, int Line, int Column)
        {
            this.Type = Type;
            this.Value = Value;
            this.Line = Line;
            this.Column = Column;
        }
        public static int SearchLine(string input, int index)
        {
            int counter = 1;
            for (int i = 0; i <= index; i++)
            {
                if (index == i)
                    return counter;
                else if (input[i] == '\n')
                    counter++;
            }
            return counter;
        }
        public static int SearchColumn(string input, int index)
        {
            for (int i = index; i >= 1; i--)
                if (input[i] == '\n')
                    return (index - i) - 1;

            return index;
        }
    }
}
