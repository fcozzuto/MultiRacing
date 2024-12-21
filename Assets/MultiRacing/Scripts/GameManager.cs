using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton instance

    [Header("Car Settings")]
    public List<GameObject> carPrefabs; // List of all car prefabs
    public int selectedCarIndex = -1; // Index of the player's chosen car (-1 for Quick Start)
    public int humanPlayerCount = 1; // Number of human players (future multiplayer support)
    private List<GameObject> playerCars = new List<GameObject>();
    private List<GameObject> AICars = new List<GameObject>();

    [Header("Scene Settings")]
    public List<Transform> spawnPoints; // Spawn points for cars
    public Transform[] waypoints; // Waypoints for AI cars
    public GameObject mainMenuCanvas; // Main Menu Canvas in the Racing scene
    public GameObject pauseGameCanvas; // Pause Menu Canvas in the Racing scene
    public GameObject guiCanvas; // GUI Canvas (for ranking, lap time, etc.)
    public TMP_Text countdownText; // Text element for the countdown (centered in GUI Canvas)
    public TMP_Text lap1RecordText;
    public TMP_Text lap2RecordText;
    public TMP_Text lapCountText;
    public Camera mainCamera; // Main Camera in the Racing scene

    [Header("Race Settings")]
    public List<GameObject> allCars; // List of spawned cars
    public bool raceStarted = false;
    public bool raceFinished = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"GameManager created. Instance ID: {GetInstanceID()}");
        }
        else if (Instance != this)
        {
            Debug.Log($"Duplicate GameManager detected. Destroying duplicate with ID: {GetInstanceID()}");
            Destroy(gameObject);
        }
    }


    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe from the event
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Racing")
        {
            Debug.Log($"Returning to Racing: SpawnPoints={StaticData.SpawnPoints.Count}, Waypoints={StaticData.Waypoints?.Length}");

            if (selectedCarIndex != -1)
            {
                Debug.Log("Car selected, setting up race scene.");
                HandleRacingSceneLoad();
            }
            else
            {
                Debug.Log("No car selected, showing Main Menu.");
            }
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Racing")
        {
            HandleRacingSceneLoad();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (raceFinished)
            {
                QuitGame();
            }
            else if (raceStarted && !raceFinished)
            {
                PauseGame();
                OpenPauseMenu();
            }
            else
            {
                CloseMenu();
                ResumeGame();
            }
        }
    }

    private void OpenPauseMenu()
    {
        OpenMenu();
        mainMenuCanvas.SetActive(false);
        pauseGameCanvas.SetActive(true);

    }

    public void ResumeRace()
    {
        ClosePauseMenu();
        ResumeGame();
    }

    private void ClosePauseMenu()
    {
        mainCamera.enabled = false;
        pauseGameCanvas.SetActive(false);
        guiCanvas.SetActive(true);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void CloseMenu()
    {
        mainCamera.enabled = false;
        mainMenuCanvas.SetActive(false);
        guiCanvas.SetActive(true);
    }

    private void OpenMenu()
    {
        mainCamera.enabled = true;
        mainMenuCanvas.SetActive(true);
        guiCanvas.SetActive(false);
    }

    private void HandleRacingSceneLoad()
    {
        if (mainMenuCanvas == null)
        {
            mainMenuCanvas = GameObject.Find("MainMenuCanvas");
        }
        if (guiCanvas == null)
        {
            guiCanvas = GameObject.Find("GUICanvas");
            if (countdownText == null)
            {
                foreach (var textObj in guiCanvas.GetComponentsInChildren<TextMeshProUGUI>(true).Where(textObj => textObj.name == "GuideText"))
                {
                    countdownText = textObj;
                }
            }
        }
        if(pauseGameCanvas == null)
        {
            pauseGameCanvas = GameObject.Find("PauseMenuCanvas");
            pauseGameCanvas.SetActive(false);
        }
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (selectedCarIndex == -1)
        {
            mainMenuCanvas.SetActive(true);
            guiCanvas.SetActive(false);
            Debug.Log("Showing Main Menu Canvas.");
        }
        else
        {
            mainMenuCanvas.SetActive(false);
            guiCanvas.SetActive(true);
            ReassignSpawnPoints();
            ReassignWaypoints();
            Debug.Log("Setting up the race scene.");
            SetupRaceScene();
        }
    }

    private void ReassignSpawnPoints()
    {
        if (StaticData.SpawnPoints != null && StaticData.SpawnPoints.Count > 0)
        {
            spawnPoints = new List<Transform>(StaticData.SpawnPoints);
            Debug.Log("Restored spawn points from StaticData.");
            return;
        }

        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
        spawnPoints = spawnPointObjects
            .OrderBy(spawnPoint => spawnPoint.name)
            .Select(spawnPoint => spawnPoint.transform)
            .ToList();

        StaticData.SpawnPoints = spawnPoints; // Store persistently
        Debug.Log($"Found and stored {spawnPoints.Count} spawn points in order: {string.Join(", ", spawnPoints.Select(p => p.name))}");
    }

    private void ReassignWaypoints()
    {
        if (StaticData.Waypoints != null && StaticData.Waypoints.Length > 0)
        {
            waypoints = StaticData.Waypoints;
            Debug.Log("Restored waypoints from StaticData.");
            return;
        }

        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("Waypoint");
        waypoints = waypointObjects
            .OrderBy(waypoint => int.Parse(System.Text.RegularExpressions.Regex.Match(waypoint.name, @"\d+").Value))
            .Select(waypoint => waypoint.transform)
            .ToArray();

        StaticData.Waypoints = waypoints; // Store persistently
        Debug.Log($"Found and stored {waypoints.Length} waypoints in order: {string.Join(", ", waypoints.Select(w => w.name))}");
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
        mainMenuCanvas.SetActive(false);
        guiCanvas.SetActive(true);
        SetupRaceScene();
    }

    public void SetupRaceScene()
    {
        if (mainCamera)
        {
            var audioListener = mainCamera.GetComponentInChildren<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = false;
            }
            mainCamera.enabled = false;
        }

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

            GameObject carPrefab = (i == 0 && selectedCarIndex >= 0)
                ? carPrefabs[selectedCarIndex]
                : GetRandomCarPrefab();

            GameObject carInstance = Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);
            carInstance.SetActive(true); // Ensure the car prefab is active
            allCars.Add(carInstance);

            SetupPlayerCar(carInstance);
            Debug.Log($"Spawned player car: {carPrefab.name} at {spawnPoint.position}");
        }

        // Spawn AI cars
        foreach (Transform spawnPoint in availableSpawnPoints)
        {
            GameObject carPrefab = GetRandomCarPrefab();
            GameObject carInstance = Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);
            carInstance.SetActive(true); // Ensure the car prefab is active
            allCars.Add(carInstance);

            SetupAICar(carInstance);
            Debug.Log($"Spawned AI car: {carPrefab.name} at {spawnPoint.position}");
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

        var audioListener = car.GetComponentInChildren<AudioListener>();
        if (audioListener != null)
        {
            audioListener.enabled = true;
        }

        var prometeoController = car.GetComponent<PrometeoCarController>();
        if (prometeoController) prometeoController.enabled = true;
        prometeoController.useUI = true;
        prometeoController.carSpeedText = guiCanvas.transform.Find("Speed Text").GetComponent<Text>();

        var aiPrometeoController = car.GetComponent<AIPrometeoCarController>();
        if (aiPrometeoController) Destroy(aiPrometeoController);
        car.tag = "Player";
        playerCars.Add(car);
    }

    private void SetupAICar(GameObject car)
    {
        car.transform.Find("PlayerCamera").gameObject.SetActive(false);
        car.transform.Find("PlayerVirtualCamera").gameObject.SetActive(false);

        var audioListener = car.GetComponentInChildren<AudioListener>();
        if (audioListener != null)
        {
            audioListener.enabled = false;
        }

        var prometeoController = car.GetComponent<PrometeoCarController>();
        var aiController = car.GetComponent<AIPrometeoCarController>();

        if (aiController && prometeoController)
        {
            // Transfer core stats
            aiController.maxSpeed = prometeoController.maxSpeed;
            aiController.maxReverseSpeed = prometeoController.maxReverseSpeed;
            aiController.accelerationMultiplier = prometeoController.accelerationMultiplier;
            aiController.brakeForce = prometeoController.brakeForce;
            aiController.maxSteeringAngle = prometeoController.maxSteeringAngle;
            aiController.steeringSpeed = prometeoController.steeringSpeed;

            aiController.frontLeftMesh = prometeoController.frontLeftMesh;
            aiController.frontLeftCollider = prometeoController.frontLeftCollider;
            aiController.frontRightMesh = prometeoController.frontRightMesh;
            aiController.frontRightCollider = prometeoController.frontRightCollider;
            aiController.rearLeftMesh = prometeoController.rearLeftMesh;
            aiController.rearLeftCollider = prometeoController.rearLeftCollider;
            aiController.rearRightMesh = prometeoController.rearRightMesh;
            aiController.rearRightCollider = prometeoController.rearRightCollider;
            aiController.engineSound = prometeoController.carEngineSound;
            aiController.tireSound = prometeoController.tireScreechSound;

            aiController.currentWaypoint = waypoints[0];
        }

        if (prometeoController)
        {
            prometeoController.enabled = false; // Disable player control for AI cars
            Destroy(prometeoController);
        }
        car.tag = "AI";
        AICars.Add(car);
    }

    private IEnumerator StartCountdown()
    {
        string[] countdownTexts = { "3", "2", "1", "GO!" };

        foreach (GameObject playerCar in playerCars)
        {
            var prometeoController = playerCar.GetComponent<PrometeoCarController>();
            prometeoController.enabled = false;
        }

        foreach (GameObject AICar in AICars)
        {
            var aIPrometeoCarController = AICar.GetComponent<AIPrometeoCarController>();
            aIPrometeoCarController.enabled = false;
        }

        for (int i = 0; i < countdownTexts.Length; i++)
        {
            countdownText.text = countdownTexts[i];
            yield return new WaitForSeconds(1f);
        }

        // Unfreeze all cars
        foreach (GameObject playerCar in playerCars)
        {
            var prometeoController = playerCar.GetComponent<PrometeoCarController>();
            prometeoController.enabled = true;
        }

        foreach (GameObject AICar in AICars)
        {
            var aIPrometeoCarController = AICar.GetComponent<AIPrometeoCarController>();
            aIPrometeoCarController.enabled = true;
        }

        countdownText.text = ""; // Clear countdown text after "GO!"
        raceStarted = true;
    }

    internal void LapStarted(int lapCount)
    {
        if (!lapCountText)
        {
            if (guiCanvas == null)
            {
                Debug.LogError("guiCanvas is null. Ensure it is assigned correctly.");
                return;
            }

            var textObjects = guiCanvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            Debug.Log($"Found {textObjects.Length} TMP_Text objects in guiCanvas.");

            foreach (var textObj in textObjects.Where(textObj => textObj.name == "LapCountText"))
            {
                lapCountText = textObj;
            }
        }
        if (lapCountText)
        {
            lapCountText.text = $"LAP {lapCount}/2";
        }
        else
        {
            Debug.LogWarning("LapsCountText not found! Ensure the object is named 'LapCountText' and is a child of guiCanvas.");
        }
    }

    internal void LapFinished(float lapTime)
    {
        if (lap1RecordText == null)
        {
            lap1RecordText = GameObject.Find("Lap1RecordText").GetComponent<TextMeshProUGUI>();
            lap1RecordText.text = "LAP 1 RECORD : " + lapTime.ToString("F2");
        }
        else if (lap2RecordText == null)
        {
            lap2RecordText = GameObject.Find("Lap2RecordText").GetComponent<TextMeshProUGUI>();
            lap2RecordText.text = "LAP 2 RECORD : " + lapTime.ToString("F2");
        }
    }

    internal void FinishRace()
    {
        PauseGame();
        countdownText.text = "Congratulations! You Finished the race!\n\nThank you for playing our game!\n\nPress ESC to Quit";
        raceFinished = true;
    }

    private void PauseGame()
    {
        Time.timeScale = 0f; // Stop the game
        UnlockCursor();      // Unlock the mouse cursor
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f; // Resume the game
        LockCursor();        // Lock the mouse cursor if needed
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true;                    // Make the cursor visible
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
        Cursor.visible = false;                    // Make the cursor invisible
    }
}