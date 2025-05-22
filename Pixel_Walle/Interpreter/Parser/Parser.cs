using Pixel_Walle;
using System;
using System.Collections.Generic;
using System.Drawing;
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
                    Utils.Errors.Add($"Error Sintáctico: No se esperaba un \"{LookAhead()?.Value}\" Line: {LookAhead()?.Line}, Column: {LookAhead()?.Column}");
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
                Utils.Errors.Add($"Error Sintáctico: No se esperaba un \"{LookAhead()?.Value}\" Line: {LookAhead()?.Line}, Column: {LookAhead()?.Column}");
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

            if (LookAhead()?.Type == Token.TokenType.Spawn)
                programCompiler.Instructions.Add(SpawnBuilder());
            else Utils.Errors.Add("Error Sintáctico: No existe el Spawn al principio del código");

            while (Index < Tokens.Length - 1)
            {
                if (LookAhead(false, Token.TokenType.UnKnown))
                {
                    if (Index + 2 < Tokens.Length)
                    {
                        if (Tokens[Index + 2].Type == Token.TokenType.Assignment)
                            programCompiler.Instructions.Add(VariableBuilder());
                        else
                        {
                            int aux = Index;
                            programCompiler.Instructions.Add(LabelBuilder());
                            Index = aux + 1;
                        }
                    }
                    else
                    {
                        int aux = Index;
                        programCompiler.Instructions.Add(LabelBuilder());
                        Index = aux + 1;
                    };
                }
                else
                    programCompiler.Instructions.Add(InstructionsBuilder());

            }
            return programCompiler;
        }
        private Instructions? InstructionsBuilder()
        {
            if (LookAhead(false, Token.TokenType.Size))
                return SizeBuilder();

            else if (LookAhead(false, Token.TokenType.Color))
                return ColorBuilder();

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
                Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");
                Index++;
                return null;
            }
        }

        //Arithmetic Expressions
        private Expression ExpressionBuilder()
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

            if (LookAhead(false, Token.TokenType.OpenParan))
            {
                Match();
                factor.Expressions = ExpressionBuilder();
                Match(Token.TokenType.ClosedParan);
            }
            else
            {
                if (LookAhead(false, Token.TokenType.Digit))
                    factor.Value = MatchReturn();

                else if (LookAhead(false, Token.TokenType.UnKnown))
                    factor.Variable = MatchReturn(Token.TokenType.UnKnown);

                else if (Utils.FunctionList.Contains(LookAhead().Type))
                    factor.Functions = FunctionBuilder();
            }
            return factor;
        }

        //Statements
        private Statement StatementBuilder()
        {
            Statement statement = new Statement();

            statement.SubState = SubStatementBuilder();
            if (LookAhead(true, Token.TokenType.AND))
                statement.State = StatementBuilder();

            return statement;
        }
        private SubStatement SubStatementBuilder()
        {
            SubStatement subStatement = new SubStatement();

            subStatement.Mol = MoleculeBuilder();
            if (LookAhead(true, Token.TokenType.OR))
                subStatement.SubState = SubStatementBuilder();

            return subStatement;
        }
        private Molecule MoleculeBuilder()
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
        private Atom AtomBuilder()
        {
            if (LookAhead(false, Token.TokenType.True, Token.TokenType.False))
                return BooleanBuilder();
            else
                return ExpressionBuilder();
        }
        private Boolean BooleanBuilder()
        {
            Boolean boolean = new Boolean();

            boolean.Value = MatchReturn(Token.TokenType.True, Token.TokenType.False);

            return boolean;
        }

        //Instructions
        private Spawn SpawnBuilder()
        {
            Spawn spawn = new Spawn();

            Match(Token.TokenType.Spawn);

            Match(Token.TokenType.OpenParan);

            spawn.X = MatchReturn(Token.TokenType.Digit);
            Match(Token.TokenType.Comma);
            spawn.Y = MatchReturn(Token.TokenType.Digit);

            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return spawn;
        }
        private Size SizeBuilder()
        {
            Size size = new Size();
            Match(Token.TokenType.Size);

            Match(Token.TokenType.OpenParan);
            size.K = StatementBuilder();

            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return size;
        }
        private DrawLine DrawLineBuilder()
        {
            DrawLine drawLine = new DrawLine();
            Match(Token.TokenType.DrawLine);

            Match(Token.TokenType.OpenParan);

            drawLine.DirX = StatementBuilder();
            Match(Token.TokenType.Comma);
            drawLine.DirY = StatementBuilder();
            Match(Token.TokenType.Comma);
            drawLine.Distance = StatementBuilder();

            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return drawLine;
        }
        private DrawCircle DrawCircleBuilder()
        {
            DrawCircle drawCircle = new DrawCircle();
            Match(Token.TokenType.DrawCircle);

            Match(Token.TokenType.OpenParan);

            drawCircle.DirX = StatementBuilder();
            Match(Token.TokenType.Comma);
            drawCircle.DirY = StatementBuilder();
            Match(Token.TokenType.Comma);
            drawCircle.Radius = StatementBuilder();

            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return drawCircle;
        }
        private DrawRectangle DrawRectangleBuilder()
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

            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return drawRectangle;
        }
        private Fill FillBuilder()
        {
            Fill fill = new Fill();

            Match(Token.TokenType.Fill);
            Match(Token.TokenType.OpenParan);
            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return fill;
        }
        private Variable VariableBuilder()
        {
            Variable variable = new Variable();

            variable.Name = MatchReturn(Token.TokenType.UnKnown);
            Match(Token.TokenType.Assignment);
            variable.Value = StatementBuilder();

            return variable;
        }
        private Color ColorBuilder()
        {
            Color color = new Color();

            Match(Token.TokenType.Color);
            Match(Token.TokenType.OpenParan);
            Match(Token.TokenType.Quote);

            color.Value = MatchReturn(Token.TokenType.Red, Token.TokenType.Blue, Token.TokenType.Green, Token.TokenType.Yellow,
                Token.TokenType.Orange, Token.TokenType.Purple, Token.TokenType.Black, Token.TokenType.White, Token.TokenType.Transparent);
            Match(Token.TokenType.Quote);
            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return color;
        }

        //Functions
        private Functions? FunctionBuilder()
        {
            if (LookAhead()?.Type == Token.TokenType.GetActualX)
                return GetActualXBuilder();
            else if (LookAhead()?.Type == Token.TokenType.GetActualY)
                return GetActualYBuilder();
            else if (LookAhead()?.Type == Token.TokenType.GetCanvasSize)
                return GetCanvasSizeBuilder();
            else if (LookAhead()?.Type == Token.TokenType.GetColorCount)
                return GetColorCountBuilder();
            else if (LookAhead()?.Type == Token.TokenType.IsBrushColor)
                return IsBrushColorBuilder();
            else if (LookAhead()?.Type == Token.TokenType.IsBrushSize)
                return IsBrushSizeBuilder();
            else if (LookAhead()?.Type == Token.TokenType.IsCanvasColor)
                return IsCanvasColorBuilder();
            else if (LookAhead()?.Type == Token.TokenType.IsColor)
                return IsColorBuilder();

            return null;
        }
        private GetActualX GetActualXBuilder()
        {
            GetActualX getActualX = new GetActualX();

            Match(Token.TokenType.GetActualX);
            Match(Token.TokenType.OpenParan);
            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return getActualX;
        }
        private GetActualY GetActualYBuilder()
        {
            GetActualY getActualY = new GetActualY();

            Match(Token.TokenType.GetActualY);
            Match(Token.TokenType.OpenParan);
            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return getActualY;
        }
        private GetCanvasSize GetCanvasSizeBuilder()
        {
            GetCanvasSize getCanvasSize = new GetCanvasSize();

            Match(Token.TokenType.GetCanvasSize);
            Match(Token.TokenType.OpenParan);
            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return getCanvasSize;
        }
        private GetColorCount GetColorCountBuilder()
        {
            GetColorCount getColorCount = new GetColorCount();

            Match(Token.TokenType.GetColorCount);

            Match(Token.TokenType.OpenParan);

            Match(Token.TokenType.Quote);
            getColorCount.Color = MatchReturn(Token.TokenType.Red, Token.TokenType.Blue, Token.TokenType.Green, Token.TokenType.Yellow,
                Token.TokenType.Orange, Token.TokenType.Purple, Token.TokenType.Black, Token.TokenType.White, Token.TokenType.Transparent);
            Match(Token.TokenType.Quote);

            Match(Token.TokenType.Comma);
            getColorCount.X1 = StatementBuilder();
            Match(Token.TokenType.Comma);
            getColorCount.Y1 = StatementBuilder();
            Match(Token.TokenType.Comma);
            getColorCount.X2 = StatementBuilder();
            Match(Token.TokenType.Comma);
            getColorCount.Y2 = StatementBuilder();

            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return getColorCount;
        }
        private IsBrushColor IsBrushColorBuilder()
        {
            IsBrushColor isBrushColor = new IsBrushColor();

            Match(Token.TokenType.IsBrushColor);

            Match(Token.TokenType.OpenParan);
            Match(Token.TokenType.Quote);
            isBrushColor.Color = MatchReturn(Token.TokenType.Red, Token.TokenType.Blue, Token.TokenType.Green, Token.TokenType.Yellow,
                Token.TokenType.Orange, Token.TokenType.Purple, Token.TokenType.Black, Token.TokenType.White, Token.TokenType.Transparent);
            Match(Token.TokenType.Quote);
            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return isBrushColor;
        }
        private IsBrushSize IsBrushSizeBuilder()
        {
            IsBrushSize isBrushSize = new IsBrushSize();

            Match(Token.TokenType.IsBrushSize);

            Match(Token.TokenType.OpenParan);
            isBrushSize.Size = StatementBuilder();
            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return isBrushSize;
        }
        private IsCanvasColor IsCanvasColorBuilder()
        {
            IsCanvasColor isCanvasColor = new IsCanvasColor();

            Match(Token.TokenType.IsCanvasColor);

            Match(Token.TokenType.OpenParan);

            Match(Token.TokenType.Quote);
            isCanvasColor.Color = MatchReturn(Token.TokenType.Red, Token.TokenType.Blue, Token.TokenType.Green, Token.TokenType.Yellow,
                Token.TokenType.Orange, Token.TokenType.Purple, Token.TokenType.Black, Token.TokenType.White, Token.TokenType.Transparent);
            Match(Token.TokenType.Quote);

            Match(Token.TokenType.Comma);
            isCanvasColor.Vertical = StatementBuilder();
            Match(Token.TokenType.Comma);
            isCanvasColor.Horizontal = StatementBuilder();
            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return isCanvasColor;
        }
        private IsColor IsColorBuilder()
        {
            IsColor isColor = new IsColor();

            Match(Token.TokenType.IsColor);
            Match(Token.TokenType.OpenParan);
            Match(Token.TokenType.Quote);
            isColor.Color = MatchReturn(Token.TokenType.Color);
            Match(Token.TokenType.Quote);
            Match(Token.TokenType.Comma);
            isColor.X = StatementBuilder();
            Match(Token.TokenType.Comma);
            isColor.Y = StatementBuilder();
            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return isColor;
        }

        //GoTo
        private GoTo GoToBuilder()
        {
            GoTo goTo = new GoTo();

            Match(Token.TokenType.GoTo);
            Match(Token.TokenType.OpenBracket);
            goTo.Label = MatchReturn(Token.TokenType.UnKnown);
            Match(Token.TokenType.ClosedBracket);
            Match(Token.TokenType.OpenParan);
            goTo.Condition = StatementBuilder();

            if (LookAhead()?.Type == Token.TokenType.ClosedParan)
                Match();
            else Utils.Errors.Add($"Error Sintáctico: No se esperaba {LookAhead()?.Value} Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");

            return goTo;
        }
        private Label LabelBuilder()
        {
            Label label = new Label();

            label.Value = MatchReturn(Token.TokenType.UnKnown);

            while (Index < Tokens.Length - 1)
            {
                if (LookAhead(false, Token.TokenType.UnKnown))
                {
                    if (Index + 2 < Tokens.Length)
                    {
                        if (Tokens[Index + 2].Type == Token.TokenType.Assignment)
                            label.Instructions.Add(VariableBuilder());
                        else
                        {
                            int aux = Index;
                            label.Instructions.Add(LabelBuilder());
                            Index = aux + 1;
                        }
                    }
                    else
                    {
                        int aux = Index;
                        label.Instructions.Add(LabelBuilder());
                        Index = aux + 1;
                    };
                }
                else
                    label.Instructions.Add(InstructionsBuilder());
            }

            return label;
        }
    }
}
