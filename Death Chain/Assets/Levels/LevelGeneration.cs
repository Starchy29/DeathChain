using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelGeneration : MonoBehaviour
{
    [SerializeField] private List<GameObject> levelChuckPrefabs;

    // Start is called before the first frame update
    void Start()
    {
        Generate(5);
    }

    private void Generate(int numChunks) {
        GameObject currentChunk = MakeRandomChunk();
        currentChunk.transform.position = Vector3.zero;
        List<GameObject> chunks = new List<GameObject>() { currentChunk };

        // create that many chunks and attach them linearly
        for(int i = 1; i < numChunks; i++) {
            GameObject newChunk = MakeRandomChunk();
            chunks.Add(newChunk);

            Vector3 doorwayHeight = new Vector3(0, currentChunk.transform.GetChild(1).localScale.y, 0);
            Vector3 startPoint = currentChunk.transform.GetChild(1).position; // exit of last chunk
            Vector3 startToMid = newChunk.transform.position - newChunk.transform.GetChild(0).position; // middle of this chunk minus start of this chunk
            newChunk.transform.position = startPoint + startToMid + doorwayHeight;

            // 50% chance to invert horizontally
            if(Random.Range(0f, 1f) < 0.5f) {
                newChunk.transform.localScale = new Vector3(-1, 1, 1);
                startToMid.x *= -1;
                newChunk.transform.position = startPoint + startToMid + doorwayHeight;
            }

            currentChunk = newChunk;
        }


        // calculate rectangle areas of all chunks
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
            //CameraScript.Instance.AddCameraZone(new Rect(left, bottom, right - left, top - bottom));
        }
    }

    private GameObject MakeRandomChunk() {
        return Instantiate(levelChuckPrefabs[Random.Range(0, levelChuckPrefabs.Count)]);
    }

    // check for player reaching end goal
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == PlayerScript.Instance.PlayerEntity) {
            SceneManager.LoadScene("Main Menu");
        }
    }
}
