#nullable enable
using UnityEngine;

namespace Nuclear.WindowsManager
{
    public class Example2 : MonoBehaviour
    {
        private void Awake()
        {
            const string rootPath = "com.nuclearband.windowsmanager/Examples/Example1/";
            StaticWindowsManager.Init(new WindowsManagerSettings(rootPath + "Canvas",
                rootPath + "InputBlocker"
            ));
            StaticWindowsManager.CreateWindow(Example2Window1.Path);
        }
    }
}
