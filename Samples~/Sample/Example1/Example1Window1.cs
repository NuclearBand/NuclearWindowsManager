#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace Nuclear.WindowsManager
{
    public class Example1Window1 : Window
    {
        public const string Path = "com.nuclearband.windowsmanager/Examples/Example1/TestWindow1";

        [SerializeField] private InputField _inputField = null!;

        public void OpenTestWindow1Click() => 
            StaticWindowsManager.CreateWindow(Example1Window1.Path);

        public void OpenTestWindow2Click() => 
            Example1Window2.CreateWindow(Example1Window2.Path, _inputField.text);
    }
}
