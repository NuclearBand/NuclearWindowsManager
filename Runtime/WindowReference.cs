#nullable enable
namespace NuclearBand
{
    public class WindowReference
    {
        public Window Window { get; private set; }

        public WindowReference(Window window)
        {
            Window = window;
            window.OnInitAfterRebuild += OnInitAfterRebuildCallback;
        }

        private void OnInitAfterRebuildCallback(Window window)
        {
            Window.OnInitAfterRebuild -= OnInitAfterRebuildCallback;
            Window = window;
            Window.OnInitAfterRebuild += OnInitAfterRebuildCallback;
        }
    }
}
