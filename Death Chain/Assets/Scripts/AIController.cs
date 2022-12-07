using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class that allows AI to control the enemies in the game
public class AIController : Controller
{
    private GameObject target; // the entity this is trying to attack

    public AIController() {

    }

    public override Vector2 GetMoveDirection() {
        return new Vector2();
    }

    public override int GetUsedAbility() {
        return -1; // not implemented yet
    }

    public override Vector2 GetAimDirection() {
        return Vector2.zero;
    }
}
