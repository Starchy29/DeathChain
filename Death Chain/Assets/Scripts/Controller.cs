using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for player and AI controllers
public abstract class Controller
{
    protected GameObject controlled; // the entity this is controlling

    public Controller(GameObject controlled) {
        this.controlled = controlled;
    }

    // Called every frame by Enemy. Allows important utility like tracking last aim and player position
    public virtual void Update() { }

    // returns a unit vector that represents the direction this character wants to move. Can return the zero vector if not moving
    public abstract Vector2 GetMoveDirection();

    // returns a unit vector that represents the direction this character is aiming. Cannot return the zero vector
    public abstract Vector2 GetAimDirection();

    // returns an int that represents which ability this is trying to use.
    // 0,1,2 are ability slots (ability 3 is reserved for possession). -1 (or any negative) is no ability
    public abstract int GetUsedAbility();

    // some abilities use hold and release, 
    public abstract int GetReleasedAbility();
}
