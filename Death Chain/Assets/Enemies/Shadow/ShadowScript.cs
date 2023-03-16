using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script for the enemy type called shadow
public class ShadowScript : Enemy
{
    protected override void ChildStart()
    {
        controller = new AIController(gameObject, AIMode.Wander, 7.0f);
        //maxSpeed = 4.0f;
    }

    protected override void UpdateAbilities() {
        
    }
}
