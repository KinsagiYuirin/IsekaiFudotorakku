using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.Animation.Manga.SO
{
    [CreateAssetMenu(fileName = "FillImageEffect", menuName = "Cutscenes/Effects/Fill")]
    public class FillImageEffect : CutsceneEffect
    {
        [SerializeField, Min(0f)]
        private float duration = 0.8f;

        [SerializeField]
        private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [SerializeField, Range(0f, 1f)]
        private float startFillAmount = 0f;

        [SerializeField, Range(0f, 1f)]
        private float endFillAmount = 1f;

        [SerializeField]
        private bool forceFilledImageType = true;

        [SerializeField]
        private Image.FillMethod fillMethod = Image.FillMethod.Horizontal;

        [SerializeField]
        private int fillOrigin = 0;

        [SerializeField]
        private bool fillClockwise = true;

        [SerializeField]
        private bool restoreOriginalSettings = false;

        public override async UniTask Play(CutsceneEffectContext context, CancellationToken token)
        {
            if (context.Image == null)
            {
                return;
            }

            var image = context.Image;
            Image.Type originalType = image.type;
            Image.FillMethod originalMethod = image.fillMethod;
            int originalOrigin = image.fillOrigin;
            bool originalClockwise = image.fillClockwise;
            float originalFillAmount = image.fillAmount;

            if (forceFilledImageType)
            {
                image.type = Image.Type.Filled;
            }

            image.fillMethod = fillMethod;
            image.fillClockwise = fillClockwise;
            image.fillOrigin = Mathf.Clamp(fillOrigin, 0, GetMaxOrigin(fillMethod));

            float clampedStart = Mathf.Clamp01(startFillAmount);
            float clampedEnd = Mathf.Clamp01(endFillAmount);
            image.fillAmount = clampedStart;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                float evaluated = curve.Evaluate(t);
                image.fillAmount = Mathf.LerpUnclamped(clampedStart, clampedEnd, evaluated);
                await UniTask.Yield(token); 
            }

            image.fillAmount = clampedEnd;

            if (restoreOriginalSettings)
            {
                image.type = originalType;
                image.fillMethod = originalMethod;
                image.fillOrigin = originalOrigin;
                image.fillClockwise = originalClockwise;
                image.fillAmount = originalFillAmount;
            }
        }

        private static int GetMaxOrigin(Image.FillMethod method)
        {
            switch (method)
            {
                case Image.FillMethod.Horizontal:
                case Image.FillMethod.Vertical:
                    return 1;
                case Image.FillMethod.Radial90:
                case Image.FillMethod.Radial180:
                    return 3;
                case Image.FillMethod.Radial360:
                    return 2;
                default:
                    return 0;
            } 
        }    
    }
}
