using Pixel_Walle;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Pixel_Walle
{
    public class Parser : IParsing // Clase que se encarga de parsear el código fuente y construir el AST (Árbol de Sintaxis Abstracta)
    {
        // Properties
        private Token[] Tokens { get; } // Lista de Tokens 
        private Token? CurrentToken { get; set; } // Token actual
        private int Index { get; set; }  // Índice del Token actual
        private bool DetectorError = false;  // Detector de errores sintácticos

        // Builder
        public Parser(Token[] tokens)
        {
            Tokens = tokens;
            CurrentToken = null;
            Index = -1;
        }
        // Methods
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
        private void Match()
        {
            if (ThereIsNext())
                CurrentToken = Tokens[++Index];
        }                // Avanza sin importar el siguiente Token
        private void Match(params Token.TokenType?[] nextTokens)
        {
            if (!DetectorError)
            {
                foreach (Token.TokenType? item in nextTokens)
                {
                    if (item == LookAhead()?.Type)
                        this.CurrentToken = Tokens[++Index];
                    else
                    {
                        Utils.Errors.Add($"Error Sintáctico: Se esperaba un \"{item}\" Linea: {LookAhead()?.Line}, Columna: {LookAhead()?.Column}");
                        Recover();
                        DetectorError = true;
                        return;
                    }
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
                string expectedTokens = string.Join(", ", nextTokens.Select(t => t.ToString()));
                Utils.Errors.Add($"Error Sintáctico: Se esperaba uno de los siguientes tokens \"{expectedTokens}\" Linea: {LookAhead()?.Line}, Columna: {LookAhead()?.Column}");
                DetectorError = true;
                Recover();
                return null;
            }
            else
            {
                this.CurrentToken = Tokens[++Index];
                return CurrentToken;
            }
        }         // Retorna uno de los coincidentes
        private void Recover()
        {
            int Line = CurrentToken?.Line ?? 0;
            while (ThereIsNext() && Line == LookAhead().Line)
            {
                this.CurrentToken = Tokens[++Index];
            }
        }  // Avanza hasta la próxima línea

        //ProgramCompiler
        public ProgramCompiler Parsing()
        {
            ProgramCompiler programCompiler = new ProgramCompiler();

            if (LookAhead()?.Type == Token.TokenType.Spawn)
                programCompiler.Instructions.Add(SpawnBuilder());
            else Utils.Errors.Add("Error Sintáctico: No existe el Spawn al principio del código");

            while (Index < Tokens.Length - 1)
            {
                DetectorError = false;
                if (LookAhead(false, Token.TokenType.UnKnown))
                {
                    if (Index + 2 < Tokens.Length)
                    {
                        if (Tokens[Index + 2].Type == Token.TokenType.Assignment)
                            programCompiler.Instructions.Add(VariableBuilder());
                        else
                        {
                            int Line = LookAhead()?.Line ?? 0;
                            if (Tokens[Index + 2].Line == LookAhead().Line)
                                programCompiler.Instructions.Add(InstructionsBuilder());
                            else
                            {
                                programCompiler.Instructions.Add(LabelBuilder());
                            }
                        }
                    }
                    else
                    {
                        programCompiler.Instructions.Add(LabelBuilder());
                    };
                }
                else
                {
                    programCompiler.Instructions.Add(InstructionsBuilder());
                }
            }
            Utils.RemoveDuplicatesFromErrors();
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

            else if (LookAhead(false, Token.TokenType.Comment))
            {
                Match();
                Recover();
                return null; // Ignorar comentarios
            }
            else
            {
                Utils.Errors.Add($"Error Sintáctico: {LookAhead()?.Value} no es una instrucción correcta Linea:{LookAhead()?.Line} Columna:{LookAhead()?.Column}");
                CurrentToken = Tokens[++Index];
                Recover();
                return null;
            }
        }

        //Arithmetic Expressions
        public Expression ParseExpression()
        {
            Expression expression = new Expression();

            expression = ParseExpressionLv1() ?? new Expression();

            return expression;
        }
        private Expression? ParseExpressionLv1()
        {
            Expression? left = ParseExpressionLv2();

            while (LookAhead(false, Token.TokenType.Plus, Token.TokenType.Minus))
            {
                Token? operatorToken = MatchReturn(Token.TokenType.Plus, Token.TokenType.Minus);
                Expression? right = ParseExpressionLv2();

                if (right == null)
                {
                    return null;
                }

                left = new Expression
                {
                    Operator = operatorToken,
                    Terms = new Term { Factor = new Factor { Expressions = left } },
                    Expressions = new Expression { Terms = new Term { Factor = new Factor { Expressions = right } } }
                };
            }
            return left;
        }
        private Expression? ParseExpressionLv2()
        {
            Expression? left = ParseExpressionLv3();

            while (LookAhead(false, Token.TokenType.Times, Token.TokenType.Divide, Token.TokenType.Module, Token.TokenType.Pow))
            {
                Token? operatorToken = MatchReturn(Token.TokenType.Times, Token.TokenType.Divide, Token.TokenType.Module, Token.TokenType.Pow);
                Expression? right = ParseExpressionLv3();

                if (right == null)
                {
                    return null;
                }

                left = new Expression
                {
                    Operator = operatorToken,
                    Terms = new Term
                    {
                        Factor = new Factor { Expressions = left },
                        Operator = operatorToken,
                        Terms = new Term { Factor = new Factor { Expressions = right } }
                    }
                };
            }

            return left;
        }
        private Expression? ParseExpressionLv3()
        {
            if (LookAhead(false, Token.TokenType.OpenParan))
            {
                Match(Token.TokenType.OpenParan);
                Expression? innerExpression = ParseExpression();
                Match(Token.TokenType.ClosedParan);
                return innerExpression;
            }
            else
            {
                if (LookAhead(false, Token.TokenType.Digit))
                {
                    Token? valueToken = MatchReturn(Token.TokenType.Digit);
                    return new Expression
                    {
                        Terms = new Term
                        {
                            Factor = new Factor { Value = valueToken }
                        }
                    };
                }
                else if (LookAhead(false, Token.TokenType.UnKnown))
                {
                    Token? variableToken = MatchReturn(Token.TokenType.UnKnown);
                    return new Expression
                    {
                        Terms = new Term
                        {
                            Factor = new Factor { Value = variableToken }
                        }
                    };
                }
                else if (Utils.FunctionList.Contains(LookAhead().Type))
                {
                    return new Expression
                    {
                        Terms = new Term
                        {
                            Factor = new Factor { Functions = FunctionBuilder() }
                        }
                    };
                }
                else
                {
                    Utils.Errors.Add($"Error Sintáctico: Se esperaba un valor o una expresión. Linea: {LookAhead()?.Line}, Columna: {LookAhead()?.Column}");
                    DetectorError = true;
                    Recover();
                    return null;
                }
            }
        }

        //Statements
        private Statement StatementBuilder()
        {
            Statement statement = new Statement();

            statement.SubState = SubStatementBuilder();
            if (LookAhead(false, Token.TokenType.AND))
            {
                statement.Symbol = MatchReturn();
                statement.State = StatementBuilder();
            }
            return statement;
        }
        private SubStatement SubStatementBuilder()
        {
            SubStatement subStatement = new SubStatement();

            subStatement.Mol = MoleculeBuilder();
            if (LookAhead(false, Token.TokenType.OR))
            {
                subStatement.Symbol = MatchReturn();
                subStatement.SubState = SubStatementBuilder();
            }
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
                return ParseExpression();
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

            spawn.X = DetectorError ? null : StatementBuilder();

            Match(Token.TokenType.Comma);

            spawn.Y = DetectorError ? null : StatementBuilder();

            Match(Token.TokenType.ClosedParan);

            return spawn;
        }
        private Size SizeBuilder()
        {
            Size size = new Size();
            Match(Token.TokenType.Size);

            Match(Token.TokenType.OpenParan);
            size.K = DetectorError ? null : StatementBuilder();
            Match(Token.TokenType.ClosedParan);

            return size;
        }
        private DrawLine DrawLineBuilder()
        {
            DrawLine drawLine = new DrawLine();

            Match(Token.TokenType.DrawLine);
            Match(Token.TokenType.OpenParan);

            drawLine.DirX = DetectorError ? null : StatementBuilder();

            Match(Token.TokenType.Comma);

            drawLine.DirY = DetectorError ? null : StatementBuilder();

            Match(Token.TokenType.Comma);

            drawLine.Distance = DetectorError ? null : StatementBuilder();

            Match(Token.TokenType.ClosedParan);


            return drawLine;
        }
        private DrawCircle DrawCircleBuilder()
        {
            DrawCircle drawCircle = new DrawCircle();
            Match(Token.TokenType.DrawCircle);

            Match(Token.TokenType.OpenParan);

            drawCircle.DirX = DetectorError ? null : StatementBuilder();
            Match(Token.TokenType.Comma);
            drawCircle.DirY = DetectorError ? null : StatementBuilder();
            Match(Token.TokenType.Comma);
            drawCircle.Radius = DetectorError ? null : StatementBuilder();

            Match(Token.TokenType.ClosedParan);

            return drawCircle;
        }
        private DrawRectangle DrawRectangleBuilder()
        {
            DrawRectangle drawRectangle = new DrawRectangle();

            Match(Token.TokenType.DrawRectangle);
            Match(Token.TokenType.OpenParan);

            drawRectangle.DirX = DetectorError ? null : StatementBuilder();
            Match(Token.TokenType.Comma);
            drawRectangle.DirY = DetectorError ? null : StatementBuilder();
            Match(Token.TokenType.Comma);
            drawRectangle.Distance = DetectorError ? null : StatementBuilder();
            Match(Token.TokenType.Comma);
            drawRectangle.Width = DetectorError ? null : StatementBuilder();
            Match(Token.TokenType.Comma);
            drawRectangle.Height = DetectorError ? null : StatementBuilder();

            Match(Token.TokenType.ClosedParan);

            return drawRectangle;
        }
        private Fill FillBuilder()
        {
            Fill fill = new Fill();

            Match(Token.TokenType.Fill);
            Match(Token.TokenType.OpenParan);
            Match(Token.TokenType.ClosedParan);

            return fill;
        }
        private Variable VariableBuilder()
        {
            Variable variable = new Variable();
            int line = LookAhead().Line;

            variable.Name = MatchReturn(Token.TokenType.UnKnown);

            if (!Utils.CheckValidLabel(variable.Name?.Value))
            {
                Utils.Errors.Add($"Error Sintáctico: \"{variable.Name?.Value}\" no es una palabra correcta. Linea: {variable.Name?.Line}, Columna: {variable.Name?.Column}");
                DetectorError = true;
                Recover();
            }

            Match(Token.TokenType.Assignment);

            if (LookAhead().Line == line)
                variable.Value = DetectorError ? null : StatementBuilder();

            else if (LookAhead().Line != line && !DetectorError)
                Utils.Errors.Add($"Error Sintáctico: No se le asigno ningún valor a \"{variable.Name?.Value}\" ");

            return variable;
        }
        private Color ColorBuilder()
        {
            Color color = new Color();

            Match(Token.TokenType.Color);
            Match(Token.TokenType.OpenParan);
            Match(Token.TokenType.Quote);

            color.Value = DetectorError ? null : MatchReturn();

            Match(Token.TokenType.Quote);
            Match(Token.TokenType.ClosedParan);

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

            return null;
        }
        private GetActualX GetActualXBuilder()
        {
            GetActualX getActualX = new GetActualX();

            Match(Token.TokenType.GetActualX);
            Match(Token.TokenType.OpenParan);
            Match(Token.TokenType.ClosedParan);

            return getActualX;
        }
        private GetActualY GetActualYBuilder()
        {
            GetActualY getActualY = new GetActualY();

            Match(Token.TokenType.GetActualY);
            Match(Token.TokenType.OpenParan);
            Match(Token.TokenType.ClosedParan);

            return getActualY;
        }
        private GetCanvasSize GetCanvasSizeBuilder()
        {
            GetCanvasSize getCanvasSize = new GetCanvasSize();

            Match(Token.TokenType.GetCanvasSize);
            Match(Token.TokenType.OpenParan);
            Match(Token.TokenType.ClosedParan);

            return getCanvasSize;
        }
        private GetColorCount GetColorCountBuilder()
        {
            GetColorCount getColorCount = new GetColorCount();

            Match(Token.TokenType.GetColorCount);

            Match(Token.TokenType.OpenParan);

            Match(Token.TokenType.Quote);
            getColorCount.Color = DetectorError ? null : MatchReturn();

            Match(Token.TokenType.Comma);
            getColorCount.X1 = DetectorError ? null : StatementBuilder();
            Match(Token.TokenType.Comma);
            getColorCount.Y1 = DetectorError ? null : StatementBuilder();
            Match(Token.TokenType.Comma);
            getColorCount.X2 = DetectorError ? null : StatementBuilder();
            Match(Token.TokenType.Comma);
            getColorCount.Y2 = DetectorError ? null : StatementBuilder();
            Match(Token.TokenType.ClosedParan);

            return getColorCount;
        }
        private IsBrushColor IsBrushColorBuilder()
        {
            IsBrushColor isBrushColor = new IsBrushColor();

            Match(Token.TokenType.IsBrushColor);

            Match(Token.TokenType.OpenParan);
            Match(Token.TokenType.Quote);
            isBrushColor.Color = DetectorError ? null : MatchReturn();
            Match(Token.TokenType.Quote);
            Match(Token.TokenType.ClosedParan);

            return isBrushColor;
        }
        private IsBrushSize IsBrushSizeBuilder()
        {
            IsBrushSize isBrushSize = new IsBrushSize();

            Match(Token.TokenType.IsBrushSize);

            Match(Token.TokenType.OpenParan);
            isBrushSize.Size = DetectorError ? null : StatementBuilder();
            Match(Token.TokenType.ClosedParan);

            return isBrushSize;
        }
        private IsCanvasColor IsCanvasColorBuilder()
        {
            IsCanvasColor isCanvasColor = new IsCanvasColor();

            Match(Token.TokenType.IsCanvasColor);

            Match(Token.TokenType.OpenParan);

            Match(Token.TokenType.Quote);
            isCanvasColor.Color = DetectorError ? null : MatchReturn();
            Match(Token.TokenType.Quote);

            Match(Token.TokenType.Comma);
            isCanvasColor.Vertical = DetectorError ? null : StatementBuilder();
            Match(Token.TokenType.Comma);
            isCanvasColor.Horizontal = DetectorError ? null : StatementBuilder();
            Match(Token.TokenType.ClosedParan);

            return isCanvasColor;
        }

        //GoTo
        private GoTo GoToBuilder()
        {
            GoTo goTo = new GoTo();

            Match(Token.TokenType.GoTo);

            Match(Token.TokenType.OpenBracket);
            goTo.Label_ = DetectorError == true ? null : MatchReturn(Token.TokenType.UnKnown);
            Match(Token.TokenType.ClosedBracket);

            Match(Token.TokenType.OpenParan);
            goTo.Condition = DetectorError ? null : StatementBuilder();
            Match(Token.TokenType.ClosedParan);

            return goTo;
        }

        //Label
        private Label LabelBuilder()
        {
            Label label = new Label();

            label.Value = LookAhead();

            if (!Utils.CheckValidLabel(LookAhead().Value))
                Utils.Errors.Add($"Error Sintáctico: \"{LookAhead()?.Value}\" no es una palabra correcta. Linea: {LookAhead()?.Line}, Columna: {LookAhead()?.Column}");

            Utils.keyLabelsReferences.Add(LookAhead()?.Value ?? "null", label);

            Match();

            while (Index < Tokens.Length - 1)
            {
                DetectorError = false;
                if (LookAhead(false, Token.TokenType.UnKnown))
                {
                    if (Index + 2 < Tokens.Length)
                    {
                        if (Tokens[Index + 2].Type == Token.TokenType.Assignment)
                            label.Instructions.Add(VariableBuilder());
                        else
                        {
                            int Line = LookAhead()?.Line ?? 0;
                            if (Tokens[Index + 2].Line == LookAhead().Line)
                                label.Instructions.Add(InstructionsBuilder());

                            else
                                label.SubLabel = LabelBuilder();
                        }
                    }
                    else
                        label.SubLabel = LabelBuilder();
                }
                else
                    label.Instructions.Add(InstructionsBuilder());
            }
            return label;
        }
    }
}
