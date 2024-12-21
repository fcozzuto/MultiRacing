using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaticData : MonoBehaviour
{
    public static List<Transform> SpawnPoints = new List<Transform>();
    public static Transform[] Waypoints;
    public static int ChosenCarIndex = -1;
    public static Text Lap1Time;
    public static Text Lap2Time;
    public static Text TotalTime;
    public static bool isLapStarted = false;
    public static int lapCount;
    public static float lapTime;
}