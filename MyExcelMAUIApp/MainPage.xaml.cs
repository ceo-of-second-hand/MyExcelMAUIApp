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

        /*// Method to update the value in the visitor's dictionary
        private void UpdateCellValue(string identifier, double value)
        {
            // Check if the cell exists in the dictionary
            if (!cells.ContainsKey(identifier))
            {
                // If not, create a new CurrentCell and add it to the dictionary
                cells[identifier] = new CurrentCell(identifier);
            }

            // Update the cell value
            cells[identifier].Value = value;
        }
        */


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

            // Reset the background color of the previously selected cell (if any)
            if (selected != null)
            {
                selected.BackgroundColor = new Color(255, 255, 255); // Reset to default background color (white)
            }

            // Update the selected cell
            selected = (Button)sender;
            selected.BackgroundColor = new Color(255, 250, 205); // Lemon yellow background color

            textInput.Text = current_grid.Cells[GetCellName(selected)].Identifier; // Update the text value corresponding to the cell
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
                // Close the application
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

                    // Reinitialize the grid
                    int rowCount = int.Parse(rootElement.Attribute("RowCount").Value);
                    int columnCount = int.Parse(rootElement.Attribute("ColumnCount").Value);
                    CreateGrid(rowCount, columnCount);

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
            await DisplayAlert("Довідка", "Лабораторна робота 1.Гіщак Іванни Богданівни",  "OK");
        }

        private void CreateGrid(int RowsCount, int ColumnsCount)
        {
            AddColumnsAndColumnLabels();
            AddRowsAndCellEntries();

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

                // Видаляємо рядок
                grid.RowDefinitions.RemoveAt(lastRowIndex);
                grid.Children.Remove(cells[$"0{lastRowIndex}"]); // Remove label

                for (var col = 1; col <= grid.ColumnDefinitions.Count - 1; col++)
                {
                    var cellName = GetColumnName(col) + lastRowIndex.ToString();
                    DeleteCell(cellName);
                }
            }
        }


        private void DeleteCell(string cellName)
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

        private void AddRowsAndCellEntries()
        {
            CurrentGrid current_grid = CurrentGrid.Instance;
            {
                int row = 0;
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(75) });
                

                for (int row_real = 1; row_real < CountRow+1; row_real++)
                {
                    //сітка буде мати на один рядок більше, і він готовий для розміщення в ньому елементів
                    //grid.RowDefinitions.Add(new RowDefinition());
                    grid.RowDefinitions.Add(new RowDefinition {  Height = new GridLength(75)  });
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
                        Grid.SetRow(button, row_real );
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



