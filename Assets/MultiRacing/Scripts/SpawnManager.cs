using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();

    private List<Transform> usedSpawnPoints = new List<Transform>();

    void Start()
    {
        // Register the client connection callback
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        // Unregister the callback when the script is destroyed
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        Transform freeSpawnPoint = GetFreeSpawnPoint();

        if (freeSpawnPoint != null)
        {
            // Spawn the player at the chosen spawn point
            GameObject playerInstance = Instantiate(
                NetworkManager.Singleton.NetworkConfig.PlayerPrefab,
                freeSpawnPoint.position,
                freeSpawnPoint.rotation
            );

            usedSpawnPoints.Add(freeSpawnPoint);

            // Spawn the player object for the client
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
        else
        {
            Debug.LogError("No free spawn points available!");
        }
    }

    private Transform GetFreeSpawnPoint()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (!usedSpawnPoints.Contains(spawnPoint))
            {
                return spawnPoint;
            }
        }

        return null; // No free spawn point available
    }
}
