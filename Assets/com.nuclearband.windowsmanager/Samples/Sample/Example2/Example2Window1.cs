namespace NuclearBand
{
    public class Example2Window1 : Example1Window1
    {
        public new const string Path = "NuclearBand/Examples_WindowsManager/Example2/TestWindow1";

        public new void OpenTestWindow1Click()
        {
            WindowsManager.CreateWindow(Path);
        }

        public new void OpenTestWindow2Click()
        {
            Example2Window2.CreateWindow(Example2Window2.Path, inputField.text);
        }
    }
}
