using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// spawns one of the enemies in the list on start
public class SpawnSpot : MonoBehaviour
{
    public List<GameObject> enemyOptions; // prefabs

    void Start()
    {
        GameObject spawned = Instantiate(enemyOptions[Random.Range(0, enemyOptions.Count)]);
        spawned.transform.position = transform.position; // place on top of this position
        Destroy(this.gameObject);
    }
}
