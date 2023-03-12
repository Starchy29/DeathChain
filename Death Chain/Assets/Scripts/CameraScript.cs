using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private PlayerScript playerScript;
    [SerializeField] private LevelGeneration levelGenerator;
    private readonly Vector2 SIZE = new Vector2(21.33f, 12); // values found through observation

    void Awake()
    {
        Vector3 playerPos = playerScript.PlayerEntity.transform.position;
        playerPos.z = transform.position.z;
        transform.position = playerPos;
    }

    void Update()
    {
        Vector3 playerPos = playerScript.PlayerEntity.transform.position;

        if(levelGenerator.ChunkRects.Count <= 0) {
            transform.position = new Vector3(playerPos.x, playerPos.y, transform.position.z);
            return;
        }

        // find which chunk the player is in
        List<Rect> chunkRects = levelGenerator.ChunkRects;
        int currentChunk = -1;
        for(int i = 0; i < chunkRects.Count; i++) {
            if(chunkRects[i].Contains(playerPos, true)) {
                currentChunk = i;
                break;
            }
        }

        if(currentChunk < 0) {
            // special case: entering or exiting the area
            return;
        }

        // find which corners are outside the camera's allowed area
        Vector2 topRight = (Vector2)playerPos + SIZE / 2;
        bool topRightOutside = true;

        Vector2 topLeft = (Vector2)playerPos + new Vector2(-SIZE.x, SIZE.y) / 2;
        bool topLeftOutside = true;

        Vector2 bottomRight = (Vector2)playerPos + new Vector2(SIZE.x, -SIZE.y) / 2;
        bool bottomRightOutside = true;

        Vector2 bottomLeft = (Vector2)playerPos - SIZE / 2;
        bool bottomLeftOutside = true;

        Rect cameraZone = chunkRects[currentChunk];
        if(cameraZone.Contains(topLeft)) {
            topLeftOutside = false;
        }
        if(cameraZone.Contains(topRight)) {
            topRightOutside = false;
        }
        if(cameraZone.Contains(bottomLeft)) {
            bottomLeftOutside = false;
        }
        if(cameraZone.Contains(bottomRight)) {
            bottomRightOutside = false;
        }

        // check next and previous zone if the doorway is on screen
        if(currentChunk < levelGenerator.Chunks.Count - 1) {
            Transform exitMarker = levelGenerator.Chunks[currentChunk].transform.GetChild(1);
            Vector3 exitPos = exitMarker.position;
            Vector3 exitScale = exitMarker.localScale;
            if(exitPos.x + exitScale.x / 2 < topRight.x
                && exitPos.x - exitScale.x / 2 > topLeft.x
                && exitPos.y + exitScale.y / 2 < topRight.y
                && exitPos.y - exitScale.y / 2 > bottomRight.y
            ) { // if the exit zone is on screen, check if the camera's corners are in the next zone
                if(chunkRects[currentChunk + 1].Contains(topLeft)) {
                    topLeftOutside = false;
                }
                if(chunkRects[currentChunk + 1].Contains(topRight)) {
                    topRightOutside = false;
                }
            }
        }

        if(currentChunk > 0) {
            Transform enterMarker = levelGenerator.Chunks[currentChunk].transform.GetChild(0);
            Vector3 enterPos = enterMarker.position;
            Vector3 enterScale = enterMarker.localScale;
            if(enterPos.x + enterScale.x / 2 < topRight.x
                && enterPos.x - enterScale.x / 2 > topLeft.x
                && enterPos.y + enterScale.y / 2 < topRight.y
                && enterPos.y - enterScale.y / 2 > bottomRight.y
            ) { // if the entrance is on screen, check if the camera's corners are in the previous zone
                if(chunkRects[currentChunk - 1].Contains(bottomLeft)) {
                    bottomLeftOutside = false;
                }
                if(chunkRects[currentChunk - 1].Contains(bottomRight)) {
                    bottomRightOutside = false;
                }
            }
        }

        // clamp edges of the camera to the current zone
        Vector3 targetPos = playerPos;
        if(topLeftOutside && bottomLeftOutside) {
            //fix left
            targetPos.x = chunkRects[currentChunk].xMin + SIZE.x / 2;
        }
        if(topLeftOutside && topRightOutside) {
            // fix top
            targetPos.y = chunkRects[currentChunk].yMax - SIZE.y / 2;
        }
        if(topRightOutside && bottomRightOutside) {
            // fix right
            targetPos.x = chunkRects[currentChunk].xMax - SIZE.x / 2;
        }
        if(bottomLeftOutside && bottomRightOutside) {
            // fix bottom
            targetPos.y = chunkRects[currentChunk].yMin + SIZE.y / 2;
        }


        // accelerate toward target position
        Vector3 difference = targetPos - transform.position;
        difference.z = 0;
        float distance = difference.magnitude;
        if(distance < 0.2f) {
            targetPos.z = transform.position.z;
            transform.position = targetPos;
        }
        else {
            float speed = 2 * distance;
            if(speed < 6f) {
                transform.position += 6 * difference.normalized * Time.deltaTime;
            } else {
                transform.position +=  2 * difference * Time.deltaTime;
            }
        }
    }
}
