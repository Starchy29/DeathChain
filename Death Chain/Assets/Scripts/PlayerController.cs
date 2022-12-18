using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Controller
{
    private int controllerIndex;
    private bool useKeyboard;
    private Vector2 lastAim = Vector2.up; // the last direction the player held. Used when the player is not aiming
    private Vector3 playerPosition; // needed for mouse aim

    // multiplayer controls: enter a big controller index to make it the keyboard user
    public PlayerController(int controllerIndex = 0) {
        this.controllerIndex = controllerIndex;
        useKeyboard = false;

        if(controllerIndex > 3) {
            // this is how to make a player use the keyboard with no controller
            useKeyboard = true;
            this.controllerIndex = 64; // assuming no computer has this many controller inputs
        }
    }

    // default for single player
    public PlayerController() {
        controllerIndex = 0;
        useKeyboard = true;
    }

    public override void Update(GameObject controlTarget) {
        playerPosition = controlTarget.transform.position;
        lastAim = GetAimDirection();
    }

    public override Vector2 GetMoveDirection() {
        if(controllerIndex < Gamepad.all.Count) {
            Vector2 joystick = Gamepad.all[controllerIndex].leftStick.ReadValue();
            if(joystick != Vector2.zero) {
                // joystick gets first priority
                joystick.Normalize();
                return joystick;
            }

            Vector2 dPad = Gamepad.all[controllerIndex].dpad.ReadValue();
            if(dPad != Vector2.zero) {
                // controller gets priority over keyboard
                dPad.Normalize();
                return dPad;
            }
        }

        if(useKeyboard) {
            Vector2 keyboard = new Vector2();
            if(Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) {
                keyboard.y += 1;
            }
            if(Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) {
                keyboard.y -= 1;
            }
            if(Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) {
                keyboard.x += 1;
            }
            if(Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) {
                keyboard.x -= 1;
            }
            if(keyboard != Vector2.zero) {
                keyboard.Normalize();
            }
            return keyboard;
        }

        return Vector2.zero;
    }

    public override int GetUsedAbility() {
        if(controllerIndex < Gamepad.all.Count) {
            // check gamepad
            Gamepad controller = Gamepad.all[controllerIndex];
            if(controller.xButton.wasPressedThisFrame || controller.rightTrigger.wasPressedThisFrame) {
                return 0;
            }
            if(controller.aButton.wasPressedThisFrame || controller.leftTrigger.wasPressedThisFrame) {
                return 1;
            }
            if(controller.bButton.wasPressedThisFrame || controller.leftShoulder.wasPressedThisFrame) {
                return 2;
            }
            if(controller.yButton.wasPressedThisFrame || controller.rightShoulder.wasPressedThisFrame) {
                return 3;
            }
        }

        if(useKeyboard) {
            // check keyboard
            if(Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) {
                return 0;
            }
            if(Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) {
                return 1;
            }
            if(Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) {
                return 2;
            }
            if(Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) {
                return 3;
            }
        }

        return -1; // no ability used
    }

    public override Vector2 GetAimDirection() {
        if(controllerIndex < Gamepad.all.Count) {
            // if they want to use the right stick to aim, prioritize that over the normal stick
            Vector2 rightStick = Gamepad.all[controllerIndex].rightStick.ReadValue();
            if(rightStick != Vector2.zero) {
                rightStick.Normalize();
                return rightStick;
            }

            // if no right stick, aim in the direction the player is moving
            Vector2 moveDirection = GetMoveDirection();
            if(moveDirection != Vector2.zero) {
                return moveDirection; // already normalized
            }
        }

        if(useKeyboard && Mouse.current != null) {
            // use mouse for aim
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 playerToMouse = mouseWorldPos - playerPosition;
            if(playerToMouse != Vector2.zero) {
                playerToMouse.Normalize();
                return playerToMouse;
            }
        }

        return lastAim; // use last aim if not currently aiming
    }
}
