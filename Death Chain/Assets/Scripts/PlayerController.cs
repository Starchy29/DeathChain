using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Controller
{
    private int controllerIndex;

    public PlayerController(int controllerIndex = 0) {
        this.controllerIndex = controllerIndex;
    }

    public override Vector2 GetMoveDirection() {
        return Input.GetDirection();
    }

    public override int GetUsedAbility() {
        for(int i = 0; i < 3; i++) {
            if(Input.AbilityJustPressed(i)) {
                return i;
            }
        }

        return -1; // no ability used
    }

    public override Vector2 GetAimDirection() {
        return Input.GetDirection();
    }
}
