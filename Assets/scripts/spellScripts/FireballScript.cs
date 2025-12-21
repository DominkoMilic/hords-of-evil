using UnityEngine;
using UnityEngine.UI;

public class FireballScript : MonoBehaviour
{
    public ManaSpellData fireballData;
    private Animator animator;

    private int damage;
    private int heal;
    private float range;
    private float speed = 10f;
    private Vector3 targetPosition;

    public void Initialize(Vector3 fallPosition){
        damage = fireballData.damage;
        heal = fireballData.heal;
        range = fireballData.range;
        targetPosition = fallPosition;

        if (animator) animator.Play("fireballFall");    
    }

    private void Update(){
        fireballFall(targetPosition);
    }

    private void fireballFall(Vector3 fallPosition){
        Vector3 direction = (fallPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        float distance = Vector3.Distance(transform.position, fallPosition);
        if(distance <= .5f) fireballExplosion(fallPosition);
    }

    private void fireballExplosion(Vector3 fallPosition){
        foreach (var enemy in EnemyBaseScript.allEnemies){
            float distance = Vector3.Distance(fallPosition, enemy.transform.position);
            if(distance <= range) enemy.setCurrentHealth(-damage);
        }

        Destroy(gameObject);
    }
}
