using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for all attacks. Must have an attached trigger collider on the attack layer
public class Attack : MonoBehaviour
{
    [SerializeField] protected int damage;
    [SerializeField] protected float knockback;
    [SerializeField] protected Status effect;
    [SerializeField] protected float effectDuration; // a duration of zero means no status is applied

    private GameObject user;
    public GameObject User { get { return user; }
        set { // must be set by the attack user on creation, remember that the user might die when the attack is still going
            user = value;
            damage = (int)(damage * value.GetComponent<Enemy>().DamageMultiplier);
            isAlly = value.GetComponent<Enemy>().IsAlly;
        }
    } 

    protected bool isAlly;

    public void OnTriggerEnter2D(Collider2D collision) {
        switch(collision.gameObject.layer) {
            case 6: // wall
            case 12: // border wall
                OnWallCollision(collision.gameObject);
                break;

            case 9: // ground enemies
            case 10: // aerial enemies
                // check if colliding with an enemy or ally
                Enemy enemyScript = collision.gameObject.GetComponent<Enemy>();
                if(enemyScript != null && enemyScript.IsAlly != isAlly) {
                    if(knockback > 0) {
                        enemyScript.Push(GetPushDirection(enemyScript.gameObject).normalized * knockback);
                        // knockback must be before damage becuase death needs to eliminate momentum
                    }
                    enemyScript.TakeDamage(damage);
                    if(effectDuration > 0) {
                        enemyScript.ApplyStatus(effect, effectDuration);
                    }
                    OnEnemyCollision(enemyScript);
                }
                break;
        }
    }

    protected virtual Vector2 GetPushDirection(GameObject hitEnemy) { return Vector2.zero; } // does not need to be normalized
    protected virtual void OnEnemyCollision(Enemy hitEnemy) { }
    protected virtual void OnWallCollision(GameObject hitWall) { }
}
