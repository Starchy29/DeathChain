using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// tracks the status of the player, no matter which object they are possessing at the moment
public class PlayerScript : MonoBehaviour
{
    [SerializeField] private GameObject possessIndicator;
    [SerializeField] private GameObject playerCharacter; // the entity the player is currently playing as, manually set to ghost at first
    [SerializeField] private EntityTracker entityTracker;
    
    private int playerHealth;

    private float decayTimer;
    private const float DECAY_FREQ = 3.0f; // number of seconds for each damage dealt
    private const float POSSESS_RANGE = 1.5f; // how far away the player can be from a corpse and possess it

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PlayerGhost ghostScript = playerCharacter.GetComponent<PlayerGhost>();

        // -- manage possession --
        List<GameObject> enemies = entityTracker.Enemies;
        GameObject closestOption = null;
        float closestDistance = POSSESS_RANGE;
        foreach(GameObject enemy in enemies) {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if(enemyScript.IsCorpse) {
                float distance = Vector3.Distance(playerCharacter.transform.position, enemy.transform.position);
                if(distance < POSSESS_RANGE && distance < closestDistance) {
                    closestDistance = distance;
                    closestOption = enemy;
                    possessIndicator.transform.position = enemy.transform.position + new Vector3(0, 1, 0);
                }
            }
        }

        if(PossessUsed() && closestOption != null) {
            // possess

        }

        // -- manage health --

        if(ghostScript == null) { // if possessing
            // decay health over time when possessing
            decayTimer -= Time.deltaTime;
            if(decayTimer <= 0) {
                decayTimer += DECAY_FREQ;
                playerCharacter.GetComponent<Enemy>().TakeDamage(1, true);
            }
        }
        else {
            playerHealth = ghostScript.Health;
            if(playerHealth <= 0) {
                // lose game
            }
        }

        // -- update UI --
            // health bar(s)
            // abilities
            // souls
    }

    private bool IsPossessing() {
        return playerCharacter.GetComponent<PlayerGhost>() != null;
    }

    private bool PossessUsed() {
        if(Gamepad.current != null && (Gamepad.current.yButton.wasPressedThisFrame || Gamepad.current.rightShoulder.wasPressedThisFrame)) {
            return true;
        }

        if(Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) {
            return true;
        }

        return false;
    }
}
