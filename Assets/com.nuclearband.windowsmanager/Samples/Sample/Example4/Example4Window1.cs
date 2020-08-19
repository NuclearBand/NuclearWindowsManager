#nullable enable
namespace NuclearBand
{
    public class Example4Window1 : Window
    {
        public const string Path = "NuclearBand/Examples_WindowsManager/Example4/TestWindow1";

        public void OpenTestWindow1Click()
        {
            WindowsManager.CreateWindow(Example4Window2.Path, Example4Window2.SetupWindow(1.0f));
        }
    }
}
