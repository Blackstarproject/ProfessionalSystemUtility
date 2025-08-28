using ProfessionalSystemUtility.Properties;
using System;
using System.Web.ApplicationServices;
using System.Windows.Forms;

namespace ProfessionalSystemUtility
{
    /// <summary>
    /// A hidden form that serves as the main application context, managing the 
    /// system tray icon and process monitoring. This is a robust pattern for tray apps.
    /// </summary>
    public class AppContextForm : Form
    {
        private readonly IAuthenticationService _authService;
        private readonly ISystemService _systemService;
        private readonly IProcessMonitorService _monitorService;
        private readonly NotifyIcon _notifyIcon;
        private Form1 _authForm;

        public AppContextForm(IAuthenticationService authService, ISystemService systemService, IProcessMonitorService monitorService)
        {
            _authService = authService;
            _systemService = systemService;
            _monitorService = monitorService;

            // Initialize the system tray icon
            _notifyIcon = new NotifyIcon()
            {
                Icon = Resources.SecurityIcon,
                ContextMenu = new ContextMenu(new MenuItem[] { new MenuItem("Exit", Exit) }),
                Visible = true,
                Text = "System Utility Monitor"
            };

            // Subscribe to the event from the monitor service
            _monitorService.ProtectedProcessDetected += OnProtectedProcessDetected;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // --- FIX ---
            // Start monitoring in the OnLoad event, which guarantees the UI thread is ready.
            // Also, make the form invisible so it only appears in the system tray.
            this.Visible = false;
            this.ShowInTaskbar = false;
            _monitorService.StartMonitoring();
        }

        private void OnProtectedProcessDetected(object sender, string processName)
        {
            // If the auth form is already open, don't open another one.
            if (_authForm != null && !_authForm.IsDisposed)
            {
                // Use BeginInvoke to safely bring the form to the front from a background thread.
                this.BeginInvoke(new Action(() => _authForm.Activate()));
                return;
            }

            // Use BeginInvoke to safely show the form from the background monitor thread.
            this.BeginInvoke(new Action(() => ShowAuthenticationForm(processName)));
        }

        private void ShowAuthenticationForm(string processName)
        {
            // First, block the unauthorized process
            _systemService.TerminateProcess(processName);

            // Now, show the password form
            _authForm = new Form1(_authService, _systemService)
            {
                ProcessToRelaunch = processName
            };

            _authForm.ShowDialog();
        }

        private void Exit(object sender, EventArgs e)
        {
            _monitorService.StopMonitoring();
            _notifyIcon.Visible = false;
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _notifyIcon?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
