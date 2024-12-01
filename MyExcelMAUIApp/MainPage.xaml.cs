//MainPage.xaml.cs
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using System;
using System.Collections.Generic;
using System.Linq; // Add for FirstOrDefault method

using Grid = Microsoft.Maui.Controls.Grid;

using System.Xml.Linq;
using LabCalculator; //  LabCalculator contains the CurrentCell class

namespace MyExcelMAUIApp
{
    public partial class MainPage : ContentPage
    {
        const int CountColumn = 5; // Number of columns
        const int CountRow = 5; // Number of rows

        private Entry? _previousEntry = null; //поле вводу (суто поле!)
        private Button selected; //кнопка, натиснувши умова на яку, можемо мати справу з Entry
        // Declare a dictionary to store cell values 
        private IDictionary<string, IView> cells;


        public MainPage()
        {
            cells = new Dictionary<string, IView>();
            InitializeComponent(); //бібіліотечний метод
            CreateGrid(CountRow, CountColumn);
            /*
                Entry — це клас, який представляє сам елемент вводу.
                textInput — це змінна, яка є конкретним екземпляром цього класу, і з якою ви працюєте в коді. 
            */
            //textInput - це ім'я нашого Entry
            textInput.Unfocused += Entry_Unfocused; // кожного разу, коли відбудеться подія Unfocused, буде викликаний метод Entry_Unfocused
        }

        private string GetColumnName(int colIndex)
        {
            int dividend = colIndex; //індекс стовпця
            string columnName = string.Empty; //числове представлення рядка

            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        private string GetCellName(object element)
        {
            //Клас View містить властивості, які дозволяють задавати положення елемента на екрані
            //element - об'єкт елемента в інтерфейсі користувача
            var col = Grid.GetColumn((View)element);
            var row = Grid.GetRow((View)element);

            return GetCellName(row, col);
        }

        private string GetCellName(int row, int column)
        {

            //повертає рядкове представленння клітини (напр. B5)
            return GetColumnName(column) + row.ToString();
        }

        /*private Entry GetNextEntry(int row, int col)
        {
            bool hasNextRow = row < CountRow - 1;
            bool hasNextColumn = col < CountColumn - 1;

            if (hasNextRow && hasNextColumn)
            {
                return (Entry)grid.Children
                    .FirstOrDefault(child => child is Entry &&
                                            Grid.GetRow((View)child) == row + 1 &&
                                            Grid.GetColumn((View)child) == col + 1);
            }
            else if (hasNextColumn)
            {
                return (Entry)grid.Children
                    .FirstOrDefault(child => child is Entry &&
                                            Grid.GetRow((View)child) == row &&
                                            Grid.GetColumn((View)child) == col + 1);
            }
            else if (hasNextRow)
            {
                return (Entry)grid.Children
                    .FirstOrDefault(child => child is Entry &&
                                            Grid.GetRow((View)child) == row + 1 &&
                                            Grid.GetColumn((View)child) == col);
            }

            return null;
        }*/

        private async void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            var entry = (Entry)sender; //переводимо об'єкт сендер до типу entry
            var cellName = GetCellName(selected); //selected - button - button is an element 

            // Assign the singleton instance
            CurrentGrid current_grid = CurrentGrid.Instance;

            // перевіряємо, чи раптом не відбулося такого, що змін не було (бо в EdutCell в нас буде false, якщо ми не перевіримо
            if (entry.Text == current_grid.Cells[cellName].Identifier)
            {
                return;
            }

            if (current_grid.Update(cellName, entry.Text))
            {
                // Refresh all affected cells
                foreach (var name in current_grid.AffectedCells)
                {
                    Update(name);
                }

                return;
            }

            // If the content doesn't pass validation, revert to the original value
            entry.Text = current_grid.Cells[cellName].Identifier;
            await DisplayAlert("Помилка", "Введено недопустимий вираз", "OK");
        }

        private void Update(string cellName)
        {
            CurrentGrid current_grid = CurrentGrid.Instance;
            var cell = (Button)cells[cellName];//переводимо клітину типу View до Button

            /*cell.Text відповідає за виведення тексту, який відображається на кнопці. 
              ТУТ CELLS - ЦЕ бібліотека інша, тому повернеться не View, а CurrentCell*/

            //оновлюємо текст в клітинці (звісно не треба прям всі Affected перевіряти, але для надійності нехай буде 
            cell.Text = current_grid.Cells[cellName].GetText();
        }

        private void UpdateFully(string cellName)
        {
            CurrentGrid current_grid = CurrentGrid.Instance;
            Update(cellName);

            foreach (var name in current_grid.Cells[cellName].AppearsIn)
            {
                UpdateFully(name);
            }
        }

        private void ChooseCell(object sender, EventArgs e)
        {
            CurrentGrid current_grid = CurrentGrid.Instance;
            if (textInput.IsReadOnly)
            {
                textInput.IsReadOnly = false;
            }

            // reset the background color of the previously selected cell (if any)
            if (selected != null)
            {
                selected.BackgroundColor = new Color(255, 255, 255); // reset to default background color (white)
            }

            // Update the selected cell
            selected = (Button)sender;
            selected.BackgroundColor = new Color(255, 250, 205); // lemon yellow background color

            textInput.Text = current_grid.Cells[GetCellName(selected)].Identifier; // update the text value corresponding to the cell
            textInput.Focus();
        }

        private async void CalculateButton_Clicked(object sender, EventArgs e)
        {
            CurrentGrid current_grid = CurrentGrid.Instance;
            double sum = 0;

            foreach (var cellName in current_grid.AffectedCells)
            {
                if (current_grid.Cells.ContainsKey(cellName))
                {
                    sum += current_grid.Cells[cellName].Value;
                }
            }

            await DisplayAlert("Sum of Affected Cells", $"The total sum is: {sum}", "OK");
        }
        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            try
            {

                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SavedGrids");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = $"Grid_{DateTime.Now:yyyyMMdd_HHmmss}.xml";
                string filePath = Path.Combine(folderPath, fileName);

                XDocument xdoc = new XDocument(new XElement("Grid"));

                xdoc.Root.Add(new XAttribute("RowCount", grid.RowDefinitions.Count - 1));
                xdoc.Root.Add(new XAttribute("ColumnCount", grid.ColumnDefinitions.Count - 1));

                for (int row = 1; row < grid.RowDefinitions.Count; row++)
                {
                    XElement rowElement = new XElement("Row");
                    for (int col = 1; col < grid.ColumnDefinitions.Count; col++)
                    {
                        var cellName = GetColumnName(col) + row.ToString();
                        if (cells.ContainsKey(cellName) && cells[cellName] is Button button)
                        {
                            XElement cellElement = new XElement("Cell",
                                new XAttribute("Name", cellName),
                                new XAttribute("Value", button.Text));
                            rowElement.Add(cellElement);
                        }
                    }
                    xdoc.Root.Add(rowElement);
                }

                xdoc.Save(filePath);

                await DisplayAlert("Save Successful", $"Grid saved successfully to your Desktop at {filePath}!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Save Failed", $"Error: {ex.Message}", "OK");
            }
        }

        private Button CreateCell()
        {
            var button = new Button
            {
                Text = "",
                BorderWidth = 3.0,
                BackgroundColor = new Color(255, 255, 255),
                TextColor = new Color(0, 0, 0),
                BorderColor = new Color(255, 192, 203, 100),
                CornerRadius = 10,

                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill
            };
            button.Clicked += ChooseCell;
            button.Focused += ChooseCell;

            return button;
        }

        private async void ExitButton_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Confirmation", "Do you really want to exit?", "Yes", "No");

            if (answer)
            {
                Environment.Exit(0);
            }
        }

        private async void ReadButton_Clicked(object sender, EventArgs e)
        {
            try
            {

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select an XML file",
                    FileTypes = null
                });

                if (result != null && File.Exists(result.FullPath))
                {

                    var xdoc = XDocument.Load(result.FullPath);
                    var rootElement = xdoc.Root;

                    grid.RowDefinitions.Clear();
                    grid.ColumnDefinitions.Clear();
                    grid.Children.Clear();
                    cells.Clear();

                    CurrentGrid current_grid = CurrentGrid.Instance;
                    current_grid.Cells.Clear();

                    int rowCount = int.Parse(rootElement.Attribute("RowCount").Value); //// reinitialize the grid
                    //int columnCount = int.Parse(rootElement.Attribute("ColumnCount").Value);
                    CreateGrid(rowCount, CountColumn);

                    // Populate the grid with saved data
                    foreach (var rowElement in rootElement.Elements("Row"))
                    {
                        foreach (var cellElement in rowElement.Elements("Cell"))
                        {
                            string cellName = cellElement.Attribute("Name").Value;
                            string cellValue = cellElement.Attribute("Value").Value;

                            // Update the logical CurrentGrid.Cells
                            if (!current_grid.Cells.ContainsKey(cellName))
                            {
                                current_grid.Cells[cellName] = new CurrentCell();
                            }
                            current_grid.Cells[cellName].Identifier = cellValue;

                            // Update the UI (Button/Text)
                            if (cells.ContainsKey(cellName) && cells[cellName] is Button button)
                            {
                                button.Text = cellValue;
                            }
                        }
                    }

                    // After populating the grid, refresh all cells to rebuild dependencies and recalculate values
                    foreach (var cellName in current_grid.Cells.Keys)
                    {
                        current_grid.UpdateDependencyCheck(cellName);
                        Update(cellName); // Update the UI with recalculated values
                    }

                    await DisplayAlert("Load Successful", "Grid loaded successfully, and dependencies restored!", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "No file selected or file does not exist.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load the file. Details: {ex.Message}", "OK");
            }
        }



        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Довідка", "Лабораторна робота 1.Гіщак Іванни Богданівни", "OK");
        }

        private void CreateGrid(int CurrentRowCount , int ColumnsCount)
        {
            AddColumnsAndColumnLabels();
            AddRowsAndCellEntries(CurrentRowCount);

        }

        private void DeleteRowButton_Clicked(object sender, EventArgs e)
        {
            DeleteRow();
        }

        private void DeleteRow()
        {
            if (grid.RowDefinitions.Count > 1) // should not be less than 1 row !!!
            {
                var lastRowIndex = grid.RowDefinitions.Count - 1;

                if (cells.ContainsKey($"0{lastRowIndex}") && cells[$"0{lastRowIndex}"].IsFocused)
                {
                    var a1Cell = cells["A1"];
                    a1Cell.Focus();
                }

                grid.RowDefinitions.RemoveAt(lastRowIndex);
                grid.Children.Remove(cells[$"0{lastRowIndex}"]); // remove label

                for (var col = 1; col <= grid.ColumnDefinitions.Count - 1; col++)
                {
                    var cellName = GetColumnName(col) + lastRowIndex.ToString();
                    DeleteCell(cellName);
                }
            }
        }

        /*
        private void DeleteRow()
        {
            CurrentGrid current_grid = CurrentGrid.Instance;
            if (grid.RowDefinitions.Count > 1) // should not be less than 1 row !!!
            {
                var lastRowIndex = grid.RowDefinitions.Count - 1;

                for (var col = 1; col <= grid.ColumnDefinitions.Count - 1; col++)
                {
                    var cellName = GetColumnName(col) + lastRowIndex.ToString();

                    if (cells.ContainsKey(cellName))
                    {
                        grid.Children.Remove(cells[cellName]); 
                        cells.Remove(cellName);              
                    }

                    if (current_grid.Cells.ContainsKey(cellName))
                    {
                        current_grid.Cells.Remove(cellName); 
                    }
                }
                grid.RowDefinitions.RemoveAt(lastRowIndex);
            }
        }

        */

        /*private void DeleteCell(string cellName)
        {
            CurrentGrid current_grid = CurrentGrid.Instance;
            foreach (var name in current_grid.Cells[cellName].AppearsIn)
            {
                var cell = current_grid.Cells[name];
                if (cell.DependsOn.Contains(cellName))
                {
                    cell.DependsOn.Clear();
                    cell.Identifier = "";
                    UpdateFully(name);
                }
            }

            grid.Children.Remove(cells[cellName]);
            cells.Remove(cellName);
            current_grid.Cells.Remove(cellName);
        }*/

        /*private void DeleteCell(string cellName)
        {
            CurrentGrid current_grid = CurrentGrid.Instance;
            var queue = new Queue<string>();
            queue.Enqueue(cellName);

            while (queue.Count > 0)
            {
                var currentCellName = queue.Dequeue();

                if (!current_grid.Cells.ContainsKey(currentCellName))
                    continue;

                var cellToDelete = current_grid.Cells[currentCellName];

                // Очистка всіх залежностей
                foreach (var dependentCellName in cellToDelete.AppearsIn.ToList())
                {
                    var dependentCell = current_grid.Cells[dependentCellName];
                    if (dependentCell.DependsOn.Contains(currentCellName))
                    {
                        dependentCell.DependsOn.Remove(currentCellName);

                        if (dependentCell.DependsOn.Count == 0)
                        {
                            // Якщо більше немає залежностей, очищаємо повністю
                            dependentCell.Identifier = "";
                            dependentCell.Value = 0.0;
                        }
                        else
                        {
                            // Якщо залишилися інші залежності, перераховуємо значення
                            dependentCell.Value = Calculator.Evaluate(dependentCell.Identifier, current_grid);
                        }

                        // Додаємо залежну клітинку до черги для подальшого оновлення
                        queue.Enqueue(dependentCellName);
                    }
                }

                // Очищаємо AppearsIn у залежностях
                foreach (var dependency in cellToDelete.DependsOn.ToList())
                {
                    if (current_grid.Cells.ContainsKey(dependency))
                    {
                        current_grid.Cells[dependency].AppearsIn.Remove(currentCellName);
                    }
                }

                // Видаляємо клітинку
                grid.Children.Remove(cells[currentCellName]);
                cells.Remove(currentCellName);
                current_grid.Cells.Remove(currentCellName);
            }
        }*/

        private void DeleteCell(string cellName)
        {
            CurrentGrid current_grid = CurrentGrid.Instance;
            var queue = new Queue<string>();//черга на оновлення ??

            //adds to the back of the queue 
            queue.Enqueue(cellName);

            while (queue.Count > 0)
            {
                //знімаєм найвищу клітину з черги ([A, B, D]) -> A
                var currentCellName = queue.Dequeue();

                if (!current_grid.Cells.ContainsKey(currentCellName))
                continue; 

                var cellToDelete = current_grid.Cells[currentCellName];//retrieves Cell object (CurrentCell)

                foreach (var dependentCellName in cellToDelete.AppearsIn.ToList())// очистка всіх залежностей
                {
                    var dependentCell = current_grid.Cells[dependentCellName];
                    if (dependentCell.DependsOn.Contains(currentCellName))//just double checking:))
                    {
                        dependentCell.DependsOn.Remove(currentCellName);

                        dependentCell.Identifier = "0";
                        dependentCell.Value = 0.0;

                        Update(dependentCellName);// оновлюємо текст у залежній клітинці

                        queue.Enqueue(dependentCellName);// додаємо залежну клітинку до черги для подальшого оновлення
                    }
                }

                // все правильно! (очищуємо такі клітинки: (A=B; B=3) -> getting rid of A in B_AppearsInList
                foreach (var dependency in cellToDelete.DependsOn.ToList())
                {
                    if (current_grid.Cells.ContainsKey(dependency))
                    {
                        current_grid.Cells[dependency].AppearsIn.Remove(currentCellName);
                    }
                }
            }
            if (current_grid.Cells.ContainsKey(cellName))
            {
                current_grid.Cells.Remove(cellName);
            }
            current_grid.AffectedCells.Remove(cellName); //не додала первірку, бо не словник
            if (cells.ContainsKey(cellName) && cells[cellName] is Button button)
            {
                grid.Children.Remove(button); 
                cells.Remove(cellName);       
            }
        }

        private void AddRowButton_Clicked(object sender, EventArgs e)
        {

            CurrentGrid current_grid = CurrentGrid.Instance;
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(75) });
            var row = grid.RowDefinitions.Count - 1;

            var label = new Label
            {
                Text = row.ToString(),

                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, row);
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);
            cells[$"0{row}"] = label;

            for (var col = 1; col < grid.ColumnDefinitions.Count; col++)
            {
                current_grid.Cells.Add(GetColumnName(col) + row.ToString(), new CurrentCell());
                var button = CreateCell();

                Grid.SetRow(button, row);
                Grid.SetColumn(button, col);
                grid.Children.Add(button);
                cells[GetCellName(button)] = button;
            }
        }

        private void AddRowsAndCellEntries(int CurrentRowCount)
        {
            CurrentGrid current_grid = CurrentGrid.Instance;
            {
                int row = 0;
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(75) });


                for (int row_real = 1; row_real < CurrentRowCount + 1; row_real++)
                {
                    //сітка буде мати на один рядок більше, і він готовий для розміщення в ньому елементів
                    //grid.RowDefinitions.Add(new RowDefinition());
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(75) });
                    // Додати підпис для номера рядка
                    var label = new Label
                    {
                        Text = (row_real).ToString(),
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
                    };
                    Grid.SetRow(label, row_real); //розміщуємо наш лейбл
                    Grid.SetColumn(label, 0); //розміщуємо наш лейбл
                    grid.Children.Add(label);//потрібно фактично додати його до сітки, щоб він став частиною інтерфейсу користувача
                                             //і був відображений на екрані.
                    cells[$"0{row_real}"] = label;// зберігає посилання на елемент(label) у колекції cells, використовуючи унікальний ключ, що містить номер рядка.

                    // Додати комірки (Entry) для вмісту
                    for (int col = 1; col < grid.ColumnDefinitions.Count; col++)
                    {
                        var entry = new Entry
                        {
                            Text = "",
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center
                        };

                        current_grid.Cells.Add(GetColumnName(col) + row_real.ToString(), new CurrentCell());
                        var button = CreateCell();
                        entry.Unfocused += Entry_Unfocused; // обробник
                        Grid.SetRow(button, row_real);
                        Grid.SetColumn(button, col);
                        grid.Children.Add(button);
                        cells[GetCellName(button)] = button;
                    }
                }
            }

        }

        private void AddColumnsAndColumnLabels()
        {
            CurrentGrid current_grid = CurrentGrid.Instance;
            // Додати стовпці та підписи для стовпців
            for (int col = 0; col < CountColumn + 1; col++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
                var newColumn = col;
                if (col > 0)
                {
                    var label = new Label
                    {
                        Text = GetColumnName(col), // для нульового стовпчика повернеться порожній текст(див. метод GetColumnName)
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
                    };
                    cells[$"{GetColumnName(newColumn)}0"] = label;
                    Grid.SetRow(label, 0);
                    Grid.SetColumn(label, col); //розташували labels з самого верху, де треба
                    grid.Children.Add(label);
                }
            }
        }

    }
}