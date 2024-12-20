using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using PolyStang;


[System.Serializable]
public struct FinishEntry : INetworkSerializable
{
    public string playerName;
    public int finishPosition;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref finishPosition);
    }
}

public class FinishLineScript : NetworkBehaviour
{
    private bool raceFinished = false;
    private int carsFinished = 0;
    public int totalCars = 4; // Set this to the total number of cars in the race
    private EditPlayerName editPlayerName;

    private List<FinishEntry> finishOrder = new List<FinishEntry>(); // Replace Dictionary

    void Start()
    {
        editPlayerName = GetComponent<EditPlayerName>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (raceFinished) return; // Exit if the race is already finished

        var carController = other.GetComponent<CarController>();
        if (carController != null && !finishOrder.Exists(entry => entry.playerName == editPlayerName.playerName))
        {
            carsFinished++;
            finishOrder.Add(new FinishEntry
            {
                playerName = editPlayerName.playerName,
                finishPosition = carsFinished
            });

            Debug.Log($"{editPlayerName.playerName} finished in position {carsFinished}!");

            // Optionally notify all clients
            NotifyFinishClientRpc(editPlayerName.playerName, carsFinished);

            // Check if all cars have finished
            if (carsFinished == totalCars)
            {
                raceFinished = true;
                EndRace();
            }
        }
    }

    // Notify clients about a car's finish position
    [ClientRpc]
    private void NotifyFinishClientRpc(string playerName, int finishPosition)
    {
        Debug.Log($"{playerName} finished in position {finishPosition} (notified on client)");
    }

    // Display the results on all clients
    private void EndRace()
    {
        DisplayResultsClientRpc(finishOrder.ToArray()); // Send finish order to clients
    }

    [ClientRpc]
    private void DisplayResultsClientRpc(FinishEntry[] results)
    {
        Debug.Log("Race Results:");
        foreach (var entry in results)
        {
            Debug.Log($"{entry.playerName} - Position {entry.finishPosition}");
        }

        // Display results in UI or handle as needed
    }
}
