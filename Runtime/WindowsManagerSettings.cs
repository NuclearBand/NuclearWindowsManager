using System;
using System.Collections.Generic;

namespace NuclearBand
{
    public class WindowsManagerSettings
    {
        public string RootPath = "Root";
        public string InputBlockPath = "InputBlock";
        public Dictionary<string, Func<bool>> SuffixesWithPredicates = new Dictionary<string, Func<bool>>();
    }
}
