using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeScript : Enemy
{
    [SerializeField] private GameObject DropPrefab;
    [SerializeField] private GameObject PuddlePrefab;
    [SerializeField] private float DropCooldown;
    [SerializeField] private float PuddleCooldown;

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Wander, 0.0f);
        maxSpeed = 4.0f;
    }

    protected override void UpdateAbilities() {
        if(UseAbility(0)) {
            // use blast ability
            cooldowns[0] = DropCooldown;

            //currentAnimation = attackAnimation;
            //attackAnimation.Reset();

            List<Vector2> fireDirections;

            // determine if aim is cardinals or diagonals
            Vector2 aim = controller.GetAimDirection();
            Vector2 firstQuad = new Vector2(Mathf.Abs(aim.x), Mathf.Abs(aim.y));
            float diagDotProd = Vector2.Dot(Vector2.one.normalized, firstQuad);
            if(Vector2.Dot(Vector2.up, firstQuad) > diagDotProd || Vector2.Dot(Vector2.right, firstQuad) > diagDotProd) {
                // shoot in cardinal directions
                fireDirections = new List<Vector2>(){ Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            } else {
                // shoot in diagonals
                float length = Mathf.Sqrt(2) / 2;
                fireDirections = new List<Vector2>() { new Vector2(length, length), new Vector2(-length, length), new Vector2(length, -length), new Vector2(-length, -length) };
            }

            foreach(Vector2 direction in fireDirections) {
                GameObject shot = Instantiate(DropPrefab);
                shot.transform.position = transform.position;
                Projectile script = shot.GetComponent<Projectile>();
                script.User = this.gameObject;
                script.SetDirection(direction);
            }
        }
        else if(UseAbility(1)) {
            // use lob puddle ability
            cooldowns[1] = PuddleCooldown;

            //currentAnimation = attackAnimation;
            //attackAnimation.Reset();

            GameObject puddleDrop = Instantiate(PuddlePrefab);
            puddleDrop.transform.position = transform.position;
            puddleDrop.GetComponent<Lobber>().Setup(controller.GetAimDirection(), gameObject);
        }
    }

    public override void AIUpdate(AIController controller) {
        
    }
}
