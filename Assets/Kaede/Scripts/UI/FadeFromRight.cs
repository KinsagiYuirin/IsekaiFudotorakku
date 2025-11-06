using PrimeTween;
using UnityEngine;

namespace Kaede.Scripts.UI
{
    public class FadeFromRight : MonoBehaviour
    {
        [SerializeField] private Material fadeMat;
        [SerializeField] private float durationIn = 1f;
        [SerializeField] private float durationOut = 1f;
        
        private Tween _tween;
        
        private void Start()
        {
            fadeMat.SetFloat("_Fade", 0f);
        }
        
        public void StartFade()
        {
            _tween = Tween.Custom(0f, 1f, durationIn, v => 
            {
                fadeMat.SetFloat("_Fade", v); 
            });
        }
        
        public void FadeOut()
        {
            // เฟดกลับจาก 1 → 0
            _tween = Tween.Custom(1f, 0f, durationOut, v =>
            {
                fadeMat.SetFloat("_Fade", v);
            });
        }
    }
}
