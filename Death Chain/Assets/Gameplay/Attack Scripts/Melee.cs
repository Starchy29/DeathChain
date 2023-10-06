using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an attack that stays adjacent to the user for a duration
public class Melee : Attack
{
    [SerializeField] private float range; // distance from center of user
    [SerializeField] private float meleeDuration;
    private Vector3 direction;

    void Start()
    {
        transform.position = User.transform.position + range * direction;

        float rotation = Mathf.Atan2(direction.y, direction.x) * 180 / Mathf.PI;
        if (direction.x < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            rotation += 180;
        }
        transform.rotation = Quaternion.Euler(0, 0, rotation);
    }

    void Update()
    {
        transform.position = User.transform.position + range * direction;

        meleeDuration -= Time.deltaTime;
        if(meleeDuration <= 0) {
            Destroy(gameObject);
        }
    }

    // must be called whenever this is created to set the aim direction
    public override void SetDirection(Vector2 direction) {
        this.direction = (Vector3)direction;
    }

    // makes the animation play as a swipe going in the opposite direction
    public void ReverseDirection() {
        GetComponent<SpriteRenderer>().flipY = true;
    }

    // push away from user
    protected override Vector2 GetPushDirection(GameObject hitEnemy) {
        return hitEnemy.transform.position - User.transform.position;
    }
}
