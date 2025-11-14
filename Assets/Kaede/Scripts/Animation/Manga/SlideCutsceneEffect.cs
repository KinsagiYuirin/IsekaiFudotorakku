using System.Collections;
using UnityEngine;

namespace Kaede.Scripts.Animation.Manga
{
    /// <summary>
    /// Slides the image from an offset position into place.
    /// </summary>
    [CreateAssetMenu(menuName = "Cutscenes/Effects/Slide", fileName = "SlideCutsceneEffect")]
    public class SlideCutsceneEffect : CutsceneEffect
    {
        [SerializeField]
        private Vector2 startOffset = new Vector2(-600f, 0f);

        [SerializeField, Min(0f)]
        private float duration = 0.6f;

        [SerializeField]
        private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public override IEnumerator Play(CutsceneEffectContext context)
        {
            if (context.RectTransform == null)
            {
                yield break;
            }

            var rectTransform = context.RectTransform;
            Vector2 originalPosition = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = originalPosition + startOffset;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                float evaluated = curve.Evaluate(t);
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(originalPosition + startOffset, originalPosition, evaluated);
                yield return null;
            }

            rectTransform.anchoredPosition = originalPosition;
        }
    }
}