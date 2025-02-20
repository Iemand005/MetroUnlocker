using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Runtime.InteropServices;
using System.Management;

namespace MetroUnlocker
{
    public class Rebooter
    {
        public enum RebootMethod
        {
            LogOff = 0,
            PowerOff = 8,
            Reboot = 2,
            ShutDown = 1,
            Suspend = -1,
            Hibernate = -2
        }

        public static void Reboot(RebootMethod method = RebootMethod.Reboot)
        {
            ManagementClass management = new ManagementClass("Win32_OperatingSystem");
            management.Scope.Options.EnablePrivileges = true;
            ManagementBaseObject mboShutdownParams = management.GetMethodParameters("Win32Shutdown");
            mboShutdownParams["Flags"] = method;
            mboShutdownParams["Reserved"] = 0;
            management.GetInstances().OfType<ManagementObject>().First().InvokeMethod("Win32Shutdown", mboShutdownParams, null);
        }
    }
}
