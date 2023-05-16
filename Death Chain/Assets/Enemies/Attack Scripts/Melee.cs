using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an attack that stays adjacent to the user for a duration. The duration is set by the game object's particle script
[RequireComponent(typeof(Particle))]
public class Melee : Attack
{
    [SerializeField] private float range; // distance from center of user
    private Vector3 direction;

    void Update()
    {
        transform.position = User.transform.position + range * direction;
        float rotation = Mathf.Atan2(direction.y, direction.x) * 180 / Mathf.PI;
        if(direction.x < 0) {
            GetComponent<SpriteRenderer>().flipX = true;
            rotation += 180;
        }
        transform.rotation = Quaternion.Euler(0, 0, rotation);
        
        // auto-deleted by attached particle script
    }

    // must be called whenever this is created to set the aim direction
    public void SetAim(Vector2 direction) {
        this.direction = (Vector3)direction;
    }

    // push away from user
    protected override Vector2 GetPushDirection(GameObject hitEnemy) {
        return hitEnemy.transform.position - User.transform.position;
    }
}
