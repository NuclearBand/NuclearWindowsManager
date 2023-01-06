#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace Nuclear.WindowsManager
{
    public class Example1Window2 : Window
    {
        public const string Path = "com.nuclearband.windowsmanager/Examples/Example1/TestWindow2";

        [SerializeField] private Text _text = null!;

        private string _title = null!;

        public override void Init()
        {
            base.Init();
            _text.text = _title;
        }

        public static Window CreateWindow(string path, string title) => 
            StaticWindowsManager.CreateWindow(path, window => ((Example1Window2)window)._title = title);
    }
}
