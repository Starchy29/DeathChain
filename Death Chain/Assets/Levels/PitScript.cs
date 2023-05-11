using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// should be attached to an empty game object whose children are all squares. The squares together define the area of the pit. Empty must have scale (1, 1, 1)
public class PitScript : MonoBehaviour
{
    private List<Rect> zones;
    private const float EDGE_BUFFER = 0.5f; // edges might not line up exactly. As long as they are this distance from each other, they are considered lining up 

    private void Start()
    {
        // these should be the values in the empty parent game object. There will be visible differences if it is set up incorrectly in the inspector
        transform.position = new Vector3(0, 0, 0);
        transform.localScale = new Vector3(1, 1, 1);
        transform.rotation = Quaternion.Euler(0, 0, 0);

        zones = new List<Rect>();

        // create rectangles from child squares
        for(int i = 0; i < transform.childCount; i++) {
            Transform rectTransform = transform.GetChild(i);
            Vector2 center = rectTransform.position;
            Vector2 dims = rectTransform.localScale; // requires parent scale (1, 1, 1)
            zones.Add(new Rect(center - dims / 2, dims));
        }

        // add rectangles that overlap edges to prevent characters from walking across
        List<Rect> additionalZones = new List<Rect>();
        for(int i = 0; i < zones.Count; i++) {
            for(int j = i + 1; j < zones.Count; j++) {
                // check if rects have a shared edge
                if(Mathf.Abs(zones[i].xMin - zones[j].xMax) <= EDGE_BUFFER || Mathf.Abs(zones[i].xMax - zones[j].xMin) <= EDGE_BUFFER) {
                    // add overlapping rectangle
                    float leftEdge = Mathf.Min(zones[i].xMin, zones[j].xMin);
                    float rightEdge = Mathf.Max(zones[i].xMax, zones[j].xMax);
                    float topEdge = Mathf.Min(zones[i].yMax, zones[j].yMax);
                    float bottomEdge = Mathf.Max(zones[i].yMin, zones[j].yMin);
                    additionalZones.Add(new Rect(leftEdge, bottomEdge, rightEdge - leftEdge, topEdge - bottomEdge));
                }
                else if(Mathf.Abs(zones[i].yMin - zones[j].yMax) <= EDGE_BUFFER || Mathf.Abs(zones[i].yMax - zones[j].yMin) <= EDGE_BUFFER) {
                    // add overlapping rectangle
                    float topEdge = Mathf.Max(zones[i].yMax, zones[j].yMax);
                    float bottomEdge = Mathf.Min(zones[i].yMin, zones[j].yMin);
                    float leftEdge = Mathf.Max(zones[i].xMin, zones[j].xMin);
                    float rightEdge = Mathf.Min(zones[i].xMax, zones[j].xMax);
                    additionalZones.Add(new Rect(leftEdge, bottomEdge, rightEdge - leftEdge, topEdge - bottomEdge));
                }
            }
        }
        zones.AddRange(additionalZones);

        // remove any rectangles that are wholly contained by another
        for(int i = zones.Count - 1; i >= 0; i--) {
            for(int j = i - 1; j >= 0; j--) {
                if(zones[j].Contains(zones[i])) {
                    zones.RemoveAt(i);
                    break;
                }
                if(zones[i].Contains(zones[j])) {
                    zones.RemoveAt(j);
                    i--;
                }
            }
        }
    }

    void Update()
    {
        // check which sides are blocked after all obstacles have loaded in
        //blockChecked = true;
        //if(!blockChecked) {
        //    blockChecked = true;
        //    Debug.Log(EntityTracker.Instance.Obstacles.Count);

        //    // create a rectangle on each side to check for intersection
        //    const float CHECK_WIDTH = 1.0f;
        //    Rect top = new Rect(area.yMax + CHECK_WIDTH, area.xMin, area.width, CHECK_WIDTH);
        //    Rect bottom = new Rect(area.yMin, area.xMin, area.width, CHECK_WIDTH);
        //    Rect left = new Rect(area.xMin - CHECK_WIDTH, area.yMax, CHECK_WIDTH, area.height);
        //    Rect right = new Rect(area.xMax, area.yMax, CHECK_WIDTH, area.height);

        //    // find which sides are blocked
        //    foreach(GameObject obstacle in EntityTracker.Instance.Obstacles) {
        //        Rect checkArea = obstacle.GetComponent<ObstacleScript>().Area;
        //        if(top.Overlaps(checkArea)) {
        //            blockedUp = true;
        //            Debug.Log("up is blocked");
        //        }
        //        if(bottom.Overlaps(checkArea)) {
        //            blockedDown = true;
        //            Debug.Log("down is blocked");
        //        }
        //        if(left.Overlaps(checkArea)) {
        //            blockedLeft = true;
        //            Debug.Log("left is blocked");
        //        }
        //        if(right.Overlaps(checkArea)) {
        //            blockedRight = true;
        //            Debug.Log("right is blocked");
        //        }
        //    }
            
        //    // if all edges are blocked, remove this pit
        //    if(blockedRight && blockedLeft && blockedUp && blockedDown) {
        //        EntityTracker.Instance.Obstacles.Remove(gameObject);
        //        Destroy(gameObject);
        //        Debug.Log("Deleted a pit because it was blocked on all sides");
        //    }
        //}

        List<GameObject> enemies = EntityTracker.Instance.Enemies;
        foreach(GameObject enemy in enemies) {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if(enemyScript.Floating || enemyScript.CurrentState != Enemy.State.Normal) {
                continue;
            }

            Vector3 pos = enemy.transform.position;
            float radius = enemy.GetComponent<Enemy>().CollisionRadius;
            Rect hitbox = new Rect(pos.x - radius, pos.y - radius, 2 * radius, 2 * radius);
            foreach(Rect zone in zones) {
                if(zone.Contains(hitbox)) {
                    enemyScript.FallInPit(zone);
                    break;
                }
            }
        }
    }
}
