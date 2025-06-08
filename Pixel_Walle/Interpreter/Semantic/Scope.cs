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
    public class Visitor : IVisitor
    {
        // Property
        public IVisitor? Parent { get; set; }                   // Padre de este objeto en el árbol de Visitor
        public IScope? Scope { get; set; }                      // Scope asociado al alcance (Máscara)
        public Dictionary<string, object> Defined { get; set; } // Variables almacenadas (nombre y valores)
        public List<string> IncreaseVariables { get; set; }    // Variables a incrementadas dentro del alcance

        // Builder
        public Visitor()
        {
            this.IncreaseVariables = new List<string>();
            this.Defined = new Dictionary<string, object>();
            this.Parent = null;
        }
        public Visitor(IScope? scope)
        {
            this.IncreaseVariables = new List<string>();
            this.Scope = scope;
            this.Defined = new Dictionary<string, object>();
            this.Parent = null;
        }

        // Methods
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
        }               // Verifica si la variable está almacenada
        public void Define(Variable variable)
        {
            string? name = variable.Name?.Value;
            Statement? value = variable.Value;

            if (!(name is null) && !(value is null))
            {
                if (!Defined.ContainsKey(name))
                    Defined.Add(name, value);
                else
                    Defined[name] = value;
            }
        }               // Agrega la variable
        public void Define(string? name, object? value)
        {
            if (!(name is null) && !(value is null))
            {
                if (!Defined.ContainsKey(name))
                    Defined.Add(name, value);
                else
                    Defined[name] = value;
            }
        }     // Agrega la variable (nombre y valor)
        public object? GetValue(string? search)
        {
            if ((search != null) && this.IsDefined(search))
            {
                if (this.Defined.ContainsKey(search))
                    return this.Defined[search];

                else if (this.Parent != null)
                    return this.Parent.GetValue(search);
            }

            return null;
        }             // Devuelve el valor específico de un tipo (variable)
        public IVisitor CreateChild()
        {
            Visitor child = new Visitor();
            child.Parent = this;

            return child;
        }                       // Retorna un nuevo hijo de este Visitor
        public IVisitor CreateChild(IScope? scope)
        {
            Visitor child = new Visitor(scope);
            child.Parent = this;

            return child;
        }          // Retorna un hijo de este objeto y le asigna un Scope
        public void AddIncrease()
        {
            foreach (string variable in IncreaseVariables)
            {
                int value = Convert.ToInt32(GetValue(variable));
                Define(variable, value + 1);
            }
        }                           // Agrega incremento a las con "++" dentro del alcance
        public void AddInstance()
        {
            if (Scope is not null)
            {
                Dictionary<string, Statement?> instance = Scope.Defined;

                foreach (string variable in instance.Keys)
                    Define(variable, instance[variable]?.Evaluate(Scope));
            }
        }                           // Agrega las variables del Scope
    }        // Alcance de las variables (Proceso para Evaluar)   
}
