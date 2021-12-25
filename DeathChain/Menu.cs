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

        public Menu BackMenu { get; set; } // the menu to go back to

        public Menu(Texture2D background, List<Button> buttons) {
            this.background = background;
            this.buttons = buttons;
        }

        public void Update() {
            foreach(Button button in buttons) {
                button.Update();
            }
        }

        public void Draw(SpriteBatch sb) {
            if(background != null) {
                sb.Draw(background, new Rectangle(0, 0, Game1.StartScreenHeight, Game1.StartScreenHeight), Color.White);
            }

            foreach(Button button in buttons) {
                button.Draw(sb);
            }
        }
    }
}
