using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace EcoSystem
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        List<Texture2D> defaultForestTextures = new List<Texture2D>();
        List<Texture2D> factoryForestTextures = new List<Texture2D>();
        List<Texture2D> citadelForestTextures = new List<Texture2D>();
        List<Texture2D> defaultUrbanTextures = new List<Texture2D>();
        List<Texture2D> factoryUrbanTextures = new List<Texture2D>();
        List<Texture2D> citadelUrbanTextures = new List<Texture2D>();
        
        const int BOARDSIZEX = 31;
        const int BOARDSIZEY = 30;
        const int SPACINGX = 40;
        const int SPACINGY = 30;
        const int TILESCALEX = 48;
        const int TILESCALEY = 50;

        Tile[,] board = new Tile[BOARDSIZEX,BOARDSIZEY];



        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = (BOARDSIZEY*SPACINGY)+SPACINGY;
            graphics.PreferredBackBufferWidth = BOARDSIZEX*SPACINGX;
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            Random rnd = new Random();
            this.IsMouseVisible = true;

            for (int x = 0; x < BOARDSIZEX; x++)
            {
                for (int y = 0; y < BOARDSIZEY; y++)
                {
                    if ((x+y)>=(BOARDSIZEX+BOARDSIZEY)/2) {
                        Texture2D rndText = defaultUrbanTextures[rnd.Next(0,defaultUrbanTextures.Count)];
                        board[x, y] = new Tile(x, y, false, rndText);
                    }
                    else if ((x + y) < (BOARDSIZEX + BOARDSIZEY) / 2)
                    {
                        Texture2D rndText = defaultForestTextures[rnd.Next(0,defaultForestTextures.Count)];
                        board[x, y] = new Tile(x, y, false, rndText);
                    }
                }
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            string folderPath;
            int count;

            folderPath = "Forest\\Default\\";
            count = Directory.GetFiles("Content\\" + folderPath).Length;

            for (int i = 0; i < count; i++)
            {
                try
                {
                    defaultForestTextures.Add(Content.Load<Texture2D>(folderPath + i.ToString()));
                }
                catch
                {
                    break;
                }
            }

            folderPath = "Urban\\Default\\";
            count = Directory.GetFiles("Content\\" + folderPath).Length;

            for (int i = 0; i < count; i++)
            {
                try
                {
                    defaultUrbanTextures.Add(Content.Load<Texture2D>(folderPath + i.ToString()));
                }
                catch
                {
                    break;
                }
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            // TODO: Add your drawing code here
            for (int y = 0; y < BOARDSIZEY; y++)
            {
                for (int x = 0; x < BOARDSIZEX; x++)
                {
                    spriteBatch.Draw(board[x, y].getTexture(), new Rectangle(x * SPACINGX, y * SPACINGY, TILESCALEX, TILESCALEY), Color.White);
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
