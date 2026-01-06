using UnityEngine;

public class GameFlowScript : MonoBehaviour
{
    public static bool Started { get; private set; }
    public static void StartGame() => Started = true;
    public static void StopGame() => Started = false;
}
