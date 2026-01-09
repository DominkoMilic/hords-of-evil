using UnityEngine;
using UnityEngine.UI;

public class SpawnFireballScript : MonoBehaviour
{
    public GameLoop game;

   public void SpawnFireball()
    {
        AchievementEvents.EmitAnyUIButtonPressed();
        
        if (!game.IsFireballReady())
            return;
    
        if (game.getIsFireballSelected())
        {
            game.setIsFireballSelected(false);
            game.dropFireballButton.GetComponent<Image>().color = Color.white;
        }
        else
        {
            game.setIsFireballSelected(true);
            game.dropFireballButton.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
        }
    }

}