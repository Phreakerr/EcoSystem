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
using System.Threading;

namespace EcoSystem
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        MouseState prevMouseState;
        MouseState currMouseState;
        List<Texture2D> defaultForestTextures = new List<Texture2D>();
        List<Texture2D> factoryForestTextures = new List<Texture2D>();
        List<Texture2D> citadelForestTextures = new List<Texture2D>();
        List<Texture2D> defaultUrbanTextures = new List<Texture2D>();
        List<Texture2D> factoryUrbanTextures = new List<Texture2D>();
        List<Texture2D> citadelUrbanTextures = new List<Texture2D>();
        List<Texture2D> menuIconTextures = new List<Texture2D>();
        
        const int BOARDSIZEX = 25;
        const int BOARDSIZEY = 24;
        const int SPACINGX = 40;
        const int SPACINGY = 32;
        const int TILESCALEX = 48;
        const int TILESCALEY = 50;

        Player forestPlayer;
        Player urbanPlayer;

        Tile[,] board = new Tile[BOARDSIZEX,BOARDSIZEY];
        Tile selectedTile;

        SpriteFont resourceFont;


        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = (BOARDSIZEY*SPACINGY)+SPACINGY+100;
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

            forestPlayer = new Player();
            urbanPlayer = new Player();

            for (int x = 0; x < BOARDSIZEX; x++)
            {
                for (int y = 0; y < BOARDSIZEY; y++)
                {
                    if ((x+y)>=(BOARDSIZEX+BOARDSIZEY)/2) {
                        Texture2D rndText = defaultUrbanTextures[rnd.Next(0,defaultUrbanTextures.Count)];
                        board[x, y] = new Tile(x, y, true, rndText);
                    }
                    else if ((x + y) < (BOARDSIZEX + BOARDSIZEY) / 2)
                    {
                        Texture2D rndText = defaultForestTextures[rnd.Next(0,defaultForestTextures.Count)];
                        board[x, y] = new Tile(x, y, false, rndText);
                    }
                }
            }

            board[15, 12].fireStart();
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

            menuIconTextures.Add(Content.Load<Texture2D>("Icons\\narrowAttack"));
            menuIconTextures.Add(Content.Load<Texture2D>("Icons\\broadAttack"));
            menuIconTextures.Add(Content.Load<Texture2D>("Icons\\fireAttack"));
            menuIconTextures.Add(Content.Load<Texture2D>("Icons\\waterAttack"));
            menuIconTextures.Add(Content.Load<Texture2D>("Icons\\upgradeAttack"));

            resourceFont = Content.Load<SpriteFont>("Resources");
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

            currMouseState = Mouse.GetState();

            if (prevMouseState.LeftButton == ButtonState.Released && currMouseState.LeftButton == ButtonState.Pressed)
            {
                detectTileClicked(prevMouseState.X, prevMouseState.Y);
            }
            else if (prevMouseState.RightButton == ButtonState.Released && currMouseState.RightButton == ButtonState.Pressed)
            {
                selectedTile = null;
            }

            spreadFires();
            doFireDamage();
            checkDeadTiles();

            forestPlayer.resources++;
            urbanPlayer.resources++;

            base.Update(gameTime);

            prevMouseState = Mouse.GetState();
        }

        private void spreadFires()
        {
            Random rndSpread = new Random();

            foreach (Tile tile in board)
            {
                if (tile.checkFire() && rndSpread.Next(0, 250) == 7)
                {
                    Random rndPosition = new Random();
                    try
                    {
                        board[(int)tile.position.X + rndPosition.Next(-1, 1), (int)tile.position.Y + rndPosition.Next(-1, 1)].fireStart();
                    }
                    catch
                    {

                    }
                }
            }
        }

        private void checkDeadTiles()
        {
            foreach (Tile tile in board)
            {
                tile.checkDestroyed();
            }
        }

        private void doFireDamage()
        {
            foreach (Tile tile in board)
            {
                tile.fireDamage();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            Random rnd = new Random();

            // TODO: Add your drawing code here
            for (int y = 0; y < BOARDSIZEY; y++)
            {
                for (int x = 0; x < BOARDSIZEX; x++)
                {
                    //spriteBatch.Draw(board[x, y].getTexture(), new Rectangle(x * SPACINGX, y * SPACINGY, TILESCALEX, TILESCALEY), Color.White);
                    if (selectedTile != null && x == selectedTile.position.X && y == selectedTile.position.Y)
                    {
                        spriteBatch.Draw(board[x, y].getTexture(), new Rectangle(x * SPACINGX, y * SPACINGY, TILESCALEX, TILESCALEY), new Color(255,100,100));
                    }
                    else
                    {
                        if (board[x, y].checkFire())
                        {
                            spriteBatch.Draw(board[x, y].getTexture(), new Rectangle(x * SPACINGX, y * SPACINGY, TILESCALEX, TILESCALEY), Color.Red);
                        }
                        else if (board[x, y].checkDestroyed())
                        {
                            spriteBatch.Draw(board[x, y].getTexture(), new Rectangle(x * SPACINGX, y * SPACINGY, TILESCALEX, TILESCALEY), Color.Black);
                        }
                        else
                        {
                            spriteBatch.Draw(board[x, y].getTexture(), new Rectangle(x * SPACINGX, y * SPACINGY, TILESCALEX, TILESCALEY), Color.White);
                        }
                    }
                }
            }

            spriteBatch.Draw(menuIconTextures[0], new Rectangle(((menuIconTextures[0].Width + 10) * 0)+10, graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10, menuIconTextures[0].Height, menuIconTextures[0].Width), Color.White);
            spriteBatch.Draw(menuIconTextures[1], new Rectangle(((menuIconTextures[0].Width + 10) * 1)+10, graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10, menuIconTextures[0].Height, menuIconTextures[0].Width), Color.White);
            spriteBatch.Draw(menuIconTextures[2], new Rectangle(((menuIconTextures[0].Width + 10) * 2)+10, graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10, menuIconTextures[0].Height, menuIconTextures[0].Width), Color.White);
            spriteBatch.Draw(menuIconTextures[3], new Rectangle(((menuIconTextures[0].Width + 10) * 3)+10, graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10, menuIconTextures[0].Height, menuIconTextures[0].Width), Color.White);
            spriteBatch.Draw(menuIconTextures[4], new Rectangle(((menuIconTextures[0].Width + 10) * 4)+10, graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10, menuIconTextures[0].Height, menuIconTextures[0].Width), Color.White);

            spriteBatch.DrawString(resourceFont, "Resources: " + forestPlayer.resources, new Vector2(graphics.PreferredBackBufferWidth - 10, graphics.PreferredBackBufferHeight - 10), Color.White, 0, resourceFont.MeasureString("Resources: " + forestPlayer.resources), 1, SpriteEffects.None, 0);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        void detectTileClicked(int X, int Y) {
            int boardX, boardY;

            boardX = (X / SPACINGX);
            boardY = (Y / SPACINGY);

            if (Y < BOARDSIZEY * SPACINGY && X < BOARDSIZEX * SPACINGX && X >= 0 && Y >=0)
            {
                if (selectedTile == board[boardX, boardY])
                {
                    selectedTile = null;
                }
                else
                {
                    selectedTile = board[boardX, boardY];
                }
            }
            else
            {
                if (Y > graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10 && Y < graphics.PreferredBackBufferHeight - 10)
                {
                    int menuIconPressed = X / (menuIconTextures[0].Width + 10);
                    Console.Write(menuIconPressed);

                    Thread thread;

                    switch (menuIconPressed)
                    {
                        case 0:
                            thread = new Thread(new ThreadStart(narrowAttack));
                            thread.Start();
                            break;
                        case 1:
                            thread = new Thread(new ThreadStart(broadAttack));
                            thread.Start();
                            break;
                        case 2:
                            thread = new Thread(new ThreadStart(fireAttack));
                            thread.Start();
                            break;
                        case 3:
                            thread = new Thread(new ThreadStart(waterAttack));
                            thread.Start();
                            break;
                        case 4:
                            thread = new Thread(new ThreadStart(upgradeUnit));
                            thread.Start();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void upgradeUnit()
        {
            Console.Write("Upgrade");
        }

        private void waterAttack()
        {
            Console.Write("Water");
        }

        private void fireAttack()
        {
            Console.Write("Fire");
        }

        private void broadAttack()
        {
            Console.Write("Broad");
        }

        private void narrowAttack()
        {
            Console.Write("Narrow");
        }
    }
}