﻿#nullable enable
using System;

namespace Nuclear.WindowsManager
{
    public interface IWindowsManager : IDisposable
    {
        event Action<Window> OnWindowCreated;
        event Action<Window> OnWindowClosed;

        Window CreateWindow(string path, Action<Window>? setupWindow = null);
        void PrefetchWindow(string path);
    }
}