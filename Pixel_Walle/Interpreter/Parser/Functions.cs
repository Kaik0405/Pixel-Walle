using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Walle
{
    public abstract class Functions
    {

    }
    public class GetActualX : Functions
    {

    }
    public class GetActualY : Functions
    {
           
    }
    public class GetCanvasSize : Functions
    {
        
    }
    public class GetColorCount : Functions
    {
        public Statement? Color;
        public Statement? X1;
        public Statement? Y1;
        public Statement? X2;
        public Statement? Y2;
    }
    public class IsBrushColor : Functions
    {
        public Statement? Color;
    }
    public class IsBrushSize : Functions
    {
        public Statement? Size;
    }
    public class IsCanvasColor : Functions
    {
        public Statement? Color;
        public Statement? Vertical;
        public Statement? Horizontal;
    }
}
