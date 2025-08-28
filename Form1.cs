using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProfessionalSystemUtility
{
    public partial class Form1 : Form
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ISystemService _systemService;
        public string ProcessToRelaunch { get; set; }

        public Form1(IAuthenticationService authenticationService, ISystemService systemService)
        {
            InitializeComponent();
            _authenticationService = authenticationService;
            _systemService = systemService;
            grpActions.Visible = false;
            Text = "Authentication Required";
        }

        private async void BtnOk_Click(object sender, EventArgs e)
        {
            await ExecuteProtectedActionAsync();
        }

        private async Task ExecuteProtectedActionAsync()
        {
            string inputPassword = txtPassword.Text;
            UpdateStatus("Authenticating...", System.Drawing.Color.Gray);
            ToggleUIState(false);
            try
            {
                bool isAuthenticated = await Task.Run(() => _authenticationService.ValidatePassword(inputPassword));
                if (isAuthenticated)
                {
                    UpdateStatus("Authentication successful.", System.Drawing.Color.Green);
                    if (!string.IsNullOrEmpty(ProcessToRelaunch))
                    {
                        _systemService.RelaunchProcess(ProcessToRelaunch);
                    }
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    UpdateStatus("Authentication failed. Incorrect password.", System.Drawing.Color.Red);
                    MessageBox.Show("Incorrect Password. Access denied.", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.SelectAll();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("An error occurred.", System.Drawing.Color.Red);
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ToggleUIState(true);
            }
        }

        private void ChkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '*';
        }

        private void ToggleUIState(bool isEnabled)
        {
            txtPassword.Enabled = isEnabled;
            chkShowPassword.Enabled = isEnabled;
            btnOk.Enabled = isEnabled;
            UseWaitCursor = !isEnabled;
        }

        private void UpdateStatus(string text, System.Drawing.Color foreColor)
        {
            if (statusStrip1.InvokeRequired)
            {
                statusStrip1.Invoke(new Action(() =>
                {
                    toolStripStatusLabel1.Text = text;
                    toolStripStatusLabel1.ForeColor = foreColor;
                }));
            }
            else
            {
                toolStripStatusLabel1.Text = text;
                toolStripStatusLabel1.ForeColor = foreColor;
            }
        }
    }
}
