#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace Nuclear.WindowsManager
{
    public class Example2Window1 : Window
    {
        public const string Path = "com.nuclearband.windowsmanager/Examples/Example2/TestWindow1";

        [SerializeField] private InputField _inputField = null!;

        public void OpenTestWindow1Click() => 
            StaticWindowsManager.CreateWindow(Path);

        public void OpenTestWindow2Click() => 
            Example1Window2.CreateWindow(Example2Window2.Path, _inputField.text);
    }
}
