using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an astract class for all obstacles to make sure they save a rectangle
public class ObstacleScript : MonoBehaviour
{
    protected Rect area;
    public Rect Area { get { return area; } }

    protected virtual void Start()
    {
        Vector3 corner = transform.position - transform.localScale / 2;
        area = new Rect(corner.x, corner.y, transform.localScale.x, transform.localScale.y);
        EntityTracker.Instance.AddObstacle(gameObject);
    }
}
