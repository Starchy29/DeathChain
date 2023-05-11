using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// either become a wall or a pit, as a 50/50 chance
public class WallOrPitScript : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject pitPrefab;

    void Start()
    {
        Debug.Log("wall or pit script needs to be updated for new pits still");
        return;

        GameObject obstacle = Instantiate(Random.Range(0, 2) == 0 ? wallPrefab : pitPrefab);
        obstacle.transform.position = transform.position;
        obstacle.transform.localScale = transform.localScale;

        Destroy(gameObject);
    }
}
