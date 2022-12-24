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
    private AIMode moveMode;

    private Vector2 startPosition;
    private const float WANDER_RANGE = 5.0f; // how far enemies are allowed to wander from their starting point
    private float vision; // how far away targets can be seen

    private int queuedAbility = -1; // the attack to use after startup is done
    private float startup; // pause before using a move

    public GameObject Target { get { return target; } }
    public bool AbilityQueued { get { return queuedAbility >= 0; } }
    public bool IgnoreStart { get; set; } // allows an enemy to ignore their start location and travel freely

    public AIController(GameObject controlTarget, AIMode startMode, float vision) : base(controlTarget) {
        moveMode = startMode;
        this.vision = vision;
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
                    if(Vector3.Distance(controlled.transform.position, enemy.transform.position) <= vision) {
                        target = enemy;
                        break;
                    }
                }
            }
        }
        else if(GetTargetDistance() > GetTrackingVision()) { // determine if target is lost, greater vision when actively tracking
            target = null;
        }

        controlled.GetComponent<Enemy>().AIUpdate(this);

        if(startup > 0) {
            startup -= Time.deltaTime;
        }
    }
    
    public void QueueAbility(int ability, float startupDuration = 0) {
        if(startup > 0) {
            return; // prevent queue-ing another move
        }

        queuedAbility = ability;
        startup = startupDuration;
    }

    public void SetMovement(AIMode mode) {
        moveMode = mode;
    }

    public override Vector2 GetMoveDirection() {
        if(startup > 0) {
            return Vector2.zero; // stay still to indicate an oncoming attack
        }

        switch(moveMode) {
            case AIMode.Still:
                return Vector2.zero;

            case AIMode.Wander:
                return Vector2.zero;
        }

        return Vector2.zero;
    }

    public override int GetUsedAbility() {
        if(startup > 0) {
            return -1;
        }

        int usedAbility = queuedAbility;
        queuedAbility = -1;
        return usedAbility;
    }

    public override Vector2 GetAimDirection() {
        if(target != null) {
            return GetDirToTarget();
        }

        return Vector2.down;
    }

    public float GetTargetDistance() {
        if(target == null) {
            return -1f;
        }

        return Vector3.Distance(controlled.transform.position, target.transform.position);
    }

    // returns the unit vector towards the target, zero vector if no target
    private Vector2 GetDirToTarget() {
        if(target == null) {
            return Vector2.zero;
        }

        return (target.transform.position - controlled.transform.position).normalized;
    }

    // determines the vision range when currently targetting
    private float GetTrackingVision() {
        if(!IgnoreStart && Vector2.Distance(controlled.transform.position, startPosition) > WANDER_RANGE) {
            return vision / 2.0f;
        }

        return vision + 2.0f; 
    }
}
