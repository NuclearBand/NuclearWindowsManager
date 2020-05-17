using UnityEngine;

namespace NuclearBand
{
    public class Example2 : MonoBehaviour
    {
        void Start()
        {
            var rootPath = "NuclearBand/Examples_WindowsManager/Example1/";
            WindowsManager.Init(new WindowsManagerSettings()
            {
                RootPath = rootPath + "Canvas",
                InputBlockPath = rootPath + "InputBlocker"
            });
            var window = WindowsManager.CreateWindow(Example2Window1.Path).Window;
        }
    }
}