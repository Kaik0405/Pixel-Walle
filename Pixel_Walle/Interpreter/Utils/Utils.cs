using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Walle
{
    public static class Utils
    {
        public static List<string> Errors = new List<string>();
        public static double Operation(double a, double b, Token? value)
        {
            switch (value.Type)
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
                    
                    //else return throw NotImplementedException();
                    return 0;
                case Token.TokenType.Module:
                    return a % b;
                default:
                    return 0;

            }
        }
    }
}
