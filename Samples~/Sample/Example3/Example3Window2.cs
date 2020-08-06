using UnityEngine.UI;

namespace NuclearBand
{
    public class Example3Window2 : Example2Window2
    {
        public new const string Path = "NuclearBand/Examples_WindowsManager/Example3/TestWindow2";
        public Text rebuildText = null!;
        private int rebuildNum;

        class Example3Window2_WindowTransientData : WindowTransientData
        {
            private readonly int rebuildNum;

            public Example3Window2_WindowTransientData(Window window) : base(window)
            {
                rebuildNum = (window as Example3Window2)!.rebuildNum;
            }

            public override void RestoreWindow(Window window)
            {
                base.RestoreWindow(window);
                (window as Example3Window2)!.rebuildNum = rebuildNum;
            }
        }

        public override WindowTransientData GetTransientData()
        {
            return new Example3Window2_WindowTransientData(this);
        }

        public override void Init()
        {
            base.Init();
            rebuildNum = 0;
            DrawRebuildNum();
        }

        public override void InitAfterRebuild(WindowTransientData windowTransientData)
        {
            base.InitAfterRebuild(windowTransientData);
            text.text = title;
            ++rebuildNum;
            DrawRebuildNum();
        }

        void DrawRebuildNum()
        {
            rebuildText.text = rebuildNum.ToString();
        }
    }
}