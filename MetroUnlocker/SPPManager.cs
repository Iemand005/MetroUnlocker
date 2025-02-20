using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;

namespace MetroUnlocker
{
    enum SPPStartupMode
    {
        Disabled,
        Enabled
    }

    class SPPManager
    {
        private static string LogFullName = "C:\\Temp\\W8Sideloader.log";

        private static string RunCommand(string command, string arguments)
        {
            string output;
            RunCommand(command, arguments, out output);
            return output;
        }

        private static void RunCommand(string command, string arguments, out string output)
        {
            Process process = Process.Start(new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(error)) throw new Exception(error);
        }

        public static string SetStartupMode(SPPStartupMode startupMode)
        {
            string output;
            bool enabled = startupMode == SPPStartupMode.Enabled;
            try
            {

                RunCommand("sc", "sdset sppsvc D:(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;SY)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(A;;CCLCSWRPLOCRRC;;;IU)(A;;CCLCSWRPLOCRRC;;;SU)(A;;LCRP;;;AU)S:(AU;FA;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;WD)", out output);
                //RunCommand("sc", "config sppsvc start=" + (enabled ? "enabled" : "disabled"), out output);
                using (var m = new ManagementObject("Win32_Service.Name=\"sppsvc\""))
                    m.InvokeMethod("ChangeStartMode", new object[] { enabled ? "Automatic" : "Disabledx" });
                
                RunCommand("net", "stop sppsvc", out output);

                if (!enabled)
                {
                    DisableScheduledTask("SvcRestartTask");
                    DisableScheduledTask("SvcRestartTaskLogon");
                    DisableScheduledTask("SvcRestartTaskNetwork");
                }

                //MessageBox.Show(output + "\n----------------------\n", "sppsvc config change");
            }
            catch { throw; }
            return output;
        }

        private static void DisableScheduledTask(string key)
        {
            try
            {
                RunCommand("schtasks", "/change /disable /tn \"\\Microsoft\\Windows\\SoftwareProtectionPlatform\\" + key + "\"");
            }
            catch { throw; }
        }

        public static string Disable()
        {
            try
            {
                return SetStartupMode(SPPStartupMode.Disabled);
            }
            catch { throw; }
        }

        public static string Enable()
        {
            try
            {
                return SetStartupMode(SPPStartupMode.Enabled);
            }
            catch { throw; }
        }
    }
}
