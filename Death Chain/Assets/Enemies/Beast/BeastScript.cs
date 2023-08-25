using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeastScript : Enemy
{
    [SerializeField] private GameObject SlashPrefab;

    private const float SLASH_CD = 1.0f;
    private const float SLASH_STARTUP = 0.4f;
    private const float RUSH_CD = 6.0f;
    private const float RUSH_STARTUP = 0.8f;

    private bool preparingAttack;

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Chase, AIMode.Wander, 4.0f);
    }

    protected override void UpdateAbilities() {
        if(preparingAttack) {
            return;
        }

        if(UseAbility(0)) {
            // big slash
            preparingAttack = true;
            cooldowns[0] = SLASH_CD;
            SetWalkSpeed(0);
            Timer.CreateTimer(gameObject, SLASH_STARTUP, false, () => {
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

            Timer.CreateTimer(gameObject, RUSH_STARTUP, false, () => {
                preparingAttack = false;
                ResetWalkSpeed();
                Dash(22.0f * direction, 5.0f);
            });
        }
    }

    public override void AIUpdate(AIController controller) {
        
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
        if(hitEnemy != null) {
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
