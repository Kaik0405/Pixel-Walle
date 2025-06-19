using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Walle
{
    public class Token
    {
        //Enum
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

        } // Enumeración de tipos de Tokens

        // Properties
        public TokenType Type { get; set; } // Tipo de Token
        public string Value { get; set; }  // Valor del Token
        public int Line { get; set; }  // Línea donde se encuentra el Token
        public int Column { get; set; }  // Columna donde se encuentra el Token

        // Builder
        public Token(TokenType Type, string Value, int Line, int Column)
        {
            this.Type = Type;
            this.Value = Value;
            this.Line = Line;
            this.Column = Column;
        }

        // Methods
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
        }    // Método para buscar la línea de un token en el código fuente
        public static int SearchColumn(string input, int index)
        {
            for (int i = index; i >= 1; i--)
                if (input[i] == '\n')
                    return (index - i) - 1;

            return index;
        }  // Método para buscar la columna de un token en el código fuente
    }
}
