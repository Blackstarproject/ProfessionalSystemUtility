using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ProfessionalSystemUtility
{
    public interface IProcessMonitorService
    {
        event EventHandler<string> ProtectedProcessDetected;
        void StartMonitoring();
        void StopMonitoring();
    }

    public class ProcessMonitorService : IProcessMonitorService
    {
        public event EventHandler<string> ProtectedProcessDetected;

        private Timer _monitorTimer;
        private readonly string[] _protectedProcesses = { "taskmgr", "regedit", "mmc" }; // mmc hosts taskschd.msc
        private volatile bool _isChecking = false; // Prevents re-entrancy

        public void StartMonitoring()
        {
            _monitorTimer = new Timer(MonitorProcesses, null, 0, 1000);
        }

        public void StopMonitoring()
        {
            _monitorTimer?.Change(Timeout.Infinite, 0);
            _monitorTimer?.Dispose();
        }

        private void MonitorProcesses(object state)
        {
            if (_isChecking) return; // Don't run if the previous check is still running
            _isChecking = true;

            try
            {
                var runningProcessNames = Process.GetProcesses().Select(p => p.ProcessName).ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var processName in _protectedProcesses)
                {
                    if (runningProcessNames.Contains(processName))
                    {
                        ProtectedProcessDetected?.Invoke(this, processName);
                        // Once detected, we can stop checking this cycle to avoid multiple pop-ups
                        return;
                    }
                }
            }
            finally
            {
                _isChecking = false;
            }
        }
    }
}
