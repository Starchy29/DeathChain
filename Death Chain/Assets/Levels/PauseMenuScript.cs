using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuScript : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;

    public static PauseMenuScript Instance { get; private set; }
    public bool Paused { get; private set; }

    private void Awake() {
        Instance = this;
    }

    void Update()
    {
        if(PausePressed()) {
            TogglePause();
        }
    }

    private bool PausePressed() {
        return Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame 
            || Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }

    // button events
    public void TogglePause() {
        Paused = !Paused;
        pauseMenu.SetActive(Paused);
        Time.timeScale = Paused ? 0 : 1;
    }

    public void Quit() {
        TogglePause(); // unpause before leaving the scene
        SceneManager.LoadScene(0);
    }
}
