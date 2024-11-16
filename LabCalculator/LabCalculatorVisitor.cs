//LabCalculatorVisitor.cs
//add more debug statements everywhere!!

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LabCalculator
{
    public class LabCalculatorVisitor : LabCalculatorBaseVisitor<double>
    {
        private CurrentGrid current_grid;

        public LabCalculatorVisitor()
        {
            current_grid = CurrentGrid.Instance;  // Singleton instance
        }

        public override double VisitCompileUnit(LabCalculatorParser.CompileUnitContext context)
        {
            Debug.WriteLine("Visiting CompileUnit: {0}", context.GetText());
            return Visit(context.expression());
        }

        


        public override double VisitParenthesizedExpr(LabCalculatorParser.ParenthesizedExprContext context)
        {
            Debug.WriteLine("Visiting Parenthesized Expression: {0}", context.GetText());
            return Visit(context.expression());
        }

        public override double VisitIncDecExpr(LabCalculatorParser.IncDecExprContext context)
        {
            var value = Visit(context.expression());
            if (context.operatorToken.Type == LabCalculatorLexer.INCREMENT)
            {
                Debug.WriteLine("Incrementing: {0} -> {1}", value, value + 1);
                return value + 1;
            }
            else
            {
                Debug.WriteLine("Decrementing: {0} -> {1}", value, value - 1);
                return value - 1;
            }
        }

        public override double VisitExponentialExpr(LabCalculatorParser.ExponentialExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            Debug.WriteLine("Exponential Expression: {0} ^ {1}", left, right);
            return System.Math.Pow(left, right);
        }

        public override double VisitHalfMultiplicativeExpr(LabCalculatorParser.HalfMultiplicativeExprContext context)
        {
            //var value = Visit(context.expression());
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == LabCalculatorLexer.MOD)
            {
                
                Debug.WriteLine("{0} mod {1}", left, right);
                return left % right;
            }
            else
            {
                Debug.WriteLine("{0} div {1}", left, right);

                return (int)left / (int)right;
            }
        }

        public override double VisitMultiplicativeExpr(LabCalculatorParser.MultiplicativeExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            if (context.operatorToken.Type == LabCalculatorLexer.MULTIPLY)
            {
                Debug.WriteLine("Multiplying: {0} * {1}", left, right);
                return left * right;
            }
            else
            {
                Debug.WriteLine("Dividing: {0} / {1}", left, right);
                return left / right;
            }
        }

        public override double VisitAdditiveExpr(LabCalculatorParser.AdditiveExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            if (context.operatorToken.Type == LabCalculatorLexer.ADD)
            {
                Debug.WriteLine("Adding: {0} + {1}", left, right);
                return left + right;
            }
            else
            {
                Debug.WriteLine("Subtracting: {0} - {1}", left, right);
                return left - right;
            }
        }
        public override double VisitNumberExpr(LabCalculatorParser.NumberExprContext context)
        {
            var result = double.Parse(context.GetText());
            Debug.WriteLine("Number: {0}", result);
            return result;
        }

        public override double VisitIdentifierExpr(LabCalculatorParser.IdentifierExprContext context)
        {
            var identifier = context.GetText();//парсер зустів клітинку у виразі (наприклад, A2 = B3 +1 -> парсер ішов-ішов і зустрів B3)
            Debug.WriteLine("Visiting Identifier: {0}", identifier);

            var editedCellName = current_grid.EvaluatingCell;//клітина В ЯКІЙ парсер зустрів іншу клітину
            var resultCell = current_grid.Cells[identifier];//повертає об'єкт CurrentCell (зі словника)

            current_grid.Cells[editedCellName].DependsOn.Add(identifier);//додали залежність (i.e. додали B3 до колекції залеєностей A2)
            Debug.WriteLine("Cell {0} depends on {1}", editedCellName, identifier);

            if (current_grid.LoopTrouble(identifier))
            {
                Debug.WriteLine("Cyclic dependency detected for {0}", identifier);
                throw new System.Exception("Invalid Expression: Cyclic dependency detected.");
            }

            if (!resultCell.AppearsIn.Contains(editedCellName)) //оновили правильно AppearsIn для клітини ЯКУ ЗУСТІРЛИ
            {
                resultCell.AppearsIn.Add(editedCellName);
            }

            return resultCell.Value;
        }

        private double WalkLeft(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(0));
        }

        private double WalkRight(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(1));
        }
    }
}











