using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private readonly Vector2 SIZE = new Vector2(21.33f, 12); // values found through observation
    private List<Rect> cameraZones = new List<Rect>(); // the camera must not leave these rectangles
    
    private static CameraScript instance;
    public static CameraScript Instance { get { return instance; } }

    void Awake() {
        instance = this;
    }

    void Update()
    {
        Vector3 playerPos = PlayerScript.Instance.PlayerEntity.transform.position;

        if(cameraZones.Count <= 0) {
            transform.position = new Vector3(playerPos.x, playerPos.y, transform.position.z);
            return;
        }

        // find which corners are outside the camera's allowed area
        Vector2 topRight = (Vector2)playerPos + SIZE / 2;
        Vector2 topLeft = (Vector2)playerPos + new Vector2(-SIZE.x, SIZE.y) / 2;
        Vector2 bottomRight = (Vector2)playerPos + new Vector2(SIZE.x, -SIZE.y) / 2;
        Vector2 bottomLeft = (Vector2)playerPos - SIZE / 2;

        bool topRightOutside = true;
        bool topLeftOutside = true;
        bool bottomRightOutside = true;
        bool bottomLeftOutside = true;

        foreach(Rect zone in cameraZones) {
            if(zone.Contains(topRight)) {
                topRightOutside = false;
            }
            if(zone.Contains(topLeft)) {
                topLeftOutside = false;
            }
            if(zone.Contains(bottomRight)) {
                bottomRightOutside = false;
            }
            if(zone.Contains(bottomLeft)) {
                bottomLeftOutside = false;
            }
        }

        // determine which directions the camera needs to shift
        Direction verticalShift = Direction.None;
        Direction horizontalShift = Direction.None;
        if(topRightOutside && topLeftOutside) {
            verticalShift = Direction.Down;
        }
        else if(bottomLeftOutside && bottomRightOutside) {
            verticalShift = Direction.Up;
        }
        if(topLeftOutside && bottomLeftOutside) {
            horizontalShift = Direction.Right;
        }
        else if(topRightOutside && bottomRightOutside) {
            horizontalShift = Direction.Left;
        }

        // find the closest zone to shift to in the desired direction
        float? newX = playerPos.x;
        float? newY = playerPos.y;
        foreach(Rect zone in cameraZones) {
            // check vertical
            if(playerPos.x > zone.xMin && playerPos.x < zone.xMax) {
                if(verticalShift == Direction.Up && zone.yMin > bottomLeft.y && (!newY.HasValue || zone.yMin < newY.Value)) {
                    newY = zone.yMin;
                }
                else if(verticalShift == Direction.Down && zone.yMax < topLeft.y && (!newY.HasValue || zone.yMax > newY.Value)) {
                    newY = zone.yMax;
                }
            }
            
            // check horizontal
            if(playerPos.y > zone.yMin && playerPos.y < zone.yMax) {
                if(horizontalShift == Direction.Right && zone.xMin > bottomLeft.x && (!newX.HasValue || zone.xMin < newX.Value)) {
                    newX = zone.xMin;
                }
                else if(horizontalShift == Direction.Left && zone.xMax < topRight.x && (!newX.HasValue || zone.xMax > newX.Value)) {
                    newX = zone.xMax;
                }
            }
        }

        // shift from the edge to the camera's middle
        if(verticalShift == Direction.Up && newY.HasValue) {
            newY += SIZE.y / 2;
        }
        else if(verticalShift == Direction.Down && newY.HasValue) {
            newY -= SIZE.y / 2;
        }
        if(horizontalShift == Direction.Right && newX.HasValue) {
            newX += SIZE.x / 2;
        }
        else if(horizontalShift == Direction.Left && newX.HasValue) {
            newX -= SIZE.x / 2;
        }

        transform.position = new Vector3(newX.HasValue ? newX.Value : playerPos.x, newY.HasValue ? newY.Value : playerPos.y, transform.position.z);

        // find which chunk the player is in
        //int currentChunk = -1;
        //for(int i = 0; i < cameraZones.Count; i++) {
        //    if(cameraZones[i].Contains(playerPos, true)) {
        //        currentChunk = i;
        //        break;
        //    }
        //}

        //if(currentChunk < 0) {
        //    // special case: entering or exiting the area
        //    return;
        //}

        //// find which corners are outside the camera's allowed area
        //Vector2 topRight = (Vector2)playerPos + SIZE / 2;
        //bool topRightOutside = true;

        //Vector2 topLeft = (Vector2)playerPos + new Vector2(-SIZE.x, SIZE.y) / 2;
        //bool topLeftOutside = true;

        //Vector2 bottomRight = (Vector2)playerPos + new Vector2(SIZE.x, -SIZE.y) / 2;
        //bool bottomRightOutside = true;

        //Vector2 bottomLeft = (Vector2)playerPos - SIZE / 2;
        //bool bottomLeftOutside = true;

        //Rect cameraZone = cameraZones[currentChunk];
        //if(cameraZone.Contains(topLeft)) {
        //    topLeftOutside = false;
        //}
        //if(cameraZone.Contains(topRight)) {
        //    topRightOutside = false;
        //}
        //if(cameraZone.Contains(bottomLeft)) {
        //    bottomLeftOutside = false;
        //}
        //if(cameraZone.Contains(bottomRight)) {
        //    bottomRightOutside = false;
        //}

        //// clamp edges of the camera to the current zone
        //Vector3 targetPos = playerPos;
        //targetPos.z = transform.position.z;
        //if(topLeftOutside && bottomLeftOutside) {
        //    //fix left
        //    targetPos.x = cameraZones[currentChunk].xMin + SIZE.x / 2;
        //}
        //if(topLeftOutside && topRightOutside) {
        //    // fix top
        //    targetPos.y = cameraZones[currentChunk].yMax - SIZE.y / 2;
        //}
        //if(topRightOutside && bottomRightOutside) {
        //    // fix right
        //    targetPos.x = cameraZones[currentChunk].xMax - SIZE.x / 2;
        //}
        //if(bottomLeftOutside && bottomRightOutside) {
        //    // fix bottom
        //    targetPos.y = cameraZones[currentChunk].yMin + SIZE.y / 2;
        //}

        //// move toward target position
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

    public void AddCameraZone(Rect zone) {
        cameraZones.Add(zone);
    }
}
