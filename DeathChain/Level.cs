using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeathChain
{
    public class Level
    {
        public const int EDGE_BUFFER = 100;

        private delegate bool EdgeCheck(Rectangle tester, Rectangle outermost);

        private List<Particle> particles;
        private List<Entity> abilities;
        private List<Enemy> enemies;
        private List<Wall> walls;
        private int endY;
        private Vector2 start;
        private float enterDist;
        private bool cleared;

        private Rectangle bounds; // where the camera is allowed to see
        public Rectangle Bounds { get { return bounds; } }
        public bool Cleared { get { return cleared; } }

        public List<Particle> Particles { get { return particles; } }
        public List<Entity> Abilities { get { return abilities; } }
        public List<Enemy> Enemies { get { return enemies; } }
        public List<Wall> Walls { get { return walls; } }

        public Level() {
            // create a sample level
            particles = new List<Particle>();
            abilities = new List<Entity>();
            enemies = new List<Enemy>();
            walls = new List<Wall>();

            Game1.Player.Midpoint = new Vector2(800, 450);

            walls.Add(new Wall(100, 0, 1400, 100, false));
            walls.Add(new Wall(0, 0, 100, 900, false));
            walls.Add(new Wall(1500, 0, 600, 800, false));
            walls.Add(new Wall(100, 800, 2000, 100, false));

            walls.Add(new Wall(1000, 400, 250, 100, false));
            walls.Add(new Wall(400, 400, 100, 250, true));

            //enemies.Add(new Zombie(1300, 300));
            //enemies.Add(new Zombie(1300, 500));
            //enemies.Add(new Zombie(1300, 700));

            //enemies.Add(new Mushroom(300, 450));
            //enemies.Add(new Mushroom(1300, 450));

            //enemies.Add(new Slime(300, 450));
            //enemies.Add(new Blight(300, 450));
            enemies.Add(new Beast(300, 450));

            //enemies.Add(new Scarecrow(300, 450));

            DefineCameraSpace();
        }

        // create a random level with a certain difficulty
        public Level(int difficulty) {
            particles = new List<Particle>();
            abilities = new List<Entity>();
            enemies = new List<Enemy>();
            walls = new List<Wall>();

            // define possible enemies and their power level
            List<EnemyTypes>[] enemyTypes = new List<EnemyTypes>[3];
            enemyTypes[0] = new List<EnemyTypes>();
            enemyTypes[1] = new List<EnemyTypes>();
            enemyTypes[2] = new List<EnemyTypes>();

            enemyTypes[0].Add(EnemyTypes.Zombie);
            enemyTypes[0].Add(EnemyTypes.Mushroom);
            enemyTypes[0].Add(EnemyTypes.Blight);

            enemyTypes[1].Add(EnemyTypes.Slime);
            enemyTypes[1].Add(EnemyTypes.Scarecrow);

            enemyTypes[2].Add(EnemyTypes.Beast);

            // choose a level shape
            LevelLayout layout;
            if(difficulty <= 3) {
                // the first couple levels must be small rooms
                layout = new LevelLayout(0, true);
            } else {
                layout = new LevelLayout(0);
            }
            endY = layout.EndY;
            Game1.Player.Midpoint = layout.Start;
            start = layout.Start;
            enterDist = EDGE_BUFFER + 25;

            // choose a floorplan
            this.walls = layout.Walls;
            List<Vector2> spawnSpots = layout.SpawnSpots;
            
            // add enemies
            Stack<int> difficulties = CreateDifficulties(difficulty);
            while(difficulties.Count > 0 && spawnSpots.Count > 0) {
                // choose next enemy
                int enemyDiff = difficulties.Pop();

                List<EnemyTypes> enemyOptions = enemyTypes[enemyDiff];
                Vector2 position = spawnSpots[0]; // pop spawn spot
                spawnSpots.RemoveAt(0);

                // add next enemy based on chosen type
                switch(enemyOptions[Game1.RNG.Next(0, enemyOptions.Count)]) {
                    case EnemyTypes.Zombie:
                        enemies.Add(new Zombie((int)position.X, (int)position.Y));
                        break;
                    case EnemyTypes.Mushroom:
                        enemies.Add(new Mushroom((int)position.X, (int)position.Y));
                        break;
                    case EnemyTypes.Slime:
                        enemies.Add(new Slime((int)position.X, (int)position.Y));
                        break;
                    case EnemyTypes.Blight:
                        enemies.Add(new Blight((int)position.X, (int)position.Y));
                        break;
                    case EnemyTypes.Scarecrow:
                        enemies.Add(new Scarecrow((int)position.X, (int)position.Y));
                        break;
                    case EnemyTypes.Beast:
                        enemies.Add(new Beast((int)position.X, (int)position.Y));
                        break;
                }
            }

            DefineCameraSpace();
        }

        public void Update(float deltaTime, Player player) {
            // start of level room enter animation
            if(enterDist > 0) {
                enterDist -= player.WalkIn(deltaTime);
                if(enterDist <= 0) {
                    // close door behind player
                    walls.Add(new Wall((int)start.X - LevelLayout.DOOR_WIDTH / 2, (int)start.Y - EDGE_BUFFER, LevelLayout.DOOR_WIDTH, EDGE_BUFFER, false));
                }
                return;
            }

            player.Update(this, deltaTime);
            if(player.Hitbox.Bottom  < endY) {
                Game1.Game.NextLevel();
            }

            bool enemiesLeft = false;
            foreach(Enemy enemy in enemies) {
                enemy.Update(this, deltaTime);
                if(enemy.Alive) {
                    enemiesLeft = true;
                }
            }
            if(!enemiesLeft && !cleared) {
                cleared = true; // make sure only deletes the wall once
                walls.RemoveAt(0);
            }

            foreach(Entity ability in abilities) {
                ability.Update(this, deltaTime);
            }
            foreach(Particle particle in particles) {
                particle.Update(deltaTime);
            }

            // clear dead enemies and projectiles, and completed particles
            for(int i = 0; i < enemies.Count; i++) {
                if(!enemies[i].IsActive) {
                    enemies.RemoveAt(i);
                    i--;
                }
            }

            for(int i = 0; i < abilities.Count; i++) {
                if(!abilities[i].IsActive) {
                    abilities.RemoveAt(i);
                    i--;
                }
            }

            for(int i = 0; i < particles.Count; i++) {
                if(particles[i].Done) {
                    particles.RemoveAt(i);
                    i--;
                }
            }
        }

        public void Draw(SpriteBatch sb) {
            // draw background
            sb.Draw(Graphics.Pixel, new Rectangle(0, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.DarkGreen * 0.5f);

            // draw level
            foreach (Wall wall in walls) { // allow entites to overlap with walls
                wall.Draw(sb);
            }

            foreach(Entity ability in abilities) { // projectiles under enemies looks better
                ability.Draw(sb);
            }

            enemies.Sort((enemy1, enemy2) => { return enemy1.Hitbox.Bottom - enemy2.Hitbox.Bottom; }); // draw enemies from back to front
            foreach(Enemy enemy in enemies) { // enemies before walls so if clipping happens, it's hidden
                enemy.Draw(sb);
            }

            Game1.Player.Draw(sb);

            foreach(Particle particle in particles) { // particles are typically small enough to fit on top of entities
                particle.Draw(sb);
            }

            Game1.Player.DrawUI(sb);
        }

        // runs at the end of the constructor
        private void DefineCameraSpace() {
            // find edge bounds
            Vector2 topLeft = new Vector2(10000, 10000);
            Vector2 bottomRight = new Vector2(-10000, -10000);
            foreach(Wall wall in walls) {
                Rectangle zone = wall.Hitbox;

                if(zone.Right < topLeft.X) {
                    topLeft.X = zone.Right;
                }
                if(zone.Left > bottomRight.X) {
                    bottomRight.X = zone.Left;
                }
                if(zone.Bottom < topLeft.Y) {
                    topLeft.Y = zone.Bottom;
                }
                if(zone.Top > bottomRight.Y) {
                    bottomRight.Y = zone.Top;
                }
            }

            bounds = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y));
            bounds.Inflate(EDGE_BUFFER, EDGE_BUFFER);
        }

        // Creates a list of possible difficulty breakdowns based on the total difficulty and returns a random one
        // There should be between 3 and 5 enemies per level.
        // Enemy difficulties are 1-3
        private Stack<int> CreateDifficulties(int difficulty) {
            // handle special cases
            if(difficulty <= 0) {
                return new Stack<int>();
            }
            else if(difficulty == 1) {
                Stack<int> res1 = new Stack<int>();
                res1.Push(0);
                return res1;
            }
            else if(difficulty == 2) {
                Stack<int> res2 = new Stack<int>();
                res2.Push(0);
                res2.Push(0);
                return res2;
            }

            // the first index of each array is the number of 1 difficulty enemies. Index 1 is difficulty 2 and index 2 is difficulty 3
            List<int[]> difficultyOptions = new List<int[]>();
            difficultyOptions.Add(new int[3]{ 3, 0, 0 });
            difficulty -= 3;
            
            // "recursively" determine all possibilities
            for(int c = 0; c < difficulty; c++) {
                List<int[]> nextOptions = new List<int[]>();

                // fill next options by adding a new 1 power enemy or powering up an existing one
                foreach(int[] option in difficultyOptions) {
                    if(option[0] + option[1] + option[2] < 5) { // don't create more than 5 enemies
                        nextOptions.Add(new int[3]{ option[0] + 1, option[1], option[2] }); // add another 1 power enemy
                    }
                    if(option[0] > 1) { // leave at least 1 power 1 enemy
                        nextOptions.Add(new int[3]{ option[0] - 1, option[1] + 1, option[2] }); // upgrade a 1 power to 2 power
                    }
                    if(option[1] > 0) { // must have a 2 power enemy to power up
                        nextOptions.Add(new int[3] { option[0], option[1] - 1, option[2] + 1 }); // upgrade a 2 power to 3 power
                    }
                }

                // pass to current options
                difficultyOptions = nextOptions;
            }

            if(difficultyOptions.Count <= 0) {
                return new Stack<int>();
            }

            // convert result to a more useful form
            Stack<int> result = new Stack<int>();
            int[] resultArr = difficultyOptions[Game1.RNG.Next(difficultyOptions.Count)]; // choose a random option here
            for(int difficultyIndex = 0; difficultyIndex <= 2; difficultyIndex++) { // for each difficulty value
                for(int counter = 0; counter < resultArr[difficultyIndex]; counter++) { // add that difficulty value this number of times
                    result.Push(difficultyIndex);
                }
            }

            return result;
        }
    }
}
