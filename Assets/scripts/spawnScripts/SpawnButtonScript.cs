using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnButtonScript : MonoBehaviour
{
    public Text[] soldierCostText;
    public Text[] upgradePriceText;

    //swordsman, shieldman, spearman, archer
    public Image[] upgradeIcons;
    public Image[] goldIcons;
    
    public GameObject soldier;
    public string soldierName;

    private GameLoop game;
    private int spawnSoldierId;

    void Start(){
        game = FindFirstObjectByType<GameLoop>();
        
        switch(soldierName){
            case "Swordsman":
                spawnSoldierId = 0;
                break;
            case "Shieldman":
                spawnSoldierId = 1;
                break;
            case "Spearman":
                spawnSoldierId = 2;
                break;
            case "Archer":
                spawnSoldierId = 3;
                break;
            default:
                break;
        }
        
        SoldierBaseScript soldierScript = soldier.GetComponent<SoldierBaseScript>();
        soldierScript.spawnButtonScript = this;

        if(game){
            LevelUpgradeData startData = game.getAllsoldiersUpgradeLevel(spawnSoldierId, 0);
            if (startData != null)
                setSoldierCostText(startData.costPrice.ToString(), spawnSoldierId);
                
            if(startData)
                updateUpgradePriceText(startData.upgradePrice.ToString(), spawnSoldierId);
        }
    }

    public void setSoldierCostText(string newCost, int soldierId){
        if(soldierCostText[soldierId])
            soldierCostText[soldierId].text = newCost;
    }

    private void updateUpgradePriceText(string newPrice, int soldierId){
        if(upgradePriceText[soldierId])
            upgradePriceText[soldierId].text = newPrice;
        if(newPrice == "MAX"){
            upgradePriceText[soldierId].color = new Color32(180, 140, 40, 255);
        }
    }

    public void upgradeLevelForSoldier(int soldierId){
        int newLevel = game.getLevelForSoldier(soldierId) + 1;
        
        if(newLevel >= 4) return;

        LevelUpgradeData upgradeData = game.getAllsoldiersUpgradeLevel(soldierId, newLevel - 1);
        LevelUpgradeData upgradeDataForNextLevel = game.getAllsoldiersUpgradeLevel(soldierId, newLevel);

        if(game.getCurrentCoins() >= upgradeData.upgradePrice && newLevel <= 3){
            game.addCoins(-upgradeData.upgradePrice);
            game.upgradeLevelForSoldier(soldierId);
            setSoldierCostText(upgradeData.costPrice.ToString(), soldierId);

            if (AudioManagerScript.Instance != null)
            {
                AudioManagerScript.Instance.PlayUpgrade();
            }

            if(newLevel < 3)
                updateUpgradePriceText(upgradeDataForNextLevel.upgradePrice.ToString(), soldierId);
            else
            {
                updateUpgradePriceText("MAX", soldierId);
                SetUpgradeUIVisible(soldierId, false);
            }

        }
    }

    private void SetUpgradeUIVisible(int soldierId, bool visible)
    {
        if (upgradeIcons != null && soldierId >= 0 && soldierId < upgradeIcons.Length && upgradeIcons[soldierId] != null)
            upgradeIcons[soldierId].gameObject.SetActive(visible);

        if (goldIcons != null && soldierId >= 0 && soldierId < goldIcons.Length && goldIcons[soldierId] != null)
            goldIcons[soldierId].gameObject.SetActive(visible);
    }

}
