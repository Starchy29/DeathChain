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
        private static Color DarkPurple = new Color(90, 20, 120);

        private Rectangle rect;
        private Action ClickEffect;
        private String text;

        public Button(Vector2 middle, int width, int height, String text, Action clickEvent) {
            rect = new Rectangle((int)middle.X - width / 2, (int)middle.Y - height / 2, width, height);
            this.ClickEffect = clickEvent;
            this.text = text;
        }

        public bool Hovered() {
            return rect.Contains(Input.GetMousePosition());
        }

        public void Click() {
            ClickEffect();
        }

        // finds the best button to go to based on the input direction. loops around the screen if necessary
        public Button GetClosestNeighbor(List<Button> buttons, Direction direction) {
            Button closestOption = this;
            float closestDistance = (direction == Direction.Down || direction == Direction.Up ? Game1.StartScreenHeight : Game1.StartScreenWidth);

            foreach(Button other in buttons) {
                if(other == this) {
                    continue;
                }

                Vector2 comparePosition = other.rect.Center.ToVector2();
                switch(direction) {
                    case Direction.Up:
                        // if below, "shift" to be above
                        if(other.rect.Bottom > this.rect.Top) {
                            comparePosition.Y -= Game1.StartScreenHeight;
                        }
                        break;
                    case Direction.Down:
                        // if above, "shift" to be below
                        if(other.rect.Top < this.rect.Bottom) {
                            comparePosition.Y += Game1.StartScreenHeight;
                        }
                        break;
                    case Direction.Left:
                        // if to the right, "shift" to be left
                        if(other.rect.Right > this.rect.Left) {
                            comparePosition.X -= Game1.StartScreenWidth;
                        }
                        break;
                    case Direction.Right:
                        // if to the left, "shift" to be right
                        if(other.rect.Left < this.rect.Right) {
                            comparePosition.X += Game1.StartScreenWidth;
                        }
                        break;
                }

                float distance = Vector2.Distance(comparePosition, rect.Center.ToVector2());
                if(distance < closestDistance) {
                    closestOption = other;
                    closestDistance = distance;
                }
            }

            return closestOption;
        }

        public void Draw(SpriteBatch sb, bool selected) {
            if(selected) {
                sb.Draw(Graphics.Pixel, rect, DarkPurple);
            } else {
                sb.Draw(Graphics.Pixel, rect, Color.Gray);
            }

            Vector2 textDims = Graphics.Font.MeasureString(text);
            sb.DrawString(Graphics.Font, text, new Vector2(rect.X + (rect.Width - textDims.X) / 2, rect.Y + (rect.Height - textDims.Y) / 2), Color.Black);
        }
    }
}
