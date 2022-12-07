using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for player and AI controllers
public abstract class Controller
{
    // returns a unit vector that represents the direction this character wants to move. Can return the zero vector if not moving
    public abstract Vector2 GetMoveDirection();

    // returns a unit vector that represents the direction this character is aiming. Can return the zero vector if not moving
    public abstract Vector2 GetAimDirection();

    // returns an int that represents which ability this is trying to use. 0,1,2 are ability slots. -1 (or any negative) is no ability
    public abstract int GetUsedAbility();
}
