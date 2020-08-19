#nullable enable
using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace NuclearBand
{
    public class Example4Window2 : Window
    {
        public const string Path = "NuclearBand/Examples_WindowsManager/Example4/TestWindow2";
        public Image background = null!;
        float size;


        public override void Init()
        {
            base.Init();
            background.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            background.rectTransform.localScale = Vector3.one * size;
        }

        public static Action<Window> SetupWindow(float size)
        {
            return window => (window as Example4Window2)!.size = size;
        }

        public void OpenTestWindow1Click()
        {
            var w = WindowsManager.CreateWindow(Path, SetupWindow(size * 0.9f)).Window;
            w.OnStartHide += window => Close();
        }
    }
}
