using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class VFX_Control : MonoBehaviour
{

    [Header("Particle/VFX")] 
    public Material screenVFXmat;
    
    [Header("ScreenVFX Settings")]
    public float screenVFXvoronoiPower;
    public float screenVFXvoronoiSpeed;
    public float screenVFXviggnetteePower;
    public float screenVFXvignetteIntensity;
    
    [Header("Light")]
    public Light2D globlaLight;
    public Light2D panLights;
    public Light2D truckLights;
    public Light2D shopLights;
    
    [Header("Pan Light Setting")]
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float flickerSpeed = 0.1f;
    

    [Header("Screen Renderer Feature")]
    public ScriptableRendererFeature ScreenRandererFeature;
    
    private float flickerTarget;
    private float velocity;

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
    
    void PanLightFlicker()
    {
        float sinValue = Mathf.Sin(Time.time * flickerSpeed);
        float normalizedValue = (sinValue + 1f) / 2f;
        panLights.intensity = Mathf.Lerp(minIntensity, maxIntensity, normalizedValue);
    }
    
    public void OnScreenRendererFeature(bool On)
    {
        ScreenRandererFeature.SetActive(On);
    }

    void WaitingCombo()
    {
        PanLightFlicker();
        OnScreenRendererFeature(false);
        globlaLight.intensity = 1f;
        truckLights.intensity = 0.2f;
        shopLights.intensity = 0.2f;
    }

    void Comboing()
    {
        PanLightFlicker();
        globlaLight.intensity = 0.75f;
    }
    
}
