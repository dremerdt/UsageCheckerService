using System.Text;

namespace UsageCheckerService.Models;

public class ProcessInfo
{
    public string Name { get; set; }
    public double UsedProcessor { get; set; }
    public string UsedProcessorMeasure { get; set; } = "%";
    public double UsedMemory { get; set; }
    public string UsedMemoryMeasure { get; set; } = "MB";
    public DateTime? Date { get; set; }

    public ProcessInfo()
    {
        
    }
    public ProcessInfo(double usedProcessorPercent, float usedMemory, DateTime? date = null)
    {
        UsedProcessor = usedProcessorPercent;
        UsedMemory = usedMemory;
        Date = date;
    }

    public ProcessInfo(string name, double usedProcessorPercent, float usedMemory, DateTime? date = null) 
        : this(usedProcessorPercent, usedMemory, date)
    {
        Name = name;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        if (Name != null)
        {
            sb.Append($"<b>{Name}</b> ");
        }
        sb.Append($"CPU: {UsedProcessor:F2}{UsedProcessorMeasure} RAM: {UsedMemory:F2}{UsedMemoryMeasure}");
        if (Date != null)
        {
            sb.Append($", Date: {Date:dd-MM-yyyy HH:mm:ss}");
        }
        return sb.ToString();
    }
}