
using System;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;
public class VFX_Control : MonoBehaviour
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
    [SerializeField] private Light2D panLights;
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

    void Update()
    {
        
    }
    
    void ScreenVFXSettings()
    {
        screenVFXvoronoiPower = screenVFXmat.GetFloat("_VoronoiPower");
        screenVFXvoronoiSpeed = screenVFXmat.GetFloat("_VoronoiSpeed");
        screenVFXviggnetteePower = screenVFXmat.GetFloat("_VignettePower"); 
        screenVFXvignetteIntensity = screenVFXmat.GetFloat("_VignetteIntensity");
    }

    public void StartPanFlickering(bool flickering)
    {
        
        if (flickering == true)
        {
            if (!_flashTween.isAlive)
                _flashTween = Tween.Custom(minIntensity, maxIntensity, flickerDuration, 
                    onValueChange: intensityField => panLights.intensity = intensityField,cycles: -1, cycleMode: CycleMode.Yoyo);
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
        ScreenRandererFeature.SetActive(On);
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

    public void WaitingCombo()
    {
        StartPanFlickering(true);
        OnScreenRendererFeature(false);
        ChangeBGColor(false);
        globlaLight.intensity = 1f;
        truckLights.intensity = 0.2f;
        shopLights.intensity = 0.2f;
    }

    public void Comboing()
    {
        StartPanFlickering(true);
        ChangeBGColor(false);
        globlaLight.intensity = 0.75f;
    }

    public void Failing()
    {
        ChangeBGColor(true);
    }

}
