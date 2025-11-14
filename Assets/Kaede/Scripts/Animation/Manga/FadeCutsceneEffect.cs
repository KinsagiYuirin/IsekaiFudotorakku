using System.Collections;
using UnityEngine;

namespace Kaede.Scripts.Animation.Manga
{
    /// <summary>
    /// Simple fade-in effect that animates the image alpha.
    /// </summary>
    [CreateAssetMenu(menuName = "Cutscenes/Effects/Fade", fileName = "FadeCutsceneEffect")]
    public class FadeCutsceneEffect : CutsceneEffect
    {
        [SerializeField, Min(0f)]
        private float duration = 0.75f;

        [SerializeField]
        private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [SerializeField]
        private bool fadeImageColor = true;

        public override IEnumerator Play(CutsceneEffectContext context)
        {
            if (context.CanvasGroup == null && context.Image == null)
            {
                yield break;
            }

            float elapsed = 0f;
            float startAlpha = 0f;

            if (context.CanvasGroup != null)
            {
                context.CanvasGroup.alpha = startAlpha;
            }

            Color originalColor = Color.white;
            if (fadeImageColor && context.Image != null)
            {
                originalColor = context.Image.color;
                context.Image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                float evaluated = curve.Evaluate(t);

                if (context.CanvasGroup != null)
                {
                    context.CanvasGroup.alpha = evaluated;
                }

                if (fadeImageColor && context.Image != null)
                {
                    context.Image.color = new Color(originalColor.r, originalColor.g, originalColor.b, evaluated);
                }

                yield return null;
            }

            if (context.CanvasGroup != null)
            {
                context.CanvasGroup.alpha = 1f;
            }

            if (fadeImageColor && context.Image != null)
            {
                context.Image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
            }
        }
    }
}
