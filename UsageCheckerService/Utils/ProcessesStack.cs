using UsageCheckerService.Models;

namespace UsageCheckerService.Utils;

public class ProcessesStack(int size)
{
    private readonly ProcessInfo[] _processes = new ProcessInfo[size];

    public void Push(ProcessInfo process)
    {
        if (_processes.Any(x => x == null))
        {
            for (var i = 0; i < _processes.Length; i++)
            {
                if (_processes[i] != null) continue;
                _processes[i] = process;
                return;
            }
        }
        else
        {
            // shift all elements to the left
            for (var i = 0; i < _processes.Length - 1; i++)
            {
                _processes[i] = _processes[i + 1];
            }
            _processes[^1] = process;
        }
    }
    
    public ProcessInfo[] GetProcesses() => _processes.Where(x => x != null).ToArray();
}