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
        //Enum
        public enum ReturnType { Bool, Number, NULL }

        // Static properties
        public static bool CycleCondition = false;                 // Variable para controlar el ciclo de ejecución (GoTo)
        
        public static Border[,] cellMatrix = new Border[10, 10];  // Matriz de referencia de las celdas que representan el canvas

        public static Wall_E wall_E = new Wall_E(0, 0, "");       // Instancia de Wall_E que representa el estado actual del robot
        
        public static List<string> Errors = new List<string>();   // Lista de errores encontrados durante la ejecución del código
        
        public static Dictionary<string, Label> keyLabelsReferences = new Dictionary<string, Label>();  // Diccionario para almacenar referencias a etiquetas (Labels) por su nombre
        
        public static HashSet<string> Colors = new HashSet<string>()
        {
            "AliceBlue", "AntiqueWhite", "Aqua", "Aquamarine", "Azure", "Beige", "Bisque", "Black", "BlanchedAlmond", "Blue",
            "BlueViolet", "Brown", "BurlyWood", "CadetBlue", "Chartreuse", "Chocolate", "Coral", "CornflowerBlue", "Cornsilk",
            "Crimson", "Cyan", "DarkBlue", "DarkCyan", "DarkGoldenrod", "DarkGray", "DarkGreen", "DarkKhaki", "DarkMagenta",
            "DarkOliveGreen", "DarkOrange", "DarkOrchid", "DarkRed", "DarkSalmon", "DarkSeaGreen", "DarkSlateBlue", "DarkSlateGray",
            "DarkTurquoise", "DarkViolet", "DeepPink", "DeepSkyBlue", "DimGray", "DodgerBlue", "Firebrick", "FloralWhite",
            "ForestGreen", "Fuchsia", "Gainsboro", "GhostWhite", "Gold", "Goldenrod", "Gray", "Green", "GreenYellow", "Honeydew",
            "HotPink", "IndianRed", "Indigo", "Ivory", "Khaki", "Lavender", "LavenderBlush", "LawnGreen", "LemonChiffon",
            "LightBlue", "LightCoral", "LightCyan", "LightGoldenrodYellow", "LightGray", "LightGreen", "LightPink", "LightSalmon",
            "LightSeaGreen", "LightSkyBlue", "LightSlateGray", "LightSteelBlue", "LightYellow", "Lime", "LimeGreen", "Linen",
            "Magenta", "Maroon", "MediumAquamarine", "MediumBlue", "MediumOrchid", "MediumPurple", "MediumSeaGreen", "MediumSlateBlue",
            "MediumSpringGreen", "MediumTurquoise", "MediumVioletRed", "MidnightBlue", "MintCream", "MistyRose", "Moccasin",
            "NavajoWhite", "Navy", "OldLace", "Olive", "OliveDrab", "Orange", "OrangeRed", "Orchid", "PaleGoldenrod", "PaleGreen",
            "PaleTurquoise", "PaleVioletRed", "PapayaWhip", "PeachPuff", "Peru", "Pink", "Plum", "PowderBlue", "Purple", "Red",
            "RosyBrown", "RoyalBlue", "SaddleBrown", "Salmon", "SandyBrown", "SeaGreen", "SeaShell", "Sienna", "Silver", "SkyBlue",
            "SlateBlue", "SlateGray", "Snow", "SpringGreen", "SteelBlue", "Tan", "Teal", "Thistle", "Tomato", "Turquoise",
            "Violet", "Wheat", "White", "WhiteSmoke", "Yellow", "YellowGreen","Transparent"
        };  // Lista de colores              
        
        public static List<Token.TokenType> FunctionList = new List<Token.TokenType>
        {
            Token.TokenType.GetActualX,
            Token.TokenType.GetActualY,
            Token.TokenType.GetCanvasSize,
            Token.TokenType.GetColorCount,
            Token.TokenType.IsBrushColor,
            Token.TokenType.IsBrushSize,
            Token.TokenType.IsCanvasColor,
        }; // Lista de tipos de tokens que representan funciones del lenguaje

        // Methods
        public static void RemoveDuplicatesFromErrors()
        {
            Errors = Errors.Distinct().ToList();
        }                  // Método para eliminar duplicados de la lista de errores
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
                        throw new Exception("Error en Tiempo de Ejecución: No se puede dividir entre 0");
                    }
                case Token.TokenType.Module:
                    return a % b;
                default:
                    return 0;
            }
        } // Método para realizar operaciones aritméticas entre dos números según el tipo de token proporcionado
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
        }     // Método para comparar dos números según el tipo de token proporcionado
        public static bool CheckValidLabel(string? name) => (name?[0] == '_' || name?[name.Length - 1] == '_') ? false : true;  // Método para verificar si un nombre de etiqueta es válido, no puede comenzar o terminar con '_'
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
        }  // Método para verificar si una instrucción es válida, comprobando su semántica y tipo de retorno
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
        }  // Método para verificar si una función es válida, comprobando su semántica y tipo de retorno
        public static void ChangeCellColor(int row, int column, string colorName)
        {
            // Convertir el nombre del color a un objeto Brush
            if (colorName != "Transparent")
            {
                Brush? newColor = (Brush?)new BrushConverter().ConvertFromString(colorName);

                // Cambiar el color de fondo de la celda
                cellMatrix[row, column].Background = newColor;
            }
        }  // Método para cambiar el color de una celda específica en la matriz de celdas
        public static bool CheckRange(int row, int column)
        {
            if (row < 0 || column < 0)
            { return false; }

            if (row >= cellMatrix.GetLength(0) || column >= cellMatrix.GetLength(1))
            { return false; }

            return true;
        }  // Método para verificar si una celda está dentro del rango de la matriz de celdas
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
                        ChangeCellColor(px, py, wall_E.PaintBrush);
                }
            }
        }  // Método para pintar un área alrededor de una celda específica según el tamaño del pincel del robot
        public static void Reset()
        {
            CycleCondition = false;
            Errors.Clear();
            keyLabelsReferences.Clear();
            wall_E = new Wall_E(0, 0, "");
        }  // Método para reiniciar el estado del intérprete, limpiando errores, referencias de etiquetas y restableciendo el estado del robot
        public static bool CheckDirections(int x, int y)
        {
            bool isValid = false;

            if (x == -1 || x == 1 || x == 0)
                isValid = true;
            else return false;
            
            if (y == -1 || y == 1 || y == 0)
                isValid = true;
            else return false;

            return isValid;
        }   // Método para verificar si las direcciones de movimiento del robot son válidas, permitiendo solo movimientos horizontales o verticales
    }
}
