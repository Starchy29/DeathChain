using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// tracks the status of the player, no matter which object they are possessing at the moment
public class PlayerScript : MonoBehaviour
{
    private static PlayerScript instance;
    public static PlayerScript Instance { get { return instance; } }

    [SerializeField] private GameObject possessIndicator;
    [SerializeField] private GameObject playerCharacter; // the entity the player is currently playing as, manually set to ghost at first
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject possessParticlePrefab;

    [SerializeField] private GameObject soulHealthBar;
    [SerializeField] private GameObject corpseHealthBar;
    [SerializeField] private GameObject soulBar;
    [SerializeField] private GameObject[] abilityButtons;

    private int souls = 2;
    private int playerHealth;

    private const float POSSESS_RANGE = 1.5f; // how far away the player can be from a corpse and possess it
    private const float ABILITY_ALPHA = 0.5f; 
    private float healthBarHeight; // used to represent the width of each health point
    private Vector3 healthBarStart;
    private GameObject possessTarget;

    public GameObject PlayerEntity { get { return playerCharacter; } }
    public bool Possessing { get { return playerCharacter != null && playerCharacter.GetComponent<PlayerGhost>() == null; } }

    void Awake() {
        instance = this;
    }

    void Start()
    {
        healthBarHeight = soulHealthBar.transform.localScale.y;
        healthBarStart = soulHealthBar.transform.localPosition - new Vector3(soulHealthBar.transform.localScale.x / 2, 0, 0);
        corpseHealthBar.transform.localScale = new Vector3(1, healthBarHeight, 1);
        soulBar.GetComponent<TMPro.TextMeshPro>().text = "" + souls;
        possessIndicator.SetActive(false);
        SetAbilityIcons();
    }

    void Update()
    {
        // -- manage health --
        if(Possessing) {
            // check if the player's possessed body dies
            if(playerCharacter.GetComponent<Enemy>().Health <= 0) {
                playerHealth--; // punish for losing body by dealing some damage, must be before Unpossess()
                Unpossess();
            }
        } else {
            playerHealth = playerCharacter.GetComponent<Enemy>().Health;
            if(playerHealth <= 0) {
                // lose game
                SceneManager.LoadScene("Main Menu");
            }
        }

        // -- manage possession --
        if(PossessPressed()) {
            // find closest corpse within range to be the possess target
            List<GameObject> enemies = EntityTracker.Instance.Enemies;
            GameObject closestOption = null;
            float closestDistance = POSSESS_RANGE;
            foreach(GameObject enemy in enemies) {
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if(!enemyScript.IsCorpse) {
                    continue;
                }

                float distance = Vector3.Distance(playerCharacter.transform.position, enemy.transform.position);
                if(distance < closestDistance) {
                    closestDistance = distance;
                    closestOption = enemy;
                }
            }

            possessTarget = closestOption;
            if(possessTarget != null) {
                // place the possesion indicator over the possess target
                PlacePossessIndicator(possessTarget.transform.position + new Vector3(0, 0.4f, 0), souls < CalcCost(possessTarget.GetComponent<Enemy>()));
            }
            else if(Possessing) {
                // create unpossess indicator over player
                PlacePossessIndicator(playerCharacter.transform.position + new Vector3(0, 1, 0), true);
            }
            else {
                // no options: hide indicator
                possessIndicator.SetActive(false);
            }
        }
        else if(PossessReleased()) {
            if(possessTarget != null && souls >= CalcCost(possessTarget.GetComponent<Enemy>())) {
                Possess(possessTarget);
            } 
            else if(Possessing && possessTarget == null) {
                Unpossess();
            }

            possessIndicator.SetActive(false);
        }

        // -- update UI --
        if(Possessing) {
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

        float[] cooldowns = playerCharacter.GetComponent<Enemy>().Cooldowns;
        for(int i = 0; i < 3; i++) {
            Color color = Color.white;
            if(cooldowns[i] > 2) {
                color = new Color(0.5f, 0.0f, 0.0f);
            }
            else if(cooldowns[i] > 1) {
                color = Color.red;
            }
            else if(cooldowns[i] > 0) {
                color = new Color(1.0f, 0.5f, 0.0f);
            }
            color.a = ABILITY_ALPHA;
            abilityButtons[i].GetComponent<SpriteRenderer>().color = color;
        }
    }

    #region input
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
    #endregion
    
    private void Possess(GameObject corpse) {
        souls -= CalcCost(corpse.GetComponent<Enemy>());
        soulBar.GetComponent<TMPro.TextMeshPro>().text = "" + souls;

        GameObject animation = Instantiate(possessParticlePrefab);
        animation.transform.position = playerCharacter.transform.position;
        animation.GetComponent<PossessMovement>().Target = corpse;

        if(Possessing) {
            // leave corpse animation
            playerCharacter.GetComponent<Enemy>().Unpossess();
        } else {
            playerCharacter.GetComponent<Enemy>().DeleteThis = true; // remove last body
        }

        playerCharacter = corpse;
        playerCharacter.GetComponent<Enemy>().Possess(new PlayerController(playerCharacter));
        SetAbilityIcons();
    }

    private void Unpossess() {
        GameObject playerGhost = Instantiate(playerPrefab);
        playerGhost.transform.position = playerCharacter.transform.position;
        playerCharacter.GetComponent<Enemy>().Unpossess();
        playerCharacter = playerGhost;
        playerCharacter.GetComponent<PlayerGhost>().Setup(playerHealth);
        SetAbilityIcons();
    }

    private int CalcCost(Enemy enemyType) {
        return enemyType.Difficulty + 1;
    }

    private void PlacePossessIndicator(Vector2 position, bool showCross) {
        possessIndicator.transform.position = position;
        possessIndicator.SetActive(true);
        possessIndicator.transform.GetChild(0).gameObject.SetActive(showCross);
    }

    // updates the abilities UI to match the current possessed form
    private void SetAbilityIcons() {
        Sprite[] icons = AbilityIcons.Instance.GetIcons(playerCharacter.GetComponent<Enemy>());
        for(int i = 0; i < 3; i++) {
            Sprite sprite = icons[i];
            if(sprite == null) {
                abilityButtons[i].transform.GetChild(0).gameObject.SetActive(false);
            } else {
                abilityButtons[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = sprite;
                abilityButtons[i].transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    // allows Enemy.cs to grant souls when an enemy dies
    public void AddSouls(int amount) {
        souls += amount;
        soulBar.GetComponent<TMPro.TextMeshPro>().text = "" + souls;
    }
}
