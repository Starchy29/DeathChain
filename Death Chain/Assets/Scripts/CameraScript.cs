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

    void Update()
    {
        Vector2 playerPos = PlayerScript.Instance.PlayerEntity.transform.position;

        if(cameraZones.Count <= 0) {
            transform.position = new Vector3(playerPos.x, playerPos.y, transform.position.z);
            return;
        }

        List<Vector2> potentialSpots = new List<Vector2>();
        foreach(Rect positionZone in positionZones) {
            // check if the player is inside this zone
            if(positionZone.Contains(playerPos)) {
                transform.position = new Vector3(playerPos.x, playerPos.y, transform.position.z);
                return;
            }

            // find the closest spot to snap to this zone
            Vector2 closestSpot;
            closestSpot.x = Mathf.Max(positionZone.xMin, Mathf.Min(playerPos.x, positionZone.xMax));
            closestSpot.y = Mathf.Max(positionZone.yMin, Mathf.Min(playerPos.y, positionZone.yMax));
            potentialSpots.Add(closestSpot);
        }

        potentialSpots.Sort((Vector2 current, Vector2 next) => {
            float diff = Vector2.Distance(playerPos, current) - Vector2.Distance(playerPos, next);
            return (int)(diff * 100);
        });
        transform.position = new Vector3(potentialSpots[0].x, potentialSpots[0].y, transform.position.z);

        // find which corners are outside the camera's allowed area
        //Vector2 topRight = (Vector2)playerPos + SIZE / 2;
        //Vector2 topLeft = (Vector2)playerPos + new Vector2(-SIZE.x, SIZE.y) / 2;
        //Vector2 bottomRight = (Vector2)playerPos + new Vector2(SIZE.x, -SIZE.y) / 2;
        //Vector2 bottomLeft = (Vector2)playerPos - SIZE / 2;

        //bool topRightOutside = true;
        //bool topLeftOutside = true;
        //bool bottomRightOutside = true;
        //bool bottomLeftOutside = true;

        //foreach(Rect zone in cameraZones) {
        //    if(zone.Contains(topRight)) {
        //        topRightOutside = false;
        //    }
        //    if(zone.Contains(topLeft)) {
        //        topLeftOutside = false;
        //    }
        //    if(zone.Contains(bottomRight)) {
        //        bottomRightOutside = false;
        //    }
        //    if(zone.Contains(bottomLeft)) {
        //        bottomLeftOutside = false;
        //    }
        //}

        //// determine which directions the camera needs to shift
        //Direction verticalShift = Direction.None;
        //Direction horizontalShift = Direction.None;
        //if(topRightOutside && topLeftOutside) {
        //    verticalShift = Direction.Down;
        //}
        //else if(bottomLeftOutside && bottomRightOutside) {
        //    verticalShift = Direction.Up;
        //}
        //if(topLeftOutside && bottomLeftOutside) {
        //    horizontalShift = Direction.Right;
        //}
        //else if(topRightOutside && bottomRightOutside) {
        //    horizontalShift = Direction.Left;
        //}

        //// find the closest zone to shift to in the desired direction
        //float? newX = playerPos.x;
        //float? newY = playerPos.y;
        //foreach(Rect zone in cameraZones) {
        //    // check vertical
        //    if(playerPos.x > zone.xMin && playerPos.x < zone.xMax) {
        //        if(verticalShift == Direction.Up && zone.yMin > bottomLeft.y && (!newY.HasValue || zone.yMin < newY.Value)) {
        //            newY = zone.yMin;
        //        }
        //        else if(verticalShift == Direction.Down && zone.yMax < topLeft.y && (!newY.HasValue || zone.yMax > newY.Value)) {
        //            newY = zone.yMax;
        //        }
        //    }
            
        //    // check horizontal
        //    if(playerPos.y > zone.yMin && playerPos.y < zone.yMax) {
        //        if(horizontalShift == Direction.Right && zone.xMin > bottomLeft.x && (!newX.HasValue || zone.xMin < newX.Value)) {
        //            newX = zone.xMin;
        //        }
        //        else if(horizontalShift == Direction.Left && zone.xMax < topRight.x && (!newX.HasValue || zone.xMax > newX.Value)) {
        //            newX = zone.xMax;
        //        }
        //    }
        //}

        //// shift from the edge to the camera's middle
        //if(verticalShift == Direction.Up && newY.HasValue) {
        //    newY += SIZE.y / 2;
        //}
        //else if(verticalShift == Direction.Down && newY.HasValue) {
        //    newY -= SIZE.y / 2;
        //}
        //if(horizontalShift == Direction.Right && newX.HasValue) {
        //    newX += SIZE.x / 2;
        //}
        //else if(horizontalShift == Direction.Left && newX.HasValue) {
        //    newX -= SIZE.x / 2;
        //}

        //transform.position = new Vector3(newX.HasValue ? newX.Value : playerPos.x, newY.HasValue ? newY.Value : playerPos.y, transform.position.z);

        // move toward target position
        //Vector3 difference = targetPos - transform.position;
        //difference.z = 0;
        //float distance = difference.magnitude;
        //float shift = 2 * distance;
        //if(shift < 6) {
        //    shift = 6;
        //}
        //if(distance < 0.2f) {
        //    transform.position = targetPos;
        //} else {
        //    transform.position += shift * Time.deltaTime / distance * difference;
        //}
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
