using Pixel_Walle.Interpreter.Evaluate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pixel_Walle
{
    public static class Utils
    {
        public static bool CycleCodition = false;
        public static Border[,] cellMatrix = new Border[10, 10];
        public static Wall_E wall_E = new Wall_E(0, 0, "");
        public enum ReturnType { Bool, Number, NULL }
        public static List<string> Errors = new List<string>();
        public static Dictionary<string, Label> keyLabelsReferences = new Dictionary<string, Label>();
        public static HashSet<string> Colors = new HashSet<string>()
        {
            "Red","Blue","Green","Yellow","Orange","Purple","Black","White","Transparent"
        };
        public static List<Token.TokenType> FunctionList = new List<Token.TokenType>
        {
            Token.TokenType.GetActualX,
            Token.TokenType.GetActualY,
            Token.TokenType.GetCanvasSize,
            Token.TokenType.GetColorCount,
            Token.TokenType.IsBrushColor,
            Token.TokenType.IsBrushSize,
            Token.TokenType.IsCanvasColor,
        };
        public static void RemoveDuplicatesFromErrors()
        {
            Errors = Errors.Distinct().ToList();
        }
        public static double Operation(double a, double b, Token? value)
        {
            switch (value?.Type)
            {
                case Token.TokenType.Plus:
                    return a + b;
                case Token.TokenType.Minus:
                    return a - b;
                case Token.TokenType.Pow:
                    return Math.Pow(a, b);
                case Token.TokenType.Times:
                    return a * b;
                case Token.TokenType.Divide:
                    if (b != 0)
                        return a / b;
                    else
                    {
                        Errors.Add($"Error: No se puede dividir por 0 Linea: {value.Line}, Column: {value.Column}");
                        return 0;
                    }
                case Token.TokenType.Module:
                    return a % b;
                default:
                    return 0;
            }
        }
        public static bool Compare(double a, double b, Token? value)
        {

            switch (value?.Type)
            {
                case Token.TokenType.LessThan:
                    return a < b;
                case Token.TokenType.LessThanEqual:
                    return a <= b;
                case Token.TokenType.GreaterThan:
                    return a > b;
                case Token.TokenType.GreaterThanEqual:
                    return a >= b;
                case Token.TokenType.Equal:
                    return a == b;
                default: return false;
            }
        }
        public static bool CheckValidLabel(string? name) => (name?[0] == '_' || name?[name.Length - 1] == '_') ? false : true;
        public static bool CheckInstruction(string nameIns, Statement statement, IScope scope)
        {
            bool checking = true;

            if (!statement.CheckSemantic(scope))
                checking = false;
            if (statement.GetType(scope) != ReturnType.Number)
            {
                Errors.Add($"Error Semántico: La instrucción \"{nameIns}\" no puede recibir como parámetro un \"{statement.GetType(scope)}\" ");
                checking = false;
            }
            return checking;
        }
        public static bool CheckFunction(string nameIns, Statement statement, IScope scope)
        {
            bool checking = true;

            if (!statement.CheckSemantic(scope))
                checking = false;
            if (statement.GetType(scope) != ReturnType.Number)
            {
                Errors.Add($"Error Semántico: La función \"{nameIns}\" no puede recibir como parámetro un \"{statement.GetType(scope)}\" ");
                checking = false;
            }
            return checking;
        }
        public static void ChangeCellColor(int row, int column, string colorName)
        {
            // Convertir el nombre del color a un objeto Brush
            if (colorName != "Transparent")
            {
                Brush? newColor = (Brush?)new BrushConverter().ConvertFromString(colorName);

                // Cambiar el color de fondo de la celda
                cellMatrix[row, column].Background = newColor;
            }
        }
        public static bool CheckRange(int row, int column)
        {
            if (row < 0 || column < 0)
            { return false; }
            if (row >= cellMatrix.GetLength(0) || column >= cellMatrix.GetLength(1))
            { return false; }

            return true;
        }
        public static void PaintBrush(int x, int y)
        {
            int distance = 0;
            if (wall_E.WidthPaint >= 2)
            {
                distance = (wall_E.WidthPaint - 1) / 2;
            }
            for (int dx = -distance; dx <= distance; dx++)
            {
                for (int dy = -distance; dy <= distance; dy++)
                {
                    int px = x + dx;
                    int py = y + dy;

                    if (CheckRange(px, py))
                    {
                        ChangeCellColor(px, py, wall_E.PaintBrush);
                    }
                }
            }
        }
        public static void Reset()
        {
            CycleCodition = false;
            Errors.Clear();
            keyLabelsReferences.Clear();
            wall_E = new Wall_E(0, 0, "");
        }
    }
}
