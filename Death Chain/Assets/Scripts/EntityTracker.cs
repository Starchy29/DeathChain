using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// provides a list of all enemies so certain mechanics can check for all enemies in the area
public class EntityTracker : MonoBehaviour
{
    private List<GameObject> removed; // ok, so we can't actually delete them because the projectiles still refer to them, so just diable
    private List<GameObject> enemies;
    public List<GameObject> Enemies { get { return enemies; } } // other classes should not modify this list

    // needs to happen before Enemy Start() is called
    void Awake()
    {
        enemies = new List<GameObject>();
        removed = new List<GameObject>();
    }

    public void AddEnemy(GameObject enemy) {
        enemies.Add(enemy);
    }

    void Update()
    {
        for(int i = 0; i < enemies.Count; i++) {
            if(enemies[i].GetComponent<Enemy>().DeleteThis) {
                enemies[i].SetActive(false);
                removed.Add(enemies[i]);
                enemies.RemoveAt(i);
                i--;
            }
        }
    }
}
