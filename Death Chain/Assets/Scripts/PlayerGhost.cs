using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The base form of the player
public class PlayerGhost : Enemy
{
    private const float BASE_WALK_SPEED = 5.0f;
    private const float SLASH_WALK_SPEED = 2.0f;
    private const float SLASH_CD = 0.4f;
    private const float SHOOT_CD = 0.8f;
    private const float SLASH_WIDTH = 70.0f / 180 * Mathf.PI; // as an angle
    private const float SLASH_RANGE = 0.5f;

    private float slashCooldown;
    private float shootCooldown;

    public GameObject SlashPrefab;
    private GameObject currentSlash; // null means not currently slashing
    private float slashAngle; // the current angle relative to the player the slash should be
    private float targetAngle; // the angle that determines the slash is complete
    private bool clockwise; // which way the slash should go, alternates every time

    protected override void ChildStart()
    {
        controller = new PlayerController();
        ally = true;
        maxSpeed = BASE_WALK_SPEED;
    }

    public override void UpdateAbilities() {
        if(currentSlash != null) {
            const float SLASH_SPEED = 4.0f; // angle per second
            slashAngle += SLASH_SPEED * Time.deltaTime * (clockwise ? -1 : 1);

            // check for end of slash
            if(clockwise && slashAngle <= targetAngle ||
                !clockwise && slashAngle >= targetAngle
            ) {
                maxSpeed = BASE_WALK_SPEED;
                Destroy(currentSlash);
                currentSlash = null;
            } else {
                // shift slash
                currentSlash.transform.position = transform.position + SLASH_RANGE * new Vector3(Mathf.Cos(slashAngle), Mathf.Sin(slashAngle), 0);
                //currentSlash.transform.rotation = Quaternion.Euler(0, 0, 180 - slashAngle / Mathf.PI * 180);
            }
        } else {
            int ability = controller.GetUsedAbility();

            switch(ability) {
                case 0: // slash
                    if(slashCooldown <= 0) {
                        slashCooldown = SLASH_CD;
                        clockwise = !clockwise;
                        maxSpeed = SLASH_WALK_SPEED; // slow while slashing

                        currentSlash = Instantiate(SlashPrefab);
                        //currentSlash.GetComponent<Melee>().IsAlly = this.ally;

                        Vector2 aimVec = controller.GetAimDirection();
                        slashAngle = Mathf.Atan2(aimVec.y, aimVec.x) + SLASH_WIDTH / 2 * (clockwise ? 1 : -1);
                        targetAngle = slashAngle + SLASH_WIDTH * (clockwise ? -1 : 1);

                        currentSlash.transform.position = transform.position + SLASH_RANGE * new Vector3(Mathf.Cos(slashAngle), Mathf.Sin(slashAngle), 0);
                        //currentSlash.transform.rotation = Quaternion.Euler(0, 0, slashAngle / Mathf.PI * 180 + 90);
                    }
                    break;

                case 1: // shoot
                    if(shootCooldown <= 0) {
                        shootCooldown = SHOOT_CD;
                    }
                    break;

                case 3: // possess
                    break;

                case 143:
                    // Why do I keep getting attracted?
                    break;
            }
        }

        // decrease cooldowns
        if(slashCooldown > 0) {
            slashCooldown -= Time.deltaTime;
        }
        if(shootCooldown > 0) {
            shootCooldown -= Time.deltaTime;
        }
    }
}
