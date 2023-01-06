#nullable enable
using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Nuclear.WindowsManager
{
    public class Example3Window2 : Window
    {
        public const string Path = "com.nuclearband.windowsmanager/Examples/Example3/TestWindow2";

        [SerializeField] private Image _background = null!;
        private float _size;

        public override void Init()
        {
            base.Init();
            _background.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            _background.rectTransform.localScale = Vector3.one * _size;
        }

        public static Action<Window> SetupWindow(float size) => 
            window => ((Example3Window2) window)._size = size;

        public void OpenTestWindow1Click()
        {
            var w = StaticWindowsManager.CreateWindow(Path, SetupWindow(_size * 0.9f));
            w.OnStartHide += _ => Close();
        }
    }
}
