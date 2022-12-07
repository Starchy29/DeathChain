using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The base form of the player
public class PlayerGhost : Enemy
{
    protected override void ChildStart()
    {
        controller = new PlayerController();
        ally = true;
        maxSpeed = 6.0f;
    }
}
