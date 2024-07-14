#nullable enable
using UnityEngine;

namespace Nuclear.WindowsManager
{
    public class Example3 : MonoBehaviour
    {
        private void Start()
        {
            const string rootPath = "com.nuclearband.windowsmanager/Examples/Example1/";
            StaticWindowsManager.Init(new WindowsManagerSettings(rootPath + "Canvas",
                rootPath + "InputBlocker"
            ));
            StaticWindowsManager.CreateWindow(Example3Window1.Path);
        }
    }
}
