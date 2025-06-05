using Pixel_Walle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Pixel_Walle
{
    public abstract class Instructions
    {
        public abstract bool CheckSemantic(IScope scope);
    }
    public class Spawn : Instructions
    {
        public Token? X;
        public Token? Y;

        public override bool CheckSemantic(IScope scope)
        {
            return true;
        }
    }
    public class Color : Instructions
    {
        public Token? Value;

        public override bool CheckSemantic(IScope scope)
        {
            return true;
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
    }
    public class Fill : Instructions
    {
        public override bool CheckSemantic(IScope scope)
        {
            return true;
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
    }
    public class GoTo : Instructions
    {
        public Statement? Condition;
        public Token? Label;

        public override bool CheckSemantic(IScope scope)
        {
            bool check = true;

            if (Condition != null && (!Condition.CheckSemantic(scope)))
            {
                check = false;
            }
            if (Label != null && (!Utils.keyLabelsReferences.ContainsKey(Label.Value)))
            {
                Utils.Errors.Add($"Error Semántico: La etiqueta {Label.Value} no existe en el contexto actual. Linea: {Label.Line} Columna: {Label.Column}");
                check = false;
            }
            return check;
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
    }
}
