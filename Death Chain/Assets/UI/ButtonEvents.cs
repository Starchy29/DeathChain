using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonEvents : MonoBehaviour
{
    public void StartClicked() {
        SceneManager.LoadScene("Tutorial");
    }

    public void QuitClicked() {
        Application.Quit();
    }
}
