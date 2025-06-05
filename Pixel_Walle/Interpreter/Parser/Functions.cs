using Pixel_Walle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Walle
{
    public abstract class Functions
    {
        public abstract bool CheckSemantic(IScope scope);
        public abstract Utils.ReturnType GetType(IScope? scope);
    }
    public class GetActualX : Functions
    {
        public override bool CheckSemantic(IScope scope)
        {
            return true;
        }

        public override Utils.ReturnType GetType(IScope? scope)
        {
            return Utils.ReturnType.Number;
        }
    }
    public class GetActualY : Functions
    {
        public override bool CheckSemantic(IScope scope)
        {
            return true;
        }

        public override Utils.ReturnType GetType(IScope? scope)
        {
            return Utils.ReturnType.Number;
        }
    }
    public class GetCanvasSize : Functions
    {
        public override bool CheckSemantic(IScope scope)
        {
            return true;
        }

        public override Utils.ReturnType GetType(IScope? scope)
        {
            return Utils.ReturnType.Number;
        }
    }
    public class GetColorCount : Functions
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

            return check;
        }

        public override Utils.ReturnType GetType(IScope? scope)
        {
            return Utils.ReturnType.Number;
        }
    }
    public class IsBrushColor : Functions
    {
        public Token? Color;

        public override bool CheckSemantic(IScope scope)
        {
            return true;
        }

        public override Utils.ReturnType GetType(IScope? scope)
        {
            return Utils.ReturnType.Number;
        }
    }
    public class IsBrushSize : Functions
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

        public override Utils.ReturnType GetType(IScope? scope)
        {
            return Utils.ReturnType.Number;
        }
    }
    public class IsCanvasColor : Functions
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

            return check;
        }

        public override Utils.ReturnType GetType(IScope? scope)
        {
            return Utils.ReturnType.Number;
        }
    }
    public class IsColor : Functions
    {
        public Token? Color;
        public Statement? X;
        public Statement? Y;
        public override bool CheckSemantic(IScope scope)
        {
            bool check = true;

            if (X != null)
            {
                if (!Utils.CheckFunction("IsColor", X, scope))
                    check = false;
            }
            if (Y != null)
            {
                if (!Utils.CheckFunction("IsColor", Y, scope))
                    check = false;
            }

            return check;
        }
        public override Utils.ReturnType GetType(IScope? scope)
        {
            return Utils.ReturnType.Number;
        }
    }
}
