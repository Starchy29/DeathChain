using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    static class Camera
    {
        private static Vector2 position; // top left

        public static Vector2 Shift { get { return -position; } }

        public static void Start() {
            position = new Vector2(0, 0);
        }
    }
}
