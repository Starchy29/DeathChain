using System;
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
        Attack,
        Secondary,
        Tertiary,
        Possess
    }

    static class Input {
        private static KeyboardState lastkb;
        private static KeyboardState keyboard;
        private static GamePadState lastgp;
        private static GamePadState gamepad;
        private static Vector2 lastAim;
        private static bool lastClicked;
        private static bool mouseClicked;

        private static Dictionary<Inputs, List<Buttons>> gamepadBinds = new Dictionary<Inputs, List<Buttons>>();
        private static Dictionary<Inputs, List<Keys>> keyboardBinds = new Dictionary<Inputs, List<Keys>>();

        public static bool IsGamepadConnected { get { return gamepad.IsConnected; } }

        public static void Setup() {
            // sets up key bindings
            gamepadBinds[Inputs.Up] = new List<Buttons>() {};
            gamepadBinds[Inputs.Attack] = new List<Buttons>() { Buttons.X };
            gamepadBinds[Inputs.Secondary] = new List<Buttons>() { Buttons.A };
            gamepadBinds[Inputs.Tertiary] = new List<Buttons>() { Buttons.B };
            gamepadBinds[Inputs.Possess] = new List<Buttons>() { Buttons.Y };
            gamepadBinds[Inputs.Up] = new List<Buttons>() { Buttons.LeftThumbstickUp };
            gamepadBinds[Inputs.Down] = new List<Buttons>() { Buttons.LeftThumbstickDown };
            gamepadBinds[Inputs.Left] = new List<Buttons>() { Buttons.LeftThumbstickLeft };
            gamepadBinds[Inputs.Right] = new List<Buttons>() { Buttons.LeftThumbstickRight };

            keyboardBinds[Inputs.Up] = new List<Keys>() { Keys.W, Keys.Up };
            keyboardBinds[Inputs.Down] = new List<Keys>() { Keys.Down, Keys.S };
            keyboardBinds[Inputs.Left] = new List<Keys>() { Keys.Left, Keys.A };
            keyboardBinds[Inputs.Right] = new List<Keys>() { Keys.Right, Keys.D };
        }

        public static void Update() {
            lastClicked = mouseClicked;
            mouseClicked = IsMouseClicked();

            lastkb = keyboard;
            keyboard = Keyboard.GetState();

            lastgp = gamepad;
            gamepad = GamePad.GetState(PlayerIndex.One);
            if(gamepad.ThumbSticks.Left.Length() > 0.8f) {
                lastAim = gamepad.ThumbSticks.Left;
                lastAim.Normalize();
            }
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

        public static bool JustPressed(Inputs input) {
            if(!IsPressed(input)) {
                return false;
            }

            // check if unpressed last frame
            if(IsGamepadConnected) {
                foreach(Buttons button in gamepadBinds[input]) {
                    if(lastgp.IsButtonDown(button)) {
                        return false;
                    }
                }
                return true;
            } else {
                foreach(Keys key in keyboardBinds[input]) {
                    if(lastkb.IsKeyDown(key)) {
                        return false;
                    }
                }
                return true;
            }
        }

        // gets the direction the player is holding, as a unit vector
        public static Vector2 GetMoveDirection() {
            if(IsGamepadConnected) {
                Vector2 angle = gamepad.ThumbSticks.Left;
                return new Vector2(angle.X, -angle.Y);
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

        public static Vector2 GetAim() {
            if(IsGamepadConnected) {
                Vector2 angle = gamepad.ThumbSticks.Left;
                if(angle.Length() == 0) {
                    angle = lastAim;
                } else {
                    angle.Normalize();
                    lastAim = angle;
                }
                return new Vector2(angle.X, -angle.Y);
            } else {
                // mouse aim
                return Vector2.Zero;
            }
        }

        public static bool IsMouseClicked() {
            return Mouse.GetState().LeftButton == ButtonState.Pressed;
        }

        public static bool MouseJustClicked() {
            return !lastClicked && mouseClicked;
        }

        // convert the mouse screen position to game window position
        public static Vector2 GetMousePosition() {
            MouseState mouse = Mouse.GetState();
            Rectangle stats = Game1.Game.WindowData; // x and y are the offset, width and height are the scale
            return new Vector2((mouse.X - stats.X) * Game1.StartScreenWidth / stats.Width, (mouse.Y - stats.Y) * Game1.StartScreenHeight / stats.Height);
        }
    }
}
