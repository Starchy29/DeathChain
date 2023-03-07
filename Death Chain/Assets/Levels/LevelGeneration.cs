using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        // create that many chunks and attach them linearly
        for(int i = 1; i < numChunks; i++) {
            GameObject newChunk = MakeRandomChunk();

            Vector3 startPoint = currentChunk.transform.GetChild(1).position; // exit of last chunk
            Vector3 startToMid = newChunk.transform.position - newChunk.transform.GetChild(0).position; // middle of this chunk minus start of this chunk
            newChunk.transform.position = startPoint + startToMid;

            currentChunk = newChunk;
        }
    }

    private GameObject MakeRandomChunk() {
        return Instantiate(levelChuckPrefabs[Random.Range(0, levelChuckPrefabs.Count)]);
    }
}
