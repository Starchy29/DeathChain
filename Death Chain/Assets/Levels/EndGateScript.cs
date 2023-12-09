using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGateScript : MonoBehaviour
{
    [SerializeField] private int soulCost;

    // open the door when the player walks into it if they have enough souls
    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerScript player = PlayerScript.Instance;
        if(collision.gameObject == player.PlayerEntity && player.Souls >= soulCost) {
            player.Souls -= soulCost;
            Destroy(gameObject);
        }
    }
}
