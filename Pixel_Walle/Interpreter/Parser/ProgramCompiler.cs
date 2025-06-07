using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Walle
{
    public class ProgramCompiler : ICheckSemantic
    {
        public List<Instructions?> Instructions { get; set; }

        public ProgramCompiler()
        {
            Instructions = new List<Instructions?>();
        }
        public bool CheckSemantic(IScope scope)
        {
            bool check = true;

            foreach (var instruction in Instructions)
                if ((instruction is not null) && (!instruction.CheckSemantic(scope)))
                    check = false;

            return check;
        }
        public void Evaluate(IScope scope)
        {
            foreach (var instruction in Instructions)
            {
                if (instruction is not null)
                    instruction.Evaluate(scope);
            }
        }
    }
}
