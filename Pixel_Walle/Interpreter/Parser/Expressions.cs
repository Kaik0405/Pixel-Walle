using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Walle
{
    public class Expression : Atom
    {
        public Token? Operator { get; set; }
        public Expression? Expressions { get; set; }
        public Term? Terms { get; set; }
        public override object? Evaluate()
        {
            if (Terms != null)
            {
                if (Expressions != null)
                {
                    double a = Convert.ToDouble(Terms.Evaluate());
                    double b = Convert.ToDouble(Expressions.Evaluate());

                    return Utils.Operation(a, b, Operator);
                }
                else
                    return Terms.Evaluate();

            }
            return 0;
        }
    }
    public class Term
    {
        public Token? Value { get; set; }
        public Term? Terms { get; set; }
        public Factor? Factor { get; set; }

        public object? Evaluate()
        {
            if (Factor != null)
            {
                if (Terms != null)
                {
                    double a = Convert.ToDouble(Factor.Evaluate());
                    double b = Convert.ToDouble(Terms.Evaluate());

                    return Utils.Operation(a, b, Value);
                }
                else
                    return Factor.Evaluate();

            }
            return 0;
        }
    }
    public class Factor
    {
        public Token? Value { get; set; }
        public Variable? Variable { get; set; }
        public Functions? Functions { get; set; }
        public Expression? Expressions { get; set; }
        public object? Evaluate()
        {
            if (Value != null)
            {
                if (Value.Type == Token.TokenType.Digit)
                    return double.Parse(Value.Value);

                else
                {
                    string mensajeError = $"El carácter de la línea {Value.Line} y columna {Value.Column} no se encuentra declarado o no es válido";
                    Utils.Errors.Add(mensajeError);
                    throw new ArgumentException(mensajeError);
                }

            }
            else return Expressions?.Evaluate();
        }
    }
}
