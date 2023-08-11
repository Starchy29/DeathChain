using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWallScript : WallScript
{
    private int health = 10;

    public void TakeDamage(int amount) {
        if(amount <= 0) {
            return;
        }

        health -= amount;
        if(health <= 0) {
            EntityTracker.Instance.Walls.Remove(gameObject);
            Destroy(gameObject);
        }
    }
}
