using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private readonly Vector2 SIZE = new Vector2(21.33f, 12); // values found through observation
    private List<Rect> cameraZones = new List<Rect>(); // areas that the camera's view area must not leave
    private List<Rect> positionZones = new List<Rect>(); // areas that the middle of the camera must not leave, determined from cameraZones
    private static CameraScript instance;
    public static CameraScript Instance { get { return instance; } }

    void Awake() {
        instance = this;
    }

    void FixedUpdate()
    {
        Vector2 playerPos = PlayerScript.Instance.PlayerEntity.transform.position;

        if(cameraZones.Count <= 0) {
            transform.position = new Vector3(playerPos.x, playerPos.y, transform.position.z);
            return;
        }

        Vector3? targetPos = null;
        List<Vector2> potentialSpots = new List<Vector2>();
        foreach(Rect positionZone in positionZones) {
            // check if the player is inside this zone
            if(positionZone.Contains(playerPos)) {
                targetPos = new Vector3(playerPos.x, playerPos.y, transform.position.z);
            } else {
                // find the closest spot to snap to this zone
                Vector2 closestSpot;
                closestSpot.x = Mathf.Max(positionZone.xMin, Mathf.Min(playerPos.x, positionZone.xMax));
                closestSpot.y = Mathf.Max(positionZone.yMin, Mathf.Min(playerPos.y, positionZone.yMax));
                potentialSpots.Add(closestSpot);
            }
        }

        if(!targetPos.HasValue) {
            potentialSpots.Sort((Vector2 current, Vector2 next) => {
                float diff = Vector2.Distance(playerPos, current) - Vector2.Distance(playerPos, next);
                return (int)(diff * 100);
            });
            targetPos = new Vector3(potentialSpots[0].x, potentialSpots[0].y, transform.position.z);
        }

        // approach the target position
        float distance = Vector3.Distance(transform.transform.position, targetPos.Value);
        float speed = 8.0f;
        speed += distance;
        float shift = speed * Time.deltaTime;
        Debug.Log(shift + ", " + distance);
        if(shift > distance) {
            transform.position = targetPos.Value;
        } else {
            transform.position = transform.position + shift * (targetPos.Value - transform.position).normalized;
        }
    }

    public void AddCameraZone(Rect newZone) {
        if(newZone.width < SIZE.x || newZone.height < SIZE.y) {
            Debug.Log("Camera Zone was too small");
            return;
        }

        // determine where the center of the camera can be inside this zone
        Rect positionZone = MakePositionZone(newZone);
        positionZones.Add(positionZone);

        // find which zones the new one is adjacent to
        List<Rect> adjacentZones = new List<Rect>();
        Rect enlarged = newZone.MakeExpanded(1); // allow zones slighlty apart to be considered adjacent
        foreach(Rect oldZone in cameraZones) {
            if(oldZone.Overlaps(enlarged)) {
                adjacentZones.Add(oldZone);
            }
        }

        cameraZones.Add(newZone); // add the new one after checking the old ones

        // connect the new zone to all of the adjacent ones
        foreach(Rect adjacent in adjacentZones) {
            Rect adjPosition = MakePositionZone(adjacent);
            if(adjPosition.xMin < positionZone.xMax && adjPosition.xMax > positionZone.xMin) {
                // above or below
                float left = Mathf.Max(adjPosition.xMin, positionZone.xMin);
                float right = Mathf.Min(adjPosition.xMax, positionZone.xMax);;
                float top = Mathf.Max(adjPosition.yMin, positionZone.yMin);
                float bottom = Mathf.Min(adjPosition.yMax, positionZone.yMax);
                positionZones.Add(new Rect(left, bottom, right - left, top - bottom));
            }
            else if(adjPosition.yMin < positionZone.yMax && adjPosition.yMax > positionZone.yMin) {
                // left or right
                float left = Mathf.Min(adjPosition.xMax, positionZone.xMax);
                float right = Mathf.Max(adjPosition.xMin, positionZone.xMin); ;
                float top = Mathf.Min(adjPosition.yMax, positionZone.yMax);
                float bottom = Mathf.Max(adjPosition.yMin, positionZone.yMin);
                positionZones.Add(new Rect(left, bottom, right - left, top - bottom));
            }
            // else diagonal, so no actual connection
        }

        // if any sides are adjacent to each other, fill in that corner space
        for(int i = 0; i < adjacentZones.Count; i++) {
            for(int j = i + 1; j < adjacentZones.Count; j++) {
                if(adjacentZones[i].MakeExpanded(1).Overlaps(adjacentZones[j])) {
                    Rect iZone = MakePositionZone(adjacentZones[i]);
                    Rect jZone = MakePositionZone(adjacentZones[j]);

                    // find which two of the three adjacent zones are horizontal to each other
                    Rect hori1 = iZone;
                    Rect hori2 = jZone;
                    Rect vert = positionZone;
                    if(Mathf.Abs(iZone.center.y - newZone.center.y) < Mathf.Abs(hori1.center.y - hori2.center.y)) {
                        hori1 = iZone;
                        hori2 = newZone;
                        vert = jZone;
                    }
                    if(Mathf.Abs(jZone.center.y - newZone.center.y) < Mathf.Abs(hori1.center.y - hori2.center.y)) {
                        hori1 = jZone;
                        hori2 = newZone;
                        vert = iZone;
                    }

                    float left = vert.xMin;
                    float right = vert.xMax;
                    float top = vert.center.y < hori1.center.y ? Mathf.Max(hori1.yMin, hori2.yMin) : vert.yMin;
                    float bottom = vert.center.y < hori1.center.y ? vert.yMax : Mathf.Min(hori1.yMax, hori2.yMax);
                    positionZones.Add(new Rect(left, bottom, right - left, top - bottom));
                }
            }
        }
    }

    private Rect MakePositionZone(Rect cameraZone) {
        return new Rect(cameraZone.x + SIZE.x / 2, cameraZone.y + SIZE.y / 2, cameraZone.width - SIZE.x, cameraZone.height - SIZE.y);
    }
}
