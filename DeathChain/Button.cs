using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeathChain
{
    public delegate void Action();

    class Button
    {
        private Rectangle rect;
        private Action clickEvent;
        private bool hovered;
        private String text;

        public Button(Vector2 middle, int width, int height, String text, Action clickEvent) {
            rect = new Rectangle((int)middle.X - width / 2, (int)middle.Y - height / 2, width, height);
            this.clickEvent = clickEvent;
            this.text = text;
        }

        public void Update() {
            hovered = false;
            if(rect.Contains(Input.GetMousePosition())) {
                hovered = true;
                if(Input.MouseJustClicked()) {
                    clickEvent();
                }
            }
        }

        public void Draw(SpriteBatch sb) {
            if(hovered) {
                sb.Draw(Graphics.Pixel, rect, Color.Red);
            } else {
                sb.Draw(Graphics.Pixel, rect, Color.Gray);
            }

            Vector2 textDims = Graphics.Font.MeasureString(text);
            sb.DrawString(Graphics.Font, text, new Vector2(rect.X + (rect.Width - textDims.X) / 2, rect.Y + (rect.Height - textDims.Y) / 2), Color.Black);
        }
    }
}
