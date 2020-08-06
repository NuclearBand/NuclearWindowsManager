using System.Collections;
using UnityEngine;

namespace NuclearBand
{
    public class Example2Window2 : Example1Window2
    {
        public new const string Path = "NuclearBand/Examples_WindowsManager/Example2/TestWindow2";
        public CanvasGroup canvasGroup = null!;

        protected override void StartShowAnimation()
        {
            canvasGroup.alpha = 0.0f;
            StartCoroutine(Appear());
        }

        protected override void StartHideAnimation()
        {
            StartCoroutine(Fade());
        }

        IEnumerator Fade()
        {
            for (var alpha = 1.0f; alpha >= -0.05f; alpha -= 0.1f)
            {
                canvasGroup.alpha = alpha;
                yield return new WaitForSeconds(0.15f);
            }

            EndHideAnimationCallback();
        }

        IEnumerator Appear()
        {
            for (var alpha = 0.0f; alpha <= 1.05f; alpha += 0.1f)
            {
                canvasGroup.alpha = alpha;
                yield return new WaitForSeconds(0.15f);
            }

            EndShowAnimationCallback();
        }

        protected override void DeInit()
        {
            base.DeInit();
            StopAllCoroutines();
            canvasGroup.alpha = 1.0f;
        }
    }
}