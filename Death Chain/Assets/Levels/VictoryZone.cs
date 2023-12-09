using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// this is for a trigger collider at the end of the level
public class VictoryZone : MonoBehaviour
{
    // next level when leaving the area
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == PlayerScript.Instance.PlayerEntity) {
            SceneManager.LoadScene("Main Menu");
        }
    }
}
