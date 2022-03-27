using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeathChain
{
    class Menu
    {
        private Texture2D background;
        private List<Button> buttons;
        private Button selected;
        private String title;
        private Vector2 titlePos;

        public Menu BackMenu { get; set; } // the menu to go back to

        public Menu(Texture2D background, String title, int titleHeight, List<Button> buttons) {
            this.background = background;
            this.buttons = buttons;
            this.title = title;

            Vector2 stringDim = Graphics.TitleFont.MeasureString(title);
            titlePos = new Vector2((Game1.StartScreenWidth - stringDim.X) / 2, titleHeight);
        }

        // highlight the selected button and check if it is clicked
        public void Update() {
            // check gamepad input
            if(Input.GamepadConnected) {
                Direction[] directions = new Direction[4] { Direction.Right, Direction.Left, Direction.Up, Direction.Down };
                Inputs[] inputs = new Inputs[4] { Inputs.Right, Inputs.Left, Inputs.Up, Inputs.Down }; // must line up with directions

                for(int i = 0; i < directions.Length; i++) {
                    if(Input.JustPressed(inputs[i])) {
                        if(selected == null) {
                            selected = buttons[0];
                        } else {
                            selected = selected.GetClosestNeighbor(buttons, directions[i]);
                        }
                        break;
                    }
                }
            }

            // check mouse hovering
            if(Input.MouseMoved()) {
                // clear selected in case it was using the gamepad
                selected = null;

                foreach(Button button in buttons) {
                    if(button.Hovered()) {
                        selected = button;
                        break;
                    }
                }
            }

            // click button
            if(selected != null && (Input.MouseJustClicked() || Input.JustPressed(Inputs.Select))) {
                selected.Click();
                selected = buttons[0];
            }
        }

        public void Draw(SpriteBatch sb) {
            if(background != null) {
                sb.Draw(background, new Rectangle(0, 0, Game1.StartScreenHeight, Game1.StartScreenHeight), Color.White);
            }

            foreach(Button button in buttons) {
                button.Draw(sb, selected == button);
            }

            if(title != null) {
                sb.DrawString(Graphics.TitleFont, title, titlePos, Color.White);
            }
        }
    }
}
