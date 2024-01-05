using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an attack that launches straight forward. Can be inherited for special features
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : Attack
{
    [SerializeField] protected float speed;
    [SerializeField] protected float range;
    [SerializeField] protected GameObject destroyParticle; // animation that plays when this is destroyed

    protected float distance; // distance travelled
    protected Rigidbody2D physicsBody;

    private void Awake() {
        physicsBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        gameObject.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(physicsBody.velocity.y, physicsBody.velocity.x) * 180 / Mathf.PI);

        Vector3 displacement = physicsBody.velocity * Time.deltaTime;
        distance += displacement.magnitude;
        if(distance >= range) {
            EndAttack();
        }

        GetComponent<SpriteRenderer>().sortingOrder = (int)(-transform.position.y * 10); // layer relative to vertical position, sorting layer should be same as enemies
    }

    // Must be called each time one is created. Input vector should have length 1 
    public override void SetDirection(Vector2 direction) {
        physicsBody.velocity = direction * speed; // auto cast to vec3
    }

    // allows modifying the speed after creation
    public void SetSpeed(float speed) {
        physicsBody.velocity = speed * physicsBody.velocity.normalized;
    }

    protected override Vector2 GetPushDirection(GameObject hitEnemy) {
        return physicsBody.velocity;
    }

    protected override void OnEnemyCollision(Enemy hitEnemy) {
        EndAttack();
    }

    protected override void OnWallCollision(List<Vector3Int> hitTiles) {
        EndAttack();
    }

    protected void EndAttack() {
        if(destroyParticle) {
            GameObject particle = Instantiate(destroyParticle);
            particle.transform.position = transform.position;
            particle.transform.rotation = transform.rotation;
            SpriteRenderer renderer = particle.GetComponent<SpriteRenderer>();
            renderer.sortingLayerName = GetComponent<SpriteRenderer>().sortingLayerName;
            renderer.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder;
        }

        Destroy(gameObject);
    }
}
