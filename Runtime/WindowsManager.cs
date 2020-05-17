using UnityEngine;
using System.Collections.Generic;
using System;

namespace NuclearBand
{
    public static class WindowsManager
    {
        class WindowBuildData
        {
            public readonly string WindowPath;
            public string Suffix;
            public readonly Action<Window> SetupWindow;
            public WindowBuildData(string windowPath, string suffix, Action<Window> setupWindow)
            {
                this.WindowPath = windowPath;
                this.Suffix = suffix;
                this.SetupWindow = setupWindow;
            }
        }

        public static bool InputBlocked { get; set; }
        public static GameObject InputBlockPrefab { get; private set; }

        static Dictionary<string, Func<bool>> suffixesWithPredicates;
        static Transform root;

        static GameObject inputBlock;
        static int numBlocks;
        static List<WindowReference> windows;
        static List<WindowBuildData> windowBuildDataList;
        static Dictionary<string, GameObject> loadedWindowPrefabs;
        static Dictionary<string, List<Window>> invisibleWindows;

        public static void Init(WindowsManagerSettings settings)
        {
            suffixesWithPredicates = settings.SuffixesWithPredicates;
            windows = new List<WindowReference>();
            windowBuildDataList = new List<WindowBuildData>();
            loadedWindowPrefabs = new Dictionary<string, GameObject>();
            invisibleWindows = new Dictionary<string, List<Window>>();

            InputBlockPrefab = Resources.Load<GameObject>(settings.InputBlockPath);
            if (InputBlockPrefab == null)
                Debug.LogError("WindowsManager: Wrong path to InputBlock");
            InputBlocked = false;
            var rootPrefab = Resources.Load<GameObject>(settings.RootPath);
            if (rootPrefab == null)
                Debug.LogError("WindowsManager: Wrong path to root");
            root = GameObject.Instantiate(rootPrefab).transform;
            root.name = root.name.Replace("(Clone)", "");
            GameObject.DontDestroyOnLoad(root.gameObject);
        }

        public static WindowReference CreateWindow(string windowPath, Action<Window> setupWindow = null)
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
            Window window = null;
            if (invisibleWindows.ContainsKey(fullWindowPath))
            {
                var windowList = invisibleWindows[fullWindowPath];
                if (windowList.Count != 0)
                {
                    window = windowList[windowList.Count - 1];
                    window.gameObject.SetActive(true);
                    window.transform.SetAsLastSibling();
                    windowList.RemoveAt(windowList.Count - 1);
                }
            }

            if (window == null)
            {
                var windowPrefab = GetWindowPrefab(fullWindowPath);
                if (windowPrefab == null)
                    return null;
                window = GameObject.Instantiate(windowPrefab, root).GetComponent<Window>();
                if (window == null)
                    Debug.LogError("WindowsManager: no window script on window " + fullWindowPath);
                window.name = window.name.Replace("(Clone)", "");
            }

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
                inputBlock.name = inputBlock.name.Replace("(Clone)", "");
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

                var windowPrefab = GetWindowPrefab(windowPath + suffix);
                if (windowPrefab != null)
                    return suffix;
            }

            return "";
        }

        static GameObject GetWindowPrefab(string path)
        {
            GameObject windowPrefab;
            if (loadedWindowPrefabs.ContainsKey(path))
            {
                windowPrefab = loadedWindowPrefabs[path];
            }
            else
            {
                windowPrefab = Resources.Load<GameObject>(path);
                if (windowPrefab != null)
                    loadedWindowPrefabs.Add(path, windowPrefab);
            }

            return windowPrefab;
        }

        static void WindowOnHidden(Window window)
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

        static void WindowOnCloseForRebuild(Window window)
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