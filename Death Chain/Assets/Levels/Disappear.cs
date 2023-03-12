using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// deletes the object when starting
public class Disappear : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }
}
