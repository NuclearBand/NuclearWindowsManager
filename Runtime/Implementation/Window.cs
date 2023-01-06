#nullable enable
using System;
using UnityEngine;

namespace Nuclear.WindowsManager
{
    public class Window : MonoBehaviour
    {
        public event Action<Window> OnShown = delegate { };
        public event Action<Window> OnHidden = delegate { };
        public event Action<Window> OnStartShow = delegate { };
        public event Action<Window> OnStartHide = delegate { };

        private enum WindowState
        {
            Opened, Closed, Opening, Closing
        }

        public bool WithInputBlockForBackground;
        public bool DestroyOnClose;
        public bool ProcessBackButton = true;

        [SerializeField] private WindowAnimation? _showAnimation;
        [SerializeField] private WindowAnimation? _hideAnimation;

        private WindowState _windowState = WindowState.Closed;

        private GameObject? _inputBlock;
        private Action? _customBackButtonAction;
        private Action _customDeInitActions = delegate { };

        public virtual void Init()
        {
            if (_showAnimation != null) 
                _showAnimation.OnEnd += EndShowAnimationCallback;

            if (_hideAnimation != null) 
                _hideAnimation.OnEnd += CloseInternal;
        }

        public void Show(bool immediate = false)
        {
            OnStartShow.Invoke(this);
            _windowState = WindowState.Opening;
            if (_showAnimation != null && !immediate)
                _showAnimation.Play();
            else
                EndShowAnimationCallback();
        }

        public void Close()
        {
            _windowState = WindowState.Closing;
            OnStartHide.Invoke(this);
            if (_hideAnimation != null)
                _hideAnimation.Play();
            else
                CloseInternal();
        }

        public void ProcessBackButtonPress()
        {
            if (_windowState == WindowState.Closing) 
                return;
            
            if (_customBackButtonAction != null)
                _customBackButtonAction.Invoke();
            else
                Close();
        }
        
        protected void SetCustomBackButtonAction(Action? action) =>
            _customBackButtonAction = action;

        public void AddDeInitAction(Action action) =>
            _customDeInitActions += action;

        private void EndShowAnimationCallback()
        {
            OnShown.Invoke(this);
            _windowState = WindowState.Opened;
        }

        private void CloseInternal()
        {
            _windowState = WindowState.Closed;
            var onHidden = OnHidden;
            DeInit();
            if (DestroyOnClose)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
            onHidden(this);
        }

        private void DeInit()
        {
            OnStartShow = delegate { };
            OnShown = delegate { };
            OnStartHide = delegate { };
            OnHidden = delegate { };
            if (_showAnimation != null) 
                _showAnimation.OnEnd -= EndShowAnimationCallback;

            if (_hideAnimation != null) 
                _hideAnimation.OnEnd -= CloseInternal;
            
            _customDeInitActions.Invoke();
            _customDeInitActions = delegate { };
        }
    }
}
