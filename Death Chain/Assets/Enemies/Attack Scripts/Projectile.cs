using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an attack that launches straight forward. Can be inherited for special features
public class Projectile : Attack
{
    [SerializeField] protected float speed;
    [SerializeField] protected float range;
    [SerializeField] protected GameObject destroyParticle; // animation that plays when this is destroyed

    protected Vector3 velocity; // z should be 0
    protected float distance; // distance travelled 

    void Update()
    {
        Vector3 displacement = velocity * Time.deltaTime;

        gameObject.transform.position += displacement;
        gameObject.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * 180 / Mathf.PI);

        distance += displacement.magnitude;
        if(distance >= range) {
            Delete();
        }
    }

    // Must be called each time one is created. Input vector should have length 1 
    public void SetDirection(Vector2 direction) {
        velocity = direction * speed; // auto cast to vec3
    }

    protected override Vector2 GetPushDirection(GameObject hitEnemy) {
        return velocity;
    }

    protected override void OnEnemyCollision(Enemy hitEnemy) {
        Delete();
    }

    protected override void OnWallCollision(GameObject hitWall) {
        Delete();
    }

    protected void Delete() {
        if(destroyParticle) {
            GameObject particle = Instantiate(destroyParticle);
            particle.transform.position = transform.position;
            particle.transform.rotation = transform.rotation;
        }

        Destroy(gameObject);
    }
}