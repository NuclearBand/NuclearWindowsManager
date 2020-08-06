using System;
using UnityEngine;

namespace NuclearBand
{
    /// <summary>
    /// Basic class for all game windows.
    /// </summary>
    public class Window : MonoBehaviour
    {
        #region WindowRebuildPart

        public class WindowTransientData
        {
            public event Action<Window>? OnStartShow;
            public event Action<Window>? OnShown;
            public event Action<Window>? OnStartHide;
            public event Action<Window>? OnHidden;
            public event Action<Window>? OnInitAfterRebuild;

            public readonly WindowState WindowState;


            public WindowTransientData(Window window)
            {
                OnStartShow = window.OnStartShow;
                OnShown = window.OnShown;
                OnStartHide = window.OnStartHide;
                OnHidden = window.OnHidden;
                OnInitAfterRebuild = window.OnInitAfterRebuild;
                WindowState = window.windowState;
            }

            public virtual void RestoreWindow(Window window)
            {
                window.OnStartHide = OnStartHide;
                window.OnHidden = OnHidden;
                window.OnStartShow = OnStartShow;
                window.OnShown = OnShown;
                window.OnInitAfterRebuild = OnInitAfterRebuild;
                window.windowState = WindowState;
            }
        }

        public virtual WindowTransientData GetTransientData()
        {
            return new WindowTransientData(this);
        }

        public virtual void InitAfterRebuild(WindowTransientData windowTransientData)
        {
            windowTransientData.RestoreWindow(this);
            if (WithInputBlockForBackground && inputBlock == null)
            {
                inputBlock = Instantiate(WindowsManager.InputBlockPrefab, transform);
                inputBlock.name = inputBlock.name.Replace("(Clone)", string.Empty);
                inputBlock.transform.SetAsFirstSibling();
            }
            OnInitAfterRebuild?.Invoke(this);
            switch (windowState)
            {
                case WindowState.Opened:
                case WindowState.Closed:
                    break;
                case WindowState.Opening:
                    EndShowAnimationCallback();
                    break;
                case WindowState.Closing:
                    EndHideAnimationCallback();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual void CloseForRebuild()
        {
            OnCloseForRebuild?.Invoke(this);
            CloseInternal();
        }
        #endregion
        public event Action<Window>? OnShown;
        public event Action<Window>? OnHidden;
        public event Action<Window>? OnStartShow;
        public event Action<Window>? OnStartHide;
        public event Action<Window>? OnCloseForRebuild;
        public event Action<Window>? OnInitAfterRebuild;


        public enum WindowState
        {
            Opened, Closed, Opening, Closing
        }

        public bool WithShowAnimation = true;
        public bool WithHideAnimation = true;
        public bool WithInputBlockForBackground = false;
        public bool DestroyOnClose = false;
        public bool ProcessBackButton = true;

        protected WindowState windowState = WindowState.Closed;

        private GameObject? inputBlock;

        public virtual void Init()
        {
            OnStartShow?.Invoke(this);

            if (!WithInputBlockForBackground && inputBlock != null)
            {
                Destroy(inputBlock);
                inputBlock = null;
            }

            if (WithInputBlockForBackground && inputBlock == null)
            {
                inputBlock = Instantiate(WindowsManager.InputBlockPrefab, transform);
                inputBlock.name = inputBlock.name.Replace("(Clone)", string.Empty);
                inputBlock.transform.SetAsFirstSibling();
            }
        }

        public void Show(bool immediate = false)
        {
            windowState = WindowState.Opening;
            if (WithShowAnimation && !immediate)
                StartShowAnimation();
            else
                EndShowAnimationCallback();
        }

        public virtual void Close()
        {
            windowState = WindowState.Closing;
            OnStartHide?.Invoke(this);
            if (WithHideAnimation)
                StartHideAnimation();
            else
                EndHideAnimationCallback();
        }

        protected virtual void StartShowAnimation()
        {
            EndShowAnimationCallback();
        }
        protected virtual void EndShowAnimationCallback()
        {
            OnShown?.Invoke(this);
            windowState = WindowState.Opened;
        }

        protected virtual void StartHideAnimation()
        {
            EndHideAnimationCallback();
        }

        protected virtual void EndHideAnimationCallback()
        {
            CloseInternal();
        }

        private void CloseInternal()
        {
            windowState = WindowState.Closed;
            var onClose = OnHidden;
            DeInit();
            if (DestroyOnClose)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
            onClose?.Invoke(this);
        }

        protected virtual void DeInit()
        {
            ClearEvents();
        }

        protected virtual void OnDestroy()
        {
            ClearEvents();
        }

        public virtual void OnBackButtonPressedCallback()
        {
            if (windowState != WindowState.Closing)
                Close();
        }

        protected virtual void ClearEvents()
        {
            OnStartShow = null;
            OnShown = null;
            OnStartHide = null;
            OnHidden = null;
            OnInitAfterRebuild = null;
            OnCloseForRebuild = null;
        }
    }
}
