using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SoldierSpawnerScript : MonoBehaviour
{
    public GameObject[] soldier;
    public GameLoop game;

    public Text maxSoldiersAmountText;

    private LanguageSetterScript languageSetter;
    private Coroutine maxTextRoutine;

    [SerializeField] private float soundCooldown = 7f;
    private float lastSoundTime = -Mathf.Infinity;

    private void Start()
    {
        languageSetter = FindFirstObjectByType<LanguageSetterScript>();

        if (maxSoldiersAmountText)
            maxSoldiersAmountText.gameObject.SetActive(false);
    }

    public void spawnBaseSoldier(int soldierId)
    {
        if (!game) return;

        if (!game.CanSpawn(soldierId))
        {
            ShowMaxSoldiersText(soldierId);
            return;
        }

        Vector3 spawnPos = new Vector3(Random.Range(-2f, 2f), -7f, 0f);

        GameObject newSoldier = Instantiate(soldier[soldierId], spawnPos, Quaternion.identity);

        SoldierBaseScript baseScript = newSoldier.GetComponent<SoldierBaseScript>();
        if (!baseScript) return;

        baseScript.soldierId = soldierId;
        baseScript.game = game;

        baseScript.Initialize();

        int cost = baseScript.getCost();
        if (game.getCurrentCoins() < cost)
        {
            Destroy(newSoldier);
            return;
        }

        TryPlaySpawnSound();

        game.RegisterSpawn(soldierId);
        game.addCoins(-cost);
    }

    private void ShowMaxSoldiersText(int soldierId)
    {
        if (!maxSoldiersAmountText || !languageSetter || !game) return;

        int current = game.GetAliveCount(soldierId);
        int max = game.maxSpawnedSoldiersPerType;

        string localizedName = languageSetter.GetSoldierName(soldierId);

        maxSoldiersAmountText.text =
            languageSetter.GetMaxSoldiersText(localizedName, current, max);

        maxSoldiersAmountText.gameObject.SetActive(true);

        if (maxTextRoutine != null)
            StopCoroutine(maxTextRoutine);

        maxTextRoutine = StartCoroutine(HideMaxTextAfterDelay());
    }

    private IEnumerator HideMaxTextAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        if (maxSoldiersAmountText)
            maxSoldiersAmountText.gameObject.SetActive(false);
        maxTextRoutine = null;
    }

    private void playSoundEffect()
    {
        if (AudioManagerScript.Instance == null)
            return;

        System.Action[] sounds =
        {
            AudioManagerScript.Instance.PlaySwordDraw,
            AudioManagerScript.Instance.PlayHornBlow
        };

        int randomIndex = Random.Range(0, sounds.Length);
        sounds[randomIndex].Invoke();
    }

    private void TryPlaySpawnSound()
    {
        if (Time.time - lastSoundTime < soundCooldown)
            return;

        playSoundEffect();
        lastSoundTime = Time.time;
    }
}
