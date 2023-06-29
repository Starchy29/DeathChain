using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// spawns one of the enemies in the list on start
public class SpawnSpot : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemyOptions; // prefabs
    [SerializeField] private bool miniboss;

    void Start()
    {
        GameObject spawned = Instantiate(enemyOptions[Random.Range(0, enemyOptions.Count)]);
        spawned.transform.position = transform.position;
        Destroy(this.gameObject);

        if(miniboss) {
            spawned.GetComponent<Enemy>().BecomeMiniboss();
        }
    }
}
