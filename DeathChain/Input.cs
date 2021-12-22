﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DeathChain
{
    public enum Inputs {
        None,
        Select,
        Back,
        Pause,
        Up,
        Down,
        Left,
        Right,
        Ability1,
        Ability2,
        Ability3,
        Ability4
    }

    static class Input {
        private static KeyboardState lastkb;
        private static KeyboardState keyboard;
        private static GamePadState lastgp;
        private static GamePadState gamepad;

        private static Dictionary<Inputs, List<Buttons>> gamepadBinds = new Dictionary<Inputs, List<Buttons>>();
        private static Dictionary<Inputs, List<Keys>> keyboardBinds = new Dictionary<Inputs, List<Keys>>();

        public static bool IsGamepadConnected { get { return gamepad.IsConnected; } }

        public static void Setup() {
            // sets up key bindings
            gamepadBinds[Inputs.Up] = new List<Buttons>() {Buttons.A };

            keyboardBinds[Inputs.Up] = new List<Keys>() { Keys.W, Keys.Up };
            keyboardBinds[Inputs.Down] = new List<Keys>() { Keys.Down, Keys.S };
            keyboardBinds[Inputs.Left] = new List<Keys>() { Keys.Left, Keys.A };
            keyboardBinds[Inputs.Right] = new List<Keys>() { Keys.Right, Keys.D };
        }

        public static void Update() {
            //lastClicked = mouseClicked;
            //mouseClicked = IsMouseClicked();

            lastkb = keyboard;
            keyboard = Keyboard.GetState();

            lastgp = gamepad;
            gamepad = GamePad.GetState(PlayerIndex.One);
        }

        public static bool IsPressed(Inputs input) {
            if(IsGamepadConnected) {
                foreach(Buttons button in gamepadBinds[input]) {
                    if(gamepad.IsButtonDown(button)) {
                        return true;
                    }
                }
                return false;
            } else {
                foreach(Keys key in keyboardBinds[input]) {
                    if(keyboard.IsKeyDown(key)) {
                        return true;
                    }
                }
                return false;
            }
        }

        // gets the direction the player is holding, as a unit vector
        public static Vector2 GetMoveDirection() {
            if(IsGamepadConnected) {
                return gamepad.ThumbSticks.Left;
            } else {
                Vector2 result = new Vector2(0, 0);
                if(IsPressed(Inputs.Up)) {
                    result.Y -= 1;
                }
                if(IsPressed(Inputs.Down)) {
                    result.Y += 1;
                }
                if(IsPressed(Inputs.Left)) {
                    result.X -= 1;
                }
                if(IsPressed(Inputs.Right)) {
                    result.X += 1;
                }

                if(result.X != 0 || result.Y != 0) {
                    result.Normalize();
                }
                return result;
            }
        }
    }
}
