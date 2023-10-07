using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// an ability type that does one hit of damage at a time. Can use a trigger or a rigidbody for collision
public class Attack : Ability
{
    [SerializeField] protected int damage;
    [SerializeField] protected float knockback;
    [SerializeField] protected Status effect;
    [SerializeField] protected float effectDuration; // a duration of zero means no status is applied

    private List<Enemy> recentHits = new List<Enemy>();

    public override Enemy User {
        get => base.User;
        set {
            base.User = value;
            damage = (int)(damage * user.DamageMultiplier);
        }
    }
    public int Damage { 
        get { return damage; }
        set { damage = (int)(value * (user == null ? 1 : user.DamageMultiplier)); } // allow changing damage after creation
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        OnCollision(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        OnCollision(collision.gameObject);
    }

    // collision settings prevent this from colliding with self or ally
    private void OnCollision(GameObject collidedObject) {
        // check if collided with enemy
        Enemy enemyScript = collidedObject.GetComponent<Enemy>();
        if(enemyScript != null && !recentHits.Contains(enemyScript)) {
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

        // check if collided with a wall
        Tilemap wallGrid = collidedObject.GetComponent<Tilemap>();
        if(wallGrid != null) {
            List<Vector3Int> overlappedTiles = LevelManager.Instance.GetOverlappedTiles(gameObject);
            List<Vector3Int> hitWalls = new List<Vector3Int>();
            foreach(Vector3Int overlappedTile in overlappedTiles) {
                WallTile wall = wallGrid.GetTile<WallTile>(overlappedTile);
                if(wall != null ) {
                    hitWalls.Add(overlappedTile);
                    
                    // damage breakable walls
                    if(wall.Type == WallType.Breakable) {
                        LevelManager.Instance.DamageWall(overlappedTile, damage);
                    }
                }
            }

            OnWallCollision(hitWalls);
        }
    }

    protected virtual Vector2 GetPushDirection(GameObject hitEnemy) { return Vector2.zero; } // does not need to be normalized
    protected virtual void OnEnemyCollision(Enemy hitEnemy) { }
    protected virtual void OnWallCollision(List<Vector3Int> hitTiles) { }
}
