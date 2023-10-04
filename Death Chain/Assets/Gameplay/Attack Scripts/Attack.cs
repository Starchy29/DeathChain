using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// base class for all attacks. Must have an attached trigger collider on the attack layer
public class Attack : MonoBehaviour
{
    [SerializeField] protected int damage;
    [SerializeField] protected float knockback;
    [SerializeField] protected Status effect;
    [SerializeField] protected float effectDuration; // a duration of zero means no status is applied

    private List<Enemy> recentHits = new List<Enemy>();

    private GameObject user;
    public GameObject User { get { return user; } // may be null if the user dies
        set { // must be set by the attack user on creation
            user = value;
            damage = (int)(damage * value.GetComponent<Enemy>().DamageMultiplier);
            isAlly = value.GetComponent<Enemy>().IsAlly;
        }
    }
    public int Damage { 
        get { return damage; }
        set { damage = (int)(value * (user == null ? 1 : user.GetComponent<Enemy>().DamageMultiplier)); } // allow changing damage after creation
    }

    protected bool isAlly;

    public void OnTriggerEnter2D(Collider2D collision) {
        switch(collision.gameObject.layer) {
            case 6: // wall
                OnWallCollision(collision.gameObject);

                // damage breakable walls
                Vector2 collisionPoint = LevelManager.Instance.WallGrid.GetComponent<TilemapCollider2D>().ClosestPoint(transform.position);
                collisionPoint += 0.1f * (collisionPoint - (Vector2)transform.position).normalized; // move off the edge of the collider and into the grid cell
                Vector3Int gridPos = LevelManager.Instance.WallGrid.WorldToCell(collisionPoint);
                LevelManager.Instance.DamageWall(gridPos, damage); // checks for non-breakable walls automatically
                break;

            case 9: // ground enemies
            case 10: // aerial enemies
                // check if colliding with an enemy or ally
                Enemy enemyScript = collision.gameObject.GetComponent<Enemy>();
                if(enemyScript != null && enemyScript.IsAlly != isAlly && !recentHits.Contains(enemyScript)) {
                    if (knockback > 0) {
                        enemyScript.Push(GetPushDirection(enemyScript.gameObject).normalized * knockback);
                        // knockback must be before damage becuase death needs to eliminate momentum
                    }
                    enemyScript.TakeDamage(damage);
                    if(effectDuration > 0) {
                        enemyScript.ApplyStatus(effect, effectDuration);
                    }
                    OnEnemyCollision(enemyScript);

                    // prevent retriggering the attack for a fraction of a second
                    recentHits.Add(enemyScript);
                    Timer.CreateTimer(gameObject, 0.3f, false, () => { 
                        recentHits.Remove(enemyScript);
                    });
                }
                break;
        }
    }

    protected virtual Vector2 GetPushDirection(GameObject hitEnemy) { return Vector2.zero; } // does not need to be normalized
    protected virtual void OnEnemyCollision(Enemy hitEnemy) { }
    protected virtual void OnWallCollision(GameObject hitWall) { }
}
