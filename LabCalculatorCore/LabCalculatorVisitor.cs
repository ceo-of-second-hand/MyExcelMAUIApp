using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LabCalculator
{
    class LabCalculatorVisitor : LabCalculatorBaseVisitor<double>
    {
        Dictionary<string, CurrentCell> cells = new Dictionary<string, CurrentCell>();

        public override double VisitCompileUnit(LabCalculatorParser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }

        /* public override double VisitAssignmentExpr(LabCalculatorParser.AssignmentExprContext context)
        {
            var identifier = context.IDENTIFIER().GetText();
            var value = Visit(context.expression());

            SetCellValue(identifier, value);
            return value;
        } */

        public override double VisitParenthesizedExpr(LabCalculatorParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitMinusExpr(LabCalculatorParser.MinusExprContext context)
        {
            var left = WalkLeft(context);

            Debug.WriteLine("-{0}", left);
            left = -left;
            return left;
        }

        public override double VisitIncDecExpr(LabCalculatorParser.IncDecExprContext context)
        {
            var value = Visit(context.expression()); // Отримуємо значення виразу, до якого застосовується операція

            if (context.operatorToken.Type == LabCalculatorLexer.INCREMENT)
            {
                Debug.WriteLine("++{0}", value);
                return value + 1; // Збільшення на 1
            }
            else // LabCalculatorLexer.DECREMENT
            {
                Debug.WriteLine("--{0}", value);
                return value - 1; // Зменшення на 1
            }
        }

        public override double VisitExponentialExpr(LabCalculatorParser.ExponentialExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            Debug.WriteLine("{0} ^ {1}", left, right);
            return System.Math.Pow(left, right);
        }

        public override double VisitMultiplicativeExpr(LabCalculatorParser.MultiplicativeExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == LabCalculatorLexer.MULTIPLY)
            {
                Debug.WriteLine("{0} * {1}", left, right);
                return left * right;
            }
            else
            {
                Debug.WriteLine("{0} / {1}", left, right);
                return left / right;
            }
        }

        public override double VisitHalfMultiplicativeExpr(LabCalculatorParser.HalfMultiplicativeExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == LabCalculatorLexer.MOD)
            {
                Debug.WriteLine("{0} % {1}", left, right);
                return left % right; // Операція залишку від ділення (mod)
            }
            else // LabCalculatorLexer.DIV
            {
                Debug.WriteLine("{0} div {1}", left, right);
                return Math.Floor(left / right); // Операція цілочислового ділення (div)
            }
        }

        public override double VisitAdditiveExpr(LabCalculatorParser.AdditiveExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == LabCalculatorLexer.ADD)
            {
                Debug.WriteLine("{0} + {1}", left, right);
                return left + right;
            }
            else // LabCalculatorLexer.SUBTRACT
            {
                Debug.WriteLine("{0} - {1}", left, right);
                return left - right;
            }
        }

        public override double VisitNumberExpr(LabCalculatorParser.NumberExprContext context)
        {
            var result = double.Parse(context.GetText());
            Debug.WriteLine(result);

            return result;
        }

        public override double VisitIdentifierExpr(LabCalculatorParser.IdentifierExprContext context)
        {
            // Отримуємо текстовий вираз (наприклад, "A1", "B2" тощо)
            var identifier = context.GetText();

            // Використовуємо CellCode для декодування координат (рядка та стовпця)
            var cellCoordinates = CellCode.FromCode(identifier);

            // Отримуємо рядок і стовпець клітинки
            int row = cellCoordinates.Item1;
            int column = cellCoordinates.Item2;

            //дісати всі елементи з грід, знайти свою комірку, знайти через кла для цієї cell
            //значення, перевірити чи нема в цьому значенні ще інших клтинок. 
            //потім отой загальний вираз обчислити через evaluate з калькулятора
            //пов'язати якось cell з expression

            // Створюємо унікальний ідентифікатор клітинки (наприклад, "A1", "B2")
            var cellIdentifier = $"{CellCode.ToCode(column)}{row + 1}";

            // Перевіряємо, чи клітинка з таким ідентифікатором існує в словнику
            if (cells.ContainsKey(cellIdentifier))
            {
                // Якщо клітинка існує, повертаємо її значення
                return cells[cellIdentifier].Value;
            }
            else
            {
                // Якщо клітинка не існує, повертаємо значення за замовчуванням (0.0) або викликаємо помилку
                Debug.WriteLine($"Cell {cellIdentifier} not found, returning 0.0");
                return 0.0;
            }
        }


        private double WalkLeft(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(0));
        }

        private double WalkRight(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(1));
        }







        /* private void SetCellValue(string identifier, double value)
         {
             if (cells.ContainsKey(identifier))
             {
                 cells[identifier].Value = value;
             }
             else
             {
                 cells[identifier] = new Cell(identifier) { Value = value };
             }
         }

         private double GetCellValue(string identifier)
         {
             if (cells.TryGetValue(identifier, out Cell cell))
             {
                 return cell.Value;
             }
             return 0.0; // або кинути виключення, якщо значення не знайдено
         } */
    }
}
