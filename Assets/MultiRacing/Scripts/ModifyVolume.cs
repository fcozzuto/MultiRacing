using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Collections;

public class ModifyVolume : MonoBehaviour
{
    private Volume volume;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private Color redColor;
    private Color blackColor;
    private float initialVignetIntensity = 0.2f;
    private float initialChromaticAberrationIntensity = 0f;

    void Start()
    {
        redColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        blackColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        // Get the Volume component attached to the GameObject
        volume = GetComponent<Volume>();
        if (volume == null)
        {
            Debug.LogError("Volume component not found!");
            return;
        }

        // Try to get the Vignette override
        if (volume.profile.TryGet(out vignette))
        {
            Debug.Log("Vignette component found!");
        }
        else
        {
            Debug.LogError("Vignette override not found in the Volume profile!");
        }

        // Try to get the Chromatic Aberration override
        if (volume.profile.TryGet(out chromaticAberration))
        {
            Debug.Log("Chromatic Aberration component found!");
        }
        else
        {
            Debug.LogError("Chromatic Aberration override not found in the Volume profile!");
        }
    }

    public void SetVignetteIntensity(float intensity)
    {
        if (vignette != null)
        {
            vignette.intensity.value = intensity;
        }
    }

    public void SetVignetteColor(Color color)
    {
        if (vignette != null)
        {
            vignette.color.value = color;
        }
    }

    public void SetChromaticAberrationIntensity(float intensity)
    {
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = intensity;
        }
    }

    public IEnumerable FlashVignetteAndAberration()
    {
        FlashVignette();
        FlashChromaticAberration();

        yield return new WaitForSeconds(0.1f);

        RevertChromaticAberration();
        RevertVignette();
    }

    public void FlashVignette()
    {
        if (vignette == null) return;

        // Apply changes immediately
        SetVignetteColor(redColor);
        SetVignetteIntensity(0.35f);
    }

    public void RevertVignette()
    {
        // Revert to default state
        SetVignetteColor(blackColor);
        SetVignetteIntensity(initialVignetIntensity);
        Debug.Log($"Reverting Vignette to {initialVignetIntensity} and color to black");
    }

    public void FlashChromaticAberration()
    {
        if (chromaticAberration == null) return;

        // Apply changes immediately
        SetChromaticAberrationIntensity(1f);
    }

    public void RevertChromaticAberration()
    {
        SetChromaticAberrationIntensity(initialChromaticAberrationIntensity);
        Debug.Log($"Reverting Chromatic Aberration to {initialChromaticAberrationIntensity}");
    }
}