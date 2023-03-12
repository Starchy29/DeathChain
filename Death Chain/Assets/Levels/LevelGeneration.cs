using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneration : MonoBehaviour
{
    [SerializeField] private List<GameObject> levelChuckPrefabs;
    //[SerializeField] private GameObject debugZone;

    private List<Rect> chunkRects;
    public List<Rect> ChunkRects { get { return chunkRects; } }
    private List<GameObject> chunks;
    public List<GameObject> Chunks { get { return chunks; } }

    // Start is called before the first frame update
    void Start()
    {
        Generate(5);
    }

    private void Generate(int numChunks) {
        GameObject currentChunk = MakeRandomChunk();
        currentChunk.transform.position = Vector3.zero;
        chunks = new List<GameObject>() { currentChunk };

        // create that many chunks and attach them linearly
        for(int i = 1; i < numChunks; i++) {
            GameObject newChunk = MakeRandomChunk();
            chunks.Add(newChunk);

            Vector3 startPoint = currentChunk.transform.GetChild(1).position; // exit of last chunk
            Vector3 startToMid = newChunk.transform.position - newChunk.transform.GetChild(0).position; // middle of this chunk minus start of this chunk
            newChunk.transform.position = startPoint + startToMid;

            // 50% chance to invert horizontally
            if(Random.Range(0f, 1f) < 0.5f) {
                newChunk.transform.localScale = new Vector3(-1, 1, 1);
                startToMid.x *= -1;
                newChunk.transform.position = startPoint + startToMid;
            }

            currentChunk = newChunk;
        }

        // calculate rectangle areas of all chunks
        chunkRects = new List<Rect>();
        foreach(GameObject chunk in chunks) {
            float top = float.MinValue;
            float bottom = float.MaxValue;
            float right = float.MinValue;
            float left = float.MaxValue;

            // check all border walls that are children of this
            float childCount = chunk.transform.childCount;
            for(int i = 0; i < childCount; i++) {
                if(chunk.transform.GetChild(i).gameObject.layer != 12) { // 12 is border layer
                    continue;
                }

                // check if any side is the furthest out
                Transform borderTransform = chunk.transform.GetChild(i).transform;
                float borderTop = borderTransform.position.y + borderTransform.localScale.y / 2;
                float borderBottom = borderTransform.position.y - borderTransform.localScale.y / 2;
                float borderRight = borderTransform.position.x + borderTransform.localScale.x / 2;
                float borderLeft = borderTransform.position.x - borderTransform.localScale.x / 2;
                
                if(borderTop > top) {
                    top = borderTop;
                }
                if(borderBottom < bottom) {
                    bottom = borderBottom;
                }
                if(borderRight > right) {
                    right = borderRight;
                }
                if(borderLeft < left) {
                    left = borderLeft;
                }
            }

            // form rectangle
            chunkRects.Add(new Rect(left, bottom, right - left, top - bottom));
        }

        // DEBUG show rects
        //foreach(Rect rect in chunkRects) {
        //    GameObject zone = Instantiate(debugZone);
        //    zone.transform.position = rect.center;
        //    zone.transform.localScale = new Vector3(rect.width, rect.height, 1);
        //}
    }

    private GameObject MakeRandomChunk() {
        return Instantiate(levelChuckPrefabs[Random.Range(0, levelChuckPrefabs.Count)]);
    }
}
