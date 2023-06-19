using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// deletes this as soon as the player possesses something for the first
public class PossessCheck : MonoBehaviour
{
    void Update()
    {
        if(PlayerScript.Instance.PlayerEntity.GetComponent<PlayerGhost>() == null) {
            EntityTracker.Instance.Walls.Remove(gameObject);
            Destroy(gameObject);
        }
    }
}
