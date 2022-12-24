using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The base form of the player
public class PlayerGhost : Enemy
{
    private const float BASE_WALK_SPEED = 5.0f;
    private const float ATTACK_WALK_SPEED = 2.0f;
    private const float SLASH_CD = 0.4f;
    private const float SHOOT_CD = 0.8f;

    private float slashCooldown;
    private float shootCooldown;
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
                    if(slashCooldown <= 0) {
                        slashCooldown = SLASH_CD;
                        clockwise = !clockwise;
                        maxSpeed = ATTACK_WALK_SPEED; // slow while slashing

                        currentSlash = Instantiate(SlashPrefab);
                        MeleeSwipe slashScript = currentSlash.GetComponent<MeleeSwipe>();
                        slashScript.User = this.gameObject;
                        slashScript.SetAim(controller.GetAimDirection(), clockwise);
                    }
                    break;

                case 1: // shoot
                    if(shootCooldown <= 0) {
                        shootCooldown = SHOOT_CD;
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

        // decrease cooldowns
        if(slashCooldown > 0) {
            slashCooldown -= Time.deltaTime;
        }
        if(shootCooldown > 0) {
            shootCooldown -= Time.deltaTime;
            if(maxSpeed == ATTACK_WALK_SPEED && shootCooldown <= SHOOT_CD - 0.2f) {
                maxSpeed = BASE_WALK_SPEED;
            }
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
