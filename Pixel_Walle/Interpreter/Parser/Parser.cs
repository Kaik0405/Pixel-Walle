using Pixel_Walle.Interpreter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Walle
{
    public class Parser
    {
        private Token[] Tokens { get; }
        private Token? CurrentToken { get; set; }
        private int Index {  get; set; }
        //Builder
        public Parser(Token[] tokens) 
        {
            Tokens = tokens;
            CurrentToken = null;
            Index = -1;
        }
        //Methods
        public bool ThereIsNext(int i = 1)
        {
            if(Index + i < Tokens.Length)
                return true;
            return false;
        }//Verifica si hay proximo elemento
        private Token LookAhead(int i = 1)
        {
            if (ThereIsNext(i))
                return Tokens[Index + i];

            return Tokens[Index - 1];
        }//Retorna el proximo token pero sin avanzar el actual
        private bool LookAhead(bool chose, params Token.TokenType[] nextTokens)
        {
            if (ThereIsNext())
            {
                foreach (Token.TokenType item in nextTokens)
                {
                    if (item == LookAhead()?.Type)
                    {
                        if (chose)
                        {
                            this.CurrentToken = Tokens[++Index];
                            return true;
                        }
                        else return true;
                    }
                }
            }
            return false;
        }// Si chose es <true> y alguno de estos elementos coincide con el próximo, avanza y retorna true. Else retorna <true>
        private bool LookBeyond(params Token.TokenType[] nextTokens)
        {
            for (int i = 0; i < nextTokens.Length; i++)
                if (ThereIsNext(i + 1) && nextTokens[i] != LookAhead(i + 1)?.Type)
                    return false;

            return true;
        }       // Retorna <true> si los siguientes Tokens corresponden con la secuencia pasada por parámetro
    }
    public class Expression
    {
        public Token? Operator { get; set; }
        public Expression? Expressions { get; set; }
        public Term? Terms { get; set; }
        public double Evaluate()
        {
            if (Terms != null)
            {
                if (Expressions != null)
                {
                    double a = Terms.Evaluate();
                    double b = Expressions.Evaluate();

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

        public double Evaluate()
        {
            if (Factor != null)
            {
                if (Terms != null)
                {
                    double a = Factor.Evaluate();
                    double b = Terms.Evaluate();

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
        public Expression? Expressions { get; set; }
        public double Evaluate()
        {
            if (Value != null)
            {
                if (Value.Type == Token.TokenType.Digit)
                    return double.Parse(Value.Value);
                //else
                //return throw NotImplementedException();

            }
            else return Expressions.Evaluate();

            return 0;
        }
    }
}
