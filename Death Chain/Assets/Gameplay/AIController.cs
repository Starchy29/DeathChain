using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIMode { // the way this character moves
    Still,
    Wander,
    Chase,
    Flee,
    Patrol
}

// Class that allows AI to control the enemies in the game
public class AIController : Controller
{
    private GameObject target; // the entity this is trying to attack
    private AIMode targetingMovement;
    private AIMode targetlessMovement;
    private readonly float vision; // how far away targets can be seen

    private const float WANDER_RANGE = 4.0f; // how far enemies are allowed to wander from their starting point
    private readonly Vector2 startPosition;

    private Vector2 currentDirection; // optional variable for movement modes that have certain paths
    private float travelTimer; // amount of time to travel in the current direction
    private bool clockwise; // for patrol mvovement
    private float projectileAlertTime; // time when this has a target after being shot at

    private Vector2 specialAim; // allows enemies to aim in specific directions
    private int queuedAbility = -1; // the attack to use after startup is done
    private bool paused; // temporarily stops this character's movements, i.e. startup or endlag from an attack
    private bool[] releasedAbilities; // specific enemies need to manually control their release mechanics 
    
    public GameObject Target { get { return target; } }
    public bool IgnoreStart { get; set; } // allows an enemy to ignore their start location and travel freely
    public float CurrentVision { get { 
        float result = vision;
        float distFromStart = Vector2.Distance(controlled.transform.position, startPosition);
        if(!IgnoreStart && distFromStart > WANDER_RANGE) {
            result *= (2 * WANDER_RANGE - distFromStart) / WANDER_RANGE; // decreased vision when outside the starting area
            result = Mathf.Max(0, result);
        } 
        if(target != null) {
            result += 3; // extra vision when tracking a target
        }
        return result;
    } }
    public AIMode CurrentMode { get {
        if(target == null) {
            return targetlessMovement;
        } else {
            return targetingMovement;
        }
    }}

    public AIController(GameObject controlTarget, AIMode targetingMovement, AIMode targetlessMovement, float visionRange) : base(controlTarget) {
        this.targetingMovement = targetingMovement;
        this.targetlessMovement = targetlessMovement;
        this.vision = visionRange;
        startPosition = controlTarget.transform.position;
        releasedAbilities = new bool[3];
    }

    public override void Update() {
        CheckTarget();

        if(!paused && queuedAbility < 0) {
            controlled.GetComponent<Enemy>().AIUpdate(this);
            ChooseMovement();
        }

        if(projectileAlertTime > 0) {
            float timeMultiplier = 1;
            if(!IgnoreStart && Vector2.Distance(controlled.transform.position, startPosition) > WANDER_RANGE) {
                timeMultiplier = 2.0f; // roam outside the wander range for less time when outside the wander radius
            }
            projectileAlertTime -= Time.deltaTime * timeMultiplier;
        }
    }

    #region Helper functions
    // either looks for a new target or checks if the current target is lost
    private void CheckTarget() {
        if(vision <= 0) {
            return;
        }

        if(target == null) {
            // check for a target
            List<GameObject> enemies = EntityTracker.Instance.GetComponent<EntityTracker>().Enemies;
            Enemy controlledScript = controlled.GetComponent<Enemy>();
            foreach(GameObject enemy in enemies) {
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if(enemyScript.IsAlly != controlledScript.IsAlly && !enemyScript.IsCorpse 
                    && Vector3.Distance(controlled.transform.position, enemy.transform.position) <= CurrentVision
                ) {
                    target = enemy;
                    projectileAlertTime = 0;
                    return;
                }
            }

            // check for an enemy projectile flying close
            const float MAX_PROJ_DISTANCE = 3.0f;
            GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
            foreach(GameObject projectile in projectiles) {
                Attack attackScript = projectile.GetComponent<Attack>();
                if(attackScript.User != null && attackScript.User.GetComponent<Enemy>().IsAlly != controlled.GetComponent<Enemy>().IsAlly &&
                    Vector2.Distance(projectile.transform.position, controlled.transform.position) <= MAX_PROJ_DISTANCE
                ) {
                    target = attackScript.User.gameObject;
                    projectileAlertTime = 5.0f;
                    return;
                }
            }
        } 
        else {
            // check if target is lost
            if(!target.activeInHierarchy || target.GetComponent<Enemy>().IsCorpse || (projectileAlertTime <= 0 && GetTargetDistance() > CurrentVision)) {
                if(paused) {
                    specialAim = CalcTargetDirection(); // if the player leaves the character's range when an attack is queued, attack at their last seen location
                }
                target = null;
            }
        }
    }

    // determine a direction to move for a span of time
    private void ChooseMovement() {
        if(CurrentMode == AIMode.Wander) {
            travelTimer -= Time.deltaTime * controlled.GetComponent<Enemy>().WalkSpeed / 4.0f; // factor in walk speed

            if(travelTimer > 0) {
                return;
            }

            // alternate between moving in a direction and pausing
            if(currentDirection == Vector2.zero) {
                travelTimer += 0.75f;

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
                travelTimer += 1.1f;
                currentDirection = Vector2.zero;
            }
        }
        else if(CurrentMode == AIMode.Patrol) {
            if(currentDirection == Vector2.zero) {
                currentDirection = new Vector2(0, -1);
            }

            // rotate the current direction
            const float ROT_PER_SEC = Mathf.PI;
            float rotationAmount = Time.deltaTime * ROT_PER_SEC * controlled.GetComponent<Enemy>().WalkSpeed / 4.0f; // turn more frequently if moving faster
            float newAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) + rotationAmount * (clockwise ? -1 : 1);
            currentDirection = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));

            travelTimer -= rotationAmount;
            if(travelTimer > 0) {
                return;
            }

            // in this case the travelTimer represents the amount to turn
            travelTimer = Random.Range(Mathf.PI / 4, Mathf.PI * 3/2);
            clockwise = !clockwise;
            
            // make sure it turns around if at the edge of the area
            if(!IgnoreStart && Vector2.Distance(controlled.transform.position, startPosition) > WANDER_RANGE * 0.5f) {
                Vector2 towardStart = startPosition - (Vector2)controlled.transform.position;
                float toStartAngle = Mathf.Atan2(towardStart.y, towardStart.x);

                // shift so the goal is PI. Makes it simpler
                float shift = Mathf.PI - toStartAngle;
                float currentAngle = newAngle + shift;
                if(currentAngle > 2*Mathf.PI) {
                    currentAngle -= 2*Mathf.PI;
                }
                else if(currentAngle < 0) {
                    currentAngle += 2*Mathf.PI;
                }
                float max = Mathf.PI + 70*Mathf.Deg2Rad;
                float min = Mathf.PI - 70*Mathf.Deg2Rad;

                if(clockwise) {
                    if(currentAngle < min) {
                        currentAngle += 2*Mathf.PI;
                    }

                    travelTimer = Random.Range(Mathf.Max(currentAngle - max, 0), currentAngle - min);
                } else {
                    if(currentAngle > max) {
                        currentAngle -= 2 * Mathf.PI;
                    }

                    travelTimer = Random.Range(Mathf.Max(min - currentAngle, 0), max - currentAngle);
                }
            }
        }
    }

    // takes the character's desired direction and modifies it to avoid walls and pits. Works best when trying to move in one direction for a while
    private Vector2 ModifyDirection(Vector2 desiredDirection) {
        if(desiredDirection == Vector2.zero) {
            return desiredDirection;
        }

        float radius = controlled.GetComponent<Enemy>().CollisionRadius;
        float checkDistance = radius * controlled.GetComponent<Enemy>().WalkSpeed / 4;
        Vector2 futureSpot = (Vector2)controlled.transform.position + checkDistance * desiredDirection.normalized;
        Rect futureArea = new Rect(futureSpot.x - radius, futureSpot.y - radius, 2 * radius, 2 * radius);
        
        List<Rect> overlaps = new List<Rect>();
        foreach(Rect wall in EntityTracker.Instance.RegularWallAreas) {
            if(wall.Overlaps(futureArea)) {
                overlaps.Add(wall);
            }
        }

        if(!controlled.GetComponent<Enemy>().Floating) {
            foreach(Rect pit in EntityTracker.Instance.PitAreas) {
                if(pit.Overlaps(futureArea)) {
                    overlaps.Add(pit);
                }
            }
        }

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

    // returns the unit vector towards the target, zero vector if no target
    private Vector2 CalcTargetDirection() {
        if(target == null) {
            return Vector2.zero;
        }

        return (target.transform.position - controlled.transform.position).normalized;
    }

    // finds the closest obstacle that blocks a straight path to the target, if any
    private Rect? FindBlocker(Vector2 targetLocation, bool checkPits) {
        float radius = controlled.GetComponent<Enemy>().CollisionRadius;
        List<Rect> obstacles = EntityTracker.Instance.RegularWallAreas;
        if(checkPits) {
            obstacles.AddRange(EntityTracker.Instance.PitAreas);
        }

        Vector2 currentPosition = controlled.transform.position;
        Vector2 idealMovement = targetLocation - currentPosition;
        Vector2 idealDirection = idealMovement.normalized;

        // find closest wall/pit that blocks the ideal movement
        float closestDistance = idealMovement.magnitude;
        Rect? closestBlock = null;
        foreach(Rect obstacle in obstacles) {
            // find if the path intersects this rectangle
            Vector2? edgePosition = null;
            float distance = 0;
            if(idealDirection.x != 0) {
                // check left or right side
                float targetX = idealDirection.x < 0 ? obstacle.xMax : obstacle.xMin;
                distance = (targetX - currentPosition.x) / idealDirection.x;
                float y = currentPosition.y + idealDirection.y * distance;

                if(distance > 0 && y >= obstacle.yMin - radius && y <= obstacle.yMax + radius) {
                    edgePosition = new Vector2(targetX, y);
                }
            }
            if(idealDirection.y != 0) {
                // check left or right side
                float targetY = idealDirection.y < 0 ? obstacle.yMax : obstacle.yMin;
                float newDistance = (targetY - currentPosition.y) / idealDirection.y;
                float x = currentPosition.x + idealDirection.x * newDistance;
                
                if(newDistance > 0 && x >= obstacle.xMin - radius && x <= obstacle.xMax + radius) {
                    if(!edgePosition.HasValue || newDistance > distance) {
                        edgePosition = new Vector2(x, targetY);
                        distance = newDistance;
                    }
                }
            }

            // determine if this collision is closest
            if(edgePosition.HasValue && distance < closestDistance) {
                closestDistance = distance;
                closestBlock = obstacle;
            }
        }

        return closestBlock;
    }
    #endregion

    #region Functions for Enemy class
    public void QueueAbility(int ability, float startup = 0, float endlag = 0) {
        if(startup > 0) {
            paused = true;
            Timer.CreateTimer(controlled, startup, false, () => {
                queuedAbility = ability;
                paused = false;
                if(endlag > 0) {
                    paused = true;
                    Timer.CreateTimer(controlled, endlag, false, () => {
                        paused = false;
                    });
                }
            });
        } else {
            queuedAbility = ability;
            if(endlag > 0) {
               paused = true;
                Timer.CreateTimer(controlled, endlag, false, () => {
                    paused = false;
                });
            } 
        }

        if(CurrentMode == AIMode.Wander && currentDirection == Vector2.zero) {
            // allow movement again right after endlag
            travelTimer = 0.0f;
        }
    }

    public void SetAbilityReleased(int ability, bool released) {
        releasedAbilities[ability] = released;
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

    // detects if the target is blocked by walls and maybe pits for selecting attacks. Accounts for having no target
    public bool IsTargetBlocked(bool checkPits) {
        return target != null && FindBlocker(target.transform.position, checkPits).HasValue;
    }
    #endregion

    #region Required controller functions
    public override Vector2 GetMoveDirection() {
        if(paused) {
            return Vector2.zero; // stay still to indicate an oncoming attack
        }
        
        switch(CurrentMode) {
            case AIMode.Still:
                if(Vector2.Distance(controlled.transform.position, startPosition) > WANDER_RANGE / 2) {
                    // return to start circle
                    return (startPosition - (Vector2)controlled.transform.position).normalized;
                }
                return Vector2.zero;

            case AIMode.Patrol:
            case AIMode.Wander:
                return ModifyDirection(currentDirection);

            case AIMode.Chase:
                if(target == null) {
                    return Vector2.zero;
                }
                Vector2 targetPosition = Approach(target.transform.position);
                return ModifyDirection((targetPosition - (Vector2)controlled.transform.position).normalized);

            case AIMode.Flee:
                if(controlled.transform.position == target.transform.position) {
                    return Vector2.zero;
                }

                Vector2 away = controlled.transform.position - target.transform.position;
                float distance = away.magnitude;
                away = (4 / distance) * away.normalized;
                Vector2 toCenter = (startPosition - (Vector2)controlled.transform.position).normalized;
                toCenter *= Vector2.Distance(startPosition, controlled.transform.position) / (WANDER_RANGE + 2);
                return ModifyDirection((away + toCenter).normalized);
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

    public override bool IsAbilityReleased(int ability) {
        return releasedAbilities[ability];
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
    #endregion

    // returns a position to move towards, navigating around obstacles as necessary
    private Vector2 Approach(Vector2 targetLocation) {
        // compile all rectangles that can block movement, expanded by the radius
        float radius = controlled.GetComponent<Enemy>().CollisionRadius;
        List<Rect> obstacles = EntityTracker.Instance.RegularWallAreas;
        if(!controlled.GetComponent<Enemy>().Floating) {
            obstacles.AddRange(EntityTracker.Instance.PitAreas);
        }

        Vector2 currentPosition = controlled.transform.position;
        Rect? closestBlock = FindBlocker(targetLocation, !controlled.GetComponent<Enemy>().Floating);
        if(!closestBlock.HasValue) {
            // straight path works
            return targetLocation;
        }

        // create a rectangle that encompasses the blocking wall and all its neighbors
        List<Rect> fullObstacle = new List<Rect>();
        Queue<Rect> addedBlocks = new Queue<Rect>();
        addedBlocks.Enqueue(closestBlock.Value);

        while(addedBlocks.Count > 0) {
            Rect addition = addedBlocks.Dequeue();
            fullObstacle.Add(addition);

            // find all adjacent blocks
            foreach(Rect obstacle in obstacles) {
                if(!addedBlocks.Contains(obstacle) && !fullObstacle.Contains(obstacle) && obstacle.MakeExpanded(radius).Overlaps(addition.MakeExpanded(radius))) {
                    addedBlocks.Enqueue(obstacle);
                }
            }
        }

        Rect obstacleSurrounder = closestBlock.Value;
        foreach(Rect portion in fullObstacle) {
            if(portion.xMin < obstacleSurrounder.xMin) {
                obstacleSurrounder.xMin = portion.xMin;
            }
            if(portion.xMax > obstacleSurrounder.xMax) {
                obstacleSurrounder.xMax = portion.xMax;
            }
            if(portion.yMin < obstacleSurrounder.yMin) {
                obstacleSurrounder.yMin = portion.yMin;
            }
            if(portion.yMax > obstacleSurrounder.yMax) {
                obstacleSurrounder.yMax = portion.yMax;
            }
        }

        // find the spots on the surrounding rectangle that the character and the target are closest to
        Vector2? FindEdgeSpot(Vector2 start) {
            Vector2 closestEdgeSpot = start;
            closestEdgeSpot.x = Mathf.Min(closestEdgeSpot.x, obstacleSurrounder.xMax);
            closestEdgeSpot.x = Mathf.Max(closestEdgeSpot.x, obstacleSurrounder.xMin);
            closestEdgeSpot.y = Mathf.Min(closestEdgeSpot.y, obstacleSurrounder.yMax);
            closestEdgeSpot.y = Mathf.Max(closestEdgeSpot.y, obstacleSurrounder.yMin);

            if(closestEdgeSpot != start) {
                // outside rectangle
                return closestEdgeSpot;
            }

            // inside rectangle
            bool blockedUp = false;
            bool blockedDown = false;
            bool blockedLeft = false;
            bool blockedRight = false;
            foreach(Rect blocker in fullObstacle) {
                if(start.x > blocker.xMin && start.x < blocker.xMax) {
                    if(blocker.center.y > start.y) {
                        blockedUp = true;
                    } else {
                        blockedDown = true;
                    }
                }

                if(start.y > blocker.yMin && start.y < blocker.yMax) {
                    if(blocker.center.x > start.x) {
                        blockedRight = true;
                    } else {
                        blockedLeft = true;
                    }
                }
            }

            List<Vector2> potentialSpots = new List<Vector2>();
            if(!blockedUp) potentialSpots.Add(new Vector2(start.x, obstacleSurrounder.yMax));
            if(!blockedDown) potentialSpots.Add(new Vector2(start.x, obstacleSurrounder.yMin));
            if(!blockedRight) potentialSpots.Add(new Vector2(obstacleSurrounder.xMax, start.y));
            if(!blockedLeft) potentialSpots.Add(new Vector2(obstacleSurrounder.xMin, start.y));

            if(potentialSpots.Count <= 0) {
                // cannot find a good valid edge spot
                return null;
            }

            potentialSpots.Sort((Vector2 current, Vector2 next) => {
                return Vector2.Distance(start, current) < Vector2.Distance(start, next) ? -1 : 1;
            });
            return potentialSpots[0];
        }

        Vector2? maybeChar = FindEdgeSpot(currentPosition);
        Vector2? maybeTarg = FindEdgeSpot(targetLocation);
        if(!maybeChar.HasValue || !maybeTarg.HasValue) {
            // give up and use direct path if no alternate route can be found
            return targetLocation;
        }

        Vector2 characterEdgeSpot = maybeChar.Value;
        Vector2 targetEdgeSpot = maybeTarg.Value;

        // find which ways around the obstacle would be blocked by a border
        List<Vector2> borderSpots = new List<Vector2>();
        foreach(Rect border in EntityTracker.Instance.BorderAreas) {
            Rect expandedBorder = border.MakeExpanded(2 * radius);
            if(expandedBorder.Overlaps(obstacleSurrounder)) {
                // find where the overlap exists by checking individual pieces of the obstacle
                foreach(Rect obstacle in fullObstacle) {
                    if(expandedBorder.Overlaps(obstacle)) {
                        // find the overlap area and use the middle to determine where this border blocks the obstacle
                        float top = Mathf.Min(obstacle.yMax, expandedBorder.yMax);
                        float bottom = Mathf.Max(obstacle.yMin, expandedBorder.yMin);
                        float left = Mathf.Max(obstacle.xMin, expandedBorder.xMin);
                        float right = Mathf.Min(obstacle.xMax, expandedBorder.xMax);
                        Rect overlapArea = new Rect(left, bottom, right - left, top - bottom);
                        borderSpots.Add(FindEdgeSpot(overlapArea.center).Value);
                        break;
                    }
                }
            }
        }

        if(borderSpots.Count > 1) {
            // test if both directions are blocked
            Vector2 startToEnd = targetEdgeSpot - characterEdgeSpot;
            Vector2 perp = new Vector2(-startToEnd.y, startToEnd.x);
            Vector2 startToBorder = borderSpots[0] - characterEdgeSpot;
            if(Vector2.Dot(startToBorder, perp) < 0) {
                perp = -perp;
            }
            for(int i = 1; i < borderSpots.Count; i++) {
                Vector2 startToNextBorder = borderSpots[i] - characterEdgeSpot;
                if(Vector2.Dot(startToNextBorder, perp) <= 0) {
                    // no valid path because blocked in both directions
                    return targetLocation;
                }
            }
        }
        if(borderSpots.Count > 0) { // if multiple borders, but there is a valid direction, use the first to represent them all
            List<Vector2> corners = new List<Vector2>() { // clockwise starting at bottom left
                new Vector2(obstacleSurrounder.xMin, obstacleSurrounder.yMin),
                new Vector2(obstacleSurrounder.xMin, obstacleSurrounder.yMax),
                new Vector2(obstacleSurrounder.xMax, obstacleSurrounder.yMax),
                new Vector2(obstacleSurrounder.xMax, obstacleSurrounder.yMin)
            };

            // determine which two corners are the potential targets
            int clockCorner = 0;
            int counterCorner = 0;
            if(corners.Contains(characterEdgeSpot)) {
                clockCorner = corners.IndexOf(characterEdgeSpot);
                clockCorner++;
                if(clockCorner > corners.Count - 1) {
                    clockCorner = 0;
                }
                counterCorner = clockCorner - 2;
                if(counterCorner < 0) {
                    counterCorner += corners.Count;
                }
            }
            else if(characterEdgeSpot.x == obstacleSurrounder.xMin) {
                clockCorner = 1;
                counterCorner = 0;
            }
            else if(characterEdgeSpot.y == obstacleSurrounder.yMax) {
                clockCorner = 2;
                counterCorner = 1;
            }
            else if(characterEdgeSpot.x == obstacleSurrounder.xMax) {
                clockCorner = 3;
                counterCorner = 2;
            }
            else {
                clockCorner = 0;
                counterCorner = 3;
            }

            // see if going clockwise gets to the target
            bool blocked = false;
            bool foundCorner = false;

            bool startBlocked = borderSpots[0].IsBetween(corners[clockCorner], characterEdgeSpot);
            bool startTarget = targetEdgeSpot.IsBetween(corners[clockCorner], characterEdgeSpot);
            if(startBlocked && startTarget) {
                if(Vector2.Distance(characterEdgeSpot, borderSpots[0]) > Vector2.Distance(characterEdgeSpot, targetEdgeSpot)) {
                    foundCorner = true;
                } else {
                    blocked = true;
                }
            }
            else if(startTarget) {
                foundCorner = true;
            }
            else if(startBlocked) {
                blocked = true;
            }

            int currentCorner = clockCorner;
            while(!blocked && !foundCorner) {
                int nextCorner = currentCorner + 1;
                if(nextCorner >= corners.Count) {
                    nextCorner = 0;
                }
                bool foundTarget = targetEdgeSpot.IsBetween(corners[currentCorner], corners[nextCorner]);
                bool foundBlocker = borderSpots[0].IsBetween(corners[currentCorner], corners[nextCorner]);
                if(foundBlocker && foundTarget) {
                    if(Vector2.Distance(corners[currentCorner], borderSpots[0]) > Vector2.Distance(corners[currentCorner], targetEdgeSpot)) {
                        foundCorner = true;
                    } else {
                        blocked = true;
                    }
                }
                else if(foundTarget) {
                    foundCorner = true;
                }
                else if(foundBlocker) {
                    blocked = true;
                }

                currentCorner = nextCorner;
            }

            // otherwise go counter clockwise instead
            Vector2 targetCorner = corners[clockCorner];
            if(!foundCorner) {
                targetCorner = corners[counterCorner];
            }
            targetCorner.x += targetCorner.x > obstacleSurrounder.center.x ? radius : -radius;
            targetCorner.y += targetCorner.y > obstacleSurrounder.center.y ? radius : -radius;
            return targetCorner;
        }

        // determine which way around the obstacle is shorter
        if (characterEdgeSpot.x == targetEdgeSpot.x || characterEdgeSpot.y == targetEdgeSpot.y) {
            // on the same side, approach the corner of that side
            Vector2 targetCorner = characterEdgeSpot;
            if(characterEdgeSpot.x == targetEdgeSpot.x) {
                targetCorner.y = targetEdgeSpot.y > characterEdgeSpot.y ? obstacleSurrounder.yMax : obstacleSurrounder.yMin;
                targetCorner.x += (characterEdgeSpot.x > obstacleSurrounder.center.x ? radius : -radius);
            }
            else if(characterEdgeSpot.y == targetEdgeSpot.y) {
                targetCorner.x = targetEdgeSpot.x > characterEdgeSpot.x ? obstacleSurrounder.xMax : obstacleSurrounder.xMin;
                targetCorner.y += (characterEdgeSpot.y > obstacleSurrounder.center.y ? radius : -radius);
            }
            return targetCorner;
        }
        else if(characterEdgeSpot.x == obstacleSurrounder.xMin && targetEdgeSpot.x == obstacleSurrounder.xMax
            || targetEdgeSpot.x == obstacleSurrounder.xMin && characterEdgeSpot.x == obstacleSurrounder.xMax
        ) {
            // opposite sides horizontal
            Vector2 targetSideMiddle = obstacleSurrounder.center;
            float upDist = (obstacleSurrounder.yMax - characterEdgeSpot.y) + (obstacleSurrounder.yMax - targetEdgeSpot.y);
            float downDist = (characterEdgeSpot.y - obstacleSurrounder.yMin) + (targetEdgeSpot.y - obstacleSurrounder.yMin);
            targetSideMiddle.y = upDist > downDist ? obstacleSurrounder.yMin - radius : obstacleSurrounder.yMax + radius;

            foreach(Rect obstacle in fullObstacle) {
                if(upDist > downDist && obstacle.yMax < currentPosition.y || downDist > upDist && obstacle.yMin > currentPosition.y) {
                    if(characterEdgeSpot.x == obstacleSurrounder.xMin && obstacle.xMin < targetSideMiddle.x) {
                        targetSideMiddle.x = obstacle.xMin - radius;
                    } 
                    else if(characterEdgeSpot.x == obstacleSurrounder.xMax && obstacle.xMax > targetSideMiddle.x) {
                        targetSideMiddle.x = obstacle.xMax + radius;
                    }
                }
            }
            return targetSideMiddle;
        }
        else if(characterEdgeSpot.y == obstacleSurrounder.yMin && targetEdgeSpot.y == obstacleSurrounder.yMax
            || targetEdgeSpot.y == obstacleSurrounder.yMin && characterEdgeSpot.y == obstacleSurrounder.yMax
        ) {
            // opposite sides vertical
            Vector2 targetSideMiddle = obstacleSurrounder.center;
            float rightDist = (obstacleSurrounder.xMax - characterEdgeSpot.x) + (obstacleSurrounder.xMax - targetEdgeSpot.x);
            float leftDist = (characterEdgeSpot.x - obstacleSurrounder.xMin) + (targetEdgeSpot.x - obstacleSurrounder.xMin);
            targetSideMiddle.x = rightDist > leftDist ? obstacleSurrounder.xMin - radius : obstacleSurrounder.xMax + radius;

            foreach(Rect obstacle in fullObstacle) {
                if(rightDist > leftDist && obstacle.xMax < currentPosition.x || leftDist > rightDist && obstacle.xMin > currentPosition.x) {
                    if(characterEdgeSpot.y == obstacleSurrounder.yMin && obstacle.yMin < targetSideMiddle.y) {
                        targetSideMiddle.y = obstacle.yMin - radius;
                    } 
                    else if(characterEdgeSpot.y == obstacleSurrounder.yMax && obstacle.yMax > targetSideMiddle.y) {
                        targetSideMiddle.y = obstacle.yMax + radius;
                    }
                }
            }
            return targetSideMiddle;
        }
        else {
            // just around the corner
            Vector2 targetCorner = new Vector2();
            if(characterEdgeSpot.x == obstacleSurrounder.xMin || characterEdgeSpot.x == obstacleSurrounder.xMax) {
                targetCorner = new Vector2(characterEdgeSpot.x, targetEdgeSpot.y);
            } else {
                targetCorner = new Vector2(targetEdgeSpot.x, characterEdgeSpot.y);
            }
            targetCorner.x += (characterEdgeSpot.x > obstacleSurrounder.center.x ? radius : -radius);
            targetCorner.y += (characterEdgeSpot.y > obstacleSurrounder.center.y ? radius : -radius);
            return targetCorner;
        }
    }
}