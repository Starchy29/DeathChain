using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// provides a list of all enemies and walls so certain mechanics can check for all entities in the area
public class EntityTracker : MonoBehaviour
{
    public GameObject CorpseParticle;
    public GameObject HitParticle;
    public GameObject[] statusParticlePrefabs; // order should match enum order

    private static EntityTracker instance;
    public static EntityTracker Instance { get { return instance; } }

    private List<GameObject> enemies;
    public List<GameObject> Enemies { get { return enemies; } } // other classes should not modify this list

    private List<GameObject> walls;
    public List<GameObject> Walls { get { return walls; } } // walls can remove themselves from this if they need to

    private List<PitScript> pits;
    public List<PitScript> Pits { get { return pits; } }

    public List<Rect> BorderAreas { get; private set; }
    public List<Rect> RegularWallAreas { get; private set; }
    public List<Rect> PitAreas { get; private set; }

    // needs to happen before Enemy.cs Start() and WallScript.cs Start() is called
    void Awake()
    {
        enemies = new List<GameObject>();
        walls = new List<GameObject>();
        pits = new List<PitScript>();

        BorderAreas = new List<Rect>();
        RegularWallAreas = new List<Rect>();
        PitAreas = new List<Rect>();

        instance = this;
        Timer.ClearTimers();
    }

    public void AddEnemy(GameObject enemy) {
        enemies.Add(enemy);
    }

    public void AddWall(GameObject wall) {
        walls.Add(wall);

        if(wall.layer == LayerMask.NameToLayer("Border")) {
            BorderAreas.Add(wall.GetComponent<WallScript>().Area);
        }
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

        // compile the current obstacle areas each frame to account for temporary obstacles
        RegularWallAreas = new List<Rect>();
        PitAreas = new List<Rect>();

        foreach(PitScript pit in pits) {
            PitAreas.AddRange(pit.Zones);
        }
        foreach(GameObject wall in walls) {
            if(wall.layer != LayerMask.NameToLayer("Border")) {
                RegularWallAreas.Add(wall.GetComponent<WallScript>().Area);
            }
        }
    }
}
