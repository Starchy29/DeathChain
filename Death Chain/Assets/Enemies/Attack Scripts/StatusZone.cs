using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an area that continually applies a status effect as long as the enemy is within
public class StatusZone : Attack
{
    [SerializeField] private float duration;
    private const float TICK_RATE = 0.2f; // seconds
    private float timer;
    private List<Enemy> enemiesWithin;

    void Start()
    {
        enemiesWithin = new List<Enemy>();
        damage = 0;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if(timer <= 0) {
            timer += TICK_RATE;
            foreach(Enemy enemy in enemiesWithin) {
                enemy.ApplyStatus(effect, TICK_RATE);
            }
        }

        duration -= Time.deltaTime;
        if(duration <= 0) {
            Destroy(gameObject);
        }
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
