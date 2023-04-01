using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// tracks the status of the player, no matter which object they are possessing at the moment
public class PlayerScript : MonoBehaviour
{
    [SerializeField] private GameObject possessIndicator;
    [SerializeField] private GameObject playerCharacter; // the entity the player is currently playing as, manually set to ghost at first
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject possessParticlePrefab;

    [SerializeField] private GameObject soulHealthBar;
    [SerializeField] private GameObject corpseHealthBar;

    private int playerHealth;
    private Timer decayTimer;

    private const float DECAY_FREQ = 2.0f; // number of seconds for each damage dealt
    private const float POSSESS_RANGE = 1.5f; // how far away the player can be from a corpse and possess it
    private float healthBarHeight; // used to represent the width of each health point
    private Vector3 healthBarStart;

    public GameObject PlayerEntity { get { return playerCharacter; } }

    void Start()
    {
        healthBarHeight = soulHealthBar.transform.localScale.y;
        healthBarStart = soulHealthBar.transform.localPosition - new Vector3(soulHealthBar.transform.localScale.x / 2, 0, 0);
        corpseHealthBar.transform.localScale = new Vector3(1, healthBarHeight, 1);
    }

    void Update()
    {
        PlayerGhost ghostScript = playerCharacter.GetComponent<PlayerGhost>(); // null if posssessing an enemy

        // -- manage health --
        if(ghostScript == null) { // if possessing
            // timer handles decay damage automatically

            if(playerCharacter.GetComponent<Enemy>().Health <= 0) {
                // die when possessing: lose body
                Unpossess();
            }
        }
        else {
            playerHealth = ghostScript.Health;
            if(playerHealth <= 0) {
                // lose game
                //return;
            }
        }

        // -- manage possession --
        if(PossessPressed() || PossessReleased()) {
            // find closest possess target
            List<GameObject> enemies = EntityTracker.Instance.Enemies;
            GameObject closestOption = null;
            float closestDistance = POSSESS_RANGE;
            foreach(GameObject enemy in enemies) {
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if(enemyScript.IsCorpse) {
                    float distance = Vector3.Distance(playerCharacter.transform.position, enemy.transform.position);
                    if(distance < POSSESS_RANGE && distance < closestDistance) {
                        closestDistance = distance;
                        closestOption = enemy;

                        // show indicator
                        possessIndicator.transform.position = enemy.transform.position + new Vector3(0, 0.4f, 0);
                        possessIndicator.SetActive(true);
                        possessIndicator.transform.GetChild(0).gameObject.SetActive(false);
                    }
                }
            }

            if(closestOption == null) {
                if(ghostScript == null) { // if possessing
                    // create unpossess indicator over player
                    possessIndicator.transform.position = playerCharacter.transform.position + new Vector3(0, 1, 0);
                    possessIndicator.SetActive(true);
                    possessIndicator.transform.GetChild(0).gameObject.SetActive(true);
                
                    // unpossess
                    if(PossessReleased()) {
                        Unpossess();
                    }
                } else {
                    possessIndicator.SetActive(false);
                }
            }
            else if(PossessReleased()) {
                // possess
                decayTimer = new Timer(DECAY_FREQ, true, () => { playerCharacter.GetComponent<Enemy>().TakeDamage(1, true); });
                GameObject animation = Instantiate(possessParticlePrefab);
                animation.transform.position = playerCharacter.transform.position;
                animation.GetComponent<PossessMovement>().Target = closestOption;

                if(ghostScript == null) {
                    // leave corpse animation
                    playerCharacter.GetComponent<Enemy>().Unpossess();
                } else {
                    // remove player ghost
                    playerCharacter.GetComponent<Enemy>().DeleteThis = true; // remove last body
                }

                playerCharacter = closestOption;
                playerCharacter.GetComponent<Enemy>().Possess(new PlayerController(playerCharacter));
            }
        }
        else { // possess not pressed
            possessIndicator.SetActive(false);
        }

        // -- update UI --
        if(ghostScript == null) {
            // update corpse health
            corpseHealthBar.SetActive(true);

            float startX = healthBarStart.x + playerHealth * healthBarHeight;
            float barWidth = playerCharacter.GetComponent<Enemy>().Health * healthBarHeight;
            corpseHealthBar.transform.localScale = new Vector3(barWidth, healthBarHeight, 1);
            corpseHealthBar.transform.localPosition = new Vector3(startX + barWidth / 2, healthBarStart.y, 1);
        } else {
            // update soul health
            corpseHealthBar.SetActive(false);

            soulHealthBar.transform.localScale = new Vector3(playerHealth * healthBarHeight, healthBarHeight, 1);
            soulHealthBar.transform.localPosition = healthBarStart + new Vector3(playerHealth * healthBarHeight / 2, 0, 0);
        }
            // abilities
            // souls
    }

    // checks for release of the possess button
    private bool PossessReleased() {
        if(Gamepad.current != null && (Gamepad.current.yButton.wasReleasedThisFrame || Gamepad.current.rightShoulder.wasReleasedThisFrame)) {
            return true;
        }

        if(Keyboard.current != null && Keyboard.current.eKey.wasReleasedThisFrame) {
            return true;
        }

        return false;
    }

    private bool PossessPressed() {
        if(Gamepad.current != null && (Gamepad.current.yButton.isPressed || Gamepad.current.rightShoulder.isPressed)) {
            return true;
        }

        if(Keyboard.current != null && Keyboard.current.eKey.isPressed) {
            return true;
        }

        return false;
    }

    private void Unpossess() {
        decayTimer.End();
        decayTimer = null;

        GameObject playerGhost = Instantiate(playerPrefab);
        playerGhost.transform.position = playerCharacter.transform.position;
        playerCharacter.GetComponent<Enemy>().Unpossess();
        playerCharacter = playerGhost;
        playerCharacter.GetComponent<PlayerGhost>().Setup(playerHealth);
    }
}
