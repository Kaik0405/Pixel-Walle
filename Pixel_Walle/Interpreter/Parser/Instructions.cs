using Pixel_Walle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Pixel_Walle
{
    public abstract class Instructions
    {
        public abstract bool CheckSemantic(IScope scope);
        public abstract void Evaluate(IVisitor visitor);
    }
    public class Spawn : Instructions
    {
        public Statement? X;
        public Statement? Y;
        public override bool CheckSemantic(IScope scope)
        {
            bool check = true;

            if (X is not null)
            {
                if (!Utils.CheckInstruction("Spawn", X, scope))
                    check = false;
            }
            if (Y is not null)
            {
                if (!Utils.CheckInstruction("Spawn", Y, scope))
                    check = false;
            }
            return check;
        }
        public override void Evaluate(IVisitor visitor)
        {
            double x, y;

            if (X is not null && Y is not null)
            {
                x = Convert.ToDouble(X.Evaluate(visitor.Scope,visitor));
                y = Convert.ToDouble(Y.Evaluate(visitor.Scope, visitor));

                if (!Utils.CheckRange((int)x, (int)y))
                    throw new Exception($"Error en tiempo de ejecución: La posición ({x}, {y}) está fuera de los límites del canvas.");

                Utils.wall_E.PosX = (int)x;
                Utils.wall_E.PosY = (int)y;
            }
        }
    }
    public class Color : Instructions
    {
        public Token? Value;

        public override bool CheckSemantic(IScope scope)
        {
            if(Value != null)
            {
                if(Utils.Colors.Contains(Value.Value))
                    return true;
                else
                {
                    Utils.Errors.Add($"Error Semántico: \"{Value.Value}\" no es un color valido. Linea: {Value.Line} Columna: {Value.Column} ");
                    return false;
                }
            }
            return false;
        }
        public override void Evaluate(IVisitor visitor)
        {
            if(Value != null) 
                Utils.wall_E.PaintBrush = Value.Value;
        }
    }
    public class Size : Instructions
    {
        public Statement? K;

        public override bool CheckSemantic(IScope scope)
        {
            bool check = true;
            if (K is not null)
            {
                if (!Utils.CheckInstruction("Size", K, scope))
                    check = false;
            }
            return check;
        }
        public override void Evaluate(IVisitor visitor)
        {
            if (K is not null )
            {
                double size = Convert.ToDouble(K.Evaluate(visitor.Scope,visitor));
                if (size < 1 || size > Utils.cellMatrix.GetLength(0) || size > Utils.cellMatrix.GetLength(1))
                    throw new Exception($"Error en tiempo de ejecución: El tamaño del pincel debe estar en un rango inferior a la dimension del canvas. Valor proporcionado: {size}");

                if ((int)size % 2 == 0)
                    size = size - 1;

                Utils.wall_E.WidthPaint = (int)size;
            }
        }
    }
    public class DrawLine : Instructions
    {
        public Statement? DirX;
        public Statement? DirY;
        public Statement? Distance;
        public override bool CheckSemantic(IScope scope)
        {
            bool check = true;

            if (DirX is not null)
            {
                if (!Utils.CheckInstruction("DrawLine", DirX, scope))
                    check = false;
            }
            if (DirY is not null)
            {
                if (!Utils.CheckInstruction("DrawLine", DirY, scope))
                    check = false;
            }
            if (Distance is not null)
            {
                if (!Utils.CheckInstruction("DrawLine", Distance, scope))
                    check = false;
            }
            return check;
        }
        public override void Evaluate(IVisitor visitor)
        {
            double x, y, distance;

            if (DirX != null && DirY != null && Distance != null)
            {
                x = Convert.ToDouble(DirX.Evaluate(visitor.Scope,visitor));
                y = Convert.ToDouble(DirY.Evaluate(visitor.Scope,visitor));
                distance = Convert.ToDouble(Distance.Evaluate(visitor.Scope, visitor));

                int currX = Utils.wall_E.PosX;
                int currY = Utils.wall_E.PosY;

                for (int step = 0; step < (int)distance; step++)
                {
                    if (Utils.CheckRange(currX, currY))
                    {
                        Utils.PaintBrush(currX, currY);
                        Utils.ChangeCellColor(currX, currY, Utils.wall_E.PaintBrush);
                        currX += (int)x;
                        currY += (int)y;
                    }
                }
                if(!Utils.CheckRange(currX, currY))
                    throw new Exception($"Error en tiempo de ejecución: La posición final de Wall-E ({currX}, {currY}) está fuera de los límites del canvas.");
                Utils.PaintBrush(currX, currY);
                Utils.wall_E.PosX = currX;
                Utils.wall_E.PosY = currY;
            }
        }
    }
    public class DrawCircle : Instructions
    {
        public Statement? DirX;
        public Statement? DirY;
        public Statement? Radius;
        public override bool CheckSemantic(IScope scope)
        {
            bool check = true;

            if (DirX is not null)
            {
                if (!Utils.CheckInstruction("DrawCircle", DirX, scope))
                    check = false;
            }
            if (DirY is not null)
            {
                if (!Utils.CheckInstruction("DrawCircle", DirY, scope))
                    check = false;
            }
            if (Radius is not null)
            {
                if (!Utils.CheckInstruction("DrawCircle", Radius, scope))
                    check = false;
            }
            return check;
        }
        public override void Evaluate(IVisitor visitor)
        {
            double dirX, dirY, radius;

            if (DirX != null && DirY != null && Radius != null)
            {
                dirX = Convert.ToDouble(DirX.Evaluate(visitor.Scope, visitor));
                dirY = Convert.ToDouble(DirY.Evaluate(visitor.Scope, visitor));
                radius = Convert.ToDouble(Radius.Evaluate(visitor.Scope, visitor));

                int centerX = Utils.wall_E.PosX + (int)dirX * (int)radius;
                int centerY = Utils.wall_E.PosY + (int)dirY * (int)radius;

                int x = 0;
                int y = (int)radius + 1;
                int d = 3 - 2 * (int)radius + 1;

                DrawCirclePoints(centerX, centerY, x, y);

                while (y >= x) //algoritmo del círculo de Bresenham
                {
                    x++;
                    if (d > 0)
                    {
                        y--;
                        d = d + 4 * (x - y) + 10;
                    }
                    else
                    {
                        d = d + 4 * x + 6;
                    }
                    DrawCirclePoints(centerX, centerY, x, y);
                }

                Utils.wall_E.PosX = centerX;
                Utils.wall_E.PosY = centerY;
            }
        }
        private void DrawCirclePoints(int cx, int cy, int x, int y)
        {
            Utils.PaintBrush(cx + x, cy + y);
            Utils.PaintBrush(cx - x, cy + y);
            Utils.PaintBrush(cx + x, cy - y);
            Utils.PaintBrush(cx - x, cy - y);
            Utils.PaintBrush(cx + y, cy + x);
            Utils.PaintBrush(cx - y, cy + x);
            Utils.PaintBrush(cx + y, cy - x);
            Utils.PaintBrush(cx - y, cy - x);
        }
    }
    public class DrawRectangle : Instructions
    {
        public Statement? DirX;
        public Statement? DirY;
        public Statement? Distance;
        public Statement? Width;
        public Statement? Height;
        public override bool CheckSemantic(IScope scope)
        {
            bool check = true;

            if (DirX is not null)
            {
                if (!Utils.CheckInstruction("DrawRectangle", DirX, scope))
                    check = false;
            }
            if (DirY is not null)
            {
                if (!Utils.CheckInstruction("DrawRectangle", DirY, scope))
                    check = false;
            }
            if (Distance is not null)
            {
                if (!Utils.CheckInstruction("DrawRectangle", Distance, scope))
                    check = false;
            }
            if (Width is not null)
            {
                if (!Utils.CheckInstruction("DrawRectangle", Width, scope))
                    check = false;
            }
            if (Height is not null)
            {
                if (!Utils.CheckInstruction("DrawRectangle", Height, scope))
                    check = false;
            }



            return check;
        }
        public override void Evaluate(IVisitor visitor)
        {
            int dirX, dirY, distance, width, height;

            if (DirX != null && DirY != null && Distance != null && Width != null && Height != null)
            {
                dirX = Convert.ToInt32(DirX.Evaluate(visitor.Scope, visitor));
                dirY = Convert.ToInt32(DirY.Evaluate(visitor.Scope, visitor));
                distance = Convert.ToInt32(Distance.Evaluate(visitor.Scope, visitor));
                height = Convert.ToInt32(Width.Evaluate(visitor.Scope, visitor));
                width = Convert.ToInt32(Height.Evaluate(visitor.Scope, visitor));

                int centerX = Utils.wall_E.PosX + dirX * distance;
                int centerY = Utils.wall_E.PosY + dirY * distance;

                // Esquinas del rectángulo
                int left = centerX - width / 2;
                int right = centerX + (width - 1) / 2;
                int top = centerY - height / 2;
                int bottom = centerY + (height - 1) / 2;

                // Lados horizontales (top y bottom)
                for (int x = left; x <= right; x++)
                {
                    Utils.PaintBrush(x, top);
                    Utils.PaintBrush(x, bottom);
                }

                // Lados verticales (left y right), sin esquinas para evitar doble pintado
                for (int y = top + 1; y <= bottom - 1; y++)
                {
                    Utils.PaintBrush(left, y);
                    Utils.PaintBrush(right, y);
                }

                Utils.wall_E.PosX = centerX;
                Utils.wall_E.PosY = centerY;
            }
        }
    }
    public class Fill : Instructions
    {
        public override bool CheckSemantic(IScope scope)
        {
            return true;
        }
        public override void Evaluate(IVisitor visitor)
        {
            int startX = Utils.wall_E.PosX; // Posición inicial X
            int startY = Utils.wall_E.PosY; // Posición inicial Y
            string targetColor = Utils.cellMatrix[startY, startX].Background.ToString(); // Color inicial de la celda
            string brushColor = Utils.wall_E.PaintBrush; // Color de la brocha

            // Si el color del pincel es igual al color objetivo, no hace falta llenar
            if (targetColor == brushColor)
                return;

            // Cola para realizar el llenado por propagación
            var queue = new Queue<(int x, int y)>();
            queue.Enqueue((startX, startY));

            // Direcciones para moverse (arriba, abajo, izquierda, derecha)
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            // Conjunto para rastrear las celdas visitadas
            var visited = new HashSet<(int x, int y)>();

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();

                // Verificar si la celda está dentro del canvas
                if (!Utils.CheckRange(x, y))
                    continue;

                // Verificar si la celda ya fue visitada
                if (visited.Contains((x, y)))
                    continue;

                // Verificar si la celda tiene el color objetivo
                if (Utils.cellMatrix[x, y].Background.ToString() != targetColor)
                    continue;

                // Pintar la celda con el color de la brocha
                Utils.ChangeCellColor(x, y, brushColor);

                // Marcar la celda como visitada
                visited.Add((x, y));

                // Expandir a las 4 direcciones
                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d];
                    int ny = y + dy[d];

                    // Agregar la celda vecina a la cola si no ha sido visitada
                    if (Utils.CheckRange(nx, ny) && !visited.Contains((nx, ny)))
                    {
                        queue.Enqueue((nx, ny));
                    }
                }
            }
        }
    }
    public class Variable : Instructions
    {
        public Token? Name;
        public Statement? Value;

        public override bool CheckSemantic(IScope scope)
        {
            bool check = true;

            if (Value != null && (!Value.CheckSemantic(scope)))
                check = false;

            if (Value is not null)
                scope.Define(this);

            return check;
        }

        public override void Evaluate(IVisitor visitor)
        {
            visitor.Define(Name?.Value, Value?.Evaluate(visitor.Scope, visitor));
        }
        public virtual object? Evaluate(IScope scope)
        {
            if (Value is not null)
            {
                return Value.Evaluate(scope);
            }
            return null;
        }

    }
    public class GoTo : Instructions
    {
        public Statement? Condition;
        public Token? Label_;
        public override bool CheckSemantic(IScope scope)
        {
            bool check = true;

            if (Condition != null && (!Condition.CheckSemantic(scope)))
            {
                check = false;
            }
            if (Label_ != null && (!Utils.keyLabelsReferences.ContainsKey(Label_.Value)))
            {
                Utils.Errors.Add($"Error Semántico: La etiqueta {Label_.Value} no existe en el contexto actual. Linea: {Label_.Line} Columna: {Label_.Column}");
                check = false;
            }
            return check;
        }
        public override void Evaluate(IVisitor visitor)
        {
            if (Condition != null)
            { 
                if (Convert.ToBoolean(Condition.Evaluate(visitor.Scope, visitor)))
                {
                    if (Label_ != null)
                    {
                        if (Utils.keyLabelsReferences.ContainsKey(Label_.Value))
                        {
                            Utils.keyLabelsReferences[Label_.Value.ToString()].Evaluate(visitor);
                            Utils.CycleCodition = true;
                        }
                        else
                            Utils.CycleCodition = false;
                    }
                }
            }
        }
    }
    public class Label : Instructions
    {
        public Token? Value;
        public Label? SubLabel;
        public List<Instructions?> Instructions = new List<Instructions?>();
        public override bool CheckSemantic(IScope scope)
        {
            bool check = true;
            foreach (var instruction in Instructions)
            {
                if ((instruction is not null) && (!instruction.CheckSemantic(scope)))
                    check = false;
            }
            if (SubLabel is not null && (!SubLabel.CheckSemantic(scope)))
                check = false;

            return check;
        }
        public override void Evaluate(IVisitor visitor)
        {  
            foreach (var instruction in Instructions)
            {
                instruction?.Evaluate(visitor);
                if (Utils.CycleCodition) break;
            }
            if (SubLabel is not null && !Utils.CycleCodition)
                SubLabel.Evaluate(visitor);      
        }
    }
}