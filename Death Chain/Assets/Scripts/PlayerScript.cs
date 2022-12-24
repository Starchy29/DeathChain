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
    [SerializeField] private GameObject playerPrefab;
    
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
        PlayerGhost ghostScript = playerCharacter.GetComponent<PlayerGhost>(); // null if posssessing an enemy

        // -- manage health --
        if(ghostScript == null) { // if possessing
            // decay health over time when possessing
            decayTimer -= Time.deltaTime;
            if(decayTimer <= 0) {
                decayTimer += DECAY_FREQ;
                playerCharacter.GetComponent<Enemy>().TakeDamage(1, true);
            }

            if(playerCharacter.GetComponent<Enemy>().Health <= 0) {
                // die when possessing: lose body
                Unpossess();
            }
        }
        else {
            playerHealth = ghostScript.Health;
            if(playerHealth <= 0) {
                // lose game
            }
        }

        // -- manage possession --
        possessIndicator.SetActive(false);
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
                    possessIndicator.SetActive(true);
                }
            }
        }

        if(PossessUsed()) {
            if(closestOption != null) {
                // possess
                playerCharacter.GetComponent<Enemy>().DeleteThis = true; // remove last body
                if(ghostScript == null) {
                    // leave corpse animation
                }

                playerCharacter = closestOption;
                playerCharacter.GetComponent<Enemy>().Possess(new PlayerController(playerCharacter));
            }
            else if(ghostScript == null) {
                Unpossess();
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

    // checks for release of the possess button
    private bool PossessUsed() {
        if(Gamepad.current != null && (Gamepad.current.yButton.wasReleasedThisFrame || Gamepad.current.rightShoulder.wasReleasedThisFrame)) {
            return true;
        }

        if(Keyboard.current != null && Keyboard.current.eKey.wasReleasedThisFrame) {
            return true;
        }

        return false;
    }

    private void Unpossess() {
        GameObject playerGhost = Instantiate(playerPrefab);
        playerGhost.transform.position = playerCharacter.transform.position;
        playerCharacter.GetComponent<Enemy>().DeleteThis = true;
        // leave corpse animation
        playerCharacter = playerGhost;
        playerCharacter.GetComponent<PlayerGhost>().Setup(playerHealth);
    }
}
