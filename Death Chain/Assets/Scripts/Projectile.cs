using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an attack that launches straight forward. Can be inherited for special features
public class Projectile : Attack
{
    [SerializeField] protected float speed;
    [SerializeField] protected float range;

    protected Vector3 velocity; // z should be 0
    protected float distance; // distance travelled 

    void Update()
    {
        Vector3 displacement = velocity * Time.deltaTime;

        gameObject.transform.position += displacement;
        distance += displacement.magnitude;
        if(distance >= range) {
            Destroy(gameObject);
        }
    }

    // Must be called each time one is created. Input vector should have length 1 
    public void SetDirection(Vector2 direction) {
        velocity = direction * speed; // auto cast to vec3
    }

    protected override void OnEnemyCollision(Enemy hitEnemy) {
        Destroy(gameObject);
    }

    protected override void OnWallCollision() {
        Destroy(gameObject);
    }
}
