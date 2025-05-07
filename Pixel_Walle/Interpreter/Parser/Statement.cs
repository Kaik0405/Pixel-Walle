using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Walle
{
    public class Statement
    {
        public SubStatement? SubState { get; set; }
        public Statement? State { get; set; }
        public object? Evaluate()
        {
            if (SubState != null)
            {
                if (State != null)
                {
                    return Convert.ToBoolean(SubState.Evaluate()) && Convert.ToBoolean(State.Evaluate());
                }
                return SubState.Evaluate();
            }
            return null;
        }
    }
    public class SubStatement
    {
        public Molecule? Mol { get; set; }
        public SubStatement? SubState { get; set; }

        public object? Evaluate()
        {
            if (Mol != null)
            {
                if (SubState != null)
                {
                    return Convert.ToBoolean(Mol.Evaluate()) || Convert.ToBoolean(SubState.Evaluate());
                }
                return Mol.Evaluate();
            }
            return null;
        }
    }
    public class Molecule
    {
        public Atom? Atoms { get; set; }
        public Molecule? Mol { get; set; }
        public Token? Symbol { get; set; }

        public object? Evaluate()
        {
            if (Atoms != null)
            {
                if (Mol != null)
                {
                    return Utils.Compare(Convert.ToDouble(Atoms.Evaluate()), Convert.ToDouble(Mol.Evaluate()), Symbol);
                }
                else return Atoms.Evaluate();
            }
            return null;
        }
    }
    public abstract class Atom
    {
        public abstract object? Evaluate();
    }
    public class Boolean : Atom
    {
        public Token? Value { get; set; }
        public override object? Evaluate()
        {
            if (Value != null)
                return Convert.ToBoolean(Value.Value);

            return null;
        }
    }
}
