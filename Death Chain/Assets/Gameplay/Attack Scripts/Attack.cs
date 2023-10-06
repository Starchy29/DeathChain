using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// base class for all attacks
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
            // find which wall tiles were hit
            List<Vector3Int> hitTiles = new List<Vector3Int>();
            Vector3Int centerTile = wallGrid.WorldToCell(transform.position);
            float radius = GetComponent<CircleCollider2D>().radius * transform.localScale.x + 0.1f; // add a little because floats are bad
            int cellRange = (int)Mathf.Ceil(radius / LevelManager.Instance.WallGrid.cellSize.x);
            for(int x = -cellRange; x <= cellRange; x++) {
                for(int y = -cellRange; y <= cellRange; y++) {
                    Vector3Int testPos = centerTile + new Vector3Int(x, y, 0);

                    // check for a breakable wall
                    WallTile wall = wallGrid.GetTile<WallTile>(testPos);
                    if(wall == null) {
                        continue;
                    }

                    // check if the wall is within range
                    Vector2 tileCenter = wallGrid.GetCellCenterWorld(testPos);
                    Vector2 toTile = tileCenter - (Vector2)transform.position;
                    Vector2 closestPoint = (Vector2)transform.position + radius * toTile.normalized;
                    if(toTile.magnitude < radius || wallGrid.WorldToCell(closestPoint) == testPos) {
                        hitTiles.Add(testPos);
                    }
                }
            }

            // damage breakable walls
            foreach(Vector3Int hitTile in hitTiles) {
                WallTile wall = wallGrid.GetTile<WallTile>(hitTile);
                if(wall.Type == WallType.Breakable) {
                    LevelManager.Instance.DamageWall(hitTile, damage);
                }
            }

            OnWallCollision(hitTiles);
        }
    }

    protected virtual Vector2 GetPushDirection(GameObject hitEnemy) { return Vector2.zero; } // does not need to be normalized
    protected virtual void OnEnemyCollision(Enemy hitEnemy) { }
    protected virtual void OnWallCollision(List<Vector3Int> hitTiles) { }
}
