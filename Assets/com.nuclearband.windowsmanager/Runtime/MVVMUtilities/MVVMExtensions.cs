#nullable enable
using System;
using System.Linq;

namespace Nuclear.WindowsManager
{
    public static class MvvmExtensions
    {
        public static void SetViewModel<TViewModel>(this Window window, ref TViewModel currentValue, TViewModel newValue) 
            where TViewModel : IWindowViewModel
        {
#if UNITY_EDITOR
            if (typeof(TViewModel).GetInterfaces().ToList().Contains(typeof(IDisposable)))
                throw new ArgumentException("MVVM extension validation - SetViewModel with IDisposable type {0}", 
                    typeof(TViewModel).FullName);
#endif
            currentValue = newValue;
        }

        public static void SetChildViewModel<TViewModel>(this Window window, 
            ref TViewModel currentValue, TViewModel newValue)
            where TViewModel : IWindowViewModel
        {
            currentValue = newValue;
        }

        public static void SetDisposableViewModel<TViewModel>(this Window window, 
            ref TViewModel currentValue, TViewModel newValue) where TViewModel : IDisposableWindowViewModel
        {
            currentValue = newValue;
            window.AddDeInitAction(newValue.Dispose);
        }
    }
}