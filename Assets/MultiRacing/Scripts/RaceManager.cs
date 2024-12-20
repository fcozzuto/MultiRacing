using UnityEngine;
using Unity.Netcode;
using System.Collections;
using TMPro;
using PolyStang;
using Cinemachine;
using Unity.VisualScripting;

public class RaceManager : NetworkBehaviour
{
    

    public TMP_Text countdownText; // UI element for countdown
    public bool raceStarted = false;

    [Header("Network")]
    public Camera MyCamera;
    public Canvas MyCanvas;
    public CinemachineVirtualCamera MyVCamera;
    private CarController carController;

    private void Start()
    {
        OnEnable();
        carController = GetComponent<CarController>();
        if (IsServer)
        {
            // Notify all clients to start the countdown
            StartCountdownOnClientRpc();
        }
    }

    // Client-side countdown
    private IEnumerator StartCountdown()
    {
        string[] countdownTexts = { "3", "2", "1", "GO!" };

        for (int i = 0; i < countdownTexts.Length; i++)
        {
            if (countdownText != null)
            {
                countdownText.text = countdownTexts[i]; // Update countdown UI
            }
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
        {
            raceStarted = true;
            countdownText.text = ""; // Clear countdown text
        }
    }


    // ClientRpc to start the countdown on all clients
    [ClientRpc]
    private void StartCountdownOnClientRpc()
    {
        if (IsClient)
        {
            StartCoroutine(StartCountdown());
        }
    }

    private void OnEnable()
    {
        if (IsLocalPlayer)
        {
            MyCamera.gameObject.SetActive(true);
            MyCanvas.gameObject.SetActive(true);
            MyVCamera.gameObject.SetActive(true);
        }
    }
}
