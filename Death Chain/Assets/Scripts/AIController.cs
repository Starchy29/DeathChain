using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIMode { // the way this character moves
    Still,
    Wander,
    Patrol,
    Chase,
    Flee
}

// Class that allows AI to control the enemies in the game
public class AIController : Controller
{
    private GameObject target; // the entity this is trying to attack
    private AIMode targetMode; // how this moves when it has a target
    private AIMode idleMode; // how this moves when it does not have a target

    private Vector2 startPosition;
    private const float RANGE = 5.0f; // how far enemies are allowed to wander from their starting point

    public bool IgnoreStart { get; set; } // allows an enemy to ignore their start location

    public AIController(GameObject controlTarget, AIMode idleMode, AIMode targetMode) : base(controlTarget) {
        this.targetMode = targetMode;
        this.idleMode = idleMode;
        startPosition = controlTarget.transform.position;
    }

    public override void Update() {
        if(target == null) {
            // check for a target
            List<GameObject> enemies = GameObject.Find("EntityTracker").GetComponent<EntityTracker>().Enemies;
            Enemy controlledScript = controlled.GetComponent<Enemy>();
            foreach(GameObject enemy in enemies) {
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if(enemyScript.IsAlly != controlledScript.IsAlly) {

                }
            }
        } else {
            // determine if target is lost
        }
    }

    public override Vector2 GetMoveDirection() {
        switch(target == null ? idleMode : targetMode) {
            case AIMode.Still:
                return Vector2.zero;

            case AIMode.Wander:
                return Vector2.zero;
        }

        return new Vector2();
    }

    public override int GetUsedAbility() {
        return -1; // not implemented yet
    }

    public override Vector2 GetAimDirection() {
        if(target != null) {
            return GetDirToTarget();
        }

        return Vector2.down;
    }

    // returns the unit vector towards the target
    private Vector2 GetDirToTarget() {
        if(target == null) {
            return Vector2.zero;
        }

        return (target.transform.position - controlled.transform.position).normalized;
    }
}
