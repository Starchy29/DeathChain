using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGateScript : MonoBehaviour
{
    [SerializeField] private GameObject gate;
    [SerializeField] private Enemy tempWatch;

    void Update()
    {
        // TEMP
        if(gate == null) {
            return;
        }
        foreach(GameObject enemy in EntityTracker.Instance.Enemies) {
            if(enemy.GetComponent<SlimeScript>() != null && enemy.transform.localScale.x == 2.1f && enemy.GetComponent<SlimeScript>().IsCorpse) {
                gate.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == PlayerScript.Instance.PlayerEntity) {
            SceneManager.LoadScene("Main Menu");
        }
    }
}
