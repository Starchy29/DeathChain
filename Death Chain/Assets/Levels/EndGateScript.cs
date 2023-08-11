using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// attached to the part that has the trigger collider. Then this attaches a copy to its own gate
public class EndGateScript : MonoBehaviour
{
    [SerializeField] private GameObject gate;
    [SerializeField] private int soulCost;

    private void Awake()
    {
        if(gate != null) {
            EndGateScript addedScript = gate.AddComponent<EndGateScript>();
            addedScript.soulCost = this.soulCost;
            gate.transform.GetChild(0).GetComponent<TMPro.TextMeshPro>().text = "" + soulCost;
            // the new script has no gate so there is no infinite loop
        }
    }

    // next level when leaving the area
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == PlayerScript.Instance.PlayerEntity) {
            SceneManager.LoadScene("Main Menu");
        }
    }

    // open the door when the player walks into it if they have enough souls
    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerScript player = PlayerScript.Instance;
        if(collision.gameObject == player.PlayerEntity && player.Souls >= soulCost) {
            player.Souls -= soulCost;
            EntityTracker.Instance.Walls.Remove(gameObject);
            Destroy(gameObject);
        }
    }
}
