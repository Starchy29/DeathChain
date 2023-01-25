using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// provides a list of all enemies so certain mechanics can check for all enemies in the area
public class EntityTracker : MonoBehaviour
{
    private List<GameObject> removed; // disable enemies instead of deleting because attacks can still refer to them
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
        // check for deleted enemies
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
