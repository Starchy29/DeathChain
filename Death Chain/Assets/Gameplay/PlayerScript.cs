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

    [SerializeField] private GameObject playerCharacter; // the entity the player is currently playing as, manually set to ghost at first

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject possessParticlePrefab;

    [SerializeField] public GameObject Aimer;
    [SerializeField] private GameObject possessIndicator;
    [SerializeField] private GameObject soulHealthBar;
    [SerializeField] private GameObject corpseHealthBar;
    [SerializeField] private GameObject spiritBar;
    [SerializeField] private TMPro.TextMeshPro soulDisplay;
    [SerializeField] private GameObject[] abilityButtons;

    private int spirit = 4; // resource used to possess enemies
    private int playerHealth;

    private const float POSSESS_RANGE = 1.5f; // how far away the player can be from a corpse and possess it
    private const float ABILITY_ALPHA = 0.7f;
    private GameObject possessTarget;

    public GameObject PlayerEntity { get { return playerCharacter; } }
    public int Souls { get; set; } // currency used to open the end gate and buy upgrades
    public bool Possessing { get { return playerCharacter != null && playerCharacter.GetComponent<PlayerGhost>() == null; } }

    void Awake() {
        instance = this;
    }

    void Start()
    {
        soulDisplay.text = "" + Souls;
        possessIndicator.SetActive(false);
        corpseHealthBar.SetActive(false);
        soulHealthBar.GetComponent<UIBar>().SetValue(playerHealth);
        spiritBar.GetComponent<UIBar>().SetValue(spirit);
        SetAbilityIcons();
    }

    void Update()
    {
        if(PauseMenuScript.Instance.Paused) {
            return;
        }

        // -- manage health --
        if(Possessing) {
            // check if the player's possessed body dies
            if(playerCharacter.GetComponent<Enemy>().Health <= 0) {
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
                PlacePossessIndicator(possessTarget.transform.position + new Vector3(0, 0.4f, 0), spirit < CalcCost(possessTarget.GetComponent<Enemy>()));
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
            if(possessTarget != null && spirit >= CalcCost(possessTarget.GetComponent<Enemy>())) {
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
            corpseHealthBar.GetComponent<UIBar>().SetValue(playerCharacter.GetComponent<Enemy>().Health);
        } else {
            // update soul health
            soulHealthBar.GetComponent<UIBar>().SetValue(playerHealth);
        }

        float[] cooldowns = playerCharacter.GetComponent<Enemy>().Cooldowns;
        for(int i = 0; i < 3; i++) {
            float brightness = 1 - 0.4f * Mathf.Min(Mathf.Ceil(cooldowns[i]), 2);
            abilityButtons[i].GetComponent<SpriteRenderer>().color = new Color(cooldowns[i] > 0 ? 0 : 0.7f, cooldowns[i] > 1 ? 0 : 0.7f, 0.7f, ABILITY_ALPHA);
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
        spirit -= CalcCost(corpse.GetComponent<Enemy>());
        spiritBar.GetComponent<UIBar>().SetValue(spirit);

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

        // place corpse bar to the right of the soul health bar
        corpseHealthBar.SetActive(true);
        corpseHealthBar.transform.position = new Vector3(
            soulHealthBar.transform.position.x + soulHealthBar.transform.localScale.x / 2 + corpseHealthBar.transform.localScale.x / 2,
            corpseHealthBar.transform.position.y,
            corpseHealthBar.transform.position.z
        );
    }

    private void Unpossess() {
        GameObject playerGhost = Instantiate(playerPrefab);
        playerGhost.transform.position = playerCharacter.transform.position;
        playerCharacter.GetComponent<Enemy>().Unpossess();
        playerCharacter = playerGhost;
        playerCharacter.GetComponent<PlayerGhost>().Setup(playerHealth);
        SetAbilityIcons();
        corpseHealthBar.SetActive(false);
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

    // allows Enemy.cs to grant souls and spirit meter when an enemy dies
    public void AddResources(int enemyDifficulty) {
        // gain 1 spirit per kill
        spirit += 1;
        spiritBar.GetComponent<UIBar>().SetValue(spirit);

        // gain currency equal to the enemy's power level
        Souls += enemyDifficulty;
        soulDisplay.text = "" + Souls;
    }
}
