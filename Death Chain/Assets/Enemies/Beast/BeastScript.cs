using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeastScript : Enemy
{
    [SerializeField] private GameObject SlashPrefab;

    private const float SLASH_CD = 1.0f;
    private const float SLASH_STARTUP = 0.6f;
    private const float RUSH_CD = 6.0f;
    private const float RUSH_STARTUP = 0.8f;

    private bool preparingAttack;
    private Timer startupTimer;

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Chase, AIMode.Wander, 5.0f);

        idleAnimation = new Animation(idleSprites, AnimationType.Oscillate, 0.5f);
    }

    protected override void UpdateAbilities() {
        if(preparingAttack) {
            return;
        }

        if(UseAbility(0)) {
            // big slash
            preparingAttack = true;
            cooldowns[0] = SLASH_CD + SLASH_STARTUP;
            SetWalkSpeed(0);
            startupTimer = Timer.CreateTimer(gameObject, SLASH_STARTUP, false, () => {
                preparingAttack = false;
                ResetWalkSpeed();
                CreateAttack(SlashPrefab, true);
                ApplyEndlag(0.2f, 0.0f);
            });
        }
        else if(UseAbility(1)) {
            // rush
            preparingAttack = true;
            cooldowns[1] = RUSH_CD;
            SetWalkSpeed(0);

            // determine direction when button is pressed instead of when dash starts
            Vector2 direction = controller.GetMoveDirection();
            if(direction == Vector2.zero) {
                direction = controller.GetAimDirection();
            }

            startupTimer = Timer.CreateTimer(gameObject, RUSH_STARTUP, false, () => {
                preparingAttack = false;
                ResetWalkSpeed();
                Dash(22.0f * direction, 2.0f);
            });
        }
    }

    protected override void ResetAndClear()
    {
        preparingAttack = false;
        if(startupTimer != null) {
            startupTimer.End();
            startupTimer = null;
        }
    }

    private float rangeTimer;
    public override void AIUpdate(AIController controller) {
        if(controller.Target == null) {
            controller.IgnoreStart = false;
            return;
        }
        //controller.IgnoreStart = true;

        float targetDistance = controller.GetTargetDistance();

        // track how long the enemy has been in rush range
        if(targetDistance >= 4.5f) {
            rangeTimer += Time.deltaTime;
        } else {
            rangeTimer = 0;
        }

        if(cooldowns[0] <= 0 && targetDistance <= 2.5f) {
            controller.QueueAbility(0);
        }
        else if(cooldowns[1] <= 0 && rangeTimer >= 1.0f) {
            controller.QueueAbility(1);
        }
    }

    // stop dashing when running into a wall or enemy
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!Dashing) {
            return;
        }

        EndDash();

        // deal damage if this hit an enemy
        Enemy hitEnemy = collision.gameObject.GetComponent<Enemy>();
        if(hitEnemy != null && hitEnemy.IsAlly != isAlly) {
            hitEnemy.TakeDamage(5);
            Vector2 pushDir = (hitEnemy.transform.position - transform.position).normalized;
            hitEnemy.Push(18.0f * pushDir);
            return;
        }

        // destroy breakable walls
        BreakableWallScript hitWall = collision.gameObject.GetComponent<BreakableWallScript>();
        if(hitWall != null) {
            hitWall.TakeDamage(100);
        }
    }
}
