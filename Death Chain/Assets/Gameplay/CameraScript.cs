using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private const float ASPECT_RATIO = 16f / 9f;
    private List<Vector2> cameraPoints = new List<Vector2>(); // points that connetc to form the camera's movable area
    private List<Rect> cameraZones = new List<Rect>(); // areas that the middle of the camera must not leave, determined from cameraZones
    private float cameraSize;
    private float startZ;
    
    private static CameraScript instance;
    public static CameraScript Instance { get { return instance; } }

    public Rect VisibleArea { get { return new Rect((Vector2)transform.position - Size / 2, Size); } }
    public Vector2 Size { get { return new Vector2(ASPECT_RATIO * 2 * cameraSize, 2 * cameraSize); } }

    void Awake() {
        instance = this;
        cameraSize = GetComponent<Camera>().orthographicSize;
        startZ = transform.position.z;

        if(!enabled) {
            // debug option for test scenes
            Deprecated_AddCameraZone(transform.position);
            enabled = true;
            return;
        }
    }

    // use fixed update to prevent camera jitters
    void FixedUpdate() {
        Vector2 playerPos = PlayerScript.Instance.PlayerEntity.transform.position;

        if(cameraZones.Count <= 0) {
            transform.position = new Vector3(playerPos.x, playerPos.y, startZ);
            return;
        }

        // approach the target position
        Vector3 targetPosition = FindTargetPosition();
        Vector3 shift = (targetPosition - transform.position) * 0.1f * Time.timeScale;
        if(shift.sqrMagnitude > 0.3f * 0.3f) {
            shift.Normalize();
            shift *= 0.3f;
        }
        transform.position += shift;
    }

    public Vector3 FindTargetPosition() {
        Vector2 playerPos = PlayerScript.Instance.PlayerEntity.transform.position;

        List<Vector2> potentialSpots = new List<Vector2>();
        foreach(Rect zone in cameraZones) {
            // check if the player is inside this zone
            if(zone.Contains(playerPos)) {
                return new Vector3(playerPos.x, playerPos.y, startZ);
            }

            // find the closest spot to snap to this zone
            Vector2 closestSpot;
            closestSpot.x = Mathf.Max(zone.xMin, Mathf.Min(playerPos.x, zone.xMax));
            closestSpot.y = Mathf.Max(zone.yMin, Mathf.Min(playerPos.y, zone.yMax));
            potentialSpots.Add(closestSpot);
        }

        potentialSpots.Sort((Vector2 current, Vector2 next) => {
            return Vector2.Distance(playerPos, current) < Vector2.Distance(playerPos, next) ? -1 : 1;
        });
        return new Vector3(potentialSpots[0].x, potentialSpots[0].y, startZ);
    }

    public void AddCameraZone(Rect zone) {
        cameraZones.Add(zone);
    }

    // adds an area that the camera can move in based on a position. It automatially connects the point to nearby points to create movable areas
    public void Deprecated_AddCameraZone(Vector2 movePoint) {
        // find adjacent points
        Rect adjacencyChecker = new Rect(movePoint - 1.1f * Size, 2 * 1.1f * Size); // stretch by 1.1 in case the point is a tad too far
        Vector2?[,] pointGrid = new Vector2?[3, 3];
        pointGrid[1, 1] = movePoint;
        foreach(Vector2 oldPoint in cameraPoints) {
            if(adjacencyChecker.Contains(oldPoint)) {
                Vector2 relativity = oldPoint - movePoint;
                int row = 1;
                const float BUFFER = 2f;
                if(relativity.y > BUFFER) {
                    row = 2;
                }
                else if(relativity.y < -BUFFER) {
                    row = 0;
                }

                int col = 1;
                if(relativity.x > BUFFER) {
                    col = 2;
                }
                else if(relativity.x < -BUFFER) {
                    col = 0;
                }

                pointGrid[row, col] = oldPoint;
            }
        }

        cameraPoints.Add(movePoint);

        // connect to the adjacent points
        Vector2Int[] shifts = new Vector2Int[4] { 
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };
        List<Rect> addedZones = new List<Rect>();
        foreach(Vector2Int shift in shifts) {
            Vector2? adjacent = pointGrid[1 + shift.x, 1 + shift.y];
            if(adjacent.HasValue) {
                addedZones.Add(new Rect(
                    Mathf.Min(adjacent.Value.x, movePoint.x),
                    Mathf.Min(adjacent.Value.y, movePoint.y),
                    Mathf.Max(adjacent.Value.x, movePoint.x) - Mathf.Min(adjacent.Value.x, movePoint.x),
                    Mathf.Max(adjacent.Value.y, movePoint.y) - Mathf.Min(adjacent.Value.y, movePoint.y)
                ));
            }
        }

        if(addedZones.Count == 0) {
            // no adjacent points, just create a single dot to snap to
            cameraZones.Add(new Rect(movePoint.x, movePoint.y, 0, 0));
            return;
        }

        // check for completed corners
        Vector2Int[] corners = new Vector2Int[4] {
            new Vector2Int(0, 0),
            new Vector2Int(2, 0),
            new Vector2Int(0, 2),
            new Vector2Int(2, 2)
        };
        foreach(Vector2Int corner in corners) {
            if(!pointGrid[corner.y, corner.x].HasValue) {
                continue;
            }

            bool completed = true;
            foreach(Vector2Int shift in shifts) {
                Vector2Int spot = corner + shift;
                if(spot.x < 0 || spot.x > 2 || spot.y < 0 || spot.y > 2) {
                    continue;
                }

                if(!pointGrid[spot.y, spot.x].HasValue) {
                    completed = false;
                    break;
                }
            }

            if(completed) {
                Vector2 cornerPos = pointGrid[corner.y, corner.x].Value;
                addedZones.Add(new Rect(
                    Mathf.Min(cornerPos.x, movePoint.x),
                    Mathf.Min(cornerPos.y, movePoint.y),
                    Mathf.Max(cornerPos.x, movePoint.x) - Mathf.Min(cornerPos.x, movePoint.x),
                    Mathf.Max(cornerPos.y, movePoint.y) - Mathf.Min(cornerPos.y, movePoint.y)
                ));
            }
        }

        // remove old zones that are now overlapped and add new zones
        foreach(Rect newZone in addedZones) {
            for(int i = cameraZones.Count - 1; i >= 0; i--) {
                if(newZone.Contains(cameraZones[i])) {
                    cameraZones.RemoveAt(i);
                }
            }

            cameraZones.Add(newZone);
        }
    }
}
