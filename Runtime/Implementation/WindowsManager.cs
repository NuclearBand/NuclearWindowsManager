#nullable enable
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using NuclearBand;
using Object = UnityEngine.Object;

namespace Nuclear.WindowsManager
{
    public class WindowsManager : IWindowsManager
    {
        public event Action<Window> OnWindowCreated = delegate { };
        public event Action<Window> OnWindowClosed = delegate { };

        private readonly ReadOnlyDictionary<string, Func<bool>> _suffixesWithPredicates;
        private readonly Transform _root ;
        private readonly GameObject _inputBlockPrefab;

        private readonly List<Window> _windows = new();
        private readonly Dictionary<string, GameObject> _loadedWindowPrefabs = new();
        private readonly Dictionary<string, List<Window>> _invisibleWindows = new();

        public WindowsManager(WindowsManagerSettings settings)
        {
            _suffixesWithPredicates = settings.SuffixesWithPredicates;

            _inputBlockPrefab = Resources.Load<GameObject>(settings.InputBlockPath) ??
                                throw new ArgumentException("WindowsManager: Wrong path to InputBlock");

            var rootPrefab = Resources.Load<GameObject>(settings.RootPath) ??
                             throw new ArgumentException("WindowsManager: Wrong path to root");

            _root = Object.Instantiate(rootPrefab).transform;
            _root.name = _root.name.Replace("(Clone)", string.Empty);
            var backButtonEventManager = _root.gameObject.AddComponent<BackButtonEventManager>();
            backButtonEventManager.OnBackButtonPressed += OnBackButtonPressedCallback;
            Object.DontDestroyOnLoad(_root.gameObject);
        }

        Window IWindowsManager.CreateWindow(string path, Action<Window>? setupWindow)
        {
            var suffix = FindSuffixFor(path);
            var window = InstantiateWindow(path + suffix);

            _windows.Add(window);
            setupWindow?.Invoke(window);
            window.OnHidden += WindowOnHidden;
            InstantiateInputBlockIfNeeded(window);
            window.Init();
            
            window.Show();
            OnWindowCreated(window);
            return window;
        }
        
        private void InstantiateInputBlockIfNeeded(Window window)
        {
            if (!window.WithInputBlockForBackground) 
                return;
            
            var inputBlock = Object.Instantiate(_inputBlockPrefab, window.transform);
            inputBlock.name = inputBlock.name.Replace("(Clone)", string.Empty);
            inputBlock.transform.SetAsFirstSibling();
        }

        private void DestroyInputBlockIfNotNeeded(Window window)
        {
            if (!window.WithInputBlockForBackground) 
                return;

            var inputBlock = window.transform.GetChild(0);
            Object.Destroy(inputBlock.gameObject);
        }

        void IWindowsManager.PrefetchWindow(string path)
        {
            var suffix = FindSuffixFor(path);
            var window = InstantiateWindow(path + suffix);
            window.gameObject.SetActive(false);
        }

        private Window InstantiateWindow(string fullWindowPath)
        {
            Window window;
            var windowName = fullWindowPath.Split('/')[^1];
            if (_invisibleWindows.ContainsKey(windowName))
            {
                var windowList = _invisibleWindows[windowName];
                if (windowList.Count != 0)
                {
                    window = windowList[^1];
                    window.gameObject.SetActive(true);
                    window.transform.SetAsLastSibling();
                    windowList.RemoveAt(windowList.Count - 1);
                    return window;
                }
            }

            var windowPrefab = GetWindowPrefab(fullWindowPath);

            window = Object.Instantiate(windowPrefab, _root).GetComponent<Window>() ??
                     throw new ArgumentException($"WindowsManager: missing Window script on window {fullWindowPath}");

            window.name = window.name.Replace("(Clone)", string.Empty);

            return window;
        }

        private void OnBackButtonPressedCallback()
        {
            for (var i = _windows.Count - 1; i >= 0; --i)
            {
                var curWindow = _windows[i];
                if (!curWindow.ProcessBackButton)
                    continue;
                curWindow.ProcessBackButtonPress();
                break;
            }
        }

        private string FindSuffixFor(string windowPath)
        {
            foreach (var (suffix, predicate) in _suffixesWithPredicates)
            {
                if (!predicate.Invoke())
                    continue;

                var windowName = windowPath.Split('/')[^1];
                if (_invisibleWindows.ContainsKey(windowName + suffix))
                    return suffix;

                if (CheckWindowPrefabExistence(windowPath + suffix))
                    return suffix;
            }

            return string.Empty;
        }

        private bool CheckWindowPrefabExistence(string path)
        {
            if (_loadedWindowPrefabs.ContainsKey(path))
                return true;
            return Resources.Load<GameObject>(path) != null;
        }

        private GameObject GetWindowPrefab(string path)
        {
            GameObject windowPrefab;
            if (_loadedWindowPrefabs.ContainsKey(path))
                windowPrefab = _loadedWindowPrefabs[path];
            else
            {
                windowPrefab = Resources.Load<GameObject>(path) ??
                               throw new ArgumentException($"Can't load prefab with such path {path}");

                _loadedWindowPrefabs.Add(path, windowPrefab);
            }

            return windowPrefab;
        }

        private void WindowOnHidden(Window window)
        {
            window.OnHidden -= WindowOnHidden;

            for (var i = 0; i < _windows.Count; ++i)
            {
                if (_windows[i] != window)
                    continue;
                if (!window.DestroyOnClose)
                {
                    DestroyInputBlockIfNotNeeded(window);
                    if (!_invisibleWindows.ContainsKey(window.name))
                        _invisibleWindows.Add(window.name, new List<Window> {window});
                    else
                        _invisibleWindows[window.name].Add(window);
                }

                _windows.RemoveAt(i);
                OnWindowClosed.Invoke(window);
                return;
            }
        }
    }
}
