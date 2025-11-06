using PrimeTween;
using UnityEngine;

namespace Kaede.Scripts.UI
{
    public class FadeFromRight : MonoBehaviour
    {
        [SerializeField] private Material fadeMat;
        
        private Tween _tween;
        
        public void StartFade(float duration)
        {
            _tween = Tween.Custom(0f, 1f, duration, v => 
            {
                fadeMat.SetFloat("_Fade", v); 
            });
        }
        
        public void FadeOut(float duration)
        {
            // เฟดกลับจาก 1 → 0
            _tween = Tween.Custom(1f, 0f, duration, v =>
            {
                fadeMat.SetFloat("_Fade", v);
            });
        }
    }
}
