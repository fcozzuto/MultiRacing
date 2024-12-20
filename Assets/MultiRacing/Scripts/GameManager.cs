    using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton instance

    [Header("Car Settings")]
    public List<GameObject> carPrefabs; // List of all car prefabs
    public int selectedCarIndex = -1; // Index of the player's chosen car (-1 for Quick Start)
    public int humanPlayerCount = 1; // Number of human players (future multiplayer support)

    [Header("Scene Settings")]
    public List<Transform> spawnPoints; // Spawn points for cars
    public Transform[] waypoints; // Waypoints for AI cars
    public GameObject guiCanvas; // GUI Canvas (for ranking, lap time, etc.)
    public TMP_Text countdownText; // Text element for the countdown (centered in GUI Canvas)
    public Camera mainCamera; // Main Camera in the Racing scene

    [Header("Race Settings")]
    public List<GameObject> allCars; // List of spawned cars
    public bool raceStarted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to the sceneLoaded event
        }
        else
        {
            Destroy(gameObject); // Enforce singleton
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe from the event
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (selectedCarIndex == -1)
        {
            HandleRacingSceneLoad();
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Racing")
        {
            HandleRacingSceneLoad();
        }
    }

    private void HandleRacingSceneLoad()
    {
        if (selectedCarIndex == -1)
        {
            // If no car has been selected yet, show the MainMenuCanvas
            guiCanvas.SetActive(false);
        }
        else
        {
            // If returning from the Garage scene, set up the race directly
            guiCanvas.SetActive(true);
            ReassignSpawnPoints();
            ReassignWaypoints();
            SetupRaceScene();
        }
    }

    private void ReassignSpawnPoints()
    {
        // Find all spawn points in the Racing scene by tag
        spawnPoints = new List<Transform>();
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
        foreach (GameObject spawnPointObject in spawnPointObjects)
        {
            spawnPoints.Add(spawnPointObject.transform);
        }
    }

    private void ReassignWaypoints()
    {
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("Waypoint");
        waypoints = new Transform[waypointObjects.Length];
        for (int i = 0; i < waypointObjects.Length; i++)
        {
            waypoints[i] = waypointObjects[i].transform;
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuickStart()
    {
        // Select a random car for the player
        selectedCarIndex = Random.Range(0, carPrefabs.Count);

        // Hide the Main Menu and set up the race
        guiCanvas.SetActive(true);
        SetupRaceScene();
    }

    public void SetupRaceScene()
    {
        if (mainCamera) mainCamera.enabled = false;

        // Spawn all cars
        SpawnCars();

        // Start the countdown
        StartCoroutine(StartCountdown());
    }

    private void SpawnCars()
    {
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);
        allCars = new List<GameObject>();

        // Spawn human players
        for (int i = 0; i < humanPlayerCount; i++)
        {
            int spawnIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[spawnIndex];
            availableSpawnPoints.RemoveAt(spawnIndex);

            // Determine the car to spawn
            GameObject carPrefab = (i == 0 && selectedCarIndex >= 0)
                ? carPrefabs[selectedCarIndex]
                : GetRandomCarPrefab();

            GameObject carInstance = Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);
            carInstance.SetActive(true); // Ensure the car prefab is active
            allCars.Add(carInstance);

            // Set up player-specific components
            SetupPlayerCar(carInstance);
        }

        // Spawn AI cars
        foreach (Transform spawnPoint in availableSpawnPoints)
        {
            GameObject carPrefab = GetRandomCarPrefab();
            GameObject carInstance = Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);
            carInstance.SetActive(true); // Ensure the car prefab is active
            allCars.Add(carInstance);

            // Set up AI-specific components
            SetupAICar(carInstance);
        }
    }

    private GameObject GetRandomCarPrefab()
    {
        return carPrefabs[Random.Range(0, carPrefabs.Count)];
    }

    private void SetupPlayerCar(GameObject car)
    {
        car.transform.Find("PlayerCamera").gameObject.SetActive(true);
        car.transform.Find("PlayerVirtualCamera").gameObject.SetActive(true);

        var prometeoController = car.GetComponent<PrometeoCarController>();
        if (prometeoController) prometeoController.enabled = true;
    }

    private void SetupAICar(GameObject car)
    {
        car.transform.Find("PlayerCamera").gameObject.SetActive(false);
        car.transform.Find("PlayerVirtualCamera").gameObject.SetActive(false);

        var prometeoController = car.GetComponent<PrometeoCarController>();
        if (prometeoController) prometeoController.enabled = false;

    }

    private IEnumerator StartCountdown()
    {
        string[] countdownTexts = { "3", "2", "1", "GO!" };

        // Freeze all cars (both player and AI)
        foreach (GameObject car in allCars)
        {
            var prometeoController = car.GetComponent<PrometeoCarController>();
            if (prometeoController) prometeoController.canMove = false;
        }

        for (int i = 0; i < countdownTexts.Length; i++)
        {
            countdownText.text = countdownTexts[i];
            yield return new WaitForSeconds(1f);
        }

        // Unfreeze all cars
        foreach (GameObject car in allCars)
        {
            var prometeoController = car.GetComponent<PrometeoCarController>();
            if (prometeoController) prometeoController.canMove = true;

        }

        countdownText.text = ""; // Clear countdown text after "GO!"
        raceStarted = true;
    }
}