using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Kaede.Art.TA_Works.Scripts
{
    public class VFXControl : MonoBehaviour
    {
        [Title("Particle/VFX")] 
        [SerializeField] private Material screenVFXmat;
    
        [Title("ScreenVFX Settings")]
        [SerializeField] private float screenVFXvoronoiPower;
        [SerializeField] private float screenVFXvoronoiSpeed;
        [SerializeField] private float screenVFXviggnetteePower;
        [SerializeField] private float screenVFXvignetteIntensity;
    
        [Title("Light")]
        [SerializeField] private Light2D globlaLight;
        [SerializeField] private Light2D panLightsNormal;
        [SerializeField] private Light2D panLightsFailed;
        [SerializeField] private Light2D truckLights;
        [SerializeField] private Light2D shopLights;
    
        [Title("Pan Flickering Setting")]
        [SerializeField] private float minIntensity = 0.5f;
        [SerializeField] private float maxIntensity = 1.5f;
        [SerializeField] private float flickerDuration = 0.25f;
        [SerializeField] private float intensityField;
    
        [Title("Sprite Setting")]
        [SerializeField] private SpriteRenderer objectSprite;
    
        [Title("Global Light Faild Setting")]
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color FailedColor;
    
        [Title("Screen Renderer Feature")]
        [SerializeField] private ScriptableRendererFeature ScreenRandererFeature;

        private Tween _flashTween;
    
        void Start()
        {
            WaitingCombo();
        }
    
        void ScreenVFXSettings()
        {
            screenVFXvoronoiPower = screenVFXmat.GetFloat("_VoronoiPower");
            screenVFXvoronoiSpeed = screenVFXmat.GetFloat("_VoronoiSpeed");
            screenVFXviggnetteePower = screenVFXmat.GetFloat("_VignettePower"); 
            screenVFXvignetteIntensity = screenVFXmat.GetFloat("_VignetteIntensity");
        }

        public void StartPanNormalFlickering(bool flickering)
        {
        
            if (flickering == true)
            {
                if (!_flashTween.isAlive)
                    _flashTween = Tween.Custom(minIntensity, maxIntensity, flickerDuration, 
                        onValueChange: intensityField => panLightsNormal.intensity = intensityField,cycles: -1, cycleMode: CycleMode.Yoyo);
            }
            else
            {
                StopPanFlickering();
            }
        
        }
    
        public void StartPanFailedFlickering(bool flickering)
        {
        
            if (flickering == true)
            {
                if (!_flashTween.isAlive)
                    _flashTween = Tween.Custom(minIntensity, maxIntensity, flickerDuration, 
                        onValueChange: intensityField => panLightsFailed.intensity = intensityField,cycles: -1, cycleMode: CycleMode.Yoyo);
            }
            else
            {
                StopPanFlickering();
            }
        
        }

        public void StopPanFlickering()
        {
            _flashTween.Stop();
        }
    
        public void SetColor(Color color)
        { 
            objectSprite.color = color;
        }
    
        public void OnScreenRendererFeature(bool On)
        {
            if (On == true)
            {
                ScreenRandererFeature.SetActive(true);
            }

            if (On == false)
            {
                ScreenRandererFeature.SetActive(false);
            }
        
        }

        public void ChangeBGColor(bool failed)
        {
            if (failed == true)
            {
                globlaLight.color = FailedColor;
            }
            else
            {
                globlaLight.color = defaultColor;
            }
        
        }

    public void OnScreenRendererFeature(bool active)
    { 
        ScreenRandererFeature.SetActive(active);
    }
        /// <summary>
        /// Default VFX
        /// </summary>
        public void WaitingCombo()
        {
            StartPanNormalFlickering(true);
            OnScreenRendererFeature(false);
            ChangeBGColor(false);
            globlaLight.intensity = 1f;
            truckLights.intensity = 0.2f;
            shopLights.intensity = 0.2f;
            panLightsNormal.gameObject.SetActive(true);
            panLightsFailed.gameObject.SetActive(false);
        }

        /// <summary>
        /// เอาไว้ตอนกำลังทำคอมโบ
        /// </summary>
        public void Comboing()
        {
            StartPanNormalFlickering(true);
            OnScreenRendererFeature(false);
            ChangeBGColor(false);
            globlaLight.intensity = 0.75f;
            panLightsNormal.gameObject.SetActive(true);
            panLightsFailed.gameObject.SetActive(false);
        }

        public void Fevering()
        {
            OnScreenRendererFeature(true);
            panLightsNormal.gameObject.SetActive(true);
            panLightsFailed.gameObject.SetActive(false);
        }
    
        public void Failing()
        {
            StopPanFlickering();
            OnScreenRendererFeature(false);
            ChangeBGColor(true);
            StartPanFailedFlickering(true);
            panLightsNormal.gameObject.SetActive(false);
            panLightsFailed.gameObject.SetActive(true);
        }

    }
}
