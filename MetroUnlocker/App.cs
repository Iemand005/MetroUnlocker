using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;
using Microsoft.Win32;

using MetroUnlocker.ProductPolicy;

namespace MetroUnlocker
{
    public partial class App : Form
    {
        public const string AppxKey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\Appx";

        string trustedAppsPolicyName = "AllowAllTrustedApps";
        string developmentPolicyName = "AllowDevelopmentWithoutDevLicense";
        string specialProfilesPolicyName = "AllowDeploymentInSpecialProfiles";

        public bool LOBEnabled
        {
            get { return GetGroupPolicy(trustedAppsPolicyName); }
            set { SetGroupPolicy(trustedAppsPolicyName, value); }
        }

        public bool DevelopmentEnabled
        {
            get { return GetGroupPolicy(developmentPolicyName); }
            set { SetGroupPolicy(developmentPolicyName, value); }
        }

        public bool SpecialProfilesEnabled
        {
            get { return GetGroupPolicy(specialProfilesPolicyName); }
            set { SetGroupPolicy(specialProfilesPolicyName, value); }
        }

        public void SetGroupPolicy(string policyName, bool enabled)
        {
            Registry.SetValue(AppxKey, policyName, enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        public bool GetGroupPolicy(string policyName)
        {
            object value = Registry.GetValue(AppxKey, policyName, 0);
            return value is int ? (int)value == 1 : false;
        }

        public App()
        {
            InitializeComponent();

            UpdatePolicyState();
        }

        public void UpdatePolicyState()
        {
            var productPolicyEditor = new ProductPolicyEditor();

            var policyState = productPolicyEditor.GetPolicyStateByName("WSLicensingService-LOBSideloadingActivated");
            var isSideloadingKeyInstalled = LOBManager.IsSideloadingKeyInstalled();

            switch (policyState)
            {
                case PolicyState.Disabled:
                    statusLabel.Text = "Disabled";
                    statusLabel.ForeColor = Color.DarkRed;
                    break;
                case PolicyState.Enabled:
                    if (isSideloadingKeyInstalled)
                    {
                        statusLabel.Text = "Sideloading enabled";
                        statusLabel.ForeColor = Color.DarkGreen;
                    }
                    else
                    {
                        statusLabel.Text = "Sideloading will be disabled soon";
                        statusLabel.ForeColor = Color.DarkOrange;
                    }
                    break;
                case PolicyState.Unknown:
                    statusLabel.Text = "Unknown";
                    statusLabel.ForeColor = Color.Black;
                    break;
            }
        }

        private string CombineArguments(params string[] arguments)
        {
            return string.Join(" ", arguments);
        }

        private void SetSetupParameter(string key, object value, RegistryValueKind valueKind)
        {
            Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\Setup", key, value, valueKind);
        }

        private void SetSetupType(int type)
        {
            SetSetupParameter("SetupType", type, RegistryValueKind.DWord);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartupArgument startupArgument;

            if (LOBCheckBox.Checked && SPPCheckBox.Checked)
                startupArgument = StartupArgument.EnableLOBAndEnableSPP;
            else if (LOBCheckBox.Checked)
                startupArgument = StartupArgument.EnableLOBAndDisableSPP;
            else if (SPPCheckBox.Checked)
                startupArgument = StartupArgument.DisableLOBAndEnableSPP;
            else
                startupArgument = StartupArgument.DisableLOBAndDisableSPP;

            string commandLine = CombineArguments(new string[] { Application.ExecutablePath, StartupArguments.GetStartupArgumentString(startupArgument) });

            SetSetupParameter("CmdLine", commandLine, RegistryValueKind.String);
            SetSetupType(1);
            DialogResult result = MessageBox.Show("Sideloading will be enabled after a reboot. Would you like to reboot now?", "Reboot?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            switch (result)
            {
                case DialogResult.Yes:
                    Rebooter.Reboot();
                    break;
                case DialogResult.Cancel:
                    SetSetupType(0);
                    break;
            }
        }

        private void JailbreakButton_Click(object sender, EventArgs e)
        {
            try
            {
                Guid productKey;
                if (LOBManager.IsSideloadingKeyInstalled(out productKey))
                    if (MessageBox.Show(this, "There is already a sideloading key installed. If you continue, the current key will be deleted and a new one will be generated.", "Already activated", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                        return;

                LOBManager.ActivateZeroCID();
                MessageBox.Show(this, "Sideloading activated!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdatePolicyState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error while activating sideloading!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Uninstall(object sender, EventArgs e)
        {
            Guid productKey;
            if (LOBManager.IsSideloadingKeyInstalled(out productKey))
            {
                if (MessageBox.Show(this, "Are you sure you want to disable sideloading?", "Really?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (LOBManager.UninstallSideloadingKey(productKey))
                        MessageBox.Show(this, "The sideloading key was uninstalled successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else MessageBox.Show(this, "Could not uninstall the sideloading key.", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdatePolicyState();
                }
            }
            else MessageBox.Show(this, "There is no sideloading key installed.", "I got nothing to do...", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
