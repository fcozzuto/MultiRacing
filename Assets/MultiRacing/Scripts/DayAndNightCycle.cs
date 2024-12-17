using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DayAndNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    [Range(0f, 24f)]
    public float currentTime;
    private float timeSpeed = 24f / 300f;

    [Header("CurrentTime")]
    public string currentTimeString;

    [Header("Sun Settings")]
    public Light sunLight;
    [Range(0f, 90f)]
    public float sunLatitude = 20f;
    [Range(-180f, 180f)]
    public float sunLongitude = -90f;
    public float sunIntensity = 1f;
    public AnimationCurve sunIntensityMultiplier;
    public AnimationCurve sunTemperatureCurve;

    public bool isDay = true;
    public bool sunActive = true;
    public bool moonActive = true;

    [Header("Moon Settings")]
    public Light moonLight;
    [Range(0f, 90f)]
    public float moonLatitude = 40f;
    [Range(-180f, 180f)]
    public float moonLongitude = 90f;
    public float moonIntensity = 1f;
    public AnimationCurve moonIntensityMultiplier;
    public AnimationCurve moonTemperatureCurve;

    [Header("Stars Settings")]
    public VolumeProfile volumeProfile;
    private PhysicallyBasedSky skySettings;
    public float starsIntensity = 1f;
    public AnimationCurve starsCurve;
    [Range(0f, 90f)]
    public float starsLatitude = 40f;
    [Range(-180f, 180f)]
    public float starsLongitude = 90f;

    // Start is called before the first frame update
    void Start()
    {
        UpdateTimeText();
        CheckShadowStatus();
        SkyStar();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime * timeSpeed;

        if (currentTime >= 24)
        {
            currentTime = 0;
        }

        UpdateTimeText();
        UpdateLight();
        CheckShadowStatus();
        SkyStar();
    }

    private void OnValidate()
    {
        UpdateLight();
        CheckShadowStatus();
        SkyStar();
    }

    private void SkyStar()
    {
        volumeProfile.TryGet < PhysicallyBasedSky>(out skySettings);
        skySettings.spaceEmissionMultiplier.value = starsCurve.Evaluate(currentTime / 24.0f) * starsIntensity;

        skySettings.spaceRotation.value = (Quaternion.Euler(90 - starsLatitude, starsLongitude, 0) * Quaternion.Euler(0, currentTime / 24.0f * 360.0f, 0)).eulerAngles;
    }

    private void CheckShadowStatus()
    {
        HDAdditionalLightData sunLightData = sunLight.GetComponent<HDAdditionalLightData>();
        HDAdditionalLightData moonLightData = moonLight.GetComponent<HDAdditionalLightData>();
        float currentSunRotation = currentTime;

        if(currentSunRotation >= 6f && currentSunRotation <= 18f)
        {
            sunLightData.EnableShadows(true);
            moonLightData.EnableShadows(false);
            isDay = true;
        }
        else
        {
            sunLightData.EnableShadows(false);
            moonLightData.EnableShadows(true);
            isDay = false;
        }

        if (currentSunRotation >= 5.7f && currentSunRotation <= 18.3f)
        {
            sunLight.gameObject.SetActive(true);
            sunActive = true;
        }
        else
        {
            sunLight.gameObject.SetActive(false);
            sunActive = false;
        }

        if (currentSunRotation >= 6.3f && currentSunRotation <= 17.7f)
        {
            moonLight.gameObject.SetActive(false);
            moonActive = false;
        }
        else
        {
            moonLight.gameObject.SetActive(true);
            moonActive= true;
        }
    }

    void UpdateLight()
    {
        // Calculate sun rotation based on currentTime
        float sunRotation = (currentTime / 24f) * 360f;
        sunLight.transform.localRotation = (Quaternion.Euler(sunLatitude - 90, sunLongitude, 0) * Quaternion.Euler(0, sunRotation, 0));
        moonLight.transform.localRotation = (Quaternion.Euler(90 - moonLatitude, moonLongitude, 0) * Quaternion.Euler(0, sunRotation, 0));

        float normalizedTime = currentTime / 24f;
        float sunIntensityCurve = sunIntensityMultiplier.Evaluate(normalizedTime);
        float moonIntensityCurve = moonIntensityMultiplier.Evaluate(normalizedTime);

        HDAdditionalLightData sunLightData = sunLight.GetComponent<HDAdditionalLightData>();
        HDAdditionalLightData moonLightData = moonLight.GetComponent<HDAdditionalLightData>();

        if(sunLightData)
        {
            sunLightData.intensity = sunIntensityCurve * sunIntensity;
        }
        
        if(moonLightData)
        {
            moonLightData.intensity = moonIntensityCurve * moonIntensity;
        }

        float sunTemperatureMultiplier = sunTemperatureCurve.Evaluate(normalizedTime);
        float moonTemperatureMultiplier = moonTemperatureCurve.Evaluate(normalizedTime);
        Light sunLightComponent = sunLight.GetComponent<Light>();
        Light moonLightComponent = moonLight.GetComponent<Light>();

        if (sunLightComponent != null)
        {
            sunLightComponent.colorTemperature = sunTemperatureMultiplier * 10000f;
        }

        if (moonLightComponent != null)
        {
            moonLightComponent.colorTemperature = moonTemperatureMultiplier * 10000f;
        }
    }

    void UpdateTimeText()
    {
        // Update the time string for display
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60);
        currentTimeString = $"{hours:00}:{minutes:00}";
    }
}