using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class VFX_Control : MonoBehaviour
{
    [Header("Pan Light")]
    public Light2D[] panLights;

    [Header("Particle/VFX")] 
    public Material screenVFXmat;
    
    [Header("ScreenVFX Settings")]
    public float screenVFXvoronoiPower;
    public float screenVFXvoronoiSpeed;
    public float screenVFXviggnetteePower;
    public float screenVFXvignetteIntensity;
    
    [Header("Pan Light Setting")]
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float flickerSpeed = 0.1f;

    private float flickerTarget;
    private float velocity;

    void Start()
    {
        if (panLights.Length > 0)
            flickerTarget = panLights[0].intensity;
    }

    void Update()
    {
        PanLightFlicker();
        ScreenVFXSettings();
    }
    void PanLightFlicker()
    {
        if (panLights.Length == 0) return;

        if (Random.value < flickerSpeed)
            flickerTarget = Random.Range(minIntensity, maxIntensity);

        foreach (var light in panLights)
        {
            if (light == null) continue;
            float vel = velocity;
            light.intensity = Mathf.SmoothDamp(light.intensity, flickerTarget, ref vel, 0.1f);
            velocity = vel;
        }
    }

    void ScreenVFXSettings()
    {
        screenVFXvoronoiPower = screenVFXmat.GetFloat("_VoronoiPower");
        screenVFXvoronoiSpeed = screenVFXmat.GetFloat("_VoronoiSpeed");
        screenVFXviggnetteePower = screenVFXmat.GetFloat("_VignettePower"); 
        screenVFXvignetteIntensity = screenVFXmat.GetFloat("_VignetteIntensity");
    }
}
