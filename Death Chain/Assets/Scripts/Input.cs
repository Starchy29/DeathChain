using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class Input
{
    // Returns the direction the player is inputting as a unit vector. Accounts for joystick, DPad, and keyboard
    public static Vector2 GetDirection() {
        Vector2 joystick = Gamepad.current.leftStick.ReadValue();
        if(joystick != Vector2.zero) {
            // joystick gets first priority
            joystick.Normalize();
            return joystick;
        }

        Vector2 dPad = Gamepad.current.dpad.ReadValue();
        if(dPad != Vector2.zero) {
            // controller gets priority over keyboard
            dPad.Normalize();
            return dPad;
        }

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

    // index is 0-3
    public static bool AbilityJustPressed(int index) {
        switch(index) {
            case 0:
                return Gamepad.current.aButton.wasPressedThisFrame || Keyboard.current.uKey.wasPressedThisFrame;

            case 1:
                return Gamepad.current.xButton.wasPressedThisFrame || Keyboard.current.iKey.wasPressedThisFrame;

            case 2:
                return Gamepad.current.bButton.wasPressedThisFrame || Keyboard.current.oKey.wasPressedThisFrame;

            case 3:
                return Gamepad.current.yButton.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame;
        }

        return false;
    }
}
