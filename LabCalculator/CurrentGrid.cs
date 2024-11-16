//CurrentGrid.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;

namespace LabCalculator
{
    public class CurrentGrid
    {
        private static CurrentGrid _instance; 

        /*public CurrentGrid() : this(new Dictionary<string, CurrentCell>()) { }*/

        public IDictionary<string, CurrentCell> Cells { get; private set; }
        public string EvaluatingCell { get; set; }
        public HashSet<string> AffectedCells { get; }

        public static CurrentGrid Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CurrentGrid();
                }
                return _instance;
            }
        }

        public CurrentGrid()
        {
            Cells = new Dictionary<string, CurrentCell>();
            EvaluatingCell = "A1"; // Default cell
            AffectedCells = new HashSet<string>();
        } 

        public void UpdateWhole(string cellName)
        {
            if (!Cells.ContainsKey(cellName))
            {
                throw new ArgumentException($"Cell {cellName} does not exist.");
            }

            var cell = Cells[cellName];
            cell.DependsOn.Clear();//значення DependsOn і AppearsIn редагуються в VisitIdentifierExpr
            EvaluatingCell = cellName;
            cell.Value = Calculator.Evaluate(cell.Identifier, this);
            AffectedCells.Add(cellName);
        }

        public void UpdateDependencyCheck(string cellName)
        {
            var stack = new Stack<string>();//перелік тих, кого треба обробити
            var processedCells = new HashSet<string>(); //слідкує за тим, щоб кожну клітинку було оброблено не більше 1 разу
            stack.Push(cellName); //push - додати, pop - видалити

            while (stack.Count > 0)
            {
                string currentCellName = stack.Pop();
                if (processedCells.Contains(currentCellName))
                    continue;

                processedCells.Add(currentCellName);
                var currentCell = Cells[currentCellName]; //в словнику находить нашу клітинку за іменем

                UpdateWhole(currentCellName);

                //приклад: клітина A3 = B7; B1 = A3; 
                foreach (var observerCell in currentCell.AppearsIn)// (B1)
                {
                    if (Cells[observerCell].DependsOn.Contains(currentCellName)) // (A3)
                    {
                        stack.Push(observerCell); //знайшли залежну клітину -> треба обробити 
                    }
                }
            }
        }
        public bool LoopTrouble(string dependentCellName)
        {
            if (dependentCellName == EvaluatingCell)
                return true;

            var visited = new HashSet<string>();
            var stack = new Stack<string>();
            stack.Push(dependentCellName);

            while (stack.Count > 0)
            {
                string currentCellName = stack.Pop();
                if (visited.Contains(currentCellName))
                    continue;

                visited.Add(currentCellName);

                foreach (var dependency in Cells[currentCellName].DependsOn)
                {
                    if (dependency == EvaluatingCell)
                        return true;

                    stack.Push(dependency);
                }
            }

            return false;
        }

        public bool Update(string cellName, string expression)
        {
            if (!Cells.ContainsKey(cellName))
            {
                return false;
            }

            var currentCell = Cells[cellName];
            var oldExpression = currentCell.Identifier;
            var oldDependencies = new List<string>(currentCell.DependsOn);

            Cells[cellName].Identifier = expression;

            try
            {
                UpdateDependencyCheck(cellName);
            }
            catch (Exception)
            {
                currentCell.Identifier = oldExpression;
                currentCell.DependsOn = oldDependencies;
                return false;
            }
            return true;
        }
    }
}
