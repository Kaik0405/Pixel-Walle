using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Walle
{
    public interface IParsing
    {
        public ProgramCompiler Parsing();
    }
    public interface ICheckSemantic
    {
        public bool CheckSemantic(IScope scope);
    }
    public interface IScope
    {
        //Properties
        public IScope? Parent { get; set; } //Padre de este objeto en el arbol del Scope
        public Dictionary<string, Statement?> Defined { get; set; } //Variables almacenadas nombres y valores

        // Methods
        public bool IsDefined(string? search);                                  // Verifica si la variable está almacenada
        public void Define(Variable variable);                                  // Agrega la variable
        public IScope CreateChild();                                            // Retorna un nuevo hijo de este Scope
        public Utils.ReturnType? GetType(string? search, IScope scope);         // Retorna el tipo de variable (String, Bool, Digit)
    }
}
