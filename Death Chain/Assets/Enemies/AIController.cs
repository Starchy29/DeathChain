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
    private AIMode targetlessMovement;

    private const float WANDER_RANGE = 4.0f; // how far enemies are allowed to wander from their starting point
    private readonly float vision; // how far away targets can be seen
    private readonly Vector2 startPosition;

    private Vector2 movementValue; // optional variable for movement modes that have certain paths
    private float travelTimer; // amount of time to travel in the current direction

    private Vector2 specialAim; // allows enemies to aim in specific directions
    private int queuedAbility = -1; // the attack to use after startup is done
    private bool paused; // temporarily stops this character's movements, i.e. startup or endlag from an attack
   
    public int ReleaseAbility { get; set; } // specific enemies need to manually control their release mechanics 
    public GameObject Target { get { return target; } }
    public bool IgnoreStart { get; set; } // allows an enemy to ignore their start location and travel freely
    public float CurrentVision { get { 
        if(!IgnoreStart && Vector2.Distance(controlled.transform.position, startPosition) > WANDER_RANGE) {
            return vision / 2; // half vision outisde the starting position
        } 
        else if(target != null) {
            return vision + 2; // extra vision when tracking a target
        }
        else {
            return vision;
        }
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

        if(CurrentMode == AIMode.Wander && movementValue == Vector2.zero) {
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
                return ModifyDirection(movementValue);

            case AIMode.Chase:
                return ModifyDirection((movementValue - (Vector2)controlled.transform.position).normalized);
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
                if(enemyScript.IsAlly != controlledScript.IsAlly && !enemyScript.IsCorpse) {
                    if(Vector3.Distance(controlled.transform.position, enemy.transform.position) <= CurrentVision) {
                        target = enemy;
                        break;
                    }
                }
            }
        } 
        // check if target is lost
        else if(!target.activeInHierarchy || target.GetComponent<Enemy>().IsCorpse || GetTargetDistance() > CurrentVision) {
            target = null;
        }
    }

    // determine a direction to move for a span of time
    private void ChooseMovement() {
        if(CurrentMode == AIMode.Wander) {
            // use movementValue as current direction
            travelTimer -= Time.deltaTime;

            if(travelTimer <= 0) {
                // alternate between moving in a direction and pausing
                if(movementValue == Vector2.zero) {
                    travelTimer += 1.0f;

                    // pick a new direction
                    Vector2 random = Random.insideUnitCircle.normalized * WANDER_RANGE / 2;
                    random += -movementValue * 0.5f; // weight it away from the current direction
                    if(!IgnoreStart) {
                        // weight random direction towards starting position, not normalized to be weighted more when further away
                        random += startPosition - new Vector2(controlled.transform.position.x, controlled.transform.position.y);
                    }

                    movementValue = random.normalized;
                } else {
                    // stay still for a bit
                    travelTimer += 0.7f;
                    movementValue = Vector2.zero;
                }

                travelTimer *= 4.0f / controlled.GetComponent<Enemy>().WalkSpeed; // factor in walk speed
            }
        }
        else if(CurrentMode == AIMode.Chase) {
            travelTimer -= Time.deltaTime;
            if(travelTimer <= 0) {
                travelTimer += 0.0f;
                if(target == null) {
                    movementValue = Vector2.zero;
                } else {
                    movementValue = NewApproach(target.transform.position);
                }
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

    // returns a direction that approaches the targetLocation, avoiding walls and pits as necessary
    //private Vector2 Approach(Vector2 targetLocation) {
    //    float radius = controlled.GetComponent<Enemy>().CollisionRadius;

    //    List<Rect> obstacles = new List<Rect>();
    //    foreach(GameObject wall in EntityTracker.Instance.Walls) {
    //        obstacles.Add(wall.GetComponent<WallScript>().Area);
    //    }
    //    if(!controlled.GetComponent<Enemy>().Floating) {
    //        foreach(PitScript pit in EntityTracker.Instance.Pits) {
    //            foreach(Rect area in pit.Zones) {
    //                obstacles.Add(area);
    //            }
    //        }
    //    }

    //    Vector2 startPosition = controlled.transform.position;
    //    Vector2 idealMovement = targetLocation - startPosition;
    //    Vector2 idealDirection = idealMovement.normalized;

    //    // find closest wall/pit that blocks the ideal movement
    //    float closestDistance = idealMovement.magnitude;
    //    Rect? closestBlock = null;
    //    Direction blockedSide = Direction.None;
    //    foreach(Rect obstacle in obstacles) {
    //        // find if the path intersects this rectangle
    //        Vector2? edgePosition = null;
    //        float distance = 0;
    //        if(idealDirection.x != 0) {
    //            // check left or right side
    //            float targetX = idealDirection.x < 0 ? obstacle.xMax : obstacle.xMin;
    //            distance = (targetX - startPosition.x) / idealDirection.x;
    //            float y = startPosition.y + idealDirection.y * distance;

    //            if(distance > 0 && y >= obstacle.yMin - radius && y <= obstacle.yMax + radius) {
    //                edgePosition = new Vector2(targetX, y);
    //                blockedSide = idealDirection.x < 0 ? Direction.Right : Direction.Left;
    //            }
    //        }
    //        if(idealDirection.y != 0) {
    //            // check left or right side
    //            float targetY = idealDirection.y < 0 ? obstacle.yMax : obstacle.yMin;
    //            float newDistance = (targetY - startPosition.y) / idealDirection.y;
    //            float x = startPosition.x + idealDirection.x * newDistance;
                
    //            if(newDistance > 0 && x >= obstacle.xMin - radius && x <= obstacle.xMax + radius) {
    //                if(!edgePosition.HasValue || newDistance > distance) {
    //                    edgePosition = new Vector2(x, targetY);
    //                    distance = newDistance;
    //                    blockedSide = idealDirection.y < 0 ? Direction.Up : Direction.Down;
    //                }
    //            }
    //        }

    //        // determine if this collision is closest
    //        if(edgePosition.HasValue && distance < closestDistance) {
    //            closestDistance = distance;
    //            closestBlock = obstacle;
    //        }
    //    }

    //    if(!closestBlock.HasValue) {
    //        // straight path works
    //        return idealDirection;
    //    }

    //    // find which way to go around this obstacle
    //    Rect blocker = closestBlock.Value;
    //    Vector2 topLeft = new Vector2(blocker.xMin - radius, blocker.yMax + radius);
    //    Vector2 bottomLeft = new Vector2(blocker.xMin - radius, blocker.yMin - radius);
    //    Vector2 topRight = new Vector2(blocker.xMax + radius, blocker.yMax + radius);
    //    Vector2 bottomRight = new Vector2(blocker.xMax + radius, blocker.yMin - radius);

    //    Dictionary<Corner, Vector2> cornerPositions = new Dictionary<Corner, Vector2>() {
    //        { new Corner(Direction.Left, Direction.Up), topLeft },
    //        { new Corner(Direction.Left, Direction.Down), bottomLeft },
    //        { new Corner(Direction.Right, Direction.Up), topRight },
    //        { new Corner(Direction.Right, Direction.Down), bottomRight }
    //    };

    //    Direction horizontalAccess = Direction.None;
    //    if(targetLocation.x < blocker.xMin) {
    //        horizontalAccess = Direction.Left;
    //    }
    //    else if(targetLocation.x > blocker.xMax) {
    //        horizontalAccess = Direction.Right;
    //    }

    //    Direction verticalAccess = Direction.None;
    //    if(targetLocation.y < blocker.yMin) {
    //        verticalAccess = Direction.Down;
    //    }
    //    else if(targetLocation.y > blocker.yMax) {
    //        verticalAccess = Direction.Up;
    //    }

    //    if(horizontalAccess == Direction.None && verticalAccess == Direction.None) {
    //        return Vector2.zero;
    //    }

    //    List<Vector2> clockwisePath = new List<Vector2>() { startPosition };
    //    List<Vector2> counterPath = new List<Vector2>() { startPosition };

    //    List<Corner> clockwiseCorners = new List<Corner>();
    //    List<Corner> counterCorners = new List<Corner>();
    //    switch(blockedSide) {
    //        case Direction.Up:
    //            clockwiseCorners.Add(new Corner(Direction.Right, Direction.Up));
    //            counterCorners.Add(new Corner(Direction.Left, Direction.Up));
    //            break;

    //        case Direction.Down:
    //            clockwiseCorners.Add(new Corner(Direction.Left, Direction.Down));
    //            counterCorners.Add(new Corner(Direction.Right, Direction.Down));
    //            break;

    //        case Direction.Left:
    //            clockwiseCorners.Add(new Corner(Direction.Left, Direction.Up));
    //            counterCorners.Add(new Corner(Direction.Left, Direction.Down));
    //            break;

    //        case Direction.Right:
    //            clockwiseCorners.Add(new Corner(Direction.Right, Direction.Down));
    //            counterCorners.Add(new Corner(Direction.Right, Direction.Up));
    //            break;
    //    }

    //    while(clockwiseCorners[clockwiseCorners.Count - 1].Horizontal != horizontalAccess 
    //        && clockwiseCorners[clockwiseCorners.Count - 1].Vertical != verticalAccess
    //    ) {
    //        clockwiseCorners.Add(clockwiseCorners[clockwiseCorners.Count - 1].GetClockwise());
    //    }

    //    while(counterCorners[counterCorners.Count - 1].Horizontal != horizontalAccess 
    //        && counterCorners[counterCorners.Count - 1].Vertical != verticalAccess
    //    ) {
    //        counterCorners.Add(counterCorners[counterCorners.Count - 1].GetCounterClockwise());
    //    }

    //    foreach(Corner corner in clockwiseCorners) {
    //        clockwisePath.Add(cornerPositions[corner]);
    //    }

    //    foreach(Corner corner in counterCorners) {
    //        counterPath.Add(cornerPositions[corner]);
    //    }

    //    clockwisePath.Add(targetLocation);
    //    counterPath.Add(targetLocation);

    //    List<Vector2> chosenPath = CalcPathDistance(clockwisePath) < CalcPathDistance(counterPath) ? clockwisePath : counterPath;

    //    if(Vector2.Dot(chosenPath[1] - chosenPath[0], chosenPath[2] - chosenPath[0]) < 0) {
    //        chosenPath.RemoveAt(1);
    //    }

    //    return (chosenPath[1] - startPosition).normalized;
    //}
    
    // takes a list of points where the first and last elements are the beginning and end of a path and returns the distance travelling one point to the next
    private float CalcPathDistance(List<Vector2> path) {
        float distance = 0;
        for(int i = 0; i < path.Count - 1; i++) {
            distance += Vector2.Distance(path[i], path[i+1]);
        }
        return distance;
    }

    // returns a position to move towards, navigating around obstacles as necessary
    private Vector2 NewApproach(Vector2 targetLocation) {
        // compile all rectangles that can block movement, expanded by the radius
        float radius = controlled.GetComponent<Enemy>().CollisionRadius;
        List<Rect> obstacles = new List<Rect>();
        foreach(GameObject wall in EntityTracker.Instance.Walls) {
            obstacles.Add(wall.GetComponent<WallScript>().Area);
        }
        if(!controlled.GetComponent<Enemy>().Floating) {
            foreach(PitScript pit in EntityTracker.Instance.Pits) {
                foreach(Rect area in pit.Zones) {
                    obstacles.Add(area);
                }
            }
        }

        Vector2 startPosition = controlled.transform.position;
        Vector2 idealMovement = targetLocation - startPosition;
        Vector2 idealDirection = idealMovement.normalized;

        // find closest wall/pit that blocks the ideal movement
        float closestDistance = idealMovement.magnitude;
        Rect? closestBlock = null;
        //Direction blockedSide = Direction.None;
        foreach(Rect obstacle in obstacles) {
            // find if the path intersects this rectangle
            Vector2? edgePosition = null;
            float distance = 0;
            if(idealDirection.x != 0) {
                // check left or right side
                float targetX = idealDirection.x < 0 ? obstacle.xMax : obstacle.xMin;
                distance = (targetX - startPosition.x) / idealDirection.x;
                float y = startPosition.y + idealDirection.y * distance;

                if(distance > 0 && y >= obstacle.yMin - radius && y <= obstacle.yMax + radius) {
                    edgePosition = new Vector2(targetX, y);
                    //blockedSide = idealDirection.x < 0 ? Direction.Right : Direction.Left;
                }
            }
            if(idealDirection.y != 0) {
                // check left or right side
                float targetY = idealDirection.y < 0 ? obstacle.yMax : obstacle.yMin;
                float newDistance = (targetY - startPosition.y) / idealDirection.y;
                float x = startPosition.x + idealDirection.x * newDistance;
                
                if(newDistance > 0 && x >= obstacle.xMin - radius && x <= obstacle.xMax + radius) {
                    if(!edgePosition.HasValue || newDistance > distance) {
                        edgePosition = new Vector2(x, targetY);
                        distance = newDistance;
                        //blockedSide = idealDirection.y < 0 ? Direction.Up : Direction.Down;
                    }
                }
            }

            // determine if this collision is closest
            if(edgePosition.HasValue && distance < closestDistance) {
                closestDistance = distance;
                closestBlock = obstacle;
            }
        }

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
        Vector2 FindEdgeSpot(Vector2 start) {
            Vector2 closestEdgeSpot = start;
            if(start.x > obstacleSurrounder.xMax) {
                closestEdgeSpot.x = obstacleSurrounder.xMax;
            }
            else if(start.x < obstacleSurrounder.xMin) {
                closestEdgeSpot.x = obstacleSurrounder.xMin;
            }
            if(start.y > obstacleSurrounder.yMax) {
                closestEdgeSpot.y = obstacleSurrounder.yMax;
            }
            else if(start.y < obstacleSurrounder.yMin) {
                closestEdgeSpot.y = obstacleSurrounder.yMin;
            }

            if(closestEdgeSpot != start) {
                // outside rectangle
                return closestEdgeSpot;
            }

            // inside rectangle
            bool blockedUp = false;
            bool blockedDown = false;
            bool blockedLeft = false;
            bool blockedRight = false;
            foreach(Rect blocker in obstacles) {
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
            if(!blockedUp) {
                potentialSpots.Add(new Vector2(start.x, obstacleSurrounder.yMax));
            }
            if(!blockedDown) {
                potentialSpots.Add(new Vector2(start.x, obstacleSurrounder.yMin));
            }
            if(!blockedRight) {
                potentialSpots.Add(new Vector2(obstacleSurrounder.xMax, start.y));
            }
            if(!blockedLeft) {
                potentialSpots.Add(new Vector2(obstacleSurrounder.xMin, start.y));
            }

            if(potentialSpots.Count <= 0) {
                // returning the start spot indicates that there was no valid direction of accessing the point
                return start;
            }

            potentialSpots.Sort((Vector2 current, Vector2 next) => {
                float diff = Vector2.Distance(start, current) - Vector2.Distance(start, next);
                return (int)(diff * 100);
            });
            return potentialSpots[0];
        }

        Vector2 characterEdgeSpot = FindEdgeSpot(startPosition);
        Vector2 targetEdgeSpot = FindEdgeSpot(targetLocation);
        
        // DEBUG LOGGING
        Direction charSide = Direction.None;
        if(characterEdgeSpot.x == obstacleSurrounder.xMin) {
            charSide = Direction.Left;
        }
        if(characterEdgeSpot.x == obstacleSurrounder.xMax) {
            charSide = Direction.Right;
        }
        if(characterEdgeSpot.y == obstacleSurrounder.yMin) {
            charSide = Direction.Down;
        }
        if(characterEdgeSpot.y == obstacleSurrounder.yMax) {
            charSide = Direction.Up;
        }
        Direction targSide = Direction.None;
        if(targetEdgeSpot.x == obstacleSurrounder.xMin) {
            targSide = Direction.Left;
        }
        if(targetEdgeSpot.x == obstacleSurrounder.xMax) {
            targSide = Direction.Right;
        }
        if(targetEdgeSpot.y == obstacleSurrounder.yMin) {
            targSide = Direction.Down;
        }
        if(targetEdgeSpot.y == obstacleSurrounder.yMax) {
            targSide = Direction.Up;
        }
        Debug.Log($"start: {charSide}, goal: {targSide}");
        Debug.Log($"start: {characterEdgeSpot}, goal: {targetEdgeSpot}");
        Debug.Log("rect: " + obstacleSurrounder.min + ", " + obstacleSurrounder.max);

        // TODO: ACCOUNT FOR ALL BLOCKED SIDES

        // TODO: FIND WHICH SIDES ARE INACCESSABLE (based on border wall?)
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
            Debug.Log($"same side, target: {targetCorner}");
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
            Debug.Log($"opposite hori, target: {targetSideMiddle}");
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
            Debug.Log($"opposite vert, target: {targetSideMiddle}");
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
            Debug.Log($"around corner, target: {targetCorner}");
            return targetCorner;
        }
    }
}
