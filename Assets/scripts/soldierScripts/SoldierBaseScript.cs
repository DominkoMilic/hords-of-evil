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
    private float separationRadius;
    private float separationStrength;

    private Vector2 spawnPoint; 
    protected Animator animator;
    private SpriteRenderer spriteRenderer;
    
    private bool isAlive = true;
    private bool isFighting = false;

    public GameLoop game;
    public int soldierId;
    public SpawnButtonScript spawnButtonScript;

    private Coroutine attackRoutine;
    
    protected bool isCasting = false;

    protected EnemyBaseScript targetEnemy;

    private bool unregistered;
    
    protected virtual bool UsesAttackReleaseEvent() => false;
    
    [SerializeField] LayerMask allyMask;
    Collider2D[] _hits = new Collider2D[16];

    [Header("Stuck handling")]
    [SerializeField] float stuckCheckInterval = 0.25f;
    [SerializeField] float stuckMoveEpsilon = 0.01f;   
    [SerializeField] float stuckTimeToTrigger = 0.6f;  
    [SerializeField] float sidestepDuration = 0.7f;
    [SerializeField] float sidestepStrength = 0.8f;

    [SerializeField] float widenSepMultiplier = 1.6f;  
    [SerializeField] float widenStrMultiplier = 1.6f;

    Vector3 _lastPos;
    float _sinceLastCheck;
    float _stuckTimer;

    bool _isSidestepping;
    float _sidestepTimer;
    int _sidestepSign; 

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

    public void setSeparationRadius(float newseparationRadius){
        separationRadius = newseparationRadius;
    }

    public void setSeparationStrength(float newseparationStrength){
        separationStrength = newseparationStrength;
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

    public float getSeparationRadius(){
        return separationRadius;
    }

    public float getSeparationStrength(){
        return separationStrength;
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
                case "Archer":
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
        separationRadius = stats.separationRadius;
        separationStrength = stats.separationStrength;
    }

   protected virtual void Update()
    {
        if(!isAlive) return;

        UpdateStuckState();

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

    void UpdateStuckState()
    {
        _sinceLastCheck += Time.deltaTime;
        if (_sinceLastCheck < stuckCheckInterval) return;

        float moved = Vector3.Distance(transform.position, _lastPos);

        bool wantsToMove = !isFighting; 

        if (wantsToMove && moved < stuckMoveEpsilon)
            _stuckTimer += _sinceLastCheck;
        else
            _stuckTimer = 0f;

        if (!_isSidestepping && _stuckTimer >= stuckTimeToTrigger)
        {
            _isSidestepping = true;
            _sidestepTimer = sidestepDuration;
            _sidestepSign = (Random.value < 0.5f) ? -1 : 1;
            _stuckTimer = 0f;
        }

        _lastPos = transform.position;
        _sinceLastCheck = 0f;

        if (_isSidestepping)
        {
            _sidestepTimer -= stuckCheckInterval;
            if (_sidestepTimer <= 0f) _isSidestepping = false;
        }
    }


    private void Move(Vector3 desiredDir)
    {
        float sepRadius = separationRadius;
        float sepStrength = separationStrength;

        if (_isSidestepping)
        {
            sepRadius *= widenSepMultiplier;
            sepStrength *= widenStrMultiplier;
        }

        Vector3 sep = GetSeparation(sepRadius) * sepStrength;

        Vector3 side = Vector3.zero;
        if (_isSidestepping)
        {
            Vector3 perp = new Vector3(-desiredDir.y, desiredDir.x, 0f);
            side = perp.normalized * (sidestepStrength * _sidestepSign);
        }

        Vector3 combined = desiredDir + sep + side;

        float mag = combined.magnitude;
        if (mag > 0.0001f)
            combined /= Mathf.Max(1f, mag);

        transform.position += combined * speed * Time.deltaTime;
    }


    private void walkStraightUp(){
       Move(Vector3.up);
    }

   private Vector3 GetSeparation(float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, allyMask);
        if (hits.Length == 0) return Vector3.zero;

        Vector2 push = Vector2.zero;
        int contributors = 0;

        foreach (var col in hits)
        {
            if (!col) continue;
            var other = col.GetComponentInParent<SoldierBaseScript>();
            if (!other || other == this) continue;

            Vector2 toMe = (Vector2)(transform.position - other.transform.position);
            float d = toMe.magnitude;
            if (d < 0.0001f) continue;

            float w = 1f - Mathf.Clamp01(d / radius);
            push += (toMe / d) * w;
            contributors++;
        }

        if (contributors == 0) return Vector3.zero;
        push /= contributors;
        return (Vector3)push;
    }

    private void followEnemy(){
        float stopRange = attackRange;
        float d = Vector3.Distance(transform.position, targetEnemy.transform.position);

        if (d > stopRange)
        {
            Vector3 direction = (targetEnemy.transform.position - transform.position).normalized;
            Move(direction);
        }

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
    
            if (!UsesAttackReleaseEvent())
                dealDamage();

            yield return new WaitForSeconds(attackSpeed);
        }
    
        attackRoutine = null;
        isFighting = false;
        
        animator.Play("walk");
        animator.speed = speed / 1.2f;
    }

    public void OnAttackRelease()
    {
        if (!isAlive || isCasting) return;
        if (targetEnemy == null) return;

        dealDamage();
    }

    protected virtual void dealDamage()
    {
        if (targetEnemy == null) return;
        ApplyInstantDamage(targetEnemy);
    }
    
    protected void StopCombat()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        isFighting = false;

        if (!isCasting && animator)
        {
            animator.Play("walk");
            animator.speed = speed / 1.2f;
        }
    }

    protected int CalculateDamage(EnemyBaseScript enemy)
    {
        int rawDamage = Random.Range(minDamage, maxDamage + 1);

        int armorTier = Mathf.Clamp(enemy.getPhysicalArmor(), 0, ARMOR_REDUCTION.Length - 1);
        float reduction = ARMOR_REDUCTION[armorTier];

        int finalDamage = Mathf.RoundToInt(rawDamage * (1f - reduction));
        return Mathf.Max(1, finalDamage);
    }

    protected void ApplyInstantDamage(EnemyBaseScript enemy)
    {
        int dmg = CalculateDamage(enemy);
        enemy.setCurrentHealth(-dmg);
    }

    protected int CalculateMagicDamage(EnemyBaseScript enemy)
    {
        int rawDamage = Random.Range(getMinDamage(), getMaxDamage() + 1);

        int armorTier = Mathf.Clamp(
            enemy.getMagicArmor(),
            0,
            ARMOR_REDUCTION.Length - 1
        );

        float reduction = ARMOR_REDUCTION[armorTier];
        int finalDamage = Mathf.RoundToInt(rawDamage * (1f - reduction));

        return Mathf.Max(1, finalDamage);
    }

    protected void ApplyMagicDamageInstant(EnemyBaseScript enemy)
    {
        int dmg = CalculateMagicDamage(enemy);
        enemy.setCurrentHealth(-dmg);
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

    public void upgradeSoldier(string selectedSoldier)
    {
        game = FindFirstObjectByType<GameLoop>();
        if (!game) return;

        if (game.getIsFireballSelected())
        {
            game.setIsFireballSelected(false);
            game.dropFireballButton.GetComponent<Image>().color = Color.white;
        }

        spawnButtonScript = FindFirstObjectByType<SpawnButtonScript>();
        if (!spawnButtonScript) return;

        bool upgraded = false;

        switch (selectedSoldier)
        {
            case "swordsman":
                spawnButtonScript.upgradeLevelForSoldier(0);
                upgraded = true;
                break;
            case "shieldman":
                spawnButtonScript.upgradeLevelForSoldier(1);
                upgraded = true;
                break;
            case "spearman":
                spawnButtonScript.upgradeLevelForSoldier(2);
                upgraded = true;
                break;
            case "archer":
                spawnButtonScript.upgradeLevelForSoldier(3);
                upgraded = true;
                break;
        }

        if (upgraded && AudioManagerScript.Instance != null)
        {
            AudioManagerScript.Instance.PlayUpgrade();
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

        if (!unregistered && game != null)
        {
            game.RegisterDeath(soldierId);
            unregistered = true;
        }

        Destroy(gameObject, 1f);
    }
}
