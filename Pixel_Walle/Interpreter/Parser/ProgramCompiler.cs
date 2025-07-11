﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Walle
{
    public class ProgramCompiler : ICheckSemantic // Clase principal del AST (Nodo Raíz)
    {
        public List<Instructions?> Instructions { get; set; } // Lista de instrucciones
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
        public void Evaluate(IVisitor scope)
        {
            foreach (var instruction in Instructions)
            {
                if (instruction is not null)
                    instruction.Evaluate(scope);
            }
        }
    }
}
