using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    private float projectileAlertTime; // time when this has a target after being shot at
    private Vector3Int[] currentPath;

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
                if(target != null && GetTargetDistance() <= 1.0f + target.GetComponent<Enemy>().CollisionRadius + controlled.GetComponent<Enemy>().CollisionRadius) {
                    // stop chasing if close enough
                    return Vector2.zero;
                }
                return currentDirection;

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
            return GetTargetDirection();
        }

        Vector2 move = GetMoveDirection();
        if(move != Vector2.zero) {
            return move;
        }

        return Vector2.down;
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
        if(target == null) {
            return false;
        }

        if(currentPath != null) {
            // this is fine here because the pathfinder does not run when following a path already
            return true;
        }

        string[] layersToCheck = checkPits ? new string[2] { "Wall", "Floor" } : new string[1] { "Wall" };
        RaycastHit2D castResult = Physics2D.Raycast(controlled.transform.position, GetTargetDirection(), GetTargetDistance(), LayerMask.GetMask(layersToCheck));
        return castResult.collider != null;
    }
    #endregion

    #region Helper functions
    // returns the unit vector towards the target, zero vector if no target
    private Vector2 GetTargetDirection() {
        if(target == null) {
            return Vector2.zero;
        }

        return (target.transform.position - controlled.transform.position).normalized;
    }

    // either looks for a new target or checks if the current target is lost
    private void CheckTarget() {
        if(vision <= 0) {
            return;
        }

        if(target == null) {
            // check for a target
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
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
                    specialAim = GetTargetDirection(); // if the player leaves the character's range when an attack is queued, attack at their last seen location
                }
                target = null;
            }
        }
    }

    // determine a direction to move for a span of time
    private void ChooseMovement() {
        switch(CurrentMode) {
            case AIMode.Wander:
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
                break;

            case AIMode.Patrol:
                if(currentDirection == Vector2.zero) {
                    currentDirection = new Vector2(0, -1);
                    return;
                }

                Vector2 toStart = (startPosition - (Vector2)controlled.transform.position).normalized;

                // lock the direction to straight through the middle
                if(Vector2.Dot(toStart, currentDirection) > 0.9f) {
                    currentDirection = toStart;
                    return;
                }
                
                // rotate back towards the start position
                if(Vector2.Distance(startPosition, controlled.transform.position) < WANDER_RANGE - 2f) {
                    return;
                }
                
                bool clockwise = false;
                float rotationAmount = 0f;
                if((currentDirection + toStart).magnitude < 0.01f) {
                    // randomly choose to rotate clockwise or counter
                    clockwise = Random.value < 0.5f;
                    rotationAmount = 0.1f;
                } else {
                    // rotate in the direction which gets this back towards the center faster
                    Vector2 clockwiseCheck = new Vector2(-toStart.y, toStart.x);
                    clockwise = Vector2.Dot(clockwiseCheck, currentDirection) > 0;

                    const float ROT_PER_SEC = Mathf.PI;
                    rotationAmount = Time.deltaTime * ROT_PER_SEC * controlled.GetComponent<Enemy>().WalkSpeed / 5.0f; // turn more if moving faster
                }

                float currentAngle = Mathf.Atan2(currentDirection.y, currentDirection.x);
                float newAngle = currentAngle + rotationAmount * (clockwise ? -1 : 1);
                currentDirection = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
                break;

            case AIMode.Chase:
                if(travelTimer > 0) {
                    travelTimer -= Time.deltaTime;
                    if(currentPath == null || currentPath.Length == 0) {
                        currentDirection = Vector2.zero;
                        return;
                    }

                    // travel along the path
                    Vector3Int currentTile = LevelManager.Instance.WallGrid.WorldToCell(controlled.transform.position);
                    for(int i = 0; i < currentPath.Length - 1; i++) {
                        if(currentPath[i] == currentTile) {
                            currentDirection = (LevelManager.Instance.WallGrid.GetCellCenterWorld(currentPath[i+1]) - controlled.transform.position).normalized;
                            return;
                        }
                    }

                    currentDirection = (LevelManager.Instance.WallGrid.GetCellCenterWorld(currentPath[0]) - controlled.transform.position).normalized;
                    return;
                }

                currentPath = null;

                if(target == null) {
                    currentDirection = Vector2.zero;
                    return;
                }

                // determine which direction to move in
                if(IsTargetBlocked(!controlled.GetComponent<Enemy>().Floating)) {
                    currentPath = FindTilePath(target.transform.position);
                    currentDirection = Vector2.zero; // start walking path next frame
                    travelTimer = 0.3f * (1 + currentPath.Length / 4);
                    return;
                } 
                else {
                    // straight path
                    currentDirection = (target.transform.position - controlled.transform.position).normalized;
                }
                break;
        }
    }

    // takes the character's desired direction and modifies it to avoid walls and pits. Works best when trying to move in one direction for a while
    private Vector2 ModifyDirection(Vector2 desiredDirection) {
        if(desiredDirection == Vector2.zero) {
            return desiredDirection;
        }

        bool checkPits = !controlled.GetComponent<Enemy>().Floating;

        // if hanging over a pit, move away from it by averaging the pit and land tile centers
        List<Vector3Int> overlappedTiles;
        if(checkPits) {
            overlappedTiles = LevelManager.Instance.GetOverlappedTiles(controlled);
            Vector3 pitCenterTotal = Vector3.zero;
            Vector3 landCenterTotal = Vector3.zero;
            float pitCount = 0f;
            float landCount = 0f;
            foreach(Vector3Int overlappedTile in overlappedTiles) {
                FloorTile floor = LevelManager.Instance.FloorGrid.GetTile<FloorTile>(overlappedTile);
                if(floor != null && floor.Type == FloorType.Pit) {
                    pitCenterTotal += LevelManager.Instance.FloorGrid.GetCellCenterWorld(overlappedTile);
                    pitCount++;
                } else {
                    landCenterTotal += LevelManager.Instance.FloorGrid.GetCellCenterWorld(overlappedTile);
                    landCount++;
                }
            }

            if(pitCount > 0) {
                return (landCenterTotal / landCount - pitCenterTotal / pitCount).normalized;
            }
        }

        // look ahead of the current movement to see if there is an upcoming obstacle
        float radius = controlled.GetComponent<Enemy>().CollisionRadius;
        float checkDistance = 2 * radius;
        Circle futureArea = new Circle(controlled.transform.position + checkDistance * (Vector3)desiredDirection.normalized, radius);
        overlappedTiles = LevelManager.Instance.GetOverlappedTiles(futureArea);
        List<Vector3Int> overlappedObstacles = new List<Vector3Int>();
        foreach(Vector3Int overlappedTile in overlappedTiles) {
            WallTile wall = LevelManager.Instance.WallGrid.GetTile<WallTile>(overlappedTile);
            FloorTile floor = LevelManager.Instance.FloorGrid.GetTile<FloorTile>(overlappedTile);
            if(wall != null || (checkPits && floor != null && floor.Type == FloorType.Pit)) {
                overlappedObstacles.Add(overlappedTile);
            }
        }

        if(overlappedObstacles.Count <= 0) {
            return desiredDirection;
        }

        // don't walk directly into an upcoming obstacle
        foreach(Vector3Int overlappedObstacle in overlappedObstacles) {
            Vector3 toTileCenter = LevelManager.Instance.WallGrid.GetCellCenterWorld(overlappedObstacle) - controlled.transform.position;
            if(Mathf.Abs(toTileCenter.x) > Mathf.Abs(toTileCenter.y)) {
                desiredDirection.x = 0;
            } else {
                desiredDirection.y = 0;
            }
        }

        return desiredDirection.normalized;
    }

    // uses the tilemap to find a path of open tiles to the target position. Returns null if no path is found
    private Vector3Int[] FindTilePath(Vector3 targetPosition) {
        Tilemap wallGrid = LevelManager.Instance.WallGrid;
        Tilemap floorGrid = LevelManager.Instance.FloorGrid;
        GridInformation gridData = LevelManager.Instance.GridData;
        Vector3Int startTile = wallGrid.WorldToCell(controlled.transform.position);
        Vector3Int endTile = wallGrid.WorldToCell(targetPosition);
        bool checkPits = !controlled.GetComponent<Enemy>().Floating;
        Vector3Int[] directions = new Vector3Int[4] { Vector3Int.up, Vector3Int.down, Vector3Int.right, Vector3Int.left };

        // find shortest path with A*
        gridData.SetPositionProperty(startTile, "travelDistance", 0);
        gridData.SetPositionProperty(startTile, "parent x", startTile.x);
        gridData.SetPositionProperty(startTile, "parent y", startTile.y);
        List<Vector3Int> closedList = new List<Vector3Int>();
        List<Vector3Int> openList = new List<Vector3Int>() { startTile };
        while(openList.Count > 0) {
            // find the best current path option
            Vector3Int currentTile = openList[0];
            int bestDistance = gridData.GetPositionProperty(currentTile, "travelDistance", 0) + CalcTileDistance(currentTile, endTile);
            for(int i = 1; i < openList.Count; i++) {
                int distance = CalcTileDistance(openList[i], startTile) + CalcTileDistance(openList[i], endTile);
                if(distance < bestDistance) {
                    bestDistance = distance;
                    currentTile = openList[i];
                }
            }

            // check for a completed path
            if(currentTile == endTile) {
                Vector3Int[] path = new Vector3Int[gridData.GetPositionProperty(currentTile, "travelDistance", 0)];
                Vector3Int pathTrace = endTile;
                for(int i = path.Length - 1; i >= 0; i--) {
                    path[i] = pathTrace;
                    pathTrace = new Vector3Int(gridData.GetPositionProperty(pathTrace, "parent x", 0), gridData.GetPositionProperty(pathTrace, "parent y", 0), 0);
                }
                return path;
            }

            // move this tile to the closed list
            openList.Remove(currentTile);
            closedList.Add(currentTile);

            // update the neighbors of this tile
            foreach(Vector3Int direction in directions) {
                Vector3Int neighbor = currentTile + direction;
                FloorTile floor = floorGrid.GetTile<FloorTile>(neighbor);
                if(neighbor != endTile && (wallGrid.GetTile(neighbor) != null || (checkPits && floor != null && floor.Type == FloorType.Pit))) {
                    continue;
                }

                bool inOpen = openList.Contains(neighbor);
                bool inClosed = closedList.Contains(neighbor);
                int currentTravelDistance = gridData.GetPositionProperty(currentTile, "travelDistance", 0);
                if(!inOpen && !inClosed) {
                    // found a new path
                    openList.Add(neighbor);
                    gridData.SetPositionProperty(neighbor, "travelDistance", currentTravelDistance + 1);
                    gridData.SetPositionProperty(neighbor, "parent x", currentTile.x);
                    gridData.SetPositionProperty(neighbor, "parent y", currentTile.y);
                }
                else if(inOpen && currentTravelDistance + 1 < gridData.GetPositionProperty(neighbor, "travelDistance", 0)) {
                    gridData.SetPositionProperty(neighbor, "travelDistance", currentTravelDistance + 1);
                    gridData.SetPositionProperty(neighbor, "parent x", currentTile.x);
                    gridData.SetPositionProperty(neighbor, "parent y", currentTile.y);
                }
            }
        }
        
        return null;
    }

    private int CalcTileDistance(Vector3Int start, Vector3Int end) {
        return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y);
    }
    #endregion
}
