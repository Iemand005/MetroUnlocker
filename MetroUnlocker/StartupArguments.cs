using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroUnlocker
{
    public enum StartupArgument
    {
        EnableLOBAndEnableSPP,
        EnableLOBAndDisableSPP,
        DisableLOBAndEnableSPP,
        DisableLOBAndDisableSPP,
        Other
    }

    public class StartupArguments
    {

        public static Dictionary<StartupArgument, string> KnownArguments = new Dictionary<StartupArgument, string>()
        {
            {StartupArgument.EnableLOBAndEnableSPP, "hi"},
            {StartupArgument.EnableLOBAndDisableSPP, "mister"},
            {StartupArgument.DisableLOBAndEnableSPP, "fox"},
            {StartupArgument.DisableLOBAndDisableSPP, ":3"},
        };

        public static StartupArgument GetStartupArgument(string[] args)
        {
            return KnownArguments.ContainsValue(args[1]) ? KnownArguments.First(arg => arg.Value == args[1]).Key : StartupArgument.Other;
        }

        public static string GetStartupArgumentString(StartupArgument startupArgument)
        {
            return KnownArguments[startupArgument];
        }

        public static bool HasStartupArgument(string[] args)
        {
            return args.Length > 1 && !HasOtherArgument(args);
        }

        public static bool HasOtherArgument(string[] args)
        {
            return GetStartupArgument(args) == StartupArgument.Other;
        }
    }
}
