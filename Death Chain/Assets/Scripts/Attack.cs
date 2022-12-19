using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for all attacks
public abstract class Attack : MonoBehaviour
{
    public GameObject User { get; set; } // must be set by the attack user on creation
    protected int damage;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        switch(collision.gameObject.layer) {
            case 6: // wall
            case 12: // border wall
                OnWallCollision();
                break;

            case 9: // ground enemies
            case 10: // aerial enemies
                // check if colliding with an enemy or ally
                Enemy enemyScript = collision.gameObject.GetComponent<Enemy>();
                if(enemyScript != null && enemyScript.IsAlly != User.GetComponent<Enemy>().IsAlly) {
                    enemyScript.TakeDamage((int)(damage * User.GetComponent<Enemy>().DamageMultiplier));
                    OnEnemyCollision(enemyScript);
                }
                break;
        }
    }

    protected void OnEnemyCollision(Enemy hitEnemy) { }
    protected void OnWallCollision() { }
}
