using System.Text;
using UsageCheckerService.Models;

namespace UsageCheckerService.Utils;

public class ReportPrinter
{
    private ProcessInfo _currentState;
    private ProcessInfo[] _topProcesses;
    private ProcessInfo[] _iisProcesses;
    private ProcessInfo[] _stateHistory;
    
    public void SetCurrentState(ProcessInfo currentState) =>
        _currentState = currentState;
    
    public void SetTopProcesses(ProcessInfo[] topProcesses) =>
        _topProcesses = topProcesses;
    
    public void SetIISProcesses(ProcessInfo[] iisProcesses) =>
        _iisProcesses = iisProcesses;
    
    public void SetStateHistory(ProcessInfo[] stateHistory) =>
        _stateHistory = stateHistory;
    
    public string Print()
    {
        var sb = new StringBuilder();
        sb.Append(PrintCurrentState());
        sb.Append(PrintProcesses(_topProcesses, $"Top {_topProcesses.Length} processes"));
        sb.Append(PrintProcesses(_iisProcesses, "IIS info"));
        sb.Append(PrintStateHistory());
        return sb.ToString();
    }
    
    private string PrintCurrentState()
    {
        if (_currentState == null)
        {
            return string.Empty;
        }
        
        return _currentState.ToString();
    }
    
    private static string PrintProcesses(ProcessInfo[] processes, string title)
    {
        if (processes == null || processes.Length == 0)
        {
            return string.Empty;
        }
        
        var sb = new StringBuilder();
        sb.Append($"<br/><br/><b>{title}:</b><br/>");
        foreach (var process in processes
                     .OrderByDescending(x => x.UsedProcessor)
                     .ThenByDescending(x => x.UsedMemory))
        {
            sb.AppendLine($" - {process.ToString()}<br/>");
        }
        return sb.ToString();
    }
    
    private string PrintStateHistory()
    {
        if (_stateHistory == null)
        {
            return string.Empty;
        }
        
        var sb = new StringBuilder();
        sb.Append(PrintGraph("CPU usage (%)", _stateHistory.Select(x => x.UsedProcessor).ToArray()));
        sb.Append(PrintGraph("RAM usage (%)", _stateHistory.Select(x => x.UsedMemory).ToArray()));
        
        return sb.ToString();
    }
    
    private static string PrintGraph(string title, double[] values)
    {
        var sb = new StringBuilder();
        sb.Append($"<br/><br/><b>{title}:</b><br/>");
        var max = values.Max(x => x);
        var min = values.Min(x => x);
        for (var i = 10 - 1; i >= 0; i--)
        {
            sb.Append($"{i * 10:00} |");
            foreach (var v in values)
            {
                var p = v / 10;
                sb.Append(p >= i ? " ■ |" : " □ |");
            }
            sb.Append("<br/>");
        }

        for (var i = 0; i < values.Length + 2; i++)
        {
            sb.Append("----");
        }
        
        sb.Append("<br/>");
        sb.Append($"Used - Min: {min:F2}%, Max: {max:F2}%");
        
        return sb.ToString();
    }
}