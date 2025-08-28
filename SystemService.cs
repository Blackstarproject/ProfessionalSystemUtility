using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ProfessionalSystemUtility
{
    public interface ISystemService
    {
        void RelaunchProcess(string processName);
        void TerminateProcess(string processName);
    }

    public class SystemService : ISystemService
    {
        public void RelaunchProcess(string processName)
        {
            string executableName = processName;
            if (processName.Equals("mmc", StringComparison.OrdinalIgnoreCase))
            {
                executableName = "taskschd.msc";
            }
            StartProcess(executableName);
        }

        public void TerminateProcess(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                foreach (var process in processes)
                {
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to terminate process {processName}: {ex.Message}");
            }
        }

        private void StartProcess(string processName)
        {
            try
            {
                Process.Start(new ProcessStartInfo(processName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred while trying to open '{processName}':\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
