using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an area that continually applies a status effect as long as the enemy is within
public class StatusZone : MonoBehaviour
{
    [SerializeField] private Status effect;
    [SerializeField] private float duration;
    [SerializeField] private float enterAmount;
    [SerializeField] private bool grounded; // allows floating enemies to be unaffected
    private const float TICK_RATE = 0.2f; // seconds
    private Timer timer;
    private List<Enemy> enemiesWithin;

    public bool? IsAlly { get; set; } // must be set by creator, null means it affects all characters

    void Start()
    {
        enemiesWithin = new List<Enemy>();

        // apply the status effect to all enemies within every interval
        timer = Timer.CreateTimer(gameObject, 0.2f, true, () => { 
            foreach(Enemy enemy in enemiesWithin) {
                enemy.ApplyStatus(effect, TICK_RATE);
            }
        });

        // end effect after the duration, or infinite if an invalid duration
        if(duration > 0) {
            Timer.CreateTimer(gameObject, duration, false, () => {
                timer.End();
                Destroy(gameObject);
            });
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Enemy script = collision.gameObject.GetComponent<Enemy>();
        if(script != null && IsTarget(script) && !(grounded && script.Floating)) {
            enemiesWithin.Add(script);
            script.ApplyStatus(effect, enterAmount);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Enemy script = collision.gameObject.GetComponent<Enemy>();
        if(enemiesWithin.Contains(script)) {
            enemiesWithin.Remove(script);
        }
    }

    private bool IsTarget(Enemy enemyScript) {
        return !IsAlly.HasValue || IsAlly != enemyScript.IsAlly;
    }
}
