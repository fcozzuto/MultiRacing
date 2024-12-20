using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    public List<GameObject> carPrefabs; // Prefabs for all cars (including different colors)
    private int currentCarIndex = 0; // Tracks the currently selected car
    private GameObject currentCarInstance; // The instantiated car in the garage

    [Header("Spawn Settings")]
    public Transform carSpawnPoint; // Position and rotation for spawning the car

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
        // Destroy the previous car instance if it exists
        if (currentCarInstance != null)
        {
            currentCarInstance.SetActive(false);
            Destroy(currentCarInstance);
        }

        // Instantiate the selected car prefab at the spawn point
        currentCarInstance = Instantiate(
            carPrefabs[currentCarIndex],
            carSpawnPoint.position,
            carSpawnPoint.rotation
        );
        currentCarInstance.GetComponent<AIPrometeoCarController>().enabled = false;
        currentCarInstance.GetComponent<PrometeoCarController>().enabled = false;
        currentCarInstance.SetActive(true);

        // Update UI
        UpdateUI();
    }

    public void SwitchCar(int direction)
    {
        // Update the index
        currentCarIndex = (currentCarIndex + direction + carPrefabs.Count) % carPrefabs.Count;

        // Instantiate the new car
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
        GameObject selectedCar = carPrefabs[currentCarIndex];
        StaticData.ChosenCarIndex = currentCarIndex;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.selectedCarIndex = currentCarIndex;
            Debug.Log($"Selected Car: {selectedCar.name}");
        }
        else
        {
            Debug.LogError("GameManager instance is null!");
        }

        // Load Racing scene
        GameManager.Instance.LoadScene("Racing");
    }
}