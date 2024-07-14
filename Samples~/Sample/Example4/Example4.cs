#nullable enable
using UnityEngine;

namespace Nuclear.WindowsManager
{
    public class Example4 : MonoBehaviour
    {
        private void Start()
        {
            const string rootPath = "com.nuclearband.windowsmanager/Examples/Example1/";
            StaticWindowsManager.Init(new WindowsManagerSettings(rootPath + "Canvas",
                rootPath + "InputBlocker"
            ));

            Example4Window1.CreateWindow(new Example4Window1ViewModel("Test"));
        }
    }
}
