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
        public override bool CheckSemantic(IScope scope)
        {
            if (Terms != null && Expressions != null)
            {
                if (!Terms.CheckSemantic(scope) || !Expressions.CheckSemantic(scope))
                    return false;

                if (Terms.GetType(scope) != Expressions.GetType(scope))
                {
                    Utils.Errors.Add($"Error Semántico: No se puede operar un valor de tipo \"{Terms.GetType(scope)}\" con un \"{Expressions.GetType(scope)}\" usando el operador \"{Operator?.Value}\". Linea: {Operator?.Line} Columna: {Operator?.Column}");
                    return false;
                }
                if (Terms.GetType(scope) == Utils.ReturnType.Bool && Expressions.GetType(scope) == Utils.ReturnType.Bool)
                {
                    Utils.Errors.Add($"Error Semántico: No se puede operar entre tipos \"{Terms.GetType(scope)}\" usando el operador \"{Operator?.Value}\". Linea: {Operator?.Line} Columna: {Operator?.Column}");
                    return false;
                }
            }
            else if (Terms != null)
            {
                if (!Terms.CheckSemantic(scope))
                    return false;
            }
            return true;
        }
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

        public override Utils.ReturnType? GetType(IScope? scope)
        {
            if (Terms is not null)
                return Terms.GetType(scope);

            return null;
        }
    }
    public class Term : ICheckSemantic
    {
        public Token? Operator { get; set; }
        public Term? Terms { get; set; }
        public Factor? Factor { get; set; }
        public Utils.ReturnType? GetType(IScope? scope)
        {
            if (Factor is not null)
                return Factor.GetType(scope);

            return null;
        }
        public bool CheckSemantic(IScope scope)
        {
            if (Factor is not null && Terms is not null)
            {
                if (!Factor.CheckSemantic(scope) || !Terms.CheckSemantic(scope))
                    return false;

                if (Factor.GetType(scope) != Terms.GetType(scope))
                {
                    Utils.Errors.Add($"Error Semántico: No se puede operar un valor de tipo \"{Factor.GetType(scope)}\" con un \"{Terms.GetType(scope)}\" usando el operador \"{Operator?.Value}\". Linea: {Operator?.Line} Columna: {Operator?.Column}");
                    return false;
                }
                if (Factor.GetType(scope) == Utils.ReturnType.Bool && Terms.GetType(scope) == Utils.ReturnType.Bool)
                {
                    Utils.Errors.Add($"Error Semántico: No se puede operar entre tipos \"{Factor.GetType(scope)}\" usando el operador \"{Operator?.Value}\". Linea: {Operator?.Line} Columna: {Operator?.Column}");
                    return false;
                }
            }
            else if (Factor is not null)
            {
                if (!Factor.CheckSemantic(scope))
                    return false;
            }
            return true;
        }
        public object? Evaluate()
        {
            if (Factor != null)
            {
                if (Terms != null)
                {
                    double a = Convert.ToDouble(Factor.Evaluate());
                    double b = Convert.ToDouble(Terms.Evaluate());

                    return Utils.Operation(a, b, Operator);
                }
                else
                    return Factor.Evaluate();
            }
            return 0;
        }
    }
    public class Factor : ICheckSemantic
    {
        public Token? Value { get; set; }
        public Functions? Functions { get; set; }
        public Expression? Expressions { get; set; }
        public Utils.ReturnType? GetType(IScope? scope)
        {
            if (Expressions is not null)
            {
                return Expressions.GetType(scope);
            }
            else if (Value?.Type == Token.TokenType.UnKnown)
            {
                if (!(scope is null) && scope.IsDefined(Value.Value))
                    return scope.GetType(Value.Value, scope);

                else
                    return Utils.ReturnType.NULL;
            }
            else if (Functions is not null)
            {
                return Functions.GetType(scope);
            }
            return Utils.ReturnType.Number;
        }
        public bool CheckSemantic(IScope scope)
        {
            if (Expressions is not null && (!Expressions.CheckSemantic(scope)))
            {
                return false;
            }
            else if (Functions is not null && (!Functions.CheckSemantic(scope)))
            {
                return false;
            }
            else
            {
                if (Value?.Type == Token.TokenType.UnKnown)
                {
                    if (!scope.IsDefined(Value.Value))
                    {
                        Utils.Errors.Add($"Error Semántico: La variable \"{Value.Value}\" no existe en el contexto actual. Linea: {Value.Line} Columna: {Value.Column}");
                        return false;
                    }
                }
                return true;
            }
        }
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
