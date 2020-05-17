
namespace NuclearBand
{
    public class Example3Window1 : Example2Window1
    {
        public new const string Path = "NuclearBand/Examples_WindowsManager/Example3/TestWindow1";

        public new void OpenTestWindow1Click()
        {
            WindowsManager.CreateWindow(Example3Window1.Path);
        }

        public new void OpenTestWindow2Click()
        {
            Example3Window2.CreateWindow(Example3Window2.Path, inputField.text);
        }
    }
}
