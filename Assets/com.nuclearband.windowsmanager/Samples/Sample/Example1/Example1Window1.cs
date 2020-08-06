using UnityEngine.UI;

namespace NuclearBand
{
    public class Example1Window1 : Window
    {
        public const string Path = "NuclearBand/Examples_WindowsManager/Example1/TestWindow1";

        public InputField inputField = null!;

        public void OpenTestWindow1Click()
        {
            WindowsManager.CreateWindow(Example1Window1.Path);
        }

        public void OpenTestWindow2Click()
        {
            Example1Window2.CreateWindow(Example1Window2.Path, inputField.text);
        }
    }
}
