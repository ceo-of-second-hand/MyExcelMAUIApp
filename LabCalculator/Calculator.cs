//Calculator.cs
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabCalculator;
//maybe will be necessary to add CurrentGrid 
public static class Calculator
{
    public static double Evaluate(string expression, CurrentGrid currentGrid)
    {
        // Assuming the expression involves referencing other cells, use the grid to resolve them
        var lexer = new LabCalculatorLexer(new AntlrInputStream(expression));
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new ThrowExceptionErrorListener());

        var tokens = new CommonTokenStream(lexer);
        var parser = new LabCalculatorParser(tokens);

        var tree = parser.compileUnit();

        //CurrentGrid _current_grid = new CurrentGrid();  // Create an instance of CurrentGrid

        var visitor = new LabCalculatorVisitor();

        return visitor.Visit(tree);  // You may need to customize this part to use the grid
    }
}

