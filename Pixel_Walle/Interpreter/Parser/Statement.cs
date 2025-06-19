using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Walle
{
    public class Statement : ICheckSemantic // Clase que representa una declaración en el lenguaje (&&)
    {
        public SubStatement? SubState { get; set; }
        public Statement? State { get; set; }
        public Token? Symbol { get; set; }
        public bool CheckSemantic(IScope scope)
        {
            if (SubState != null && State != null)
            {
                if (!SubState.CheckSemantic(scope) || !State.CheckSemantic(scope))
                {
                    return false;
                }
                if (SubState.GetType(scope) != State.GetType(scope))
                {
                    Utils.Errors.Add($"Error Semántico: No se puede establecer una relación entre tipos \"{SubState.GetType(scope)}\" y \"{State.GetType(scope)}\" mediante el operador \"&&\". Linea: {Symbol?.Line} Column: {Symbol?.Column} ");
                    return false;
                }
                if (SubState.GetType(scope) == Utils.ReturnType.Number && State.GetType(scope) == Utils.ReturnType.Number)
                {
                    Utils.Errors.Add($"Error Semántico: No se puede establecer una relación entre tipos \"{SubState.GetType(scope)}\" y \"{State.GetType(scope)}\" mediante el operador \"&&\" . Linea: {Symbol?.Line} Column: {Symbol?.Column}");
                    return false;
                }
            }
            else if (SubState != null)
            {
                if (!SubState.CheckSemantic(scope))
                    return false;

            }
            return true;
        }
        public object? Evaluate(IScope? scope,IVisitor? visitor = null)
        {
            if (SubState != null)
            {
                if (State != null)
                {
                    return Convert.ToBoolean(SubState.Evaluate(scope,visitor)) && Convert.ToBoolean(State.Evaluate(scope,visitor));
                }
                return SubState.Evaluate(scope, visitor);
            }
            return null;
        }
        public Utils.ReturnType? GetType(IScope scope)
        {
            if (SubState != null)
                return SubState.GetType(scope);

            return null;
        }
    }
    public class SubStatement : ICheckSemantic // Clase que representa una sub-declaración en el lenguaje (||)
    {
        public Molecule? Mol { get; set; }
        public SubStatement? SubState { get; set; }
        public Token? Symbol { get; set; }
        public bool CheckSemantic(IScope scope)
        {
            if (Mol != null && SubState != null)
            {
                if (!Mol.CheckSemantic(scope) || !SubState.CheckSemantic(scope))
                    return false;
                if (Mol.GetType(scope) != SubState.GetType(scope))
                {
                    Utils.Errors.Add($"Error Semántico: No se puede establecer una relación entre tipos \"{Mol.GetType(scope)}\" y \"{SubState.GetType(scope)}\" mediante el operador \"||\". Linea: {Symbol?.Line} Column: {Symbol?.Column}");
                    return false;
                }
                if (Mol.GetType(scope) == Utils.ReturnType.Number && SubState.GetType(scope) == Utils.ReturnType.Number)
                {
                    Utils.Errors.Add($"Error Semántico: No se puede establecer una relación entre tipos \"{Mol.GetType(scope)}\" y \"{SubState.GetType(scope)}\" mediante el operador \"||\". Linea: {Symbol?.Line} Column: {Symbol?.Column}");
                    return false;
                }
            }
            else if (Mol != null)
            {
                if (!Mol.CheckSemantic(scope))
                {
                    return false;
                }
            }
            return true;
        }
        public object? Evaluate(IScope? scope,IVisitor? visitor = null)
        {
            if (Mol != null)
            {
                if (SubState != null)
                {
                    return Convert.ToBoolean(Mol.Evaluate(scope,visitor)) || Convert.ToBoolean(SubState.Evaluate(scope,visitor));
                }
                return Mol.Evaluate(scope,visitor);
            }
            return null;
        }
        public Utils.ReturnType? GetType(IScope scope)
        {
            if (Mol != null)
                return Mol.GetType(scope);

            return null;
        }
    }
    public class Molecule : ICheckSemantic // Clase que representa una molécula en el lenguaje (==, !=, <, >, <=, >=)
    {
        public Atom? Atoms { get; set; }
        public Molecule? Mol { get; set; }
        public Token? Symbol { get; set; }
        public bool CheckSemantic(IScope scope)
        {
            bool check = true;

            if (Atoms != null && Mol != null)
            {
                if (!Atoms.CheckSemantic(scope) || !Mol.CheckSemantic(scope))
                    check = false;

                if (Atoms.GetType(scope) == Mol.GetType(scope))
                {
                    Utils.ReturnType? type = Atoms.GetType(scope);

                    if (Symbol?.Type != Token.TokenType.Equal && type == Utils.ReturnType.Bool)
                    {
                        check = false;
                        Utils.Errors.Add($"Error Semántico: No se puede establecer una relación entre tipos \"{type}\" mediante el operador \"{Symbol?.Value}\". Linea: {Symbol?.Line} Columna: {Symbol?.Column}");
                    }
                }
                else
                {
                    check = false;
                    Utils.Errors.Add($"Error Semántico: No se puede establecer una relación entre tipos \"{Atoms.GetType(scope)}\" y \"{Mol.GetType(scope)}\" mediante el operador \"{Symbol?.Value}\". Linea: {Symbol?.Line} Columna: {Symbol?.Column}");
                }
            }
            else if (Atoms != null)
            {
                if (!Atoms.CheckSemantic(scope))
                    check = false;

            }
            return check;
        }
        public object? Evaluate(IScope? scope, IVisitor? visitor = null)
        {
            if (Atoms != null)
            {
                if (Mol != null)
                {
                    return Utils.Compare(Convert.ToDouble(Atoms.Evaluate(scope,visitor)), Convert.ToDouble(Mol.Evaluate(scope, visitor)), Symbol);
                }
                else return Atoms.Evaluate(scope, visitor);
            }
            return null;
        }
        public Utils.ReturnType? GetType(IScope scope)
        {
            if (Atoms != null && Mol != null)
                return Utils.ReturnType.Bool;
            if (Atoms != null)
                return Atoms.GetType(scope);
            
            return Utils.ReturnType.NULL;
        }
    }
    public abstract class Atom // Clase abstracta que representa un átomo en el lenguaje (números, booleanos, variables, etc.)
    {
        public abstract object? Evaluate(IScope? scope, IVisitor? visitor = null);
        public abstract bool CheckSemantic(IScope scope);
        public abstract Utils.ReturnType? GetType(IScope scope);
    }
    public class Boolean : Atom // Clase que representa un valor booleano en el lenguaje
    {
        public Token? Value { get; set; }
        public override bool CheckSemantic(IScope scope)
        {
            return true;
        }
        public override object? Evaluate(IScope? scope, IVisitor? visitor = null)
        {
            if (Value != null)
                return Convert.ToBoolean(Value.Value);

            return null;
        }
        public override Utils.ReturnType? GetType(IScope scope)
        {
            return Utils.ReturnType.Bool;
        }
    }
}
