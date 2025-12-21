using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class SoldierBaseScript : MonoBehaviour
{
    public static List<SoldierBaseScript> allSoldiers = new List<SoldierBaseScript>();

    private string soldierName;
    private int minDamage;
    private int maxDamage;
    private int maxHealth;
    private int currentHealth;
    private int cost;
    private float speed;
    private float attackSpeed;
    private int physicalArmor;
    private int magicArmor;
    private float attackRange;
    private int healingPerSecond;

    private Vector2 spawnPoint; 
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    private bool isAlive = true;
    private bool isFighting = false;

    private GameLoop game;
    public SpawnButtonScript spawnButtonScript;

    private Coroutine attackRoutine;

    private EnemyBaseScript targetEnemy;
    
    private static readonly float[] ARMOR_REDUCTION =
    {
        0.0f,  
        0.07f, 
        0.15f, 
        0.25f  
    };

    public void setSoldierName(string newName){
        soldierName = newName;
    }
    
    public void setMaxHealth(int newMaxHealth) {
        maxHealth = newMaxHealth;
    }

    public void setCurrentHealth(int changeHealthBy) {
        flashColor(changeHealthBy);
        currentHealth += changeHealthBy;
    }

    public void setMinDamage(int newMinDamage) {
        minDamage = newMinDamage;
    }

    public void setMaxDamage(int newMaxDamage) {
        maxDamage = newMaxDamage;
    }

    public void setSpeed(float newSpeed) {
        speed = newSpeed;
    }

    public void setAttackSpeed(float newAttackSpeed) {
        attackSpeed = newAttackSpeed;
    }

    public void setCost(int newCost) {
        cost = newCost;
    }

    public void setMagicArmor(int newMagicArmor){
        magicArmor = newMagicArmor;
    }

    public void setPhysicalArmor(int newPhysicalArmor){
        physicalArmor = newPhysicalArmor;
    }

    public void setAttackRange(float newAttackRange){
        attackRange = newAttackRange;
    }

    public void setSpawnPoint(Vector2 newSpawnPoint){
        spawnPoint = newSpawnPoint;
    }

    public string getSoldierName(){
        return soldierName;
    }

    public int getMaxHealth() {
        return maxHealth;
    }
  
    public int getCurrentHealth() {
        return currentHealth;
    }

    public int getMinDamage() {
        return minDamage;
    }

    public int getMaxDamage() {
        return maxDamage;
    }

    public float getSpeed() {
        return speed;
    }
  
    public float getAttackSpeed() {
        return attackSpeed;
    }

    public int getCost() {
        return cost;
    }

    public int getPhysicalArmor(){
        return physicalArmor;
    }

    public int getMagicArmor(){
        return magicArmor;
    }

    public float getAttackRange(){
        return attackRange;
    }

    public Vector2 getSpawnPoint(){
        return spawnPoint;
    }

    protected virtual void Awake(){
        if (!allSoldiers.Contains(this))
            allSoldiers.Add(this);
    }

    virtual public void Initialize()
    {
        if(!isAlive) return;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        game = FindFirstObjectByType<GameLoop>();
        if(spriteRenderer) spriteRenderer.sortingLayerName = "Characters"; 
        if(game && spawnButtonScript){
            soldierName = spawnButtonScript.soldierName;
            switch (soldierName)
            {
                case "Swordsman":
                    upgradeSystem(game.getLevelForSoldier(0), 0);
                    break;
                case "Shieldman":
                    upgradeSystem(game.getLevelForSoldier(1), 1);
                    break;
                case "Spearman":
                    upgradeSystem(game.getLevelForSoldier(2), 2);
                    break;
                case "Mage":
                    upgradeSystem(game.getLevelForSoldier(3), 3);
                    break;
                default:
                    break;
            }
        }
        if (animator && isAlive){ 
            animator.Play("walk");
            animator.speed = speed / 1.2f;    
        }
    }

    private void upgradeSystem(int level, int soldierId){
        SoldierStats stats = game.getAllSoldierStats(soldierId, level);
        if(stats == null) return;
        minDamage = stats.minDamage;
        maxDamage = stats.maxDamage;
        maxHealth = stats.maxHealth;
        currentHealth = stats.maxHealth;
        cost = stats.cost;
        speed = stats.speed;
        attackSpeed = stats.attackSpeed;
        physicalArmor = stats.physicalArmor;
        magicArmor = stats.magicArmor;
        attackRange = stats.attackRange;
    }

   protected virtual void Update()
    {
        if(!isAlive) return;

        if(!isFighting){
            findClosestEnemy();

            if(targetEnemy) followEnemy();
            else walkStraightUp();
        }
        else{
            battle();
        }

        if(isSoldierDead()) soldierDeath();
    }

    void LateUpdate() {
        if(!isAlive) return;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
    }

    private void walkStraightUp(){
        transform.position += Vector3.up * (speed * Time.deltaTime);
    }

    private void followEnemy(){
        Vector3 direction = (targetEnemy.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    
        battle();
    }

    private void findClosestEnemy(){
        float closestDistance = Mathf.Infinity;
        EnemyBaseScript closest = null;

        foreach (var enemy in EnemyBaseScript.allEnemies)
        {
            if(!enemy) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if(distance < closestDistance){
                closestDistance = distance;
                closest = enemy;
            }

            targetEnemy = closest ? closest : null;
        } 
    }

    private void battle(){
        if(targetEnemy){
            float distance = Vector3.Distance(targetEnemy.transform.position, transform.position);

            if(distance <= attackRange && attackRoutine == null){
                isFighting = true;
                attackRoutine = StartCoroutine(AttackLoop());    
            }
            else if(distance > attackRange && attackRoutine != null){
                StopCoroutine(attackRoutine);
                attackRoutine = null;

                isFighting = false;
                animator.Play("walk");
                animator.speed = speed / 1.2f; 
            }
        }
        else{
            if(attackRoutine != null){
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }
            isFighting = false;
            animator.Play("walk");
            animator.speed = speed / 1.2f; 
        }
    }

    private IEnumerator AttackLoop(){
       while (targetEnemy != null)
        {
            if(animator){
                animator.SetTrigger("FightStarted");
            }
    
            dealDamage();
            yield return new WaitForSeconds(attackSpeed);
        }
    
        attackRoutine = null;
        isFighting = false;
        
        animator.Play("walk");
        animator.speed = speed / 1.2f;
    }

    private void dealDamage()
    {
        if (targetEnemy == null) return;

        int rawDamage = Random.Range(minDamage, maxDamage + 1);

        int armorTier = targetEnemy.getPhysicalArmor();
        armorTier = Mathf.Clamp(armorTier, 0, ARMOR_REDUCTION.Length - 1);

        float reduction = ARMOR_REDUCTION[armorTier];
        int finalDamage = Mathf.RoundToInt(rawDamage * (1f - reduction));
    
        finalDamage = Mathf.Max(1, finalDamage);

        targetEnemy.setCurrentHealth(-finalDamage);
    }

    private void flashColor(int takenHealth){
        if(!spriteRenderer) return; 
        
        if(takenHealth >= 0)
            StartCoroutine(flashColorRoutine(0.6f, 1f, 0.6f));
        else
            StartCoroutine(flashColorRoutine(1f, 0.5f, 0.5f));
    }

    private IEnumerator flashColorRoutine(float r, float g, float b){
        spriteRenderer.color = new Color(r, g, b);
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.color = Color.white;
    }

    private bool isSoldierDead(){
        if(getCurrentHealth() > 0 && transform.position.y > -13f && transform.position.y < 13f) return false;
        else if(transform.position.y < -13f || transform.position.y > 13f) {
            if(game) game.addCoins(cost);
            return true;
        }
        else {
            if(attackRoutine != null) StopCoroutine(attackRoutine);
            return true;
        }
    }

    public void upgradeSoldier(string selectedSoldier){
        game = FindFirstObjectByType<GameLoop>();

        if(!game) return;

        if(game.getIsFireballSelected()){
            game.setIsFireballSelected(false);
            game.dropFireballButton.GetComponent<Image>().color = Color.white;
        }

        spawnButtonScript = FindFirstObjectByType<SpawnButtonScript>();
        
        if(!spawnButtonScript) return;
        
        switch (selectedSoldier)
        {
            case "swordsman":
               spawnButtonScript.upgradeLevelForSoldier(0); 
                break;
            case "shieldman":
                spawnButtonScript.upgradeLevelForSoldier(1);
                break;
            case "spearman":
                spawnButtonScript.upgradeLevelForSoldier(2);
                break;
            case "mage":
                spawnButtonScript.upgradeLevelForSoldier(3);
                break;
            default:
                break;
        }
    }

    private void soldierDeath(){
        speed = 0f;
        isAlive = false;

        if(animator){
            animator.SetTrigger("Die");
        }

        allSoldiers.Remove(this);

        game.addMana(1);

        Destroy(gameObject, 1f);
    }
}
