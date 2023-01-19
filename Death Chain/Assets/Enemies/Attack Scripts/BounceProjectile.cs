using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceProjectile : Projectile
{
    [SerializeField] private int bounces;

    protected override void OnEnemyCollision(Enemy hitEnemy)
    {
        bounces--;
        if(bounces <= 0) {
            Destroy(gameObject);
        } else {
            Vector3 toTarget = hitEnemy.gameObject.transform.position - transform.position;
            Vector3 component = Vector3.Project(velocity, toTarget);
            velocity += -2 * component;
        }
    }

    protected override void OnWallCollision(GameObject hitWall)
    {
        bounces--;
        if(bounces <= 0) {
            Destroy(gameObject);
        } else {
            // assume center of circle is outside of the wall
            Vector3 center = transform.position;
            Vector3 wallCenter = hitWall.transform.position;
            Vector3 wallDims = hitWall.transform.localScale;
            bool horizontal = false;
            bool vertical = false;
            if(center.x > wallCenter.x + wallDims.x / 2 || center.x < wallCenter.x - wallDims.x / 2) {
                // if to the left or right, reflect horizontally
                velocity.x = -velocity.x;
                horizontal = true;
            } 
            if(center.y > wallCenter.y + wallDims.y / 2 || center.y < wallCenter.y - wallDims.y / 2) {
                velocity.y = -velocity.y;
                vertical = true;
            }

            // special cases
            if(vertical && horizontal) {
                // hit a corner, just move away from the center
                float length = velocity.magnitude;
                velocity = (center - wallCenter).normalized * length;
            }

            if(!vertical && !horizontal) {
                // if this is lodged inside a wall, just delete it
                Destroy(gameObject);
            }
        }
    }
}
