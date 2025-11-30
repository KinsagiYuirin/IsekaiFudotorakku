using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kaede.Scripts.Animation.Manga.SO
{
    [CreateAssetMenu(fileName = "ZoomEffect", menuName = "Cutscenes/Effects/Zoom")]
    public class ZoomEffect : CutsceneEffect
    {
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private Vector3 startScale = Vector3.one * 0.9f;
        [SerializeField] private Vector3 endScale = Vector3.one;
        [SerializeField] private bool restoreOriginalScale = true;
        
        public override async UniTask Play(CutsceneEffectContext contex, CancellationToken token)
        {
            if (contex.RectTransform == null)
            {
                return;
            }

            var rectTransform = contex.RectTransform;
            Vector3 originalScale = rectTransform.localScale;

            Vector3 scaledStart = Vector3.Scale(originalScale, startScale);
            Vector3 scaledEnd = Vector3.Scale(originalScale, endScale);
            rectTransform.localScale = scaledStart;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                float evaluated = curve.Evaluate(t);
                rectTransform.localScale = Vector3.LerpUnclamped(scaledStart, scaledEnd, evaluated);
                await UniTask.Yield(token);
            }

            rectTransform.localScale = restoreOriginalScale ? originalScale : scaledEnd;
        }
    }
}
