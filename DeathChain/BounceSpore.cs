using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    class BounceSpore : Projectile
    {
        private int bouncesLeft = 2;

        public BounceSpore(Vector2 midpoint, Vector2 aim, bool fromPlayer)
            : base(midpoint, aim * 800, fromPlayer, 20, Graphics.Spore,
                  new Particle(new Rectangle(0, 0, 20, 20), Graphics.SporeBreak, 0.1f),
                  new Particle(new Rectangle(0, 0, 20, 20), Graphics.SporeTrail, 0.1f)
        ) {}

        protected override void OnWallHit(List<Direction> collisions, Vector2 hitVelocity) {
            // bounce
            if(bouncesLeft > 0) {
                bouncesLeft--;
                if(collisions.Contains(Direction.Up) || collisions.Contains(Direction.Down)) {
                    velocity.Y = -hitVelocity.Y;
                }
                if(collisions.Contains(Direction.Left) || collisions.Contains(Direction.Right)) {
                    velocity.X = -hitVelocity.X;
                }
            } else {
                IsActive = false;
            }
        }
    }
}
