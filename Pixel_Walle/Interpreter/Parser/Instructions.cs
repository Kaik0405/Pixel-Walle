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
    }
    public class Spawn : Instructions
    {
        public Token? X;
        public Token? Y;
    }
    public class Color : Instructions
    {
        public Token? Value;
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
    public class GoTo : Instructions
    {
        public Statement? Condition;
        public Token? Label;
    }
    public class Label : Instructions
    {
        public Token? Value;
        public List<Instructions?> Instructions = new List<Instructions?>();
    }
}
