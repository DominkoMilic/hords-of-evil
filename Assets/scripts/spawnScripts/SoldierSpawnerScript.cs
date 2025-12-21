using UnityEngine;
using UnityEngine.UI;

public class SoldierSpawnerScript : MonoBehaviour
{
    public GameObject[] soldier;
    public GameLoop game;

    public void spawnBaseSoldier(int soldierId)
    {
        if (!game) return;

        if (game.getIsFireballSelected())
        {
            game.setIsFireballSelected(false);
            game.dropFireballButton.GetComponent<Image>().color = Color.white;
        }

        Vector3 spawnPos = new Vector3(Random.Range(-2f, 2f), -7f, 0f);

        GameObject newSoldier = Instantiate(soldier[soldierId], spawnPos, Quaternion.identity);

        SoldierBaseScript soldierBaseScript = newSoldier.GetComponent<SoldierBaseScript>();
        soldierBaseScript.Initialize();

        int cost = soldierBaseScript.getCost();

        if (game.getCurrentCoins() < cost)
        {
            Destroy(newSoldier);
            return;
        }

        game.addCoins(-cost);
    }
}
