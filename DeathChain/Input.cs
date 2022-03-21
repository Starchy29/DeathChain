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
        private static MouseState mouse;
        private static MouseState lastMouse;
        private static Inputs buffer;
        private static float bufferTime;
        private static float vibrationDuration;

        private static Dictionary<Inputs, List<Buttons>> gamepadBinds = new Dictionary<Inputs, List<Buttons>>();
        private static Dictionary<Inputs, List<Keys>> keyboardBinds = new Dictionary<Inputs, List<Keys>>();

        public static bool GamepadConnected { get { return gamepad.IsConnected; } }

        public static void Setup() {
            lastAim = new Vector2(0, -1);
            mouse = Mouse.GetState();
            lastMouse = mouse;

            // sets up gamepad bindings
            gamepadBinds[Inputs.Attack] = new List<Buttons>() { Buttons.X, Buttons.RightTrigger };
            gamepadBinds[Inputs.Secondary] = new List<Buttons>() { Buttons.A, Buttons.LeftTrigger };
            gamepadBinds[Inputs.Tertiary] = new List<Buttons>() { Buttons.B, Buttons.LeftShoulder };
            gamepadBinds[Inputs.Possess] = new List<Buttons>() { Buttons.Y, Buttons.RightShoulder };

            gamepadBinds[Inputs.Up] = new List<Buttons>() { Buttons.LeftThumbstickUp, Buttons.DPadUp };
            gamepadBinds[Inputs.Down] = new List<Buttons>() { Buttons.LeftThumbstickDown, Buttons.DPadDown };
            gamepadBinds[Inputs.Left] = new List<Buttons>() { Buttons.LeftThumbstickLeft, Buttons.DPadLeft };
            gamepadBinds[Inputs.Right] = new List<Buttons>() { Buttons.LeftThumbstickRight, Buttons.DPadRight };

            gamepadBinds[Inputs.Pause] = new List<Buttons>() { Buttons.Start, Buttons.Back };
            gamepadBinds[Inputs.Select] = new List<Buttons>() { Buttons.A, Buttons.X };
            gamepadBinds[Inputs.Back] = new List<Buttons>() { Buttons.Y, Buttons.B };

            // set up keyboard bindings
            keyboardBinds[Inputs.Up] = new List<Keys>() { Keys.Up, Keys.W };
            keyboardBinds[Inputs.Down] = new List<Keys>() { Keys.Down, Keys.S };
            keyboardBinds[Inputs.Left] = new List<Keys>() { Keys.Left, Keys.A };
            keyboardBinds[Inputs.Right] = new List<Keys>() { Keys.Right, Keys.D };

            keyboardBinds[Inputs.Attack] = new List<Keys>() { }; // mouse left click
            keyboardBinds[Inputs.Secondary] = new List<Keys>() { Keys.Space };
            keyboardBinds[Inputs.Tertiary] = new List<Keys>() { }; // mouse right click
            keyboardBinds[Inputs.Possess] = new List<Keys>() { Keys.E };

            keyboardBinds[Inputs.Pause] = new List<Keys>() { Keys.Escape, Keys.Enter };
            keyboardBinds[Inputs.Select] = new List<Keys>() { Keys.Space, Keys.Enter }; // uses mouse
            keyboardBinds[Inputs.Back] = new List<Keys>() { Keys.Escape, Keys.X };
        }

        public static void Update(float deltaTime) {
            lastkb = keyboard;
            keyboard = Keyboard.GetState();

            lastMouse = mouse;
            mouse = Mouse.GetState();

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

            // handle controller vibration
            if(vibrationDuration > 0) {
                vibrationDuration -= deltaTime;
                if(vibrationDuration <= 0) {
                    GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
                }
            }
        }

        public static bool IsPressed(Inputs input) {
            if(GamepadConnected) {
                foreach(Buttons button in gamepadBinds[input]) {
                    if(gamepad.IsButtonDown(button)) {
                        return true;
                    }
                }
                return false;
            }
            else { // keyboard
                if(input == Inputs.Attack && Mouse.GetState().LeftButton == ButtonState.Pressed ||
                    input == Inputs.Tertiary && Mouse.GetState().RightButton == ButtonState.Pressed
                ) {
                    return true;
                }

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
            if(GamepadConnected) {
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
            if(GamepadConnected) {
                Vector2 angle = gamepad.ThumbSticks.Left;

                // check for DPad input
                if(angle == Vector2.Zero) {
                    angle = GetDPadDirection();
                }

                if(angle != Vector2.Zero) {
                    angle.Normalize();
                }
                return new Vector2(angle.X, -angle.Y);
            } else { // keyboard
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
            if(GamepadConnected) {
                Vector2 angle = gamepad.ThumbSticks.Right; // right stick gets priority for twin-stick controls

                // check for left stick
                if(angle == Vector2.Zero) {
                    angle = gamepad.ThumbSticks.Left;
                }

                if(angle == Vector2.Zero) {
                    // use last aim if not currently aiming
                    angle = lastAim;
                } else {
                    angle.Normalize();
                    lastAim = angle;
                }
                return new Vector2(angle.X, -angle.Y);
            } else {
                // mouse aim
                Vector2 aim = Mouse.GetState().Position.ToVector2() - Game1.Player.Midpoint;
                if(aim == Vector2.Zero) {
                    aim = new Vector2(0, 1);
                } else {
                    aim.Normalize();
                }
                return aim;
            }
        }

        public static bool IsMouseClicked() {
            return Mouse.GetState().LeftButton == ButtonState.Pressed;
        }

        public static bool MouseJustClicked() {
            return lastMouse.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed;
        }

        // convert the mouse screen position to game window position
        public static Vector2 GetMousePosition() {
            MouseState mouse = Mouse.GetState();
            Rectangle stats = Game1.Game.WindowData; // x and y are the offset, width and height are the scale
            return new Vector2((mouse.X - stats.X) * Game1.StartScreenWidth / stats.Width, (mouse.Y - stats.Y) * Game1.StartScreenHeight / stats.Height);
        }

        public static bool MouseMoved() {
            return mouse.Position != lastMouse.Position;
        }

        public static void Vibrate(float amount, float duration) {
            GamePad.SetVibration(PlayerIndex.One, amount, amount);
            vibrationDuration = duration;
        }

        private static Vector2 GetDPadDirection() {
            Vector2 direction = Vector2.Zero;
            if(gamepad.IsButtonDown(Buttons.DPadUp)) {
                direction.Y += 1; // gets inverted later, this matches how thumbsticks work
            }
            if(gamepad.IsButtonDown(Buttons.DPadDown)) {
                direction.Y -= 1;
            }
            if(gamepad.IsButtonDown(Buttons.DPadLeft)) {
                direction.X -= 1;
            }
            if(gamepad.IsButtonDown(Buttons.DPadRight)) {
                direction.X += 1;
            }

            if(direction != Vector2.Zero) {
                direction.Normalize();
            }
            return direction;
        }
    }
}
