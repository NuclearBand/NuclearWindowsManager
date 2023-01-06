#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nuclear.WindowsManager
{
    public class WindowsManagerSettings
    {
        public string RootPath { get; }
        public string InputBlockPath { get; }
        public ReadOnlyDictionary<string, Func<bool>> SuffixesWithPredicates { get; } = new (new Dictionary<string, Func<bool>>());
        
        public WindowsManagerSettings(string rootPath = "Root", 
            string inputBlockPath = "InputBlock", 
            IDictionary<string, Func<bool>>? suffixesWithPredicates = null)
        {
            RootPath = rootPath;
            InputBlockPath = inputBlockPath;
            if (suffixesWithPredicates != null)
                SuffixesWithPredicates = new ReadOnlyDictionary<string, Func<bool>>(suffixesWithPredicates);
        }
    }
}
