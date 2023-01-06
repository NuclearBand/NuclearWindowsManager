#nullable enable
using UnityEngine;

namespace Nuclear.WindowsManager
{
    public class Example4Window1ViewModel : IExample4Window1ViewModel
    {
        private readonly string _text;

        public Example4Window1ViewModel(string text) => _text = text;

        void IExample4Window1ViewModel.HandleButtonClick() => Debug.Log("Button clicked");

        string IExample4Window1ViewModel.Text => _text;
    }
}