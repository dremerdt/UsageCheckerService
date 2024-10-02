using System.Text;
using Microsoft.Extensions.Primitives;
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
    
    private string PrintProcesses(ProcessInfo[] processes, string title)
    {
        if (processes == null)
        {
            return string.Empty;
        }
        
        var sb = new StringBuilder();
        sb.Append($"<br/><br/><b>{title}:</b><br/>");
        foreach (var process in processes)
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
        sb.Append(PrintCpuGraph());
        sb.Append(PrintRamGraph());
        
        return sb.ToString();
    }
    
    private string PrintCpuGraph()
    {
        var sb = new StringBuilder();
        sb.Append("<br/><br/><b>CPU usage (%):</b><br/>");
        var max = _stateHistory.Max(x => x.UsedProcessor);
        var min = _stateHistory.Min(x => x.UsedProcessor);
        for (var i = 10 - 1; i >= 0; i--)
        {
            sb.Append($"{i * 10:00} |");
            foreach (var info in _stateHistory)
            {
                var p = info.UsedProcessor / 10;
                sb.Append(p >= i ? " X |" : " _ |");
            }
            sb.Append("<br/>");
        }

        for (var i = 0; i < _stateHistory.Length + 2; i++)
        {
            sb.Append("----");
        }
        
        sb.Append("<br/>");
        sb.Append($"Used - Min: {min}%, Max: {max}%");
        
        return sb.ToString();
    }
    
    private string PrintRamGraph()
    {
        var sb = new StringBuilder();
        sb.Append("<br/><br/><b>RAM usage (%):</b><br/>");
        var max = _stateHistory.Max(x => x.UsedMemory);
        var min = _stateHistory.Min(x => x.UsedMemory);
        for (var i = 10 - 1; i >= 0; i--)
        {
            sb.Append($"{i * 10:00} |");
            foreach (var info in _stateHistory)
            {
                var p = info.UsedMemory / 10;
                sb.Append(p >= i ? " X |" : " _ |");
            }
            sb.Append("<br/>");
        }
        for (var i = 0; i < _stateHistory.Length + 2; i++)
        {
            sb.Append("----");
        }
        sb.Append("<br/>");
        sb.Append($"Used - Min: {min}%, Max: {max}%");
        
        return sb.ToString();
    }
}