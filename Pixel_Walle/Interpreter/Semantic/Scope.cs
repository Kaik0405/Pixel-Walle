using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Walle
{
    public class Scope : IScope
    {
        public IScope? Parent { get; set; }
        public Dictionary<string, Statement?> Defined { get; set; }

        //Builder
        public Scope()
        {
            Defined = new Dictionary<string, Statement?>();
            Parent = null;
        }

        //Methods
        public IScope CreateChild()
        {
            Scope child = new Scope();
            child.Parent = this;

            return child;
        }
        public void Define(Variable variable)
        {
            if (!(variable.Name?.Value is null) && !(variable.Value is null) && !Defined.ContainsKey(variable.Name.Value))
                Defined.Add(variable.Name.Value, variable.Value);
        }
        public Utils.ReturnType? GetType(string? search, IScope scope)
        {
            if ((search != null) && scope.IsDefined(search))
            {
                if (scope.Defined.ContainsKey(search))
                    return scope.Defined[search]?.GetType(scope);

                else if (scope.Parent != null)
                    return scope.Parent.GetType(search, scope.Parent);
            }
            return null;
        }

        public bool IsDefined(string? search)
        {
            if (search is not null)
            {
                if (Defined.ContainsKey(search))
                    return true;

                else if (Parent != null)
                    return Parent.IsDefined(search);
            }

            return false;
        }
    }
}
