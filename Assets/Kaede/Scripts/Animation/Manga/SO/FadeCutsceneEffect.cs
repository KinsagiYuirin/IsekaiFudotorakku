using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kaede.Scripts.Animation.Manga.SO
{
    /// <summary>
    /// Simple fade-in effect that animates the image alpha.
    /// </summary>
    [CreateAssetMenu(menuName = "Cutscenes/Effects/Fade", fileName = "FadeCutsceneEffect")]
    public class FadeCutsceneEffect : CutsceneEffect
    {
        public enum FadeDirection
        {
            FadeIn,
            FadeOut,
        }

        [SerializeField, Min(0f)]
        private float duration = 0.75f;

        [SerializeField]
        private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [SerializeField]
        private bool fadeImageColor = true;

        [SerializeField]
        private FadeDirection direction = FadeDirection.FadeIn;

        public override async UniTask Play(CutsceneEffectContext context, CancellationToken token)
        {
            if (context.CanvasGroup == null && context.Image == null)
            {
                return;
            }

            float elapsed = 0f;
            float startAlpha = direction == FadeDirection.FadeIn ? 0f : 1f;
            float targetAlpha = direction == FadeDirection.FadeIn ? 1f : 0f;

            if (context.CanvasGroup != null)
            {
                context.CanvasGroup.alpha = startAlpha;
            }

            Color originalColor = Color.white;
            if (fadeImageColor && context.Image != null)
            {
                originalColor = context.Image.color;
                context.Image.color = new Color(originalColor.r, originalColor.g, originalColor.b, startAlpha);
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                float evaluated = curve.Evaluate(t);
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, evaluated);

                if (context.CanvasGroup != null)
                {
                    context.CanvasGroup.alpha = alpha;
                }

                if (fadeImageColor && context.Image != null)
                {
                    context.Image.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                }

                await UniTask.Yield(token); 
            }

            if (context.CanvasGroup != null)
            {
                context.CanvasGroup.alpha = 1f;
                context.CanvasGroup.alpha = targetAlpha;
            }

            if (fadeImageColor && context.Image != null)
            {
                context.Image.color = new Color(originalColor.r, originalColor.g, originalColor.b, targetAlpha);
            }
        }
    }
}
