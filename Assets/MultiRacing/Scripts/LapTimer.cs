using UnityEngine;
using UnityEngine.UI;

public class LapTimer : MonoBehaviour
{
    public Text lapTimeText;  // UI Text element to display time
    public Transform startLine;  // Starting line to trigger lap
    private bool isLapStarted = false;
    private float lapTime = 0f;

    void Update()
    {
        // Only count time if lap has started
        if (isLapStarted)
        {
            lapTime += Time.deltaTime;
            lapTimeText.text = "Lap Time: " + lapTime.ToString("F2");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isLapStarted)
            {
                isLapStarted = true;
                lapTime = 0f; // Reset time for new lap
            }
            else
            {
                // You can add lap completion logic here if you want to track lap count
                Debug.Log("Lap Completed!");
            }
        }
    }
}