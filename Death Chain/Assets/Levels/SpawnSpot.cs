using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// spawns one of the enemies in the list on start
public class SpawnSpot : MonoBehaviour
{
    [SerializeField] private int difficulty; // must match values in Enemy.cs
    [SerializeField] private Tag tag;
    [SerializeField] private List<GameObject> enemyOptions; // prefabs
    [SerializeField] private bool miniboss;

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
            Status effect = (new Status[4] {Status.Speed, Status.Energy, Status.Strength, Status.Resistance }) [Random.Range(0, 4)];
            spawned.GetComponent<Enemy>().ApplyStatus(effect);
        }

        Destroy(gameObject);
    }

    private enum Tag {
        Any,
        Ranged,
        Melee,
        Chase,
        Still,
        Floating,
        Guard,
        Zoner
    }

    private static Dictionary<Tag, List<GameObject>[]> tagStorage; // tag gets a 3 length array, where the index is a difficulty. Each element of the array is a list of prefabs

    // gets the list of enemy prefabs with the input tag belonging to the member difficulty value. The result can be an empty list
    private List<GameObject> GetPrefabOptions(Tag tag) {
        return null;
    }
}
