using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an area that continually applies a status effect as long as the enemy is within
public class StatusZone : Attack
{
    [SerializeField] private float duration;
    private const float TICK_RATE = 0.2f; // seconds
    private Timer timer;
    private List<Enemy> enemiesWithin;

    void Start()
    {
        enemiesWithin = new List<Enemy>();
        damage = 0;

        // apply the status effect to all enemies within every interval
        timer = Timer.CreateTimer(0.2f, true, () => { 
            foreach(Enemy enemy in enemiesWithin) {
                enemy.ApplyStatus(effect, TICK_RATE);
            }
        });

        // end effect after the duration
        Timer.CreateTimer(duration, false, () => {
            timer.End();
            Destroy(gameObject);
        });
    }

    private new void OnTriggerEnter2D(Collider2D collision) {
        Enemy script = collision.gameObject.GetComponent<Enemy>();
        if(script != null && script.IsAlly != isAlly) {
            enemiesWithin.Add(script);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Enemy script = collision.gameObject.GetComponent<Enemy>();
        if(script != null && script.IsAlly != isAlly) {
            enemiesWithin.Remove(script);
        }
    }
}
