using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// attach to a rectangle to create a zone the camera can move in
public class CameraZoneSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector3 mid = transform.position;
        Vector2 scale = transform.localScale;
        CameraScript.Instance.AddCameraZone(new Rect(mid.x - scale.x / 2, mid.y - scale.y / 2, scale.x, scale.y));

        Destroy(gameObject);
    }
}
