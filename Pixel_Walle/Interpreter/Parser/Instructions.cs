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
        public int? X;
        public int? Y;

        
    }
    public class Color : Instructions 
    {
        public int? ColorString;
    }
    public class Size : Instructions
    {
        public int? K;
    }
    public class DrawLine : Instructions 
    {
        public int? DirX;
        public int? DirY;
        public int? Distance;

    }
    public class DrawCircle : Instructions
    {
        public int? DirX;
        public int? DirY;
        public int? Radius;
        
    }
    public class DrawRectangle : Instructions
    {
        public int? DirX;
        public int? DirY;
        public int? Distance;
        public int? Width;
        public int? Height;
    }
    public class Fill : Instructions
    {

    }
}
