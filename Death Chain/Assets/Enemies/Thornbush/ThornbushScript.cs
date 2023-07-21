using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornbushScript : Enemy
{
    [SerializeField] private GameObject SpikeCounterPrefab;
    [SerializeField] private GameObject ThornBulletPrefab;

    private bool trapping;
    private bool counterAttacking;
    private Timer trapTimer;

    private const float SHOOT_CD = 1.2f;
    private const float COUNTER_CD = 3.0f;

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Flee, AIMode.Wander, 5.0f);
    }

    protected override void UpdateAbilities() {
        if(trapping || counterAttacking) {
            return;
        }

        if(UseAbility(0)) {
            // triple thorn shot
            cooldowns[0] = SHOOT_CD;
            Vector2 aim = controller.GetAimDirection();
            Vector2 perp = new Vector2(-aim.y, aim.x) * 0.5f;
            Vector2[] directions = new Vector2[3] {
                aim,
                (aim + perp).normalized,
                (aim - perp).normalized
            };

            foreach(Vector2 direction in directions) {
                Projectile projectile = CreateAttack(ThornBulletPrefab, true).GetComponent<Projectile>();
                projectile.SetDirection(direction);
            }
        }
        else if(UseAbility(1)) {
            // trap counter
            cooldowns[1] = COUNTER_CD;
            trapping = true;
            trapTimer = Timer.CreateTimer(gameObject, 0.5f, false, () => { 
                trapping = false;
                ResetWalkSpeed();
                GetComponent<SpriteRenderer>().color = Color.white;
            });
            SetWalkSpeed(0);
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    public override void TakeDamage(int amount, bool ignoreStatus = false) {
        if(trapping) {
            // trigger counter attack
            GetComponent<SpriteRenderer>().color = Color.white;
            trapping = false;
            counterAttacking = true;
            if(trapTimer != null) {
                trapTimer.End();
            }
            CreateAttack(SpikeCounterPrefab);
            Timer.CreateTimer(gameObject, 0.3f, false, () => {
                counterAttacking = false;
                ResetWalkSpeed();
            });
        } else {
            base.TakeDamage(amount, ignoreStatus);
        }
    }

    public override void AIUpdate(AIController controller) {
        if(controller.Target == null) {
            return;
        }

        if(cooldowns[1] <= 0 && controller.GetTargetDistance() <= 2.0f) {
            controller.QueueAbility(1);
        }
        else if(cooldowns[0] <= 0) {
            controller.QueueAbility(0, 0.6f);
        }
    }
}
