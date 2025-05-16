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

        //ProgramCompiler
        public ProgramCompiler Parsing()
        {
            ProgramCompiler programCompiler = new ProgramCompiler();
            SpawnBuilder();

            while (Index < Tokens.Length)
                programCompiler.Instructions.Add(InstructionsBuilder());
            
            return programCompiler;
        }
        public Instructions InstructionsBuilder()
        {
            if (LookAhead(false, Token.TokenType.Size))
                return SizeBuilder();
            else if (LookAhead(false, Token.TokenType.DrawLine))
                return DrawLineBuilder();
            else if (LookAhead(false, Token.TokenType.DrawCircle))
                return DrawCircleBuilder();
            else if (LookAhead(false, Token.TokenType.DrawRectangle))
                return DrawRectangleBuilder();
            else if (LookAhead(false, Token.TokenType.Fill))
                return FillBuilder();
            else if (LookAhead(false, Token.TokenType.GoTo))
                return GoToBuilder();
            else
            {
                if (LookAhead(false, Token.TokenType.UnKnown))
                {
                    for (int i = 2; i < Tokens.Length; i++)
                        if (Tokens[Index + i].Type == Token.TokenType.Assignment)
                            return VariableBuilder();
                }
                return LabelBuilder();
            }
        }

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
        public Term TermBuilder()
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
        public Factor FactorBuilder()
        {
            Factor factor = new Factor();
            
            if (LookAhead(false, Token.TokenType.OpenParan))
            {
                Match();
                factor.Expressions = ExpressionBuilder();
                Match(Token.TokenType.ClosedParan);
            }
            else    // hard-pes-caado <- 
            {
                if (LookAhead(false, Token.TokenType.Digit))
                    factor.Value = MatchReturn();
                
                else if (LookAhead(false, Token.TokenType.UnKnown))
                    factor.Variable = VariableOfValueBuilder();

                else if (Utils.FunctionList.Contains(LookAhead().Type))
                    factor.Functions = GetActualXBuilder();
            }
            return factor;
        }

        //Statements
        public Statement StatementBuilder()
        {
            Statement statement = new Statement();

            statement.SubState = SubStatementBuilder();
            if (LookAhead(true, Token.TokenType.AND))
                statement.State = StatementBuilder();

            return statement;
        }
        public SubStatement SubStatementBuilder()
        {
            SubStatement subStatement = new SubStatement();

            subStatement.Mol = MoleculeBuilder();
            if (LookAhead(true, Token.TokenType.OR))
                subStatement.SubState = SubStatementBuilder();

            return subStatement;
        }
        public Molecule MoleculeBuilder()
        {
            Molecule molecule = new Molecule();

            molecule.Atoms = AtomBuilder();

            if (LookAhead(false, Token.TokenType.Equal, Token.TokenType.GreaterThan, Token.TokenType.LessThan, Token.TokenType.LessThanEqual, Token.TokenType.GreaterThanEqual))
            {
                molecule.Symbol = MatchReturn();
                molecule.Mol = MoleculeBuilder();
            }

            return molecule;
        }
        public Atom AtomBuilder()
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

            spawn.X = StatementBuilder();
            Match(Token.TokenType.Comma);
            spawn.Y = StatementBuilder();


            Match(Token.TokenType.ClosedParan);

            return spawn;
        }
        public Size SizeBuilder()
        {
            Size size = new Size();
            Match(Token.TokenType.Size);

            Match(Token.TokenType.OpenParan);
            size.K = StatementBuilder();

            Match(Token.TokenType.ClosedParan);

            return size;
        }
        public DrawLine DrawLineBuilder()
        {
            DrawLine drawLine = new DrawLine();
            Match(Token.TokenType.DrawLine);

            Match(Token.TokenType.OpenParan);

            drawLine.DirX = StatementBuilder();
            Match(Token.TokenType.Comma);
            drawLine.DirY = StatementBuilder();
            Match(Token.TokenType.Comma);
            drawLine.Distance = StatementBuilder();

            Match(Token.TokenType.ClosedParan);

            return drawLine;
        }
        public DrawCircle DrawCircleBuilder()
        {
            DrawCircle drawCircle = new DrawCircle();
            Match(Token.TokenType.DrawCircle);

            Match(Token.TokenType.OpenParan);

            drawCircle.DirX = StatementBuilder();
            Match(Token.TokenType.Comma);
            drawCircle.DirY = StatementBuilder();
            Match(Token.TokenType.Comma);
            drawCircle.Radius = StatementBuilder();

            Match(Token.TokenType.ClosedParan);

            return drawCircle;
        }
        public DrawRectangle DrawRectangleBuilder()
        {
            DrawRectangle drawRectangle = new DrawRectangle();
            Match(Token.TokenType.DrawRectangle);

            Match(Token.TokenType.OpenParan);

            drawRectangle.DirX = StatementBuilder();
            Match(Token.TokenType.Comma);
            drawRectangle.DirY = StatementBuilder();
            Match(Token.TokenType.Comma);
            drawRectangle.Distance = StatementBuilder();
            Match(Token.TokenType.Comma);
            drawRectangle.Width = StatementBuilder();
            Match(Token.TokenType.Comma);
            drawRectangle.Height = StatementBuilder();

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
        public Variable VariableBuilder()
        {
            Variable variable = new Variable();

            if (LookAhead(false, Token.TokenType.UnKnown)) 
                variable.Name = MatchReturn()?.ToString();
            
            
            while (!LookAhead(false, Token.TokenType.Assignment))
            {
                if (LookAhead(false, Token.TokenType.Minus))
                    variable.Name += MatchReturn()?.ToString();
                
                variable.Name += MatchReturn(Token.TokenType.UnKnown)?.ToString();
            }
            Match();
            variable.Value = StatementBuilder();

            return variable;
        }
        public Variable VariableOfValueBuilder()
        {
            Variable variable = new Variable();
            do
            {
                variable.Name += Token.TokenType.UnKnown.ToString();

                if (LookAhead(false, Token.TokenType.Minus))
                    variable.Name += Token.TokenType.Minus.ToString();

            } while (LookAhead(true, Token.TokenType.Minus));

            return variable;
        }

        //Functions
        public GetActualX GetActualXBuilder()
        {
            GetActualX getActualX = new GetActualX();

            Match(Token.TokenType.GetActualX);
            Match(Token.TokenType.OpenParan);
            Match(Token.TokenType.ClosedParan);

            return getActualX;
        }
        public GetActualY GetActualYBuilder()
        {
            GetActualY getActualY = new GetActualY();

            Match(Token.TokenType.GetActualY);
            Match(Token.TokenType.OpenParan);
            Match(Token.TokenType.ClosedParan);

            return getActualY;
        }
        public GetCanvasSize GetCanvasSizeBuilder()
        {
            GetCanvasSize getCanvasSize = new GetCanvasSize();

            Match(Token.TokenType.GetCanvasSize);
            Match(Token.TokenType.OpenParan);
            Match(Token.TokenType.ClosedParan);

            return getCanvasSize;
        }
        public GetColorCount GetColorCountBuilder()
        {
            GetColorCount getColorCount = new GetColorCount();

            Match(Token.TokenType.GetColorCount);

            Match(Token.TokenType.OpenParan);

            getColorCount.Color = StatementBuilder();
            Match(Token.TokenType.Comma);
            getColorCount.X1 = StatementBuilder();
            Match(Token.TokenType.Comma);
            getColorCount.Y1 = StatementBuilder();
            Match(Token.TokenType.Comma);
            getColorCount.X2 = StatementBuilder();
            Match(Token.TokenType.Comma);
            getColorCount.Y2 = StatementBuilder();

            Match(Token.TokenType.ClosedParan);

            return getColorCount;
        }
        public IsBrushColor IsBrushColorBuilder()
        {
            IsBrushColor isBrushColor = new IsBrushColor();

            Match(Token.TokenType.IsBrushColor);

            Match(Token.TokenType.OpenParan);
            isBrushColor.Color = StatementBuilder();
            Match(Token.TokenType.ClosedParan);

            return isBrushColor;
        }
        public IsBrushSize IsBrushSizeBuilder()
        {
            IsBrushSize isBrushSize = new IsBrushSize();

            Match(Token.TokenType.IsBrushSize);

            Match(Token.TokenType.OpenParan);
            isBrushSize.Size = StatementBuilder();
            Match(Token.TokenType.ClosedParan);

            return isBrushSize;
        }
        public IsCanvasColor IsCanvasColorBuilder()
        {
            IsCanvasColor isCanvasColor = new IsCanvasColor();

            Match(Token.TokenType.IsCanvasColor);

            Match(Token.TokenType.OpenParan);
            isCanvasColor.Color = StatementBuilder();
            Match(Token.TokenType.Comma);
            isCanvasColor.Vertical = StatementBuilder();
            Match(Token.TokenType.Comma);
            isCanvasColor.Horizontal = StatementBuilder();
            Match(Token.TokenType.ClosedParan);

            return isCanvasColor;
        }

        //GoTo
        private GoTo GoToBuilder()
        {
            GoTo goTo = new GoTo();

            Match(Token.TokenType.GoTo);
            Match(Token.TokenType.OpenBracket);
            goTo.Label = LabelBuilder();
            Match(Token.TokenType.ClosedBracket);
            Match(Token.TokenType.OpenParan);
            goTo.Condition = StatementBuilder();
            Match(Token.TokenType.ClosedParan);

            return goTo;
        }
        public Label LabelBuilder()
        {
            Label label = new Label();
            do
            {
                label.Name+= Token.TokenType.UnKnown.ToString();
                
                if(LookAhead(false,Token.TokenType.Minus)) 
                    label.Name += Token.TokenType.Minus.ToString();

            } while (LookAhead(true, Token.TokenType.Minus));

            return label;
        }
    }
}
