using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIMode { // the way this character moves
    Still,
    Wander,
    Chase,
    Flee,
    //Duel
}

// Class that allows AI to control the enemies in the game
public class AIController : Controller
{
    private GameObject target; // the entity this is trying to attack
    private AIMode targetingMovement;
    private AIMode untargetedMovement;

    private const float WANDER_RANGE = 4.0f; // how far enemies are allowed to wander from their starting point
    private readonly float vision; // how far away targets can be seen
    private readonly Vector2 startPosition;

    private Vector2 currentDirection; // optional variable for movement modes that have certain paths
    private float travelTimer; // amount of time to travel in the current direction

    private Vector2 specialAim; // allows enemies to aim in specific directions
    private int queuedAbility = -1; // the attack to use after startup is done
    private bool paused; // temporarily stops this character's movements, i.e. startup or endlag from an attack
   
    public int ReleaseAbility { get; set; } // specific enemies need to manually control their release mechanics 
    public GameObject Target { get { return target; } }
    public bool IgnoreStart { get; set; } // allows an enemy to ignore their start location and travel freely

    public AIController(GameObject controlTarget, AIMode targetingMovement, AIMode untargetedMovement, float visionRange) : base(controlTarget) {
        this.targetingMovement = targetingMovement;
        this.untargetedMovement = untargetedMovement;
        this.vision = visionRange;
        startPosition = controlTarget.transform.position;
    }

    public override void Update() {
        CheckVision();

        if(!paused) {
            controlled.GetComponent<Enemy>().AIUpdate(this);
            ChooseMovement();
        }
    }
    
// Functions for Enemy class
    public void QueueAbility(int ability, float startup = 0, float endlag = 0) {
        if(startup > 0) {
            paused = true;
            Timer.CreateTimer(startup, false, () => {
                queuedAbility = ability;
                paused = false;
                if(endlag > 0) {
                    paused = true;
                    Timer.CreateTimer(endlag, false, () => {
                        paused = false;
                    });
                }
            });
        } else {
            queuedAbility = ability;
            if(endlag > 0) {
                paused = true;
                Timer.CreateTimer(endlag, false, () => {
                    paused = false;
                });
            }
        }

        if(DetermineCurrentMode() == AIMode.Wander && currentDirection == Vector2.zero) {
            // allow movement again right after endlag
            travelTimer = 0.0f;
        }
    }

    // override the automatic aim towards the target for something else
    public void SetAim(Vector2 direction) {
        specialAim = direction.normalized;
    }

    public float GetTargetDistance() {
        if(target == null) {
            return -1f;
        }

        return Vector3.Distance(controlled.transform.position, target.transform.position);
    }

// Required controller functions
    public override Vector2 GetMoveDirection() {
        if(paused) {
            return Vector2.zero; // stay still to indicate an oncoming attack
        }

        switch(DetermineCurrentMode()) {
            case AIMode.Still:
                return Vector2.zero;

            case AIMode.Wander:
                return currentDirection;

            case AIMode.Chase:
                if(target == null) {
                    return Vector2.zero;
                }
                return CalcTargetDirection();
        }

        return Vector2.zero;
    }

    public override bool AbilityUsed(int ability) {
        if(ability == queuedAbility) {
            queuedAbility = -1;
            return true;
        }

        return false;
    }

    public override int GetReleasedAbility() {
        return ReleaseAbility;
    }

    public override Vector2 GetAimDirection() {
        if(specialAim != Vector2.zero) {
            Vector2 result = specialAim;
            specialAim = Vector2.zero;
            return result;
        }

        if(target != null) {
            return CalcTargetDirection();
        }

        Vector2 move = GetMoveDirection();
        if(move != Vector2.zero) {
            return move;
        }

        return Vector2.down;
    }

// Private helper functiosn
    private void CheckVision() {
        if(vision <= 0) {
            return;
        }

        if(target == null) {
            // check for a target
            List<GameObject> enemies = EntityTracker.Instance.GetComponent<EntityTracker>().Enemies;
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
        } else {
            // determine if target is lost
            const float TRACK_RANGE_BOOST = 2.0f;
            float currentVision = vision;
            if(!IgnoreStart && Vector2.Distance(controlled.transform.position, startPosition) > WANDER_RANGE) {
                currentVision /= 2;
            } else {
                currentVision += TRACK_RANGE_BOOST;
            }

            if(!target.activeInHierarchy || GetTargetDistance() > currentVision) {
                target = null;
            }
        }
    }

    private void ChooseMovement() {
        if(DetermineCurrentMode() == AIMode.Wander) {
            travelTimer -= Time.deltaTime;

            if(travelTimer <= 0) {
                // alternate between moving in a direction and pausing
                if(currentDirection == Vector2.zero) {
                    travelTimer += 1.0f;

                    // pick a new direction
                    Vector2 random = Random.insideUnitCircle.normalized * WANDER_RANGE / 2;
                    random += -currentDirection * 0.5f; // weight it away from the current direction
                    if(!IgnoreStart) {
                        // weight random direction towards starting position, not normalized to be weighted more when further away
                        random += startPosition - new Vector2(controlled.transform.position.x, controlled.transform.position.y);
                    }

                    currentDirection = random.normalized;
                } else {
                    // stay still for a bit
                    travelTimer += 0.7f;
                    currentDirection = Vector2.zero;
                }

                travelTimer *= 4.0f / controlled.GetComponent<Enemy>().WalkSpeed; // factor in walk speed
            }
        }
    }

    // returns the unit vector towards the target, zero vector if no target
    private Vector2 CalcTargetDirection() {
        if(target == null) {
            return Vector2.zero;
        }

        return (target.transform.position - controlled.transform.position).normalized;
    }

    private AIMode DetermineCurrentMode() {
        if(target == null) {
            return untargetedMovement;
        } else {
            return targetingMovement;
        }
    }
}
