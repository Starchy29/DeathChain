using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an object which gives a player a temporary status effect when collided
public class StatusPickup : MonoBehaviour
{
    [SerializeField] private Status effect;
    private const float duration = 10.0f;

    private void OnTriggerEnter2D(Collider2D collision) {
        Enemy enemyScript = collision.gameObject.GetComponent<Enemy>();
        if(enemyScript != null && enemyScript.IsAlly) {
            enemyScript.ApplyStatus(effect, duration);
            Destroy(gameObject);
        }
    }
}
