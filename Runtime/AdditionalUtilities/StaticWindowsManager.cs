#nullable enable
using System;
using UnityEngine;

namespace Nuclear.WindowsManager
{
    public static class StaticWindowsManager
    {
        private static IWindowsManager? _instance;

        public static void Init(WindowsManagerSettings settings)
        {
            if (_instance != null)
            {
                Debug.LogError("WindowsManager already initialized");
                return;
            }

            _instance = new WindowsManager(settings);
        }

        public static Window CreateWindow(string path, Action<Window>? setupWindow = null) =>
            _instance != null
                ? _instance.CreateWindow(path, setupWindow)
                : throw new ArgumentException("WindowsManager not initialized");
        
        public static void PrefetchWindow(string path)
        {
            if (_instance != null)
                _instance.PrefetchWindow(path);
            else
                throw new ArgumentException("WindowsManager not initialized");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            _instance = null!;
        }
    }
}