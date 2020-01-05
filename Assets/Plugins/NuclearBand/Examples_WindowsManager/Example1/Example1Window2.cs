using UnityEngine.UI;

namespace NuclearBand
{
    public class Example1Window2 : Window
    {
        public const string Path = "NuclearBand/Examples_WindowsManager/Example1/TestWindow2";

        protected string title;
        public Text text;
        public override void Init()
        {
            base.Init();
            text.text = title;
        }

        public static WindowReference CreateWindow(string path, string title)
        {
            return WindowsManager.CreateWindow(path, window => (window as Example1Window2).title = title);
        }
    }
}