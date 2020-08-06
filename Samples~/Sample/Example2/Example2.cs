using UnityEngine;

namespace NuclearBand
{
    public class Example2 : MonoBehaviour
    {
        private void Start()
        {
            const string rootPath = "NuclearBand/Examples_WindowsManager/Example1/";
            WindowsManager.Init(new WindowsManagerSettings()
            {
                RootPath = rootPath + "Canvas",
                InputBlockPath = rootPath + "InputBlocker"
            });
            WindowsManager.CreateWindow(Example2Window1.Path);
        }
    }
}