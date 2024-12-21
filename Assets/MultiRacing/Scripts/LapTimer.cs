using UnityEngine;
using UnityEngine.UI;

public class LapTimer : MonoBehaviour
{
    public Text lapTimeText;  // UI Text element to display time
    public Transform startLine;  // Starting line to trigger lap
    //private float lapTime = 0f;
    //private int lapCount = 0;

    void Update()
    {
        // Only count time if lap has started
        if (StaticData.isLapStarted)
        {
            StaticData.lapTime += Time.deltaTime;
            lapTimeText.text = "LAP " + StaticData.lapCount + " TIME : " + StaticData.lapTime.ToString("F2");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!StaticData.isLapStarted)
            {
                if (gameObject.tag == "Start")
                {
                    StaticData.lapTime = 0f; // Reset time for new lap
                    StaticData.lapCount++;
                    GameManager.Instance.LapStarted(StaticData.lapCount);
                    StaticData.isLapStarted = true;
                }
            }
            else
            {
                if (gameObject.tag == "Finish")
                {
                    StaticData.isLapStarted = false;
                    GameManager.Instance.LapFinished(StaticData.lapTime);
                    if (StaticData.lapCount == 1)
                        StaticData.Lap1Time = lapTimeText;
                    else
                    {
                        StaticData.Lap2Time = lapTimeText;
                        GameManager.Instance.FinishRace();
                    }
                    
                    // You can add lap completion logic here if you want to track lap count
                    Debug.Log("Lap Completed!");
                }
            }
        }
    }
}