using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Walle.Interpreter.Evaluate
{
    public class Wall_E
    {
        public int PosX { get; set; }
        public int PosY { get; set; }
        public string PaintBrush { get; set; } // Color de Brocha
        public int WidthPaint { get; set; }    // Ancho de Brocha
        public Wall_E(int posX,int posY,string paint)
        {
            PosX = posX;
            PosY = posY;
            PaintBrush = paint;
            WidthPaint = 1;
        }


    }
}
