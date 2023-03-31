using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitScript : MonoBehaviour
{
    private Rect area; 

    void Start()
    {
        Vector3 corner = transform.position - transform.localScale / 2;
        area = new Rect(corner.x, corner.y, transform.localScale.x, transform.localScale.y);
    }

    void Update()
    {
        List<GameObject> enemies = EntityTracker.Instance.Enemies;
        foreach(GameObject enemy in enemies) {
            Vector3 pos = enemy.transform.position;
            if(!area.Contains(pos)) {
                continue;
            }

            float radius = enemy.GetComponent<CircleCollider2D>().radius;
            if(pos.x + radius >= area.xMin && pos.x - radius <= area.xMax
                && pos.y + radius >= area.yMin && pos.y - radius <= area.yMax
            ) {
                Debug.Log(enemy.name + " entered pit");
            }
        }
    }
}
