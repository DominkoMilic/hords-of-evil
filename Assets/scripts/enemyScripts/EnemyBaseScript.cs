using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class EnemyBaseScript : MonoBehaviour
{
    public static List<EnemyBaseScript> allEnemies = new List<EnemyBaseScript>();

    private string enemyName;
    private int minDamage;
    private int maxDamage;
    private int maxHealth;
    private int currentHealth;
    private int bounty;
    private float speed;
    private float attackSpeed;
    private int physicalArmor;
    private int magicArmor;
    private float attackRange;
    private float separationRadius;
    private float separationStrength;

    private Vector2 spawnPoint; 
    protected Animator animator;
    private SpriteRenderer spriteRenderer;
    public EnemyStats stats;
    private GameLoop game;

    private bool isAlive = true;
    private bool isFighting = false;
    
    protected bool isCasting = false;

    private Coroutine attackRoutine;

    protected SoldierBaseScript targetSoldier;

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

    [Header("Frontline / Fight zone")]
    [SerializeField] float fightZoneTopY = 7.8f;    
    [SerializeField] float fightZoneBottomY = -7f;
    [SerializeField] float yEpsilon = 0.15f;

    Vector3 _lastPos;
    float _sinceLastCheck;
    float _stuckTimer;

    bool _isSidestepping;
    float _sidestepTimer;
    int _sidestepSign; 

    protected static readonly float[] ARMOR_REDUCTION =
    {
        0.0f,  
        0.07f, 
        0.15f, 
        0.25f  
    };

    private DamageSource lastDamageSource = DamageSource.Unknown;

    const string PREF_KEY = "SelectedDifficulty";

    private float GetDifficultyMultiplier()
    {
        switch (Game.SelectedDifficulty)
        {
            case Difficulty.Easy:  return 0.85f;
            case Difficulty.Hard:  return 1.2f;
            case Difficulty.Normal:
            default:               return 1f;
        }
    }

    private void ApplyDifficultyScaling()
    {
        float m = GetDifficultyMultiplier();

        maxHealth = Mathf.Max(1, Mathf.RoundToInt(maxHealth * m));
        currentHealth = maxHealth;

        minDamage = Mathf.Max(0, Mathf.RoundToInt(minDamage * m));
        maxDamage = Mathf.Max(minDamage, Mathf.RoundToInt(maxDamage * m));
    }

    public void setEnemyName(string newName){
        enemyName = newName;
    }
    
    public void setMaxHealth(int newMaxHealth) {
        maxHealth = newMaxHealth;
    }

    public void setCurrentHealth(int changeHealthBy)
    {
        setCurrentHealth(changeHealthBy, DamageSource.Soldier);
    }

    public void setCurrentHealth(int changeHealthBy, DamageSource source)
    {
        flashColor(changeHealthBy);

        if (changeHealthBy < 0)
            lastDamageSource = source;

        currentHealth += changeHealthBy;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
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

    public void setBounty(int newBounty) {
        bounty = newBounty;
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

    public string getEnemyName(){
        return enemyName;
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

    public int getBounty() {
        return bounty;
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
        if (!allEnemies.Contains(this))
            allEnemies.Add(this);
    }
    
    virtual public void Initialize()
    {
        if(!isAlive) return;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        game = FindFirstObjectByType<GameLoop>();
        if(spriteRenderer) spriteRenderer.sortingLayerName = "Characters"; 
        if(stats){
            enemyName = stats.enemyName;
            minDamage = stats.minDamage;
            maxDamage = stats.maxDamage;
            maxHealth = stats.maxHealth;
            currentHealth = stats.maxHealth;
            bounty = stats.bounty;
            speed = stats.speed;
            attackSpeed = stats.attackSpeed;
            physicalArmor = stats.physicalArmor;
            magicArmor = stats.magicArmor;
            attackRange = stats.attackRange;
            separationRadius = stats.separationRadius;
            separationStrength = stats.separationStrength;

            ApplyDifficultyScaling();
        }
        if (animator && isAlive){ 
            animator.Play("walk");
            animator.speed = speed / 1.2f;    
        } 
    }

   protected virtual void Update()
    {
        if(!isAlive) return;

        if (!GameFlowScript.Started)
            return;

        if (isCasting) return;

        UpdateStuckState();

        if(!isFighting){
            findClosestSoldier();

            if(targetSoldier) followSoldier();
            else walkStraightDown();
        }
        else{
            battle();
        }

        if(isEnemyDead()) enemyDeath();
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
 
    private bool CanAttackNow()
    {
        float y = transform.position.y;
        return y <= fightZoneTopY + yEpsilon && y >= fightZoneBottomY - yEpsilon;
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


    private void walkStraightDown(){
       Move(Vector3.down);
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
            var other = col.GetComponentInParent<EnemyBaseScript>();
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



    private void followSoldier(){
        float stopRange = attackRange;
        float d = Vector3.Distance(transform.position, targetSoldier.transform.position);

        if (!CanAttackNow())
            stopRange = 0f;

        if (d > stopRange)
        {
            Vector3 direction = (targetSoldier.transform.position - transform.position).normalized;
            Move(direction);
        }

        battle();
    }


    private void findClosestSoldier(){
        float closestDistance = Mathf.Infinity;
        SoldierBaseScript closest = null;

        foreach (var soldier in SoldierBaseScript.allSoldiers)
        {
            if(!soldier) continue;

            float distance = Vector3.Distance(transform.position, soldier.transform.position);
            if(distance < closestDistance){
                closestDistance = distance;
                closest = soldier;
            }

            targetSoldier = closest ? closest : null;
        }
    }

    private void battle(){
        if (isCasting) return;

        if (!CanAttackNow())
        {
            StopCombat();
            return;
        }

        if(targetSoldier){
            float distance = Vector3.Distance(targetSoldier.transform.position, transform.position);

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

    private IEnumerator AttackLoop()
    {
        while (targetSoldier != null)
        {
            if (animator)
                animator.SetTrigger("FightStarted");

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
        if (targetSoldier == null) return;
        if (!CanAttackNow()) return;
        if(!isFighting) return;

        dealDamage();
    }

    protected virtual void dealDamage()
    {
        if (targetSoldier == null) return;
        ApplyInstantDamage(targetSoldier);
    }

    protected int CalculateDamage(SoldierBaseScript soldier)
    {
        int rawDamage = Random.Range(minDamage, maxDamage + 1);

        int armorTier = Mathf.Clamp(soldier.getPhysicalArmor(), 0, ARMOR_REDUCTION.Length - 1);
        float reduction = ARMOR_REDUCTION[armorTier];

        int finalDamage = Mathf.RoundToInt(rawDamage * (1f - reduction));
        return Mathf.Max(1, finalDamage);
    }

    protected void ApplyInstantDamage(SoldierBaseScript soldier)
    {
        int dmg = CalculateDamage(soldier);
        soldier.setCurrentHealth(-dmg);
    }

    protected int CalculateMagicDamage(SoldierBaseScript soldier)
    {
        int rawDamage = Random.Range(getMinDamage(), getMaxDamage() + 1);

        int armorTier = Mathf.Clamp(
            soldier.getMagicArmor(),
            0,
            ARMOR_REDUCTION.Length - 1
        );

        float reduction = ARMOR_REDUCTION[armorTier];
        int finalDamage = Mathf.RoundToInt(rawDamage * (1f - reduction));

        return Mathf.Max(1, finalDamage);
    }

    protected void ApplyMagicDamageInstant(SoldierBaseScript soldier)
    {
        int dmg = CalculateMagicDamage(soldier);
        soldier.setCurrentHealth(-dmg);
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


    private bool isEnemyDead(){
        if(getCurrentHealth() > 0 && transform.position.y > -7f && transform.position.y < 13f) return false;
        else if(transform.position.y < -7f){
            game.setIsGameOver(true);
            return true;
        }
        else{
            if(attackRoutine != null) StopCoroutine(attackRoutine);
            return true;
        } 
    }

    private void enemyDeath(){
        speed = 0f;
        isAlive = false;

        if(animator) animator.SetTrigger("Die");
        if(game){
            game.totalKills++;
            game.waveEnemyKillCount++;
            game.updateWaveText(true);
            game.addCoins(bounty);
            game.shouldNewWaveDeployChecker();
        } 

        allEnemies.Remove(this);

        AchievementEvents.EmitEnemyKilled();

        bool abilityKill =
            lastDamageSource == DamageSource.Fireball ||
            lastDamageSource == DamageSource.Ability;

        if (abilityKill)
            AchievementEvents.EmitEnemyKilledByAbility(true);

        
        Destroy(gameObject, 1f);
    }

}
