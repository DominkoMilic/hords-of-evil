using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SingleWaveStats", menuName = "Game Data/Wave Data")]
public class SingleWaveStats : ScriptableObject
{
    public int levelNumber;
    public int waveNumber;
    public List<TroopPerWave> troopPerWaveInfo;
    public float timeBeforeNextWaveDeployement;
}
