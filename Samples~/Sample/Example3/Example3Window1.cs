#nullable enable
namespace Nuclear.WindowsManager
{
    public class Example3Window1 : Window
    {
        public const string Path = "com.nuclearband.windowsmanager/Examples/Example3/TestWindow1";

        public void OpenTestWindow1Click() => 
            StaticWindowsManager.CreateWindow(Example3Window2.Path, Example3Window2.SetupWindow(1.0f));
    }
}
