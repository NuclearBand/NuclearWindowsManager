#nullable enable
using System.Collections;
using UnityEngine;

namespace Nuclear.WindowsManager
{
    public class TweenAlphaWindowAnimation : WindowAnimation
    {
         private CanvasGroup _canvasGroup = null!;

         [SerializeField]
         private bool _showAnimation;

         private void Awake() => _canvasGroup = GetComponent<CanvasGroup>();

         public override void Play()
         {
             if (_showAnimation)
             {
                 _canvasGroup.alpha = 0.0f;
                 StartCoroutine(Appear());
             }
             else
             {
                 _canvasGroup.alpha = 1.0f;
                 StartCoroutine(Fade());
             }
         }

         private IEnumerator Fade()
        {
            for (var alpha = 1.0f; alpha >= -0.05f; alpha -= 0.1f)
            {
                _canvasGroup.alpha = alpha;
                yield return new WaitForSeconds(0.15f);
            }

            End();
        }

        private IEnumerator Appear()
        {
            for (var alpha = 0.0f; alpha <= 1.05f; alpha += 0.1f)
            {
                _canvasGroup.alpha = alpha;
                yield return new WaitForSeconds(0.15f);
            }

            End();
        }
    }
}