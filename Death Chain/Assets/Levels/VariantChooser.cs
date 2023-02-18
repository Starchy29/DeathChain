using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// only one child of this object remains after starting
public class VariantChooser : MonoBehaviour
{
    void Start()
    {
        if(transform.childCount > 0) {
            transform.GetChild(Random.Range(0, transform.childCount)).parent = transform.parent;
        }

        Destroy(gameObject);
    }
}
