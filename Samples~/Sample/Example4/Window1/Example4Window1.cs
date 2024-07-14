#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace Nuclear.WindowsManager
{
    public class Example4Window1 : Window
    {
        #region Creation
        private const string Path = "com.nuclearband.windowsmanager/Examples/Example4/TestWindow1";

        public static Example4Window1 CreateWindow(IExample4Window1ViewModel viewModel) =>
            (Example4Window1)StaticWindowsManager.CreateWindow(Path, window => 
                window.SetViewModel(ref ((Example4Window1)window)._viewModel, viewModel));
        #endregion

        [SerializeField] private Text _label = null!;
        
        private IExample4Window1ViewModel _viewModel = null!;

        public override void Init()
        {
            base.Init();
            _label.text = _viewModel.Text;
        }

        public void ButtonClicked() => _viewModel.HandleButtonClick();
    }
}
