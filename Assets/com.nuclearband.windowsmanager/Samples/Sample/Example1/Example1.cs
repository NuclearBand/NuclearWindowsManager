#nullable enable
using UnityEngine;

namespace NuclearBand
{
    public class Example1 : MonoBehaviour
    {
        void Start()
        {
            var rootPath = "NuclearBand/Examples_WindowsManager/Example1/";
            WindowsManager.Init(new WindowsManagerSettings()
            {
                RootPath = rootPath + "Canvas",
                InputBlockPath = rootPath + "InputBlocker"
            });
            WindowsManager.CreateWindow(Example1Window1.Path);
        }
    }
}
