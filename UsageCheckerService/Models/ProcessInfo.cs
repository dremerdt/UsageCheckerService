namespace UsageCheckerService.Models;

public class ProcessInfo
{
    public string Name { get; set; }
    public float UsedProcessorPercent { get; set; }
    public float UsedMemory { get; set; }
    
    public override string ToString() => $"Name: {Name} CPU: {UsedProcessorPercent}% RAM: {UsedMemory}MB";
}