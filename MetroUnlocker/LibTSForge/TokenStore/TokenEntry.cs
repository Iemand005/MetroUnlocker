using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetroUnlocker.LibTSForge.TokenStore
{
    public class TokenEntry
    {
        public string Name;
        public string Extension;
        public byte[] Data;
        public bool Populated;
    }
}
