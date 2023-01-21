using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The base form of the player
public class PlayerGhost : Enemy
{
    private const float BASE_WALK_SPEED = 5.0f;
    private const float ATTACK_WALK_SPEED = 2.0f;
    [SerializeField] private float slashCooldown;
    [SerializeField] private float shootCooldown;
    private float invulnTimer;

    public GameObject SlashPrefab;
    private GameObject currentSlash; // null means not currently slashing
    private bool clockwise; // which way the slash should go, alternates every time

    public GameObject ShotPrefab;

    protected override void ChildStart()
    {
        controller = new PlayerController(gameObject);
        isAlly = true;
        maxSpeed = BASE_WALK_SPEED;
    }

    protected override void UpdateAbilities() {
        if(currentSlash != null) {
            // slash updates on its own

            if(currentSlash.GetComponent<MeleeSwipe>().Finished) {
                Destroy(currentSlash);
                currentSlash = null;
                maxSpeed = BASE_WALK_SPEED;
            }
        } else {
            int ability = controller.GetUsedAbility();

            switch(ability) {
                case 0: // slash
                    if(cooldowns[0] <= 0) {
                        cooldowns[0] = slashCooldown;
                        clockwise = !clockwise;
                        maxSpeed = ATTACK_WALK_SPEED; // slow while slashing

                        currentSlash = Instantiate(SlashPrefab);
                        MeleeSwipe slashScript = currentSlash.GetComponent<MeleeSwipe>();
                        slashScript.User = this.gameObject;
                        slashScript.SetAim(controller.GetAimDirection(), clockwise);
                    }
                    break;

                case 1: // shoot
                    if(cooldowns[1] <= 0) {
                        cooldowns[1] = shootCooldown;
                        GameObject shot = Instantiate(ShotPrefab);
                        shot.transform.position = transform.position;
                        Projectile script = shot.GetComponent<Projectile>();
                        script.User = this.gameObject;
                        script.SetDirection(controller.GetAimDirection());

                        maxSpeed = ATTACK_WALK_SPEED;
                    }
                    break;
            }
        }

        if(maxSpeed == ATTACK_WALK_SPEED && cooldowns[1] <= shootCooldown - 0.2f) {
            maxSpeed = BASE_WALK_SPEED;
        }
        if(invulnTimer > 0) {
            invulnTimer -= Time.deltaTime;
            if(invulnTimer <= 0) {
                invincible = false;
            }
        }
    }

    // when unpossessing, allow the player info tracker to pass the right values for the player
    public void Setup(int health) {
        this.health = health;
        invincible = true;
        invulnTimer = 1.0f;
    }
}
