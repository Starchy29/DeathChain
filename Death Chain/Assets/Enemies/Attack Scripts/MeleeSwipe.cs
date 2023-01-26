using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a melee attack that arcs around the user
public class MeleeSwipe : Attack
{
    private bool clockwise; // false: counter-clockwise
    private float currentAngle; // in radians
    private float targetAngle; // in radians

    [SerializeField] private float range; // distance from center of user
    [SerializeField] private float width; // angle in degrees
    [SerializeField] private float speed; // angle in degrees rotated per second
    [SerializeField] private float offset; // downward shift from game object center

    private bool finished; // inform the user when this is complete. They must check this variable every frame
    public bool Finished { get { return finished; } }

    // Update is called once per frame
    void Update()
    {
        currentAngle += (speed / 180 * Mathf.PI) * Time.deltaTime * (clockwise ? -1 : 1);

        if(clockwise && currentAngle <= targetAngle ||
            !clockwise && currentAngle >= targetAngle
        ) {
            finished = true;
        }
        else {
            transform.position = User.transform.position + new Vector3(0, -offset, 0) + range * new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0);
        }
    }

    // called by the attack user
    public void SetAim(Vector2 aim, bool clockwise) {
        this.clockwise = clockwise;

        float radianWidth = width / 180 * Mathf.PI;
        currentAngle = Mathf.Atan2(aim.y, aim.x) + radianWidth / 2 * (clockwise ? 1 : -1);
        targetAngle = currentAngle + radianWidth * (clockwise ? -1 : 1);
        transform.position = User.transform.position + new Vector3(0, -offset, 0) + range * new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0);
    }

    protected override Vector2 GetPushDirection(GameObject hitEnemy) {
        return hitEnemy.transform.position - User.transform.position;
    }
}
