using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// provides a list of all enemies and walls so certain mechanics can check for all entities in the area
public class EntityTracker : MonoBehaviour
{
    public GameObject[] statusParticlePrefabs; // order should match enum order

    private static EntityTracker instance;
    public static EntityTracker Instance { get { return instance; } }

    private List<GameObject> enemies;
    public List<GameObject> Enemies { get { return enemies; } } // other classes should not modify this list

    private List<GameObject> obstacles; // walls, borders, pits
    public List<GameObject> Obstacles { get { return obstacles; } } // obstacles can remove themselves from this if they need to

    // needs to happen before Enemy.cs Start()  and WallScript.cs Start() is called
    void Awake()
    {
        enemies = new List<GameObject>();
        obstacles = new List<GameObject>();

        instance = this;
    }

    public void AddEnemy(GameObject enemy) {
        enemies.Add(enemy);
    }

    public void AddObstacle(GameObject obstacle) {
        obstacles.Add(obstacle);
    }

    void Update()
    {
        Timer.UpdateAll(Time.deltaTime);

        // check for deleted enemies
        for(int i = 0; i < enemies.Count; i++) {
            if(enemies[i].GetComponent<Enemy>().DeleteThis) {
                enemies[i].SetActive(false);
                Destroy(enemies[i]);
                enemies.RemoveAt(i);
                i--;
            }
        }
    }
}
