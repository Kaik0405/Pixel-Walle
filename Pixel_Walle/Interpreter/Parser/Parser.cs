using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Pixel_Walle
{
    public class Parser
    {
        private Token[] Tokens { get; }
        private Token? CurrentToken { get; set; }
        private int Index { get; set; }
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
            if (Index + i < Tokens.Length)
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
        private void Match()
        {
            if (ThereIsNext())
                CurrentToken = Tokens[++Index];
        }                // Avanza sin importar el siguiente Token
        private void Match(params Token.TokenType?[] nextTokens)
        {
            foreach (Token.TokenType? item in nextTokens)
            {
                if (item == LookAhead()?.Type)
                    this.CurrentToken = Tokens[++Index];
                else
                {
                    Utils.Errors.Add($"Error: No se esperaba un \"{LookAhead()?.Value}\" Line: {LookAhead()?.Line}, Column: {LookAhead()?.Column}");
                    if (ThereIsNext())
                        this.CurrentToken = Tokens[++Index];
                }
            }
        }                // Avanza en el orden de parámetros de entrada
        private Token? MatchReturn(params Token.TokenType[] nextTokens)
        {
            if (nextTokens.Length != 0)
            {
                foreach (Token.TokenType item in nextTokens)
                {
                    if (item == LookAhead()?.Type)
                    {
                        this.CurrentToken = Tokens[++Index];
                        return CurrentToken;
                    }
                }
                Utils.Errors.Add($"Error: No se esperaba un \"{LookAhead()?.Value}\" Line: {LookAhead()?.Line}, Column: {LookAhead()?.Column}");
                if (ThereIsNext())
                    this.CurrentToken = Tokens[++Index];
            }
            else
            {
                this.CurrentToken = Tokens[++Index];
                return CurrentToken;
            }

            return null;
        }         // Retorna uno de los coincidentes

        //Arithmetic Expressions
        public Expression ExpressionBuilder()
        {
            Expression expression = new Expression();
            expression.Terms = TermBuilder();

            if (LookAhead(false, Token.TokenType.Plus, Token.TokenType.Minus))
            {
                expression.Operator = MatchReturn();
                expression.Expressions = ExpressionBuilder();
            }

            return expression;
        }
        private Term TermBuilder()
        {
            Term term = new Term();
            term.Factor = FactorBuilder();
            if (LookAhead(false, Token.TokenType.Times, Token.TokenType.Divide, Token.TokenType.Pow, Token.TokenType.Module))
            {
                term.Value = MatchReturn();
                term.Terms = TermBuilder();
            }
            return term;
        }
        private Factor FactorBuilder()
        {
            Factor factor = new Factor();
            if (LookAhead(false, Token.TokenType.Digit))
            {
                factor.Value = MatchReturn();
            }
            else if (LookAhead(false, Token.TokenType.OpenParan))
            {
                Match();
                factor.Expressions = ExpressionBuilder();
                Match(Token.TokenType.ClosedParan);
            }
            else
            {
                //variable implementation
            }
            return factor;
        }

        //Statements
        public Statement StatementBuilder()
        {
            Statement statement = new Statement();

            if (LookAhead()?.Type == Token.TokenType.OpenParan)
            {
                Match();
                statement.SubState = SubStatementBuilder();

                if (LookAhead(true, Token.TokenType.ClosedParan))
                {
                    if (LookAhead(true, Token.TokenType.AND))
                        statement.State = StatementBuilder();
                }
                else if (LookAhead(true, Token.TokenType.AND))
                {
                    statement.State = StatementBuilder();
                    Match(Token.TokenType.ClosedParan);
                }
                else Utils.Errors.Add($@"Error: Se esperaba un ')' Linea: {LookAhead()?.Line}, Columna: {LookAhead()?.Column}");
            }
            else
            {
                statement.SubState = SubStatementBuilder();
                if (LookAhead(true, Token.TokenType.AND))
                    statement.State = StatementBuilder();
            }

            return statement;
        }
        public SubStatement SubStatementBuilder()
        {
            SubStatement subStatement = new SubStatement();

            if (LookAhead()?.Type == Token.TokenType.OpenParan)
            {
                Match();
                subStatement.Mol = MoleculeBuilder();

                if (LookAhead(true, Token.TokenType.ClosedParan))
                {
                    if (LookAhead(true, Token.TokenType.OR))
                        subStatement.SubState = SubStatementBuilder();
                }
                else if (LookAhead(true, Token.TokenType.OR))
                {
                    subStatement.SubState = SubStatementBuilder();
                    Match(Token.TokenType.ClosedParan);
                }
                else Utils.Errors.Add($@"Error: Se esperaba un ')' Linea: {LookAhead()?.Line}, Columna: {LookAhead()?.Column}");
            }
            else
            {
                subStatement.Mol = MoleculeBuilder();
                if (LookAhead(true, Token.TokenType.OR))
                    subStatement.SubState = SubStatementBuilder();
            }

            return subStatement;
        }
        public Molecule MoleculeBuilder()
        {
            Molecule molecule = new Molecule();

            if (LookAhead()?.Type == Token.TokenType.OpenParan)
            {
                Match();
                molecule.Atoms = AtomBuilder();

                if (LookAhead(true, Token.TokenType.ClosedParan))
                {
                    if (LookAhead(false, Token.TokenType.Equal, Token.TokenType.GreaterThan, Token.TokenType.LessThan, Token.TokenType.LessThanEqual, Token.TokenType.GreaterThanEqual))
                    {
                        molecule.Symbol = MatchReturn();
                        molecule.Mol = MoleculeBuilder();
                    }
                }
                else if (LookAhead(false, Token.TokenType.Equal, Token.TokenType.GreaterThan, Token.TokenType.LessThan, Token.TokenType.LessThanEqual, Token.TokenType.GreaterThanEqual))
                {
                    molecule.Symbol = MatchReturn();
                    molecule.Mol = MoleculeBuilder();
                }
                else Utils.Errors.Add($@"Error: Se esperaba un ')' Linea: {LookAhead()?.Line}, Columna: {LookAhead()?.Column}");
            }
            else
            {
                molecule.Atoms = AtomBuilder();

                if (LookAhead(false, Token.TokenType.Equal, Token.TokenType.GreaterThan, Token.TokenType.LessThan, Token.TokenType.LessThanEqual, Token.TokenType.GreaterThanEqual))
                {
                    molecule.Symbol = MatchReturn();
                    molecule.Mol = MoleculeBuilder();
                }
            }

            return molecule;
        }
        private Atom AtomBuilder()
        {
            if (LookAhead(false, Token.TokenType.True, Token.TokenType.False))
                return BooleanBuilder();
            else
                return ExpressionBuilder();
        }
        public Boolean BooleanBuilder()
        {
            Boolean boolean = new Boolean();

            boolean.Value = MatchReturn(Token.TokenType.True, Token.TokenType.False);

            return boolean;
        }

        //Instructions
        public Spawn SpawnBuilder()
        {
            Spawn spawn = new Spawn();

            Match(Token.TokenType.Spawn);

            Match(Token.TokenType.OpenParan);

            spawn.X = Convert.ToInt32(MatchReturn(Token.TokenType.Digit));
            Match(Token.TokenType.Comma);
            spawn.Y = Convert.ToInt32(MatchReturn(Token.TokenType.Digit));


            Match(Token.TokenType.ClosedParan);

            return spawn;
        }
        public Size SizeBuilder()
        {
            Size size = new Size();
            Match(Token.TokenType.Size);

            Match(Token.TokenType.OpenParan);
            size.K = Convert.ToInt32(MatchReturn(Token.TokenType.Digit));

            Match(Token.TokenType.ClosedParan);

            return size;
        }
        public DrawLine DrawLineBuilder()
        {
            DrawLine drawLine = new DrawLine();
            Match(Token.TokenType.DrawLine);

            Match(Token.TokenType.OpenParan);

            drawLine.DirX = Convert.ToInt32(MatchReturn(Token.TokenType.Digit));
            Match(Token.TokenType.Comma);
            drawLine.DirY = Convert.ToInt32(MatchReturn(Token.TokenType.Digit));
            Match(Token.TokenType.Comma);
            drawLine.Distance = Convert.ToInt32(MatchReturn(Token.TokenType.Digit));

            Match(Token.TokenType.ClosedParan);

            return drawLine;
        }
        public DrawCircle DrawCircleBuilder()
        {
            DrawCircle drawCircle = new DrawCircle();
            Match(Token.TokenType.DrawCircle);

            Match(Token.TokenType.OpenParan);

            drawCircle.DirX = Convert.ToInt32(MatchReturn(Token.TokenType.Digit));
            Match(Token.TokenType.Comma);
            drawCircle.DirY = Convert.ToInt32(MatchReturn(Token.TokenType.Digit));
            Match(Token.TokenType.Comma);
            drawCircle.Radius = Convert.ToInt32(MatchReturn(Token.TokenType.Digit));

            Match(Token.TokenType.ClosedParan);

            return drawCircle;
        }
        public DrawRectangle DrawRectangleBuilder()
        {
            DrawRectangle drawRectangle = new DrawRectangle();
            Match(Token.TokenType.DrawRectangle);

            Match(Token.TokenType.OpenParan);

            drawRectangle.DirX = Convert.ToInt32(MatchReturn(Token.TokenType.Digit));
            Match(Token.TokenType.Comma);
            drawRectangle.DirY = Convert.ToInt32(MatchReturn(Token.TokenType.Digit));
            Match(Token.TokenType.Comma);
            drawRectangle.Distance = Convert.ToInt32(MatchReturn(Token.TokenType.Digit));
            Match(Token.TokenType.Comma);
            drawRectangle.Width = Convert.ToInt32(MatchReturn(Token.TokenType.Digit));
            Match(Token.TokenType.Comma);
            drawRectangle.Height = Convert.ToInt32(MatchReturn(Token.TokenType.Digit));

            Match(Token.TokenType.ClosedParan);

            return drawRectangle;
        }
        public Fill FillBuilder()
        {
            Fill fill = new Fill();

            Match(Token.TokenType.Fill);
            Match(Token.TokenType.OpenParan);
            Match(Token.TokenType.ClosedParan);

            return fill;
        }

        //Functions


    }
}
