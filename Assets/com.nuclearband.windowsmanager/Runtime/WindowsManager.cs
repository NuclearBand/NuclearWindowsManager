using UnityEngine;
using System.Collections.Generic;
using System;

namespace NuclearBand
{
    public static class WindowsManager
    {
        private class WindowBuildData
        {
            public string WindowPath { get; }
            public string Suffix { get; set; }

            public Action<Window>? SetupWindow { get; }

            public WindowBuildData(string windowPath, string suffix, Action<Window>? setupWindow)
            {
                this.WindowPath = windowPath;
                this.Suffix = suffix;
                this.SetupWindow = setupWindow;
            }
        }

        public static bool InputBlocked { get; private set; }
        public static GameObject InputBlockPrefab { get; private set; } = null!;

        private static Dictionary<string, Func<bool>> suffixesWithPredicates = new Dictionary<string, Func<bool>>();
        private static Transform root = null!;

        private static GameObject inputBlock = null!;
        private static int numBlocks;
        private static readonly List<WindowReference> windows = new List<WindowReference>();
        private static readonly List<WindowBuildData> windowBuildDataList = new List<WindowBuildData>();
        private static readonly Dictionary<string, GameObject> loadedWindowPrefabs = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, List<Window>> invisibleWindows = new Dictionary<string, List<Window>>();

        public static void Init(WindowsManagerSettings settings)
        {
            suffixesWithPredicates = settings.SuffixesWithPredicates;

            InputBlockPrefab = Resources.Load<GameObject>(settings.InputBlockPath) ??
                                   throw new ArgumentException("WindowsManager: Wrong path to InputBlock");

            InputBlocked = false;
            var rootPrefab = Resources.Load<GameObject>(settings.RootPath) ??
                             throw new ArgumentException("WindowsManager: Wrong path to root");

            root = GameObject.Instantiate(rootPrefab).transform;
            root.name = root.name.Replace("(Clone)", string.Empty);
            GameObject.DontDestroyOnLoad(root.gameObject);
        }

        public static WindowReference CreateWindow(string windowPath, Action<Window>? setupWindow = null)
        {
            var suffix = FindSuffixFor(windowPath);
            var window = InstantiateWindow(windowPath + suffix);

            var windowReference = new WindowReference(window);
            windows.Add(windowReference);
            windowBuildDataList.Add(new WindowBuildData(windowPath, suffix, setupWindow));

            setupWindow?.Invoke(window);
            window.OnHidden += WindowOnHidden;
            window.OnCloseForRebuild += WindowOnCloseForRebuild;
            window.Init();

            window.Show();
            return windowReference;
        }

        private static Window InstantiateWindow(string fullWindowPath)
        {
            Window window;
            if (invisibleWindows.ContainsKey(fullWindowPath))
            {
                var windowList = invisibleWindows[fullWindowPath];
                if (windowList.Count != 0)
                {
                    window = windowList[windowList.Count - 1];
                    window.gameObject.SetActive(true);
                    window.transform.SetAsLastSibling();
                    windowList.RemoveAt(windowList.Count - 1);
                    return window;
                }
            }

            var windowPrefab = GetWindowPrefab(fullWindowPath);

            window = GameObject.Instantiate(windowPrefab, root).GetComponent<Window>() ??
                     throw new ArgumentException($"WindowsManager: no Window script on window {fullWindowPath}");

            window.name = window.name.Replace("(Clone)", string.Empty);

            return window;
        }

        public static void PrefetchWindow(string windowPath)
        {
            var suffix = FindSuffixFor(windowPath);
            var window = InstantiateWindow(windowPath + suffix);
            window.gameObject.SetActive(false);
        }

        public static void RefreshLayout()
        {
            for (var i = 0; i < windows.Count; ++i)
            {
                var windowReference = windows[i];
                var path = windowBuildDataList[i].WindowPath;
                var suffix = FindSuffixFor(path);

                if (windowBuildDataList[i].Suffix != suffix)
                {
                    var windowTransientData = windowReference.Window.GetTransientData();
                    windowReference.Window.CloseForRebuild();
                    windowBuildDataList[i].Suffix = suffix;
                    var newWindow = InstantiateWindow(path + suffix);
                    windowBuildDataList[i].SetupWindow?.Invoke(newWindow);
                    newWindow.OnCloseForRebuild += WindowOnCloseForRebuild;
                    newWindow.InitAfterRebuild(windowTransientData);
                }
                else
                {
                    windowReference.Window.transform.SetAsLastSibling();
                }
            }
        }

        public static void ProcessBackButton()
        {
            for (var i = windows.Count - 1; i >= 0; --i)
            {
                var curWindow = windows[i].Window;
                if (!curWindow.ProcessBackButton)
                    continue;
                curWindow.OnBackButtonPressedCallback();
                break;
            }
        }

        public static void BringOnTop(Window window)
        {
            window.transform.SetAsLastSibling();
            for (var i = 0; i < windows.Count; ++i)
            {
                var windowReference = windows[i];
                if (windowReference.Window != window)
                    continue;
                windows.RemoveAt(i);
                windows.Add(windowReference);
                break;
            }
        }

        public static void BlockInput()
        {
            numBlocks++;
            if (InputBlocked)
                return;

            if (inputBlock == null)
            {
                inputBlock = GameObject.Instantiate(InputBlockPrefab, root);
                inputBlock.name = inputBlock.name.Replace("(Clone)", string.Empty);
            }

            inputBlock.transform.SetAsLastSibling();
            inputBlock.gameObject.SetActive(true);
            InputBlocked = true;
        }

        public static void UnblockInput(bool forced = false)
        {
            if (!InputBlocked)
                return;
            numBlocks--;
            if (forced)
                numBlocks = 0;

            if (numBlocks != 0)
                return;
            InputBlocked = false;
            inputBlock.gameObject.SetActive(false);
        }

        private static string FindSuffixFor(string windowPath)
        {
            foreach (var suffixWithPredicate in suffixesWithPredicates)
            {
                if (!suffixWithPredicate.Value.Invoke())
                    continue;

                var suffix = suffixWithPredicate.Key;
                if (invisibleWindows.ContainsKey(windowPath + suffix))
                    return suffix;

                if (CheckWindowPrefabExistence(windowPath + suffix))
                    return suffix;
            }

            return string.Empty;
        }

        private static bool CheckWindowPrefabExistence(string path)
        {
            if (loadedWindowPrefabs.ContainsKey(path))
                return true;
            return Resources.Load<GameObject>(path) != null;
        }

        private static GameObject GetWindowPrefab(string path)
        {
            GameObject windowPrefab;
            if (loadedWindowPrefabs.ContainsKey(path))
                windowPrefab = loadedWindowPrefabs[path];
            else
            {
                windowPrefab = Resources.Load<GameObject>(path) ??
                               throw new ArgumentException($"Can't load prefab with such path {path}");

                loadedWindowPrefabs.Add(path, windowPrefab);
            }

            return windowPrefab;
        }

        private static void WindowOnHidden(Window window)
        {
            window.OnHidden -= WindowOnHidden;
            window.OnCloseForRebuild -= WindowOnCloseForRebuild;

            for (var i = 0; i < windows.Count; ++i)
            {
                if (windows[i].Window != window)
                    continue;
                var windowPath = windowBuildDataList[i].WindowPath + windowBuildDataList[i].Suffix;
                if (!window.DestroyOnClose)
                {
                    if (!invisibleWindows.ContainsKey(windowPath))
                        invisibleWindows.Add(windowPath, new List<Window> {window});
                    else
                        invisibleWindows[windowPath].Add(window);
                }

                windows.RemoveAt(i);
                windowBuildDataList.RemoveAt(i);
                return;
            }
        }

        private static void WindowOnCloseForRebuild(Window window)
        {
            window.OnCloseForRebuild -= WindowOnCloseForRebuild;

            for (var i = 0; i < windows.Count; ++i)
            {
                if (windows[i].Window != window)
                    continue;
                var windowPath = windowBuildDataList[i].WindowPath + windowBuildDataList[i].Suffix;
                if (window.DestroyOnClose)
                    return;

                if (!invisibleWindows.ContainsKey(windowPath))
                    invisibleWindows.Add(windowPath, new List<Window> {window});
                else
                    invisibleWindows[windowPath].Add(window);
                return;
            }
        }
    }
}