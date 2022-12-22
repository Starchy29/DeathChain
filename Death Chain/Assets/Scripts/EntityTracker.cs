using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityTracker : MonoBehaviour
{
    private List<GameObject> enemies;
    public List<GameObject> Enemies { get { return enemies; } } // other classes should not modify this list

    // needs to happen before Enemy Start() is called
    void Awake()
    {
        enemies = new List<GameObject>();
    }

    public void AddEnemy(GameObject enemy) {
        enemies.Add(enemy);
    }

    void Update()
    {
        for(int i = 0; i < enemies.Count; i++) {
            if(enemies[i].GetComponent<Enemy>().DeleteThis) {
                Destroy(enemies[i]);
                enemies.RemoveAt(i);
                i--;
            }
        }
    }
}
