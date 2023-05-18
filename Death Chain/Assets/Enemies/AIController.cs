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
    public AIMode CurrentMode { get {
        if(target == null) {
            return untargetedMovement;
        } else {
            return targetingMovement;
        }
    }}

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

        if(CurrentMode == AIMode.Wander && currentDirection == Vector2.zero) {
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

        switch(CurrentMode) {
            case AIMode.Still:
                return Vector2.zero;

            case AIMode.Wander:
                return ModifyDirection(currentDirection);

            case AIMode.Chase:
                if(target == null) {
                    return Vector2.zero;
                } else {
                    return ApproachPosition(target.transform.position);
                }
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

// Private helper functions
    // returns a direction to move that tries to go straight to a location but moves around obstacles when necessary
    private Vector2 ApproachPosition(Vector2 targetPosition) {
        Vector2 idealDirection = (targetPosition - (Vector2)controlled.transform.position).normalized;
        if(idealDirection == Vector2.zero) {
            return idealDirection;
        }

        List<Rect> collisions = FindFutureCollisions(idealDirection);
        if(collisions.Count <= 0) {
            return idealDirection;
        }

        // find middle of all collided walls
        Vector2 collisionCenter = Vector2.zero;
        foreach(Rect collision in collisions) {
            collisionCenter += collision.center;
        }
        collisionCenter /= collisions.Count;

        // determine which circular direction gets to the target position faster
        Vector2 targetVec = targetPosition - collisionCenter;
        Vector2 currentVec = (Vector2)controlled.transform.position - collisionCenter;
        float targetAngle = Mathf.Atan2(targetVec.y, targetVec.x);
        float currentAngle = Mathf.Atan2(currentVec.y, currentVec.x);
        float clockwiseDelta = targetAngle - currentAngle;
        float counterDelta = currentAngle - targetAngle;
        if(clockwiseDelta < 0) {
            clockwiseDelta += 2 * Mathf.PI;
        }
        if(counterDelta < 0) {
            counterDelta += 2 * Mathf.PI;
        }

        Vector2 circularDirection = new Vector2(-idealDirection.y, idealDirection.x); // perpendicular counter-clockwise
        if(clockwiseDelta < counterDelta) {
            circularDirection *= -1; // perpendicular clockwise instead
        }

        circularDirection = ModifyDirection(circularDirection); // make sure it passes walls and doesn't walk into a pit

        return circularDirection;
    }

    // takes the character's desired direction and modifies it to avoid walls and pits. Works best when trying to move in one direction for a while
    private Vector2 ModifyDirection(Vector2 desiredDirection) {
        if(desiredDirection == Vector2.zero) {
            return desiredDirection;
        }

        float radius = controlled.GetComponent<Enemy>().CollisionRadius;
        List<Rect> overlaps = FindFutureCollisions(desiredDirection);
        if(overlaps.Count <= 0) {
            return desiredDirection;
        }

        bool horiBlocked = false;
        bool vertBlocked = false;
        Vector2 horiMid = (Vector2)controlled.transform.position + radius * new Vector2(desiredDirection.x, 0).normalized;
        Vector2 vertMid = (Vector2)controlled.transform.position + radius * new Vector2(0, desiredDirection.y).normalized;
        Rect horiCheck = new Rect(horiMid.x - radius, horiMid.y - radius, 2 * radius, 2 * radius);
        Rect vertCheck = new Rect(vertMid.x - radius, vertMid.y - radius, 2 * radius, 2 * radius);
        foreach(Rect area in overlaps) {
            if(area.Overlaps(horiCheck)) {
                horiBlocked = true;
            }
            if(area.Overlaps(vertCheck)) {
                vertBlocked = true;
            }
        }

        if(horiBlocked && vertBlocked) {
            // if walking into a corner, go backwards
            return -desiredDirection;
        }
        else if(horiBlocked) {
            desiredDirection.x = 0;
            return desiredDirection.normalized;
        }
        else if(vertBlocked) {
            desiredDirection.y = 0;
            return desiredDirection.normalized;
        }

        return desiredDirection;
    }

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
        if(CurrentMode == AIMode.Wander) {
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

    // finds the walls and pits this character would intersect if they moved in this direction
    private List<Rect> FindFutureCollisions(Vector2 direction) {
        float radius = controlled.GetComponent<Enemy>().CollisionRadius;
        Vector2 futureSpot = (Vector2)controlled.transform.position + radius * direction.normalized;
        Rect futureArea = new Rect(futureSpot.x - radius, futureSpot.y - radius, 2 * radius, 2 * radius);
        
        List<Rect> overlaps = new List<Rect>();
        foreach(GameObject wall in EntityTracker.Instance.Walls) {
            WallScript wallScript = wall.GetComponent<WallScript>();
            if(wallScript.Area.Overlaps(futureArea)) {
                overlaps.Add(wallScript.Area);
            }
        }

        if(!controlled.GetComponent<Enemy>().Floating) {
            foreach(PitScript pit in EntityTracker.Instance.Pits) {
                foreach(Rect zone in pit.Zones) {
                    if(zone.Overlaps(futureArea)) {
                        overlaps.Add(zone);
                    }
                }
            }
        }

        return overlaps;
    }
}
