using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// should be attached to an empty game object whose children are all squares. The squares together define the area of the pit
public class PitScript : ObstacleScript
{
    private bool blockedLeft;
    private bool blockedRight;
    private bool blockedUp;
    private bool blockedDown;
    private bool blockChecked = false;

    public bool BlockedLeft { get { return blockedLeft; } }
    public bool BlockedRight { get { return blockedRight; } }
    public bool BlockedUp { get { return blockedUp; } }
    public bool BlockedDown { get { return blockedDown; } }

    void Update()
    {
        // check which sides are blocked after all obstacles have loaded in
        blockChecked = true;
        if(!blockChecked) {
            blockChecked = true;
            Debug.Log(EntityTracker.Instance.Obstacles.Count);

            // create a rectangle on each side to check for intersection
            const float CHECK_WIDTH = 1.0f;
            Rect top = new Rect(area.yMax + CHECK_WIDTH, area.xMin, area.width, CHECK_WIDTH);
            Rect bottom = new Rect(area.yMin, area.xMin, area.width, CHECK_WIDTH);
            Rect left = new Rect(area.xMin - CHECK_WIDTH, area.yMax, CHECK_WIDTH, area.height);
            Rect right = new Rect(area.xMax, area.yMax, CHECK_WIDTH, area.height);

            // find which sides are blocked
            foreach(GameObject obstacle in EntityTracker.Instance.Obstacles) {
                Rect checkArea = obstacle.GetComponent<ObstacleScript>().Area;
                if(top.Overlaps(checkArea)) {
                    blockedUp = true;
                    Debug.Log("up is blocked");
                }
                if(bottom.Overlaps(checkArea)) {
                    blockedDown = true;
                    Debug.Log("down is blocked");
                }
                if(left.Overlaps(checkArea)) {
                    blockedLeft = true;
                    Debug.Log("left is blocked");
                }
                if(right.Overlaps(checkArea)) {
                    blockedRight = true;
                    Debug.Log("right is blocked");
                }
            }
            
            // if all edges are blocked, remove this pit
            if(blockedRight && blockedLeft && blockedUp && blockedDown) {
                EntityTracker.Instance.Obstacles.Remove(gameObject);
                Destroy(gameObject);
                Debug.Log("Deleted a pit because it was blocked on all sides");
            }
        }

        List<GameObject> enemies = EntityTracker.Instance.Enemies;
        foreach(GameObject enemy in enemies) {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            Vector3 pos = enemy.transform.position;
            if(enemyScript.Floating || enemyScript.CurrentState != Enemy.State.Normal || !area.Contains(pos)) {
                continue;
            }

            float radius = enemy.GetComponent<Enemy>().CollisionRadius;
            if(pos.x - radius >= area.xMin && pos.x + radius <= area.xMax
                && pos.y - radius >= area.yMin && pos.y + radius <= area.yMax
            ) {
                enemyScript.FallInPit(this);
            }
        }
    }
}
