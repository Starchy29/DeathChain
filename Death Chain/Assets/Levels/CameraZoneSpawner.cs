using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// attach to a rectangle to create a zone the camera can move in
public class CameraZoneSpawner : MonoBehaviour
{
    void Start()
    {
        CameraScript.Instance.AddCameraZone(transform.position);
        Destroy(gameObject);
    }
}
