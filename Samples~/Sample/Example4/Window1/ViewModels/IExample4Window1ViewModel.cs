#nullable enable

namespace Nuclear.WindowsManager
{
    public interface IExample4Window1ViewModel : IWindowViewModel
    {
        string Text { get; }
        void HandleButtonClick();
    }
}