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
        controller = new AIController(gameObject, AIMode.Wander, 7.0f);

        idleAnimation = new Animation(idleSprites, AnimationType.Oscillate, 0.5f);
        walkAnimation = new Animation(walkSprites, AnimationType.Loop, 0.5f);
        deathAnimation = new Animation(deathSprites, AnimationType.Forward, 0.6f);
    }

    protected override void UpdateAbilities() {
        if(UseAbility(0)) {
            // use quad shot ability
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
                float length = Mathf.Sqrt(2) / 2.0f;
                fireDirections = new List<Vector2>() { new Vector2(length, length), new Vector2(-length, length), new Vector2(length, -length), new Vector2(-length, -length) };
            }

            foreach(Vector2 direction in fireDirections) {
                GameObject shot = CreateAttack(DropPrefab);
                shot.GetComponent<Projectile>().SetDirection(direction);
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
        if(cooldowns[1] <= 0) {
            if(controller.Target == null) {
                controller.SetAim(new Vector2(Random.value - 0.5f, Random.value - 0.5f));
            }
            controller.QueueAbility(1, 1);
        }
        else if(cooldowns[0] <= 0 && controller.GetMoveDirection() == Vector2.zero) {
            aiToggle = !aiToggle;
            if(aiToggle) {
                controller.SetAim(new Vector2(1, 0));
            } else {
                controller.SetAim(new Vector2(1, 1));
            }
            controller.QueueAbility(0, 0.5f);
        }
    }
    private bool aiToggle;
}
