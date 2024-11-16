namespace LabCalculator;
public class CurrentCell
{
    public string Identifier { get; set; }
    public double Value { get; set; }

    public CurrentCell(string identifier)
    {
        Identifier = identifier;
        Value = 0.0; // початкове значення
    }
}
