using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeScript : Enemy
{
    [SerializeField] private Sprite[] shootSprites;
    [SerializeField] private GameObject DropPrefab;
    [SerializeField] private GameObject PuddlePrefab;
    private const float SHOOT_CD = 1.0f;
    private const float PUDDLE_CD = 5.0f;

    private Animation shootAnimation;

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Wander, AIMode.Wander, 7.0f);

        idleAnimation = new Animation(idleSprites, AnimationType.Oscillate, 0.5f);
        walkAnimation = new Animation(walkSprites, AnimationType.Loop, 0.5f);
        deathAnimation = new Animation(deathSprites, AnimationType.Forward, 0.6f);
        shootAnimation = new Animation(shootSprites, AnimationType.Rebound, 0.2f);
    }

    protected override void UpdateAbilities() {
        if(UseAbility(0)) {
            // use quad shot ability
            cooldowns[0] = SHOOT_CD;

            StartAnimation(shootAnimation);

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
            cooldowns[1] = PUDDLE_CD;
            StartAnimation(shootAnimation);
            CreateAttack(PuddlePrefab);
        }
    }

    private bool aiToggle;
    public override void AIUpdate(AIController controller) {
        if(cooldowns[1] <= 0) {
            if(controller.Target == null) {
                controller.SetAim(new Vector2(Random.value - 0.5f, Random.value - 0.5f));
            }
            controller.QueueAbility(1, 0.6f, 0.6f);
        }
        else if(cooldowns[0] <= 0 && controller.GetMoveDirection() == Vector2.zero) {
            aiToggle = !aiToggle;
            if(aiToggle) {
                controller.SetAim(new Vector2(1, 0));
            } else {
                controller.SetAim(new Vector2(1, 1));
            }
            controller.QueueAbility(0, 0.5f, 0.4f);
        }
    }
}
