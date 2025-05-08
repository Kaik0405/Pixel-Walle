using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Walle
{
    public abstract class Instructions
    {
    }
    public class Spawn : Instructions
    {
        public Statement? X;
        public Statement? Y;
    }
    public class Color : Instructions 
    {
        public Statement? ColorString;
    }
    public class Size : Instructions
    {
        public Statement? K;
    }
    public class DrawLine : Instructions 
    {
        public Statement? DirX;
        public Statement? DirY;
        public Statement? Distance;

    }
    public class DrawCircle : Instructions
    {
        public Statement? DirX;
        public Statement? DirY;
        public Statement? Radius;
        
    }
    public class DrawRectangle : Instructions
    {
        public Statement? DirX;
        public Statement? DirY;
        public Statement? Distance;
        public Statement? Width;
        public Statement? Height;
    }
    public class Fill : Instructions
    {

    }
    public class Variable : Instructions
    {
        public Token? Name;
        public Statement? Value;
    }
}
