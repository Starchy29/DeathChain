using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// make the possess particle move into the enemy being possessed
public class PossessMovement : MonoBehaviour
{
    private Vector3 offset = new Vector3(0, 0.5f, 0);
    public GameObject Target { get; set; }

    // Update is called once per frame
    void Update()
    {
        if(transform.parent == Target.transform) {
            return;
        }

        Vector2 direction = Target.transform.position + offset - transform.position;
        Vector3 shift = direction.normalized * Time.deltaTime * 3;
        transform.position += shift;

        if(Vector2.Distance(transform.position, Target.transform.position + offset) <= 0.05) {
            transform.parent = Target.transform;
            transform.position = Target.transform.position + offset;
        }
    }
}
