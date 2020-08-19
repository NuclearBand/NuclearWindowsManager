#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NuclearBand
{
    public class Example3 : MonoBehaviour
    {
        void Start()
        {
            var rootPath = "NuclearBand/Examples_WindowsManager/Example1/";
            WindowsManager.Init(new WindowsManagerSettings()
            {
                RootPath = rootPath + "Canvas",
                InputBlockPath = rootPath + "InputBlocker",
                SuffixesWithPredicates = new Dictionary<string, Func<bool>>
                {
                    {"_p", WindowPredicates.IsPortrait}
                }
            });
            OrientationEventManager.Instance.OnOrientationChanged += WindowsManager.RefreshLayout;
            WindowsManager.CreateWindow(Example3Window1.Path);
        }
    }
}
