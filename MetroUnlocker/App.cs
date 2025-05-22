using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using System.Diagnostics;
using Microsoft.Win32;

using MetroUnlocker.ProductPolicy;

namespace MetroUnlocker
{
    public partial class App : Form
    {
        public const string AppxRegistryKey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\Appx";

        string TrustedAppsPolicyName = "AllowAllTrustedApps";
        string DevelopmentPolicyName = "AllowDevelopmentWithoutDevLicense";
        string SpecialProfilesPolicyName = "AllowDeploymentInSpecialProfiles";

        public bool LOBEnabled
        {
            get { return GetGroupPolicy(TrustedAppsPolicyName); }
            set { SetGroupPolicy(TrustedAppsPolicyName, value); }
        }

        public bool DevelopmentEnabled
        {
            get { return GetGroupPolicy(DevelopmentPolicyName); }
            set { SetGroupPolicy(DevelopmentPolicyName, value); }
        }

        public bool SpecialProfilesEnabled
        {
            get { return GetGroupPolicy(SpecialProfilesPolicyName); }
            set { SetGroupPolicy(SpecialProfilesPolicyName, value); }
        }

        public void SetGroupPolicy(string policyName, bool enabled)
        {
            Registry.SetValue(AppxRegistryKey, policyName, enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        public bool GetGroupPolicy(string policyName)
        {
            object value = Registry.GetValue(AppxRegistryKey, policyName, 0);
            return value is int ? (int)value == 1 : false;
        }

        public App()
        {
            InitializeComponent();

            UpdatePolicyState();
        }

        public void UpdatePolicyState()
        {
            var productPolicyEditor = new ProductPolicyReader();

            var policyState = productPolicyEditor.GetPolicyStateByName("WSLicensingService-LOBSideloadingActivated");
            var isSideloadingKeyInstalled = LOBManager.IsSideloadingKeyInstalled();

            statusLabel.ForeColor = isSideloadingKeyInstalled ? Color.DarkGreen: Color.DarkOrange;
            disableButton.Enabled = isSideloadingKeyInstalled;

            switch (policyState)
            {
                case PolicyState.Disabled:
                    if (isSideloadingKeyInstalled)
                        statusLabel.Text = "Enabling...";
                    else
                    {
                        statusLabel.Text = "Disabled";
                        statusLabel.ForeColor = Color.DarkRed;
                    }
                    break;
                case PolicyState.Enabled:
                    statusLabel.Text = isSideloadingKeyInstalled ? "Sideloading enabled" : "Disabling...";
                    break;
                default:
                    statusLabel.Text = "Unknown";
                    statusLabel.ForeColor = Color.Black;
                    break;
            }
        }

        private void Jailbreak(object sender, EventArgs e)
        {
            try
            {
                Guid productKey;
                if (LOBManager.IsSideloadingKeyInstalled(out productKey))
                    if (MessageBox.Show(this, "There is already a sideloading key installed. If you continue, the current key will be deleted and a new one will be generated.", "Already activated", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                        return;

                LOBManager.ActivateZeroCID();

                LOBEnabled = true;
                DevelopmentEnabled = true;

                MessageBox.Show(this, "Sideloading activated!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                UpdatePolicyState();
            }
            catch (Exception ex)
            {
                if (ex is COMException && ((uint)((COMException)ex).ErrorCode) == 0xC004F014)
                    MessageBox.Show(this, "You likely ran out of storage or something else caused your tokens.dat to get corrupted. SPPSVC will recreate it. Try rebooting to let Windows reactivate itself before trying again. Make sure you have at least 30MB free before using Sideloading Unlocker.", "The key was recognised but I can't activate it.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show(this, ex.Message, "Error while activating sideloading!", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
