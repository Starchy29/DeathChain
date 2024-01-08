using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearOnDeath : MonoBehaviour
{
    [SerializeField] Enemy watch;

    private void Start()
    {
        GetComponent<TMPro.TextMeshPro>().enabled = false;
    }

    private void Update()
    {
        if(watch.IsCorpse) {
            GetComponent<TMPro.TextMeshPro>().enabled = true;
        }
    }
}
