//CurrentCell.cs
namespace LabCalculator;
public class CurrentCell
{
    public string Identifier { get; set; }
    public double Value { get; set; }

    public CurrentCell() : this("") { }
    public IList<string> AppearsIn { get; }

    //DependsOn, contrary to AppearsIn, can be drastically changed (ex. A1 = B1 -> A1 = B4 ...)
    public IList<string> DependsOn { get; set; }

    public CurrentCell(string identifier)
    {
        Identifier = identifier;
        Value = 0.0; // initial value
        AppearsIn = new List<string>();
        DependsOn = new List<string>();
    }

    public string GetText()
    {
        return Identifier == "" ? "" : Value.ToString();
    }
}
