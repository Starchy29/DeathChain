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
        private static Inputs buffer;
        private static float bufferTime;

        private static Dictionary<Inputs, List<Buttons>> gamepadBinds = new Dictionary<Inputs, List<Buttons>>();
        private static Dictionary<Inputs, List<Keys>> keyboardBinds = new Dictionary<Inputs, List<Keys>>();

        public static bool IsGamepadConnected { get { return gamepad.IsConnected; } }

        public static void Setup() {
            // sets up key bindings
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
            keyboardBinds[Inputs.Attack] = new List<Keys>() { Keys.J };
            keyboardBinds[Inputs.Secondary] = new List<Keys>() { Keys.K };
            keyboardBinds[Inputs.Tertiary] = new List<Keys>() { Keys.L };
            keyboardBinds[Inputs.Possess] = new List<Keys>() { Keys.I };
        }

        public static void Update(float deltaTime) {
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

            // manage buffers
            if(bufferTime > 0) {
                bufferTime -= deltaTime;
                if(bufferTime <= 0) {
                    buffer = Inputs.None;
                }
            }

            Inputs[] bufferWatch = new Inputs[] { Inputs.Possess, Inputs.Tertiary, Inputs.Secondary, Inputs.Attack };
            foreach(Inputs input in bufferWatch) {
                if(PressedThisFrame(input)) {
                    buffer = input;
                    bufferTime = 0.2f;
                }
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
            // check buffer first
            if(buffer == input) {
                buffer = Inputs.None; // use each buffer input only once
                return true;
            }

            return PressedThisFrame(input);
        }

        private static bool PressedThisFrame(Inputs input) {
            return IsPressed(input) && !WasPressed(input);
        }

        public static bool JustReleased(Inputs input) {
            return !IsPressed(input) && WasPressed(input);
        }

        // checks if a key was pressed last frame
        private static bool WasPressed(Inputs input) {
            if(IsGamepadConnected) {
                foreach(Buttons button in gamepadBinds[input]) {
                    if(lastgp.IsButtonDown(button)) {
                        return true;
                    }
                }
                return false;
            } else {
                foreach(Keys key in keyboardBinds[input]) {
                    if(lastkb.IsKeyDown(key)) {
                        return true;
                    }
                }
                return false;
            }
        }

        // gets the direction the player is trying to move, as a unit vector
        public static Vector2 GetMoveDirection() {
            if(IsGamepadConnected) {
                Vector2 angle = gamepad.ThumbSticks.Left;
                if(angle != Vector2.Zero) {
                    angle.Normalize();
                }
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

        // gets the direction the player is aiming in
        public static Vector2 GetAim() {
            if(IsGamepadConnected) {
                Vector2 angle = gamepad.ThumbSticks.Left;
                if(angle == Vector2.Zero) {
                    angle = lastAim;
                } else {
                    angle.Normalize();
                    lastAim = angle;
                }
                return new Vector2(angle.X, -angle.Y);
            } else {
                // mouse aim
                return GetMoveDirection();
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
