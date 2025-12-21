using UnityEngine;

[CreateAssetMenu(fileName = "New Level Data", menuName = "Game Data/Level Data")]
public class LevelData : ScriptableObject
{
    public int levelNumber;
    public int totalWaveNumber;
    public int startCoins;
    public SingleWaveStats[] waves;
}
