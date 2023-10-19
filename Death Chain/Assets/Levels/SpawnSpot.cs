using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// spawns one of the enemies in the list on start
public class SpawnSpot : MonoBehaviour
{
    [SerializeField] private int difficulty; // must match values in Enemy.cs
    [SerializeField] private List<GameObject> enemyOptions;
    [SerializeField] private bool miniboss;

    private static Status[] boosts = new Status[4] { Status.Speed, Status.Energy, Status.Strength, Status.Resistance };

    void Start()
    {
        // choose a valid enemy type
        GameObject spawned = Instantiate(enemyOptions[Random.Range(0, enemyOptions.Count)]);
        spawned.transform.position = transform.position;

        // apply modifiers
        if(miniboss) {
            spawned.GetComponent<Enemy>().BecomeMiniboss();
        }
        else if(Random.value <= 0.1f) {
            // chance to have a status boost
            spawned.GetComponent<Enemy>().ApplyStatus(boosts[Random.Range(0, boosts.Length)]);
        }

        Destroy(gameObject);
    }
}
