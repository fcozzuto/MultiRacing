using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton instance

    [Header("Player Settings")]
    public int humanPlayerCount = 1; // Number of human players
    public GameObject selectedCarPrefab; // Player's chosen car prefab
    public List<GameObject> availableCars; // List of all available car prefabs
    public List<Transform> spawnPoints; // Spawn points in the racing scene

    [Header("Race Settings")]
    public List<string> rankings; // Rankings of all players (human and AI)
    public bool musicEnabled = true; // Toggle for music

    private void Awake()
    {
        // Ensure the GameManager persists across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Enforce the singleton pattern
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SetSelectedCar(GameObject carPrefab)
    {
        selectedCarPrefab = carPrefab;
    }

    public GameObject GetRandomAvailableCar()
    {
        if (availableCars.Count > 0)
        {
            int randomIndex = Random.Range(0, availableCars.Count);
            return availableCars[randomIndex];
        }
        return null;
    }

    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Count);
            return spawnPoints[randomIndex];
        }
        return null;
    }

    public void AddToRankings(string playerName)
    {
        rankings.Add(playerName);
    }

    public void ClearRankings()
    {
        rankings.Clear();
    }
}