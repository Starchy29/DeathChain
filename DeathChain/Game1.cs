using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DeathChain
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Level currentLevel;
        private Player player;
        public Player Player { get { return player; } }

        public const int StartScreenWidth = 1600;
        public const int StartScreenHeight = 900;
        private Matrix transforms = Matrix.Identity; // transformation matrix that positions gameplay in the window
        private int xOffset;
        private int yOffset;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            graphics.PreferredBackBufferWidth = StartScreenWidth;
            graphics.PreferredBackBufferHeight = StartScreenHeight;
            this.Window.Title = "Death Chain";
            IsMouseVisible = true;

            Window.ClientSizeChanged += OnResize;

            Input.Setup();
            Camera.Start();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            Graphics.Pixel = Content.Load<Texture2D>("Pixel");

            player = new Player();
            currentLevel = new Level();
        }

        protected override void UnloadContent() { }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Input.Update();

            currentLevel.Update();
            player.Update(currentLevel);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(20, 20, 20));
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, transforms);

            currentLevel.Draw(spriteBatch);
            player.Draw(spriteBatch);

            spriteBatch.End();

            // add black bars
            spriteBatch.Begin(); // reset scaling and everything
            if(xOffset > 0) {
                spriteBatch.Draw(Graphics.Pixel, new Rectangle(0, 0, xOffset, GraphicsDevice.Viewport.Height), Color.Black); // left
                spriteBatch.Draw(Graphics.Pixel, new Rectangle(GraphicsDevice.Viewport.Width - xOffset, 0, xOffset, GraphicsDevice.Viewport.Height), Color.Black); // right
            }
            else if(yOffset > 0) {
                spriteBatch.Draw(Graphics.Pixel, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, yOffset), Color.Black); // top
                spriteBatch.Draw(Graphics.Pixel, new Rectangle(0, GraphicsDevice.Viewport.Height - yOffset, GraphicsDevice.Viewport.Width, yOffset), Color.Black); // bottom
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void OnResize(Object sender, EventArgs e) {
            xOffset = 0;
            yOffset = 0;
            if(GraphicsDevice.Viewport.Width > GraphicsDevice.Viewport.Height * 16 / 9f) { // determine which dimension is the limit
                // bars on left and right because wider than tall
                float scale = (float)GraphicsDevice.Viewport.Height / StartScreenHeight;
                transforms = Matrix.CreateScale(scale, scale, 1);
                xOffset = (int) ((GraphicsDevice.Viewport.Width - GraphicsDevice.Viewport.Height * 16 / 9f) / 2f);
                transforms = transforms * Matrix.CreateTranslation(xOffset, 0, 0);
            } else {
                // bars above and below because taller than wide
                float scale = (float)GraphicsDevice.Viewport.Width / StartScreenWidth;
                transforms = Matrix.CreateScale(scale, scale, 1);
                yOffset = (int) ((GraphicsDevice.Viewport.Height - GraphicsDevice.Viewport.Width * 9 / 16f) / 2f);
                transforms = transforms * Matrix.CreateTranslation(0, yOffset, 0);
            }
        }

        public char GetOsDelimeter() {
            switch(Environment.OSVersion.Platform) {
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    return '/';
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return '\\';
                // OSs other than Mac/Windows are not supported
                default:
                    throw new SystemException("Unknown Operating System");
            }
        }
    }
}
