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

    private Vector2 currentDirection; // optional variable for movement modes that have certain paths
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
                return currentDirection;
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
        else if(CurrentMode == AIMode.Chase) {
            travelTimer -= Time.deltaTime;
            if(travelTimer <= 0) {
                travelTimer += 0.1f;
                if(target == null) {
                    currentDirection = Vector2.zero;
                } else {
                    currentDirection = NewApproach(target.transform.position);
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
    private Vector2 Approach(Vector2 targetLocation) {
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
        Direction blockedSide = Direction.None;
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
                    blockedSide = idealDirection.x < 0 ? Direction.Right : Direction.Left;
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
                        blockedSide = idealDirection.y < 0 ? Direction.Up : Direction.Down;
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
            return idealDirection;
        }

        // find which way to go around this obstacle
        Rect blocker = closestBlock.Value;
        Vector2 topLeft = new Vector2(blocker.xMin - radius, blocker.yMax + radius);
        Vector2 bottomLeft = new Vector2(blocker.xMin - radius, blocker.yMin - radius);
        Vector2 topRight = new Vector2(blocker.xMax + radius, blocker.yMax + radius);
        Vector2 bottomRight = new Vector2(blocker.xMax + radius, blocker.yMin - radius);

        Dictionary<Corner, Vector2> cornerPositions = new Dictionary<Corner, Vector2>() {
            { new Corner(Direction.Left, Direction.Up), topLeft },
            { new Corner(Direction.Left, Direction.Down), bottomLeft },
            { new Corner(Direction.Right, Direction.Up), topRight },
            { new Corner(Direction.Right, Direction.Down), bottomRight }
        };

        Direction horizontalAccess = Direction.None;
        if(targetLocation.x < blocker.xMin) {
            horizontalAccess = Direction.Left;
        }
        else if(targetLocation.x > blocker.xMax) {
            horizontalAccess = Direction.Right;
        }

        Direction verticalAccess = Direction.None;
        if(targetLocation.y < blocker.yMin) {
            verticalAccess = Direction.Down;
        }
        else if(targetLocation.y > blocker.yMax) {
            verticalAccess = Direction.Up;
        }

        if(horizontalAccess == Direction.None && verticalAccess == Direction.None) {
            return Vector2.zero;
        }

        List<Vector2> clockwisePath = new List<Vector2>() { startPosition };
        List<Vector2> counterPath = new List<Vector2>() { startPosition };

        List<Corner> clockwiseCorners = new List<Corner>();
        List<Corner> counterCorners = new List<Corner>();
        switch(blockedSide) {
            case Direction.Up:
                clockwiseCorners.Add(new Corner(Direction.Right, Direction.Up));
                counterCorners.Add(new Corner(Direction.Left, Direction.Up));
                break;

            case Direction.Down:
                clockwiseCorners.Add(new Corner(Direction.Left, Direction.Down));
                counterCorners.Add(new Corner(Direction.Right, Direction.Down));
                break;

            case Direction.Left:
                clockwiseCorners.Add(new Corner(Direction.Left, Direction.Up));
                counterCorners.Add(new Corner(Direction.Left, Direction.Down));
                break;

            case Direction.Right:
                clockwiseCorners.Add(new Corner(Direction.Right, Direction.Down));
                counterCorners.Add(new Corner(Direction.Right, Direction.Up));
                break;
        }

        while(clockwiseCorners[clockwiseCorners.Count - 1].Horizontal != horizontalAccess 
            && clockwiseCorners[clockwiseCorners.Count - 1].Vertical != verticalAccess
        ) {
            clockwiseCorners.Add(clockwiseCorners[clockwiseCorners.Count - 1].GetClockwise());
        }

        while(counterCorners[counterCorners.Count - 1].Horizontal != horizontalAccess 
            && counterCorners[counterCorners.Count - 1].Vertical != verticalAccess
        ) {
            counterCorners.Add(counterCorners[counterCorners.Count - 1].GetCounterClockwise());
        }

        foreach(Corner corner in clockwiseCorners) {
            clockwisePath.Add(cornerPositions[corner]);
        }

        foreach(Corner corner in counterCorners) {
            counterPath.Add(cornerPositions[corner]);
        }

        clockwisePath.Add(targetLocation);
        counterPath.Add(targetLocation);

        List<Vector2> chosenPath = CalcPathDistance(clockwisePath) < CalcPathDistance(counterPath) ? clockwisePath : counterPath;

        if(Vector2.Dot(chosenPath[1] - chosenPath[0], chosenPath[2] - chosenPath[0]) < 0) {
            chosenPath.RemoveAt(1);
        }

        return (chosenPath[1] - startPosition).normalized;
    }
    
    // takes a list of points where the first and last elements are the beginning and end of a path and returns the distance travelling one point to the next
    private float CalcPathDistance(List<Vector2> path) {
        float distance = 0;
        for(int i = 0; i < path.Count - 1; i++) {
            distance += Vector2.Distance(path[i], path[i+1]);
        }
        return distance;
    }

    private Vector2 NewApproach(Vector2 targetLocation) {
        // NEWER PROCESS
        // 1: find if this will run into a wall
        // 2: trace a path following edges around the wall until finding LOS on the target
        // 3: skip to the middle of the path if it's closer to the start position
        // 3: find which path is shorter and go that way

        // 1: find the first wall that blocks the path
        // 2: consider both directions of going around the blocking wall
        // - go clockwise or counter-clockwise until reaching the spot on the wall closest to the target
        // - can go around one? adjacent wall (sidequest) if that wall has no other adjacent walls (no sidequest of a sidequest)
        // - 
        // 3: choose the shorter path that is valid

        List<Rect> obstacles = GetObstacles();
        Vector2 idealMovement = targetLocation - (Vector2)controlled.transform.position;

        // find the wall that will block a straight path
        Vector2? edgeSpot;
        Rect? blocker = Raycast(controlled.transform.position, idealMovement, obstacles, out edgeSpot);
        if(!blocker.HasValue) {
            return idealMovement.normalized;
        }

        // figure out which way to go around the obstacle (and its neighbors)
        Direction hitSide = Direction.None;
        if(edgeSpot.Value.x == blocker.Value.xMin) hitSide = Direction.Left;
        else if(edgeSpot.Value.x == blocker.Value.xMax) hitSide = Direction.Right;
        else if(edgeSpot.Value.y == blocker.Value.yMin) hitSide = Direction.Down;
        else if(edgeSpot.Value.y == blocker.Value.yMax) hitSide = Direction.Up;

        List<Vector2> clockwisePath = new List<Vector2>() { startPosition, edgeSpot.Value };
        List<Vector2> counterPath = new List<Vector2>() { startPosition, edgeSpot.Value };

        Stack<Direction> clockwiseMovements = new Stack<Direction>();
        Stack<Direction> counterMovements = new Stack<Direction>();

        if(hitSide == Direction.Left) {
            
            // side stepping
            // circling (once)
        }

        return Vector2.zero;
    }

    // starting from the position, finds the first obstacle going straight in the given direction.
    // Returns the closest rectangle, null if there is none, and outputs the spot on the edge of the wall that is hit
    private Rect? Raycast(Vector2 position, Vector2 direction, List<Rect> obstacles, out Vector2? edgePosition) {
        if(direction == Vector2.zero) {
            edgePosition = null;
            return null;
        }

        direction.Normalize();

        float closestDistance = float.MaxValue;
        Rect? closestBlock = null;
        Vector2? closestEdge = null;
        foreach(Rect obstacle in obstacles) {
            // find if the path intersects this rectangle
            Vector2? currentEdgeSpot = null;
            float distance = 0;
            if(direction.x != 0) {
                // check left or right side
                float targetX = direction.x < 0 ? obstacle.xMax : obstacle.xMin;
                distance = (targetX - startPosition.x) / direction.x;
                float y = startPosition.y + direction.y * distance;

                if(distance > 0 && y > obstacle.yMin && y < obstacle.yMax) {
                    currentEdgeSpot = new Vector2(targetX, y);
                }
            }
            if(!currentEdgeSpot.HasValue && direction.y != 0) {
                // check left or right side
                float targetY = direction.y < 0 ? obstacle.yMax : obstacle.yMin;
                distance = (targetY - startPosition.y) / direction.y;
                float x = startPosition.x + direction.x * distance;
                
                if(distance > 0 && x > obstacle.xMin && x < obstacle.xMax) {
                    currentEdgeSpot = new Vector2(x, targetY);
                }
            }

            // determine if this collision is closest
            if(currentEdgeSpot.HasValue && distance < closestDistance) {
                closestDistance = distance;
                closestBlock = obstacle;
                closestEdge = currentEdgeSpot;
            }
        }

        edgePosition = closestEdge;
        return closestBlock;
    }

    // creates a list of all rectangles that can block this character's movement, expanded by the radius of the circle collider
    private List<Rect> GetObstacles() {
        List<Rect> obstacles = new List<Rect>();
        foreach(GameObject wall in EntityTracker.Instance.Walls) {
            obstacles.Add(wall.GetComponent<WallScript>().Area.MakeExpanded(controlled.GetComponent<Enemy>().CollisionRadius));
        }
        if(!controlled.GetComponent<Enemy>().Floating) {
            foreach(PitScript pit in EntityTracker.Instance.Pits) {
                foreach(Rect area in pit.Zones) {
                    obstacles.Add(area.MakeExpanded(controlled.GetComponent<Enemy>().CollisionRadius));
                }
            }
        }
        return obstacles;
    }
}
