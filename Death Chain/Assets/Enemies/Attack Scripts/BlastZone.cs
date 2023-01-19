using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a type of attack that does a quick, short burst, then goes away
public class BlastZone : Attack
{
    [SerializeField] private float duration;
    [SerializeField] private GameObject particlePrefab;

    void Start()
    {
        if(particlePrefab != null) {
            GameObject particle = Instantiate(particlePrefab);
            particle.transform.position = gameObject.transform.position;
        }
    }

    void Update()
    {
        duration -= Time.deltaTime;
        if(duration <= 0) {
            Destroy(gameObject);
        }
    }
}
