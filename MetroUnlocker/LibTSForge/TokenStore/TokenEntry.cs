using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetroUnlocker.LibTSForge.TokenStore
{
    public class TokenEntry
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public byte[] Data { get; set; }
        public bool Populated { get; set; }

        public TokenEntry() : this("", "", new byte[] { }, false) {}
        public TokenEntry(string name, string extension, byte[] data, bool populated = true)
        {
            Name = name;
            Extension = extension;
            Data = data;
            Populated = populated;
        }
    }
}
