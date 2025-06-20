﻿using Pixel_Walle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Formats.Asn1.AsnWriter;

namespace Pixel_Walle
{
    public abstract class Functions // Clase base para las funciones del lenguaje
    {
        public abstract bool CheckSemantic(IScope scope);
        public abstract object Evaluate(IScope? scope, IVisitor? visitor = null);
        public abstract Utils.ReturnType GetType(IScope? scope);
    }
    public class GetActualX : Functions // Clase que representa la función GetActualX
    {
        public override bool CheckSemantic(IScope scope)
        {
            return true;
        }
        public override object Evaluate(IScope? scope, IVisitor? visitor = null)
        {
            return Utils.wall_E.PosX;
        }
        public override Utils.ReturnType GetType(IScope? scope)
        {
            return Utils.ReturnType.Number;
        }
    }
    public class GetActualY : Functions // Clase que representa la función GetActualY
    {
        public override bool CheckSemantic(IScope scope)
        {
            return true;
        }
        public override object Evaluate(IScope? scope, IVisitor? visitor = null)
        {
            return Utils.wall_E.PosY;
        }
        public override Utils.ReturnType GetType(IScope? scope)
        {
            return Utils.ReturnType.Number;
        }
    }
    public class GetCanvasSize : Functions  // Clase que representa la función GetCanvasSize
    {
        public override bool CheckSemantic(IScope scope)
        {
            return true;
        }
        public override object Evaluate(IScope? scope, IVisitor? visitor = null)
        {
            return Utils.cellMatrix.GetLength(0);
        }
        public override Utils.ReturnType GetType(IScope? scope)
        {
            return Utils.ReturnType.Number;
        }
    }
    public class GetColorCount : Functions // Clase que representa la función GetColorCount
    {
        public Token? Color;
        public Statement? X1;
        public Statement? Y1;
        public Statement? X2;
        public Statement? Y2;
        public override bool CheckSemantic(IScope scope)
        {
            bool check = true;

            if (X1 != null)
            {
                if (!Utils.CheckFunction("GetColorCount", X1, scope))
                    check = false;
            }
            if (Y1 != null)
            {
                if (!Utils.CheckFunction("GetColorCount", Y1, scope))
                    check = false;
            }
            if (X2 != null)
            {
                if (!Utils.CheckFunction("GetColorCount", X2, scope))
                    check = false;
            }
            if (Y2 != null)
            {
                if (!Utils.CheckFunction("GetColorCount", Y2, scope))
                    check = false;
            }
            if (Color != null)
            {
                if (Utils.Colors.Contains(Color.Value))
                    check = false;
                else
                {
                    Utils.Errors.Add($"Error Semántico: \"{Color.Value}\" no es un color valido. Linea: {Color.Line} Columna: {Color.Column} ");
                    check = false;
                }
            }
            return check;
        }
        public override object Evaluate(IScope? scope, IVisitor? visitor = null)
        {
            string color = Color?.Value ?? "Transparent";
            int x1 = Convert.ToInt32(X1?.Evaluate(scope, visitor));
            int y1 = Convert.ToInt32(Y1?.Evaluate(scope, visitor));
            int x2 = Convert.ToInt32(X2?.Evaluate(scope, visitor));
            int y2 = Convert.ToInt32(Y2?.Evaluate(scope, visitor));
            
            if (!Utils.CheckRange(x1, y1) || !Utils.CheckRange(x2, y2)) 
                return 0;

            // Asegurarse de que x1, y1 sea la esquina superior izquierda y x2, y2 la inferior derecha
            int startX = Math.Min(x1, x2);
            int endX = Math.Max(x1, x2);
            int startY = Math.Min(y1, y2);
            int endY = Math.Max(y1, y2);

            int count = 0;

            // Recorrer el rectángulo y contar las celdas con el color especificado
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    if (Utils.cellMatrix[y, x].Background.ToString() == color)
                    {
                        count++;
                    }
                }
            }

            return count;

        }
        public override Utils.ReturnType GetType(IScope? scope)
        {
            return Utils.ReturnType.Number;
        }
    }
    public class IsBrushColor : Functions  // Clase que representa la función IsBrushColor
    {
        public Token? Color;
        public override bool CheckSemantic(IScope scope)
        {
            if (Color != null)
            {
                if (Utils.Colors.Contains(Color.Value))
                    return true;
                else
                {
                    Utils.Errors.Add($"Error Semántico: \" {Color.Value}\" no es un color valido. Linea: {Color.Line} Columna: {Color.Column} ");
                    return false;
                }
            }
            return false;
        }
        public override object Evaluate(IScope? scope, IVisitor? visitor = null)
        {
            if(Color?.Value == Utils.wall_E.PaintBrush)
                return 1;
            
            return 0;
        }
        public override Utils.ReturnType GetType(IScope? scope)
        {
            return Utils.ReturnType.Number;
        }
    }
    public class IsBrushSize : Functions  // Clase que representa la función IsBrushSize
    {
        public Statement? Size;
        public override bool CheckSemantic(IScope scope)
        {
            if (Size != null)
            {
                if (!Utils.CheckFunction("IsBrushSize", Size, scope))
                    return false;
            }
            return true;
        }
        public override object Evaluate(IScope? scope, IVisitor? visitor = null)
        {
            int size = Convert.ToInt32(Size?.Evaluate(scope, visitor));
            if (size == Utils.wall_E.WidthPaint)
                return 1;

            return 0;
        }
        public override Utils.ReturnType GetType(IScope? scope)
        {
            return Utils.ReturnType.Number;
        }
    }
    public class IsCanvasColor : Functions  // Clase que representa la función IsCanvasColor
    {
        public Token? Color;
        public Statement? Vertical;
        public Statement? Horizontal;
        public override bool CheckSemantic(IScope scope)
        {
            bool check = true;

            if (Vertical != null)
            {
                if (!Utils.CheckFunction("IsCanvasColor", Vertical, scope))
                    check = false;
            }
            if (Horizontal != null)
            {
                if (!Utils.CheckFunction("IsCanvasColor", Horizontal, scope))
                    check = false;
            }
            if (Color != null)
            {
                if (Utils.Colors.Contains(Color.Value))
                    check = false;
                else
                {
                    Utils.Errors.Add($"Error Semántico: \"{Color.Value} \" no es un color valido. Linea:  {Color.Line}  Columna:  {Color.Column} ");
                    check = false;
                }
            }
           
            return check;
        }
        public override object Evaluate(IScope? scope, IVisitor? visitor = null)
        {
            int x = Convert.ToInt32(Vertical?.Evaluate(scope, visitor));
            int y = Convert.ToInt32(Horizontal?.Evaluate(scope, visitor));

            int posX = Utils.wall_E.PosX + x;
            int posY = Utils.wall_E.PosY + y;

            if (!Utils.CheckRange(posX, posY))
                return 0;
            if (Utils.cellMatrix[posY, posX].Background.ToString() == Color?.Value)
                return 1;

            return 0;
        }
        public override Utils.ReturnType GetType(IScope? scope)
        {
            return Utils.ReturnType.Number;
        }
    }
}
