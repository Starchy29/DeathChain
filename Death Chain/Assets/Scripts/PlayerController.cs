using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Controller
{
    private const float DEAD_RADIUS = 0.2f;
    private const float BUFFER_DURATION = 0.5f;
    private int controllerIndex;
    private bool useKeyboard;

    private Vector2 aim = Vector2.up; // the last direction the player held. Used when the player is not aiming
    private int bufferAbility = -1;

    // multiplayer controls: enter a big controller index to make it the keyboard user
    public PlayerController(GameObject controlTarget, int controllerIndex = 99) : base(controlTarget) {
        this.controllerIndex = controllerIndex;
        useKeyboard = false;

        if(controllerIndex > 3) {
            // this is how to make a player use the keyboard with no controller
            useKeyboard = true;
            this.controllerIndex = 64; // assuming no computer has this many controller inputs
        }
    }

    // default for single player
    public PlayerController(GameObject controlTarget) : base(controlTarget) {
        controllerIndex = 0;
        useKeyboard = true;
    }

    public override void Update() {
        aim = DetermineAim();

        int abilityUsed = DetermineUsedAbility();
        if(abilityUsed >= 0) {
            bufferAbility = abilityUsed;
            new Timer(BUFFER_DURATION, false, () => { bufferAbility = -1; });
        }
    }

    public override Vector2 GetAimDirection() {
        return aim;
    }

    public override bool AbilityUsed(int ability) {
        if(ability == bufferAbility) {
            bufferAbility = -1; // this funtion must be not be called when the ability is on cooldown, because then the buffer will not work
            return true;
        }

        return false;
    }

    public override Vector2 GetMoveDirection() {
        if(controllerIndex < Gamepad.all.Count) {
            Vector2 joystick = Gamepad.all[controllerIndex].leftStick.ReadValue();
            if(joystick.sqrMagnitude >= DEAD_RADIUS * DEAD_RADIUS) {
                // joystick gets first priority
                joystick.Normalize();
                return joystick;
            }

            Vector2 dPad = Gamepad.all[controllerIndex].dpad.ReadValue();
            if(dPad.sqrMagnitude >= DEAD_RADIUS * DEAD_RADIUS) {
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

    private int DetermineUsedAbility() {
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
        }

        return -1; // no ability used
    }

    public override int GetReleasedAbility() {
        if(controllerIndex < Gamepad.all.Count) {
            // check gamepad
            Gamepad controller = Gamepad.all[controllerIndex];
            if(controller.xButton.wasReleasedThisFrame || controller.rightTrigger.wasReleasedThisFrame) {
                return 0;
            }
            if(controller.aButton.wasReleasedThisFrame || controller.leftTrigger.wasReleasedThisFrame) {
                return 1;
            }
            if(controller.bButton.wasReleasedThisFrame || controller.leftShoulder.wasReleasedThisFrame) {
                return 2;
            }
        }

        if(useKeyboard) {
            // check keyboard
            if(Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) {
                return 0;
            }
            if(Mouse.current != null && Mouse.current.rightButton.wasReleasedThisFrame) {
                return 1;
            }
            if(Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame) {
                return 2;
            }
        }

        return -1; // no ability used
    }

    private Vector2 DetermineAim() {
        if(controllerIndex < Gamepad.all.Count) { // prioritize using controller
            // if they want to use the right stick to aim, prioritize that over the normal stick
            Vector2 rightStick = Gamepad.all[controllerIndex].rightStick.ReadValue();
            if(rightStick.sqrMagnitude >= DEAD_RADIUS * DEAD_RADIUS) {
                return rightStick.normalized;
            }

            // if no right stick, aim in the direction the player is moving
            Vector2 moveDirection = GetMoveDirection();
            if(moveDirection.sqrMagnitude >= DEAD_RADIUS * DEAD_RADIUS) {
                return moveDirection; // already normalized
            }

            return aim;
        }

        if(useKeyboard && Mouse.current != null) {
            // use mouse for aim
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 playerToMouse = mouseWorldPos - controlled.transform.position;
            if(playerToMouse != Vector2.zero) {
                playerToMouse.Normalize();
                return playerToMouse;
            }
        }

        return aim; // use last aim if not currently aiming
    }
}
