using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CarStats
{
    public string carName;       // Name of the car
    public float speed;          // Speed stat
    public float handling;       // Handling stat
    public float acceleration;   // Acceleration stat
    public float drift;           // Drift stat
    public float braking;        // Braking stat
}

public class GarageManager : MonoBehaviour
{
    [Header("Car Settings")]
    public List<GameObject> carPrefabs; // All car models (including different colors)
    private int currentCarIndex = 0; // Tracks the currently active car

    [Header("Car Stats")]
    public List<CarStats> carStats; // List of stats for each car

    [Header("UI Elements")]
    public TextMeshProUGUI carNameText; // Displays the car name
    public List<Slider> statSliders; // Sliders for stats (Speed, Handling, Acceleration, Grip, Braking)

    private void Start()
    {
        InitializeCar();
    }

    private void InitializeCar()
    {
        // Deactivate all cars
        foreach (GameObject car in carPrefabs)
        {
            car.SetActive(false);
        }

        // Activate the selected car
        carPrefabs[currentCarIndex].SetActive(true);

        // Update UI
        UpdateUI();
    }

    public void SwitchCar(int direction)
    {
        // Update the index
        currentCarIndex = (currentCarIndex + direction%12 + carPrefabs.Count) % carPrefabs.Count;

        // Activate the new car
        InitializeCar();
    }
    public void SwitchCarColor(int colorIndex)
    {
        int startIndex = 0;
        if (currentCarIndex > 3 && currentCarIndex < 8)
        {
            startIndex = 4;
        }
        else if (currentCarIndex >= 8)
        {
            startIndex = 8;
        }

        currentCarIndex = startIndex + colorIndex;

        InitializeCar();
    }

    private void UpdateUI()
    {
        // Update the car name
        carNameText.text = carStats[currentCarIndex].carName;

        // Update the stat sliders
        CarStats stats = carStats[currentCarIndex];
        statSliders[0].value = stats.speed;
        statSliders[1].value = stats.handling;
        statSliders[2].value = stats.acceleration;
        statSliders[3].value = stats.drift;
        statSliders[4].value = stats.braking;
    }

    public void ConfirmSelection()
    {
        Debug.Log($"Selected Car: {carStats[currentCarIndex].carName}");
        // Proceed to the next scene or gameplay
    }
}