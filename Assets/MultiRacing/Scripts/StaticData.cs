using System.Collections.Generic;
using UnityEngine;

public class StaticData : MonoBehaviour
{
    public static List<Transform> SpawnPoints = new List<Transform>();
    public static Transform[] Waypoints;
    public static int ChosenCarIndex = -1;
}